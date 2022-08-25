Imports System.Drawing
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Globalization
Imports System.Timers
Imports UvARescue.Math
Imports AForge.Fuzzy
Imports System.Math
Imports System.Threading.Timer
Imports UvARescue.Agent




''' <summary>
''' Base class for a single agent that provides the following core 
''' functionalities:
''' - spawning the virtual robot in the game
''' - sending and recieving of messages 
''' - sensor and actor mounting
''' 
''' The Agent and all its Sensors run in the Driver's thread. Therefore
''' they assume free access to each other's members where no locking is
''' necessary. 
''' 
''' Observers attached to Agent typically run on an external thread. Therefore
''' Adding, Removing and Notifying observers IS implemented with locking.
''' 
''' </summary>
''' <remarks></remarks>
''' 
Public Class Agent
    Inherits WorldView

#Region " Constructor "
    'Public _CurrentPath As Vector2() '(Okke) I think this is not used anywhere.

    Public Sub New(ByVal manifold As Manifold, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(manifold, name, Nothing)

        'EMO
        Me.PathPlanGoalPos2 = Nothing

        Me._manifold = manifold

        Me._newTarget = New Pose2D(0, 0, 0)

        If IsNothing(agentConfig) Then Throw New ArgumentNullException("agentConfig")
        If IsNothing(teamConfig) Then Throw New ArgumentNullException("teamConfig")

        If Not CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = NumberFormatInfo.InvariantInfo.NumberDecimalSeparator Then
            'This prevents BufferedLayer overflow, due to huge laser ranges if ',' and '.' are interchanged
            Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US", False)
            Console.WriteLine("[Agent]: CurrentCulture is now {0}.", CultureInfo.CurrentCulture.Name)
        End If

        If agentConfig.AgentType <> "operator" Then 'Operator doesn't need all these mounted sensors

            'by default, the agent will mount several standard sensors
            Me._StatusSensor = New StatusSensor 'no need to mount this one, uses STA messsage instead of SEN messages

            'Temp disabled to reduce load
            Me._GroundTruthSensor = New GroundTruthSensor

            'If agentConfig.RobotModel.ToLower = "nomad" Then
            'this is the real robot!!!
            'Me.Mount(New LaserRangeSensor("LaserScannerURG", LaserRangeDeviceType.Hokuyo, 0.2F, 3.8F, 0.38F, -0.134285569F))
            'Me.Mount(New CameraSensor(agentConfig, "Camera"))

            'ElseIf agentConfig.RobotModel.ToLower = "nomadwithsick" Then
            'this is the real robot with a Sick from Sweden !!!
            'http://www.science.uva.nl/research/ias/links/cogniron/fs2hsc/Data/Home1/Sensors/Scanner.xml

            '   Me.Mount(New LaserRangeSensor("SICK", LaserRangeDeviceType.SickLMS, 0.2F, 7.9F, 0.07F, 0.005F)) 'and height 0.633 meter
            '  Me.Mount(New CameraSensor(agentConfig, "Camera"))

            'ElseIf agentConfig.RobotModel.ToLower = "talon" Then

            'the talon has no room for a SICK, so we equipped it with a Hokuyo
            'Me.Mount(New LaserRangeSensor("Scanner1", LaserRangeDeviceType.Hokuyo, 0.2F, 3.8F, 0.38F, -0.134285569F))

            ' RIKNOTE: disabled after discussion with Arnoud
            'ElseIf agentConfig.RobotModel.ToLower = "p2dx" Then
            '    'Pioneer also has a Hokuyo, but on its center
            'Me.Mount(New LaserRangeSensor("Scanner1", LaserRangeDeviceType.Hokuyo, 0.2F, 4.0F, 0.0F, 0.0F))
            'Me.Mount(New CameraSensor(agentConfig, "FCam")) 'and also GCam, Acam, Rcam

            'ElseIf agentConfig.RobotModel.ToLower = "hummer" Then
            '   Me.Mount(New LaserRangeSensor("Scanner1", LaserRangeDeviceType.SickLMS, 0.2F, 79.0F, 0.0F, 0.0F))
            '  Me.Mount(New CameraSensor(agentConfig, "Camera"))

            'ElseIf agentConfig.RobotModel.ToLower = "ers" Then 'simulate the IR rangescanner with a laserscanner with a distance of 1m
            '   Me.Mount(New LaserRangeSensor("Scanner1", LaserRangeDeviceType.SickLMS, 0.05F, 1.0F, 0.0F, 0.0F))
            '  Me.Mount(New CameraSensor(agentConfig, "Camera"))

            If agentConfig.RobotModel.ToLower = "airrobot" Then

                Me.Mount(New CameraSensor(agentConfig, "Camera"))
                Me.Mount(New LaserRangeSensor("Hokuyo", LaserRangeDeviceType.Hokuyo, 0.2F, 5.4F, -0.008F, 0.0F))
                'at most a Hokuyo on a airrobot

            ElseIf agentConfig.RobotModel.ToLower = "kenaf" Then
                Me.Mount(New LaserRangeSensor("Hokuyo", LaserRangeDeviceType.Hokuyo, 0.2F, 5.4F, -0.008F, 0.0F))
                Me.Mount(New CameraSensor(agentConfig, "fishEyeCamera"))
                Me.Mount(New CameraSensor(agentConfig, "Camera")) 'Front

            ElseIf agentConfig.RobotModel.ToLower = "zerg" Then

                Me.Mount(New LaserRangeSensor("Scanner1", LaserRangeDeviceType.Hokuyo, 0.2F, 3.8F, 0.038F, 0.0F))
                Me.Mount(New CameraSensor(agentConfig, "Camera"))

                'ElseIf agentConfig.RobotModel.ToLower = "matilda" OrElse agentConfig.RobotModel.ToLower = "element" Then

                'If agentConfig.LogPlayback = True Then
                'Me.Mount(New LaserRangeSensor("HokuyoURG4", LaserRangeDeviceType.Hokuyo, 0.2F, 4.8F, 0.055F, 0.0F))
                'Else
                '   Me.Mount(New LaserRangeSensor("scanner1", LaserRangeDeviceType.Hokuyo, 0.2F, 4.8F, 0.055F, 0.0F))
                'E() 'nd If
                'Me.Mount(New CameraSensor(agentConfig, "Camera"))

            ElseIf agentConfig.RobotModel.ToLower = "p3at" Then
                Console.WriteLine("p3at is spawned")
                Me.Mount(New LaserRangeSensor("lms200", LaserRangeDeviceType.SickLMS, 0.2F, 79.0F, 0.0F, 0.0F))
                Me.Mount(New TachometerSensor(4)) '4 wheels
                Me.Mount(New CameraSensor(agentConfig, "Camera"))

            ElseIf agentConfig.RobotModel.ToLower = "comstation" Then
                Console.WriteLine("p3at is spawned")
                Me.Mount(New LaserRangeSensor("lms200", LaserRangeDeviceType.SickLMS, 0.2F, 79.0F, 0.0F, 0.0F))
                Me.Mount(New TachometerSensor(4)) '4 wheels
                Me.Mount(New CameraSensor(agentConfig, "Camera"))

            ElseIf agentConfig.RobotModel.ToLower = "atrv" Then

                Me.Mount(New LaserRangeSensor("lms200", LaserRangeDeviceType.SickLMS, 0.2F, 19.8F, 0.0F, 0.0F))
                'No Tachometer
                Me.Mount(New CameraSensor(agentConfig, "WebCam"))

            ElseIf agentConfig.RobotModel.ToLower = "p2at" Then
                Me.Mount(New LaserRangeSensor("Scanner1", LaserRangeDeviceType.SickLMS, 0.2F, 19.8F, 0.0F, 0.0F))
                'Me.Mount(New LaserRangeSensor("TiltedScanner", LaserRangeDeviceType.Hokuyo, 0.2F, 3.8F, 0.038F, 0.0F))
                Me.Mount(New CameraSensor(agentConfig, "Camera"))

            Else

                'default to a SICK for all other robots
                Me.Mount(New LaserRangeSensor("Scanner1", LaserRangeDeviceType.SickLMS, 0.2F, 31.8F, 0.0F, 0.0F))
                Me.Mount(New CameraSensor(agentConfig, "Camera"))

            End If

            If agentConfig.RobotModel.ToLower <> "p3at" AndAlso agentConfig.RobotModel.ToLower <> "atrv" Then
                'UT3 robot have no sonars
                'Console.WriteLine("fucking missunderstanding")
                Me._SonarSensor = New SonarSensor
                Me.Mount(Me._SonarSensor)
                Me._SonarSensor.DefaultGeometry(agentConfig.RobotModel)
            End If



            
            Me._GPSSensor = New GPSSensor

            Me._InsSensor = New InsSensor

            Me._OdometrySensor = New OdometrySensor

            Me._EncoderSensor = New EncoderSensor


            Me._RfidSensor = New RfidSensor

            Me.Mount(Me._StatusSensor)
            Me.Mount(Me._GroundTruthSensor)

            'Temp (see note above)
            Me.Mount(Me._GPSSensor)

            Me.Mount(Me._InsSensor)
            'Me.Mount(Me._OdometrySensor) 'to prevent activating AirRobot localization

            Me.Mount(Me._EncoderSensor)

            Me.Mount(Me._RfidSensor)

            'by default, the agent will also mount a DriveActor
            Me._DriveActor = Me.CreateDriveActor
            If Not IsNothing(Me._DriveActor) Then
                Me.Mount(Me._DriveActor)
            End If

            'fafa add this for create nao agent
            Me._NaoAgent = Me.createNao
            If Not IsNothing(Me._NaoAgent) Then
                Me.Mount(Me._NaoAgent)
            End If


            'by default, the agent will also mount a FlyActor
            Me._FlyActor = Me.CreateFlyActor
            If Not IsNothing(Me._FlyActor) Then
                Me.Mount(Me._FlyActor)
            End If

            Me._TargetLocations = New Queue(Of Pose2D)

            Me._CurrentCamera = 0
            Me._ShowImages = True

        End If 'If not operator


        Me._TeamConfig = teamConfig

        Me._AgentConfig = agentConfig

        With agentConfig

            Me._Number = .AgentNumber
            Console.WriteLine("robot model :: {0}", .RobotModel)
            Me._RobotModel = .RobotModel
            Me._Size = New Size(450, 450) 'mm
            Me._StartLocation = .StartLocation
            Me._StartRotation = .StartRotation

            'extract Pose2D from start-location and -rotation
            Console.WriteLine("Start location: {0}", .StartLocation)
            Dim x As Single = 0
            Dim y As Single = 0
            Dim rot As Single = 0
            Dim parts() As String = Strings.Split(.StartLocation, ",")
            Try
                x = Single.Parse(parts(0))
                y = Single.Parse(parts(1))
            Catch ex As Exception 'FormatException or IndexOutOfRangeException
                ' Do nothing, x and y are initialized to 0.
                Console.WriteLine("Start location did not parse. Set to 0,0.")
            End Try
            parts = Strings.Split(.StartRotation, ",")
            Try
                rot = Single.Parse(parts(2))
            Catch ex As Exception 'FormatException or IndexOutOfRangeException
                ' Do nothing, rot is initialized at 0.
                Console.WriteLine("Start rotation did not parse. Set to 0.")
            End Try
            ' Set the path plan goal
            'parts = Strings.Split(.PathPlanGoal, ",")

            Try
                Me._PathPlanGoalPos = New Vector2(Double.Parse(parts(0)), Double.Parse(parts(1)))
            Catch
                Me._PathPlanGoalPos = New Vector2(83, 49)
            End Try



            Me._StartPose = New Pose2D(x, y, rot)

            'also init current pose, but beware that this should be in mm 
            SyncLock poseMutex
                Me._CurrentPoseEstimate = New Pose2D(x * 1000, y * 1000, rot)
            End SyncLock

        End With

        'Try
        'Me.myMainAlgorithm = New Astar(Me._manifold, Me.Name, Me._AgentConfig, Me._TeamConfig)
        'Catch ex As Exception

        'End Try

    End Sub

    

#End Region

#Region " Agent Properties "


    Public Shared ReadOnly SENSORTYPE_RANGESCANNER As String = "RangeScanner"

    Private _Number As Integer
    Public ReadOnly Property Number() As Integer
        Get
            Return _Number
        End Get
    End Property

    Private _RobotModel As String
    Public ReadOnly Property RobotModel() As String
        Get
            Return _RobotModel
        End Get
    End Property

    Public position As String

    Private _Size As Size
    Public ReadOnly Property Size() As Size
        Get
            Return Me._Size
        End Get
    End Property

    Private _manifold As Manifold
    


    Private _StartLocation As String
    Public ReadOnly Property StartLocation() As String
        Get
            Return _StartLocation
        End Get
    End Property

    Private _StartRotation As String
    Public ReadOnly Property StartRotation() As String
        Get
            Return _StartRotation
        End Get
    End Property

    Private _StartPose As Pose2D
    Public ReadOnly Property StartPose() As Pose2D
        Get
            Return Me._StartPose
        End Get
    End Property


    Private _TeamConfig As TeamConfig
    Public ReadOnly Property TeamConfig() As TeamConfig
        Get
            Return Me._TeamConfig
        End Get
    End Property

    Private _AgentConfig As AgentConfig
    Public ReadOnly Property AgentConfig() As AgentConfig
        Get
            Return Me._AgentConfig
        End Get
    End Property



    Private _PathPlanGoalPos As Vector2
    Public ReadOnly Property PathPlanGoalPos() As Vector2
        Get
            Return Me._PathPlanGoalPos
        End Get
    End Property

    Private _PathPlanGoalPos2 As Vector2()
    Public Property PathPlanGoalPos2() As Vector2()
        Get
            Return Me._PathPlanGoalPos2
        End Get
        Set(ByVal value As Vector2())
            Me._PathPlanGoalPos2 = value
        End Set
    End Property

    Public _TargetLocations As Queue(Of Pose2D)
    Public _ResetCurrentPath As Boolean = False
    Public _SignalStrengthMatrix(,) As Double


#End Region

#Region " Agent Default Sensors "

    Private _StatusSensor As StatusSensor
    Protected ReadOnly Property StatusSensor() As StatusSensor
        Get
            Return Me._StatusSensor
        End Get
    End Property
    Public ReadOnly Property Status() As StatusData
        Get
            Return Me._StatusSensor.CurrentData
        End Get
    End Property

    Private _GroundTruthSensor As GroundTruthSensor
    Protected ReadOnly Property GroundTruthSensor() As GroundTruthSensor
        Get
            Return Me._GroundTruthSensor
        End Get
    End Property
    'Public ReadOnly Property GroundTruth() As GroundTruthData
    '    Get
    '        Return Me._GroundTruthSensor.CurrentData
    '    End Get
    'End Property

    Public ReadOnly Property GroundTruthPose() As Pose3D
        Get
            If Not IsNothing(Me.GroundTruthSensor) AndAlso Not IsNothing(Me.GroundTruthSensor.PoseEstimate) Then
                Return Me.GroundTruthSensor.PoseEstimate
            Else
                Return New Pose3D(Me.CurrentPoseEstimate.X, Me.CurrentPoseEstimate.Y, 0.0, 0.0, 0.0, Me.CurrentPoseEstimate.Rotation)
            End If
        End Get
    End Property

    Private _GPSSensor As GPSSensor
    Public ReadOnly Property GPSSensor() As GPSSensor
        Get
            Return Me._GPSSensor
        End Get
    End Property
    Public ReadOnly Property GpsPose() As Vector2
        Get
            If Not IsNothing(Me.GPSSensor) AndAlso Not IsNothing(Me.GPSSensor.PoseEstimate) Then
                Return Me.GPSSensor.PoseEstimate
            Else
                Return New Vector2(Me.CurrentPoseEstimate.X, Me.CurrentPoseEstimate.Y)
            End If
        End Get
    End Property

    Private _SonarSensor As SonarSensor
    Public ReadOnly Property SonarSensor() As SonarSensor
        Get
            Return Me._SonarSensor
        End Get
    End Property

   


    Private _RfidSensor As RfidSensor
    Public ReadOnly Property RfidSensor() As RfidSensor
        Get
            Return Me._RfidSensor
        End Get
    End Property

    Private _InsSensor As InsSensor
    Protected ReadOnly Property InsSensor() As InsSensor
        Get
            Return Me._InsSensor
        End Get
    End Property
    'Public ReadOnly Property Ins() As InsData
    '    Get
    '        Return Me._InsSensor.CurrentData
    '    End Get
    'End Property

    Public ReadOnly Property InsPose() As Pose3D
        Get
            If Not IsNothing(Me.InsSensor) AndAlso Not IsNothing(Me.InsSensor.PoseEstimate) Then
                Return Me._InsSensor.PoseEstimate
            Else
                Return New Pose3D(Me.CurrentPoseEstimate.X, Me.CurrentPoseEstimate.Y, 0.0, 0.0, 0.0, Me.CurrentPoseEstimate.Rotation)
            End If
        End Get
    End Property

    Private _OdometrySensor As OdometrySensor
    Protected ReadOnly Property OdometrySensor() As OdometrySensor
        Get
            Return Me._OdometrySensor
        End Get
    End Property

    'Public ReadOnly Property Odometry() As OdometryData
    '    Get
    '        Return Me._OdometrySensor.CurrentData
    '    End Get
    'End Property

    Public _newTarget As Pose2D
    Protected ReadOnly Property getnewTarget() As Pose2D
        Get
            Return Me._newTarget
        End Get
    End Property


    Public ReadOnly Property OdometryPose() As Pose2D
        Get
            If Not IsNothing(Me.OdometrySensor) AndAlso Not IsNothing(Me.OdometrySensor.PoseEstimate) Then
                Return Me.OdometrySensor.PoseEstimate
            Else
                Return Me.CurrentPoseEstimate
            End If
        End Get
    End Property
    Private _EncoderSensor As EncoderSensor
    Public ReadOnly Property EncoderSensor() As EncoderSensor
        Get
            Return Me._EncoderSensor
        End Get
    End Property

    'Private ReadOnly Property Encoder() As EncoderData
    '    Get
    '        Return Me._EncoderSensor.CurrentData
    '    End Get
    'End Property

    Public ReadOnly Property EncoderPose() As Pose2D
        Get
            If Not IsNothing(Me.EncoderSensor) AndAlso Not IsNothing(Me.EncoderSensor.PoseEstimate) Then
                Return Me.EncoderSensor.PoseEstimate
            Else
                Return Me.CurrentPoseEstimate
            End If
        End Get
    End Property

    Private _TouchSensor As TouchSensor
    Public ReadOnly Property TouchSensor() As TouchSensor
        Get
            Return Me._TouchSensor
        End Get
    End Property

    
#End Region

#Region "Platform capabilities"

    Public Overridable ReadOnly Property MaxSpeed() As Single
        Get
            Return 8
        End Get
    End Property

    Public Overridable ReadOnly Property WheelRadius() As Single
        Get
            Return 0.0
        End Get
    End Property

    Public Overridable ReadOnly Property WheelBase() As Single
        Get
            Return 0.0
        End Get
    End Property

    Public Overridable ReadOnly Property WheelSeparation() As Single
        Get
            Return 0.0
        End Get
    End Property
#End Region

#Region " Drive Actor "

    Private _DriveActor As DriveActor
    Protected ReadOnly Property DriveActor() As DriveActor
        Get
            Return Me._DriveActor
        End Get
    End Property

    Public Function CanDrive() As Boolean
        Return Not IsNothing(Me._DriveActor)
    End Function

    ''' <summary>
    ''' Override this function if you want to mount a different kind of DriveActor.
    ''' By default the StandardDriveActor will be created.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overridable Function CreateDriveActor() As DriveActor
        Return New StandardDriveActor
    End Function

    

    Public Function ForwardSpeed() As Single
        If Me.RobotModel.ToLower = "airrobot" AndAlso Not IsNothing(Me._FlyActor) Then
            Return Me._FlyActor.ForwardSpeed
        ElseIf Not IsNothing(Me._DriveActor) Then
            Return Me._DriveActor.ForwardSpeed
        End If
    End Function

    Public Function TurningSpeed() As Single
        If Me.RobotModel.ToLower = "airrobot" AndAlso Not IsNothing(Me._FlyActor) Then
        ElseIf Not IsNothing(Me._DriveActor) Then
            Return Me._DriveActor.TurningSpeed
        End If
    End Function

    Public Function StrafeSpeed() As Single
        If Me.RobotModel.ToLower = "airrobot" AndAlso Not IsNothing(Me._FlyActor) Then
            Return Me._FlyActor.StrafeSpeed
        End If
        Return 0.0
    End Function

    Public Function FlySpeed() As Single
        If Me.RobotModel.ToLower = "airrobot" AndAlso Not IsNothing(Me._FlyActor) Then
            Return Me._FlyActor.FlySpeed
        End If
        Return 0.0
    End Function

    Public Sub pathPlanTo(ByVal goalPose As Pose2D)
        If Not IsNothing(Me._DriveActor) Then
            Me._DriveActor.pathPlanTo(goalPose)
        End If
    End Sub

    Public Sub ToggleHeadLight()
        If Not IsNothing(Me._DriveActor) Then
            Me._DriveActor.ToggleHeadLight()
        End If
    End Sub

    Public Sub DifferentialDrive(ByVal speed As Single, ByVal turning_speed As Single)
        If Not IsNothing(Me._DriveActor) Then
            Me._DriveActor.DifferentialDrive(speed, turning_speed)
        End If
    End Sub

    Public Sub Drive(ByVal speed As Single)
        If Not IsNothing(Me._DriveActor) Then
            Me._DriveActor.Drive(speed)
        End If
    End Sub
    Public Sub Reverse(ByVal speed As Single)
        If Not IsNothing(Me._DriveActor) Then
            Me._DriveActor.Drive(-speed)
        End If
    End Sub

    
    Public Sub Halt()
        If Not IsNothing(Me._DriveActor) Then
            If Me.AgentConfig.RobotModel = "AirRobot" Then
                Me._DriveActor.Fly(0)
            Else
                Me._DriveActor.Drive(0)
            End If
        End If
    End Sub

    Public Sub Flip()
        If Not IsNothing(Me._DriveActor) Then
            Me._DriveActor.Flip()
        End If
    End Sub

    Public cTimer As Timers.Timer

    Public Sub TurnLeft(ByVal speed As Single)

        If Not IsNothing(Me._DriveActor) Then

            Me._DriveActor.Turn(speed)
        End If
    End Sub
    Public Sub TurnRight(ByVal speed As Single)
        If Not IsNothing(Me._DriveActor) Then
            Me._DriveActor.Turn(-speed)
        End If
    End Sub

    Public Sub CamUp()
        If Not IsNothing(Me._DriveActor) Then
            Me._DriveActor.CameraPanTilt(UvARescue.Tools.Constants.CAM_TILT_INCREMENT)
        End If
    End Sub
    Public Sub CamStop()
        Me._DriveActor.CameraPanTilt(0)
    End Sub
    Public Sub CamDown()
        If Not IsNothing(Me._DriveActor) Then
            Me._DriveActor.CameraPanTilt(-1 * UvARescue.Tools.Constants.CAM_TILT_INCREMENT)
        End If
    End Sub

    Public Sub ReleaseRFIDTag()
        Me._DriveActor.ReleaseRFIDTag()
    End Sub

#End Region

#Region " Fly Actor "
    Private _FlyActor As FlyActor
    Protected ReadOnly Property FlyActor() As FlyActor
        Get
            Return Me._FlyActor
        End Get
    End Property

    Public Function CanFly() As Boolean
        Return Not IsNothing(Me._FlyActor)
    End Function

    ''' <summary>
    ''' Override this function if you want to mount a different kind of FlyActor.
    ''' By default the StandardFlyActor will be created.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overridable Function CreateFlyActor() As FlyActor
        Return New StandardFlyActor
    End Function

    'Public Function ForwardSpeed() As Single
    '    If Not IsNothing(Me._FlyActor) Then
    '        Return Me._FlyActor.ForwardSpeed
    '    End If
    'End Function

    'Public Function TurningSpeed() As Single
    '    If Not IsNothing(Me._FlyActor) Then
    '        Return Me._FlyActor.TurningSpeed
    '    End If
    'End Function

    Public Sub DifferentialFly(ByVal speed As Single, ByVal turning_speed As Single)
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.DifferentialFly(speed, turning_speed)
        End If
    End Sub

    Public Sub Forward(ByVal speed As Single)
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.Fly(speed, 0)
        End If
    End Sub
    Public Sub Backward(ByVal speed As Single)
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.Fly(-speed, 0)
        End If
    End Sub
    Public Sub FlyHalt()
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.Fly(0, 0)
        End If
    End Sub

    Public Sub FlyTurnLeft(ByVal speed As Single)
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.Turn(-speed)
        End If
    End Sub
    Public Sub FlyTurnRight(ByVal speed As Single)
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.Turn(speed)
        End If
    End Sub
    Public Sub StrafeLeft(ByVal speed As Single)
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.Strafe(-speed)
        End If
    End Sub
    Public Sub StrafeRight(ByVal speed As Single)
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.Strafe(speed)
        End If
    End Sub

    Public Sub FlyCamUp()
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.CameraPanTilt(-1 * UvARescue.Tools.Constants.CAM_TILT_INCREMENT)
        End If
    End Sub
    Public Sub FlyCamStop()
        Me._FlyActor.CameraPanTilt(0)
    End Sub
    Public Sub FlyCamDown()
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.CameraPanTilt(UvARescue.Tools.Constants.CAM_TILT_INCREMENT)
        End If
    End Sub

    Public Sub FlyUp(ByVal speed As Single)
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.Fly(0, speed)
        End If
    End Sub
    Public Sub FlyDown(ByVal speed As Single)
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.Fly(0, -speed)
        End If
    End Sub
    Public Sub FlyDifLeft(ByVal speed As Single)
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.DifferentialFly(speed, 0.1)
        End If
    End Sub
    Public Sub FlyDifRight(ByVal speed As Single)
        If Not IsNothing(Me._FlyActor) Then
            Me._FlyActor.DifferentialFly(speed, -0.1)
        End If
    End Sub

    Public Sub ReleaseRFIDTagFly()
        Me._FlyActor.ReleaseRFIDTagFly()
    End Sub

#End Region


#Region "Nao"
    'Public NaoAgent As NaoMovement
    Private _NaoAgent As NaoMovement
    Protected ReadOnly Property NaoAgent() As NaoMovement
        Get
            Return Me._NaoAgent
        End Get
    End Property

    Protected Overridable Function createNao() As NaoMovement
        Return New StandardNano
    End Function


    Public Sub HeadYaw(ByVal angle As Single)
        If Not IsNothing(Me._NaoAgent) Then
            Me._NaoAgent.HeadYaw(angle)
        End If

    End Sub

    Public Sub HeadPitch(ByVal angle As Single)
        If Not IsNothing(Me._NaoAgent) Then
            Me._NaoAgent.HeadPitch(angle)
        End If
    End Sub

    Public Sub move()
        Console.WriteLine("under construction")
        'Me.NaoAgent.move()
    End Sub


#End Region



#Region " Kenaf "
    Public Sub FrontUp()
        Me._DriveActor.KenafFrontUp()
    End Sub

    Public Sub FrontDown()
        Me._DriveActor.KenafFrontDown()
    End Sub

    Public Sub RearUp()
        Me._DriveActor.KenafRearUp()
    End Sub

    Public Sub RearDown()
        Me._DriveActor.KenafRearDown()
    End Sub
#End Region


#Region " Agent Lifetime / Driver "

    Private _Driver As IDriver

    ''' <summary>
    ''' The Agent will run in the Driver's thread. Therefore the
    ''' two assume free access to each other's members. If any extnernal
    ''' thread needs access to Agent's members, thread-safety MUST
    ''' be enforced by the EXTERNAL thread.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Driver() As IDriver 'Get was Protected, Set was Friend
        Get
            Return Me._Driver
        End Get
        Set(ByVal value As IDriver)
            Me._Driver = value
        End Set
    End Property

    Public ReadOnly Property IsRunning() As Boolean
        Get
            If Not IsNothing(Me._Driver) Then
                Return Me._Driver.IsRunning
            End If
            Return False
        End Get
    End Property
    Public ReadOnly Property IsPaused() As Boolean
        Get
            If Not IsNothing(Me._Driver) Then
                Return Me._Driver.IsPaused
            End If
            Return False
        End Get
    End Property

    ''' <summary>
    ''' This is just a wrapper method. No synchronization is
    ''' enforced here, should be enforced by the Driver.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Pause()
        If Not IsNothing(Me._Driver) Then
            Me._Driver.Pause()
        End If
    End Sub

    ''' <summary>
    ''' This is just a wrapper method. No synchronization is
    ''' enforced here, should be enforced by the Driver.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub [Resume]()
        If Not IsNothing(Me._Driver) Then
            Me._Driver.Resume()
        End If
    End Sub

    Protected Friend Overridable Sub OnAgentStarted()
        SyncLock Me.observers
            For Each observer As IAgentObserver In Me.observers
                observer.NotifyAgentStarted()
            Next
        End SyncLock
    End Sub
    Protected Friend Overridable Sub OnAgentPaused()
        SyncLock Me.observers
            For Each observer As IAgentObserver In Me.observers
                observer.NotifyAgentPaused()
            Next
        End SyncLock
    End Sub
    Protected Friend Overridable Sub OnAgentResumed()
        SyncLock Me.observers
            For Each observer As IAgentObserver In Me.observers
                observer.NotifyAgentResumed()
            Next
        End SyncLock
    End Sub
    Protected Friend Overridable Sub OnAgentStopped()
        SyncLock Me.observers
            For Each observer As IAgentObserver In Me.observers
                observer.NotifyAgentStopped()
            Next
        End SyncLock
    End Sub

#End Region

#Region " Agent Sensors and Actors "

    Private sensors As New Hashtable

    Public Sub Mount(ByVal sensor As Sensor)
        Dim type As String = sensor.SensorType
        Dim table As Hashtable
        If sensors.ContainsKey(type) Then
            table = DirectCast(sensors(type), Hashtable)
        Else
            table = New Hashtable
            sensors(type) = table
        End If

        Dim name As String = sensor.SensorName
        If table.ContainsKey(name) Then
            Throw New InvalidOperationException("Sensor with this name already mounted")
        Else
            table(name) = sensor
        End If

        sensor.SetAgent(Me)
    End Sub

    Public Sub UnMount(ByVal sensor As Sensor)
        Dim type As String = sensor.SensorType
        Dim table As Hashtable
        If Not sensors.ContainsKey(type) Then
            Throw New InvalidOperationException("Sensor was not found")
        Else
            table = DirectCast(sensors(type), Hashtable)
        End If

        Dim name As String = sensor.SensorName
        If Not table.ContainsKey(name) Then
            Throw New InvalidOperationException("Sensor was not found")
        Else
            table.Remove(name)
        End If

        sensor.SetAgent(Nothing)
    End Sub






    Public Function IsMounted(ByVal type As String) As Boolean
        If sensors.ContainsKey(type) Then
            Return True
        End If

        Return False
    End Function

    Public Function IsMounted(ByVal type As String, ByVal name As String) As Boolean
        If sensors.ContainsKey(type) Then
            Dim table As Hashtable = DirectCast(sensors(type), Hashtable)
            If table.ContainsKey(name) Then
                Return True
            End If
        End If

        Return False
    End Function

    ''' <summary>
    ''' This method is invoked by sensors when new sensor data was received.
    ''' Does nothing much by default, just forwards the message to observers.
    ''' Provides a hook (template method) for 
    ''' subclasses.
    ''' </summary>
    ''' <param name="sensor"></param>
    ''' <remarks></remarks>
    ''' 

    Protected Friend Overridable Sub ProcessSensorUpdate(ByVal sensor As Sensor)
        If sensor.SensorType = laserRangeSensor.SENSORTYPE_RANGESCANNER Then
            Me.autonomousExploration(DirectCast(sensor, LaserRangeSensor).PeekData)
        End If
    End Sub



    Protected Friend Overridable Sub NotifySensorUpdate(ByVal sensor As Sensor)
        SyncLock Me.observers
            For Each observer As IAgentObserver In Me.observers
                observer.NotifySensorUpdate(sensor)
            Next
        End SyncLock
        If sensor.SensorType = StatusSensor.SENSORTYPE_STATUS Then
            Me.ProcessStatusData(DirectCast(sensor, StatusSensor).CurrentData)
        ElseIf sensor.SensorType = GroundTruthSensor.SENSORTYPE_GROUNDTRUTH Then
            Me.ProcessGroundTruthData(DirectCast(sensor, GroundTruthSensor).CurrentData)
        ElseIf sensor.SensorType = GPSSensor.SENSORTYPE_GPS Then
            If Not IsNothing(Me.GPSSensor) Then
                Me.GPSSensor.ProcessGPSData(DirectCast(sensor, GPSSensor).CurrentData)
            End If
        ElseIf sensor.SensorType = LaserRangeSensor.SENSORTYPE_RANGESCANNER Then
            Me.pathfinding(DirectCast(sensor, LaserRangeSensor).PeekData, Me.Number)
            'Me.autonomousExploration(DirectCast(sensor, LaserRangeSensor).PeekData)
        ElseIf sensor.SensorType = EncoderSensor.SENSORTYPE_ENCODER Then
            If Not IsNothing(Me.EncoderSensor) Then
                Me.EncoderSensor.ProcessEncoderData(DirectCast(sensor, EncoderSensor).CurrentData)
            End If
        End If
    End Sub

    Protected Overridable Sub ProcessStatusData(ByVal current_status As StatusData)
        If current_status.Battery < 1 AndAlso Not IsPaused Then
            Pause()
        End If
    End Sub

    Private Function NormalizeAngle(ByVal radians As Double) As Double
        While radians > PI
            radians -= 2 * PI
        End While
        While radians <= -PI
            radians += 2 * PI
        End While
        Return radians
    End Function

    Protected Overridable Sub ProcessGroundTruthData(ByVal current_data As GroundTruthData)
        Dim seedPose As New Pose2D(0, 0, 0)

        'acquire global ground-truth estimate
        'With Agent.GroundTruth
        seedPose.X = CType(current_data.X * 1000, Single)
        seedPose.Y = CType(current_data.Y * 1000, Single)
        seedPose.Rotation = CType(current_data.Yaw, Single)
        'End With

        'Console.WriteLine(String.Format("[Agent] - Ground Truth Position = ({0:f2} , {1:f2}) m", seedPose.X / 1000, seedPose.Y / 1000))
        If Me.RobotModel.ToLower = "airrobot" Then
            Return 'AirRobot needs to tilt to move
        End If
        'TBC Do something with Roll,Pitch
        If Abs(NormalizeAngle(current_data.Roll)) > 0.1 OrElse Abs(NormalizeAngle(current_data.Pitch)) > 0.1 Then
            Console.WriteLine(String.Format("[Agent] - Warning: tilting ({0:f2} , {1:f2}) rad", NormalizeAngle(current_data.Roll), NormalizeAngle(current_data.Pitch)))
            'Console.WriteLine(String.Format("[Hanne] - GroundTruth: ({0:f2} = {1:f2}, {2:f2} = {3:f2} , {4:f2})", seedPose.X, current_data.X, seedPose.Y, current_data.Y, current_data.Z))
        End If

    End Sub

#End Region

#Region " Spawning and Automatic Number in Spawn Order "

    ''' <summary>
    ''' This method gets invoked by the Driver when the agent needs
    ''' to be spawned on the usarsim server.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Overridable Sub Spawn()
        Dim tmp As String

        tmp = Me.GetUsarBotClassName
        'Console.WriteLine(tmp)

        If tmp = "USARBot.ComStation" Then
            tmp = "USARBot.P3AT"
        End If


        'construct init command
        'Console.WriteLine("this part is constructed too")
        Dim cmd As New StringBuilder()
        cmd.Append("INIT ")
        cmd.AppendFormat("{{ClassName {0}}} ", tmp)
        cmd.AppendFormat("{{Name {0}}} ", Me.Name)
        cmd.AppendFormat("{{Location {0}}} ", Me.StartLocation)
        cmd.AppendFormat("{{Rotation {0}}} ", Me.StartRotation)
        cmd.Append(Environment.NewLine) 'newline! very important!

        'simply use the standard interface for sending commands.
        Me.SendUsarSimCommand(cmd.ToString)

    End Sub

    Public Function GetUsarBotClassName() As String
        Return String.Format(My.Settings.UsarBotNameFormat, Me.RobotModel)
    End Function

    ''' <summary>
    ''' This member is invoked exactly once immediately after the 
    ''' usarsim server confirms that the agent was successfully spawned
    ''' in the simulator. 
    ''' 
    ''' See also the ReceiveMessage routine where the NFO message is identified
    ''' as the server's confirmation message.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Friend Overridable Sub OnAgentSpawned()
        SyncLock Me.observers
            For Each observer As IAgentObserver In Me.observers
                observer.NotifyAgentSpawned()
            Next
        End SyncLock
    End Sub

#End Region

#Region " Send Commands to UsarSim "

    ''' <summary>
    ''' The agent should keep a queue of all its commands here. 
    ''' The driver will Dequeue the commands when sent.
    ''' </summary>
    ''' <remarks></remarks>
    Private _UsarSimCommmands As New Queue(Of String)
    Public ReadOnly Property UsarSimCommands() As Queue(Of String)
        Get
            Return Me._UsarSimCommmands
        End Get
    End Property

    ''' <summary>
    ''' The agent simply queues the commands. The driver will 
    ''' actually send the commands to the server.
    ''' </summary>
    ''' <param name="command"></param>
    ''' <remarks></remarks>
    Public Overridable Sub SendUsarSimCommand(ByVal command As String)
        If Not command.EndsWith(Environment.NewLine) Then
            command += Environment.NewLine
        End If
        'If (_UsarSimCommmands.Count = 10000) Then
        '_UsarSimCommmands.Clear()
        'Else
        Me._UsarSimCommmands.Enqueue(command)
        'End If

    End Sub



#End Region

#Region " Receive Messages "

    ''' <summary>
    ''' Invoked by the Driver whenever a new text message was received
    ''' from the UsarSim simulater.
    ''' </summary>
    ''' <param name="msgtype"></param>
    ''' <param name="type"></param>
    ''' <param name="name"></param>
    ''' <param name="parts"></param>
    ''' <remarks>
    ''' Actually it's a helper method in the LineParsers utility that will 
    ''' invoke this method. But conceptually it's the same.
    ''' </remarks>
    Protected Friend Overridable Sub ReceiveMessage(ByVal msgtype As MessageType, ByVal type As String, ByVal name As String, ByVal parts As Specialized.StringCollection)

        'forward the message to relevant sensors
        Select Case msgtype
            Case MessageType.Info
                'this is the very first message always received from usarsim
                'basically it confirms that the agent was successfully spawned
                'in the simulator
                Me.OnAgentSpawned()

            Case MessageType.Status
                If Not IsNothing(Me._StatusSensor) Then
                    Me._StatusSensor.ReceiveMessage(msgtype, parts)
                End If

            Case MessageType.Geometry
                Me.ReceiveGeoMessage(msgtype, parts)

            Case MessageType.Configuration
                Me.ReceiveConfMessage(msgtype, parts)

            Case MessageType.Unknown
                'ignore it

            Case Else
                If Not String.IsNullOrEmpty(type) AndAlso sensors.ContainsKey(type) Then
                    Dim table As Hashtable = DirectCast(sensors(type), Hashtable)
                    For Each key As String In table.Keys
                        If key = Sensor.MATCH_ANYNAME Then
                            DirectCast(table(key), Sensor).ReceiveMessage(msgtype, parts)
                        ElseIf Not name = String.Empty AndAlso key = name Then
                            DirectCast(table(key), Sensor).ReceiveMessage(msgtype, parts)
                        End If
                    Next
                End If


        End Select

    End Sub

    Protected Overridable Sub ReceiveConfMessage(ByVal msgtype As MessageType, ByVal parts As Specialized.StringCollection)
        'Body-parts will be handled in UsarAgent

    End Sub

    Protected Overridable Sub ProcessConfMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)

        'ConfigurationRequest should be sent one at a time. When the Configuration of the robot is known, we can start asking the information from the sensors

        'This is done in a fixed order
        '1) Robot
        '2) Sonar
        '3) ... (up to you)

        If msg.ContainsKey("Type") Then
            If (msg("Type") = "GroundVehicle" OrElse msg("Type") = "LeggedRobot" OrElse msg("Type") = "NauticVehicle" OrElse msg("Type") = "AerialVehicle") Then
                If Me.RobotModel.ToLower = "p3at" OrElse Me.RobotModel.ToLower = "atrv" Then
                    Dim table As Hashtable = DirectCast(sensors("RangeScanner"), Hashtable)
                    For Each key As String In table.Keys
                        DirectCast(table(key), Sensor).SendConfigurationRequest()
                    Next
                Else
                    'In all other cases, start with Sonar
                    SonarSensor.SendConfigurationRequest()
                End If
            End If
        End If

        If msg.ContainsKey("Type") Then
            If (msg("Type") = "Sonar") Then
                If Me.RobotModel.ToLower.StartsWith("nomad") Then
                    'only robot without GPS!

                    Dim table As Hashtable = DirectCast(sensors("RangeScanner"), Hashtable)
                    For Each key As String In table.Keys
                        DirectCast(table(key), Sensor).SendConfigurationRequest()
                    Next
                Else
                    GPSSensor.SendConfigurationRequest()
                End If
            End If
        End If

        If msg.ContainsKey("Type") Then
            If (msg("Type") = "RangeScanner") Then
                Dim table As Hashtable = DirectCast(sensors("Camera"), Hashtable)
                For Each key As String In table.Keys
                    DirectCast(table(key), Sensor).SendConfigurationRequest()
                Next
            End If
        End If

        If msg.ContainsKey("Type") AndAlso sensors.ContainsKey(msg("Type")) Then
            Dim table As Hashtable = DirectCast(sensors(msg("Type")), Hashtable)
            For Each key As String In table.Keys
                DirectCast(table(key), Sensor).ProcessConfMessage(msgtype, msg)
            Next
        End If
    End Sub


    Protected Overridable Sub ReceiveGeoMessage(ByVal msgtype As MessageType, ByVal parts As Specialized.StringCollection)

        'Body-parts will be handled in UsarAgent

        'Me.ProcessGeoMessage(msgtype, Me.ToDictionary(parts))
    End Sub

    ''' <summary>
    ''' Assumes the default format for geo messages. The message parts
    ''' will be broken down further into key-value pairs using the ToKeyValuePair
    ''' member and subsequently be put in a stringdictionary.
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overridable Function ToDictionary(ByVal msg As Specialized.StringCollection) As Specialized.StringDictionary
        Dim dict As New Specialized.StringDictionary

        Dim pair As KeyValuePair(Of String, String)
        For Each part As String In msg
            pair = Me.ToKeyValuePair(part)
            If Not IsNothing(pair) AndAlso Not String.IsNullOrEmpty(pair.Key) AndAlso Not dict.ContainsKey(pair.Key) Then
                dict.Add(pair.Key, pair.Value)
            End If
        Next

        Return dict
    End Function
    ''' <summary>
    ''' Breaks down a single message part into a key and its corresponding value.
    ''' </summary>
    ''' <param name="part"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overridable Function ToKeyValuePair(ByVal part As String) As KeyValuePair(Of String, String)
        'by default, every part will be processed as follows:
        '- tokenize each part by splitting on spaces
        '- key: the first token 
        '- value: concatenation of all remaining tokens

        Dim parts() As String = Strings.Split(part, " ")
        If parts.Length = 2 Then
            Dim key As String = parts(0)
            Dim value As String = ""
            value &= parts(1) & " "

            Return New KeyValuePair(Of String, String)(key, value.Trim)
        ElseIf parts.Length > 2 Then
            'Use Name Sensor as Key
            Dim key As String = parts(0)
            key &= " " & parts(1)
            Dim value As String = ""
            For i As Integer = 2 To parts.Length - 1
                value &= parts(i) & " "
            Next

            Return New KeyValuePair(Of String, String)(key, value.Trim)



        End If

        Return Nothing

    End Function

    Protected Overridable Sub ProcessGeoMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)

        'GeometryRequest should be sent one at a time. When the Geometry of the body is known, we can start asking the other information

        'This is done in a fixed order
        '1) Robot
        '2) Sonar
        '3) GPS  
        '4) RangeScanner
        '5) ... (up to you)

        If msg.ContainsKey("Type") Then
            If (msg("Type") = "GroundVehicle" OrElse msg("Type") = "LeggedRobot" OrElse msg("Type") = "NauticVehicle" OrElse msg("Type") = "AerialVehicle") Then
                If Me.RobotModel.ToLower = "p3at" OrElse Me.RobotModel.ToLower = "atrv" Then
                    Dim table As Hashtable = DirectCast(sensors("RangeScanner"), Hashtable)
                    For Each key As String In table.Keys
                        DirectCast(table(key), Sensor).SendGeometryRequest()
                    Next
                Else
                    'In all other cases, start with Sonar
                    SonarSensor.SendGeometryRequest()
                End If

            End If
        End If

        If msg.ContainsKey("Type") Then
            If (msg("Type") = "Sonar") Then
                If Me.RobotModel.ToLower.StartsWith("nomad") Then
                    'only robot without GPS!

                    Dim table As Hashtable = DirectCast(sensors("RangeScanner"), Hashtable)
                    For Each key As String In table.Keys
                        DirectCast(table(key), Sensor).SendGeometryRequest()
                    Next
                Else
                    GPSSensor.SendGeometryRequest()
                End If
            End If
        End If

        'Carefull, from now one this sensors are not the default sensors mounted in Agent, but the robotType depended sensors mounted in UsarAgent.
        'Cannot be handled in UsarAgent.ProcessGeoMessage, because no access to Agent.sensors

        If msg.ContainsKey("Type") Then
            If (msg("Type") = "GPS") Then
                Dim table As Hashtable = DirectCast(sensors("RangeScanner"), Hashtable)
                If Not IsNothing(table) Then
                    For Each key As String In table.Keys
                        DirectCast(table(key), Sensor).SendGeometryRequest()
                    Next
                End If
            End If
        End If

        If msg.ContainsKey("Type") Then
            If (msg("Type") = "RangeScanner") Then
                Dim table As Hashtable = DirectCast(sensors("Camera"), Hashtable)
                For Each key As String In table.Keys
                    DirectCast(table(key), Sensor).SendGeometryRequest()
                Next
            End If
        End If

        'Add your own SensorType

        'This doesn't work, because GEO Request have to come one at a time
        'For Each typekey As String In Me.sensors.Keys
        '    Dim table As Hashtable = DirectCast(sensors(typekey), Hashtable)
        '    For Each namekey As String In table.Keys
        '        DirectCast(table(namekey), Sensor).SendGeometryRequest()
        '    Next
        'Next

        'For sensors with multiple instances ANYNAME is used. The names and GEOs of the all instances are returned

        'GEO {Type string} {Name string Location x,y,z Orientation x,y,z Mount string} {Name string Location x,y,z Orientation x,y,z Mount string} 
        If msg.ContainsKey("Type") AndAlso sensors.ContainsKey(msg("Type")) Then
            Dim table As Hashtable = DirectCast(sensors(msg("Type")), Hashtable)
            For Each key As String In table.Keys
                DirectCast(table(key), Sensor).ProcessGeoMessage(msgtype, msg)
            Next
        End If


    End Sub

    ''' <summary>
    ''' Invoked by the Driver whenever a new byte data was received
    ''' from the ImageServer.
    ''' </summary>
    ''' <param name="type"></param>
    ''' <param name="name"></param>
    ''' <param name="bytes"></param>
    ''' <remarks></remarks>
    Protected Friend Sub ReceiveBytes(ByVal type As String, ByVal name As String, ByVal bytes() As Byte)

        'forward the message to relevant sensors
        If Not String.IsNullOrEmpty(type) AndAlso sensors.ContainsKey(type) Then
            Dim table As Hashtable = DirectCast(sensors(type), Hashtable)
            For Each key As String In table.Keys
                If key = Sensor.MATCH_ANYNAME OrElse name = Sensor.MATCH_ANYNAME Then
                    DirectCast(table(key), Sensor).ReceiveBytes(bytes)
                ElseIf Not name = String.Empty AndAlso key = name Then
                    DirectCast(table(key), Sensor).ReceiveBytes(bytes)
                End If
            Next
        End If

    End Sub

#End Region

#Region " Manifold Sync "

    Public Sub Sync()
        Me.OnSync()
    End Sub

    Protected Overridable Sub OnSync()
    End Sub

#End Region

#Region " Observers "

    Private observers As New List(Of IAgentObserver)

    Public Sub AddObserver(ByVal observer As IAgentObserver)
        SyncLock Me.observers
            Me.observers.Add(observer)
        End SyncLock
    End Sub
    Public Sub RemoveObserver(ByVal observer As IAgentObserver)
        SyncLock Me.observers
            Me.observers.Remove(observer)
        End SyncLock
    End Sub

#End Region

#Region " Pose Estimate "




    Dim poseMutex As New Object

    Public _CurrentPoseEstimate As Pose2D
    Public ReadOnly Property CurrentPoseEstimate() As Pose2D
        Get
            SyncLock poseMutex
                Return Me._CurrentPoseEstimate
            End SyncLock
        End Get
    End Property


    Public _rotation As Double
    Public ReadOnly Property CurrentRotationEstimate() As Double
        Get
            SyncLock poseMutex
                Return Me._rotation
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Hook used by SLAM methods to notify the agent of updates pose estimates.
    ''' The agent will notify all attached observers.
    ''' </summary>
    ''' <param name="pose"></param>
    ''' <remarks></remarks>


    Public Overridable Sub NotifyPoseEstimateUpdated(ByVal pose As Pose2D)

        'Console.WriteLine(String.Format("[Agent.NotifyPoseEstimateUpdated] Resetting pose from ({0},{1})m to ({2},{3})m", _CurrentPoseEstimate.X / 1000, _CurrentPoseEstimate.Y / 1000, pose.X / 1000, pose.Y / 1000))


        SyncLock poseMutex
            Me._CurrentPoseEstimate = pose
            Me._rotation = (Me.GroundTruthPose.Yaw * 180) / PI
            
            Me._manifold._robotsAngle = (Me.GroundTruthPose.Yaw * 180) / PI

        End SyncLock

        'pose is in global coords, convert to startpose-relative coords for odometry
        Dim relative As Pose2D = pose.ToLocal(Me.StartPose)

        ' If Not (Me.AgentConfig.MappingMode = "DeadReckoning" AndAlso Me.AgentConfig.SeedMode = "Odometry") Then
        'Console.WriteLine(String.Format("[Agent] Resetting Odometry to ({0},{1})m", relative.X / 1000, relative.Y / 1000))
        'Me.OdometrySensor.Reset(relative.X / 1000, relative.Y / 1000, relative.Rotation)
        'End If
        'If Not (Me.AgentConfig.MappingMode = "DeadReckoning" AndAlso Me.AgentConfig.SeedMode = "INS") Then
        'Console.WriteLine(String.Format("[Agent] Resetting INS to ({0},{1})m", pose.X / 1000, pose.Y / 1000))
        'Me.InsSensor.Reset(pose.X / 1000, pose.Y / 1000, pose.Rotation)
        'End If
        'Me.EncoderSensor.ResetOffsets(relative.X / 1000, relative.Y / 1000, relative.Rotation)



        SyncLock Me.observers
            For Each observer As IAgentObserver In Me.observers
                observer.NotifyPoseEstimateUpdated(pose)
            Next
        End SyncLock
        Return

    End Sub

#End Region

#Region " Alerts "

    Private _MotionTimeStamp As Date = DateTime.Now
    Public Property MotionTimeStamp() As Date
        Get
            Return Me._MotionTimeStamp
        End Get
        Set(ByVal value As Date)
            _MotionTimeStamp = value
        End Set
    End Property

    ''' <summary>
    ''' Hook used by conservativeTeleOpMotion to notify the agent of alerts.
    ''' The agent will notify all attached observers.
    ''' </summary>
    ''' <param name="alert"></param>
    ''' <remarks></remarks>
    Public Overridable Sub NotifyAlertReceived(ByVal alert As String)
        SyncLock Me.observers
            For Each observer As IAgentObserver In Me.observers
                observer.NotifyAlertReceived(alert)
            Next
        End SyncLock
    End Sub

#End Region

#Region " Signal Strength to Operator "

    Public Overridable Sub NotifySignalStrengthUpdated(ByVal pathloss As Double)
        SyncLock Me.observers
            For Each observer As IAgentObserver In Me.observers
                observer.NotifySignalStrengthReceived(pathloss)
            Next
        End SyncLock
    End Sub

#End Region

#Region " Cam Image "
    Private _ShowImages As Boolean = False
    Public Property ShowImages() As Boolean
        Get
            Return _ShowImages
        End Get
        Set(ByVal value As Boolean)
            _ShowImages = value
        End Set
    End Property

    Public Sub ToggleShowImages(ByVal ShowImages As Boolean)
        'Console.WriteLine("[Agent] Stops CamReq in LiveProxyDriver")
        _ShowImages = ShowImages
    End Sub
    Private _CurrentCamera As Integer
    Public ReadOnly Property CurrentCamera() As Integer
        Get
            Return _CurrentCamera
        End Get
    End Property




    Public ReadOnly Property NumberOfCameras() As Integer
        Get
            Dim table As Hashtable = DirectCast(sensors(CameraSensor.SENSORTYPE_CAMERA), Hashtable)
            Return table.Count
        End Get
    End Property
    Public Sub ToggleMultiView()
        Console.WriteLine("[Agent] ToggleMultiView")
        Me._CurrentCamera += 1
        If Me._CurrentCamera >= NumberOfCameras Then
            Me._CurrentCamera = 0
        End If

    End Sub

    Public Overridable Sub NotifyTrackerImage(ByVal displayImage As Image)
        SyncLock Me.observers
            For Each observer As IAgentObserver In Me.observers
                observer.NotifyTrackerImage(displayImage)
            Next
        End SyncLock
    End Sub

    Public Sub CamReq()
        'Reachable for other objects
        Me.OnCamReq()
    End Sub

    Protected Friend Overridable Sub OnCamReq()
        'Console.WriteLine(String.Format("[{0}] - Handling camera request", Me.Name))

        'This one can be overloaded by Specific Agents
        If IsMounted(CameraSensor.SENSORTYPE_CAMERA) Then

            'Console.WriteLine(String.Format("[{0}]:OnCamReq - camera is mounted", Me.Name))

            Dim table As Hashtable = DirectCast(sensors(CameraSensor.SENSORTYPE_CAMERA), Hashtable)
            For Each key As String In table.Keys
                '                If key = Sensor.MATCH_ANYNAME Then (in 2010-code named camera's)

                'Console.WriteLine(String.Format("[{0}]:OnCamReq - found camera with name '{1}'", Me.Name, key))

                'DirectCast(table(key), CameraSensor).LatestCamIm.Save(String.Concat("E:\test_", otherName, ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg)
                Me.CamRepl(Me.Name, DirectCast(table(key), CameraSensor).LatestCamIm)
                '               End If
            Next
        End If
    End Sub

    Public Sub CamRepl(ByVal fromRobot As String, ByVal camIm As Bitmap)
        'Console.WriteLine(String.Format("[{0}] - Replying on camera request", Me.Name))

        Me.OnCamRepl(fromRobot, camIm)
    End Sub

    Protected Friend Overridable Sub OnCamRepl(ByVal fromRobot As String, ByVal camIm As Bitmap)

        If IsMounted(CameraSensor.SENSORTYPE_CAMERA) Then
            'Console.WriteLine("Should be first Handled by CommAgent.OnCamRepl, OperatorAgent.OnCamRepl or ProxyAgent.OnCamRepl")
            Dim table As Hashtable = DirectCast(sensors(CameraSensor.SENSORTYPE_CAMERA), Hashtable)
            For Each key As String In table.Keys
                'If key = Sensor.MATCH_ANYNAME Then (in 2010-code named camera's)

                'DirectCast(table(key), CameraSensor).LatestCamIm.Save(String.Concat("E:\test_", otherName, ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg)
                DirectCast(table(key), CameraSensor).LatestCamIm = camIm
                Me.NotifySensorUpdate(DirectCast(table(key), CameraSensor))
                'End If
            Next
        End If
    End Sub

    Public Overridable Sub OnCamReplReceived(ByVal otherName As String)
        If IsMounted(CameraSensor.SENSORTYPE_CAMERA) Then

            Dim table As Hashtable = DirectCast(sensors(CameraSensor.SENSORTYPE_CAMERA), Hashtable)
            For Each key As String In table.Keys
                If key = Sensor.MATCH_ANYNAME Then

                    'DirectCast(table(key), CameraSensor).LatestCamIm.Save(String.Concat("E:\test_", otherName, ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg)
                    Me.CamRepl(otherName, DirectCast(table(key), CameraSensor).LatestCamIm)
                End If
            Next
        End If
    End Sub

    Public Overridable Sub toggleTracker()
        'overriden in UsarTracker
    End Sub

    'CamBrightness set by slider bar next to cam image in UsarCommander
    Private _CamBrightness As Integer = 10
    Public Property CamBrightness() As Integer
        Get
            Return _CamBrightness
        End Get
        Set(ByVal value As Integer)
            _CamBrightness = value
        End Set
    End Property

    'CamContrast set by slider bar next to cam image in UsarCommander
    Private _CamContrast As Integer = 5
    Public Property CamContrast() As Integer
        Get
            Return _CamContrast
        End Get
        Set(ByVal value As Integer)
            _CamContrast = value
        End Set
    End Property

    Public Sub AdjustCamImage(ByRef image As Image)
        AdjustBrightness(image)
        AdjustContrast(image)
    End Sub

    Private Sub AdjustBrightness(ByRef image As Image)
        ' As explained on http://www.bobpowell.net/image_contrast.htm

        Dim brt As Single = CSng((10 - CamBrightness) / 10)
        Dim image_attr As New Imaging.ImageAttributes
        Dim cm As Imaging.ColorMatrix = New Imaging.ColorMatrix(New Single()() _
            { _
            New Single() {1.0, 0.0, 0.0, 0.0, 0.0}, _
            New Single() {0.0, 1.0, 0.0, 0.0, 0.0}, _
            New Single() {0.0, 0.0, 1.0, 0.0, 0.0}, _
            New Single() {0.0, 0.0, 0.0, 1.0, 0.0}, _
            New Single() {brt, brt, brt, 0.0, 1.0}})
        Dim rect As Rectangle = _
            Rectangle.Round(image.GetBounds(GraphicsUnit.Pixel))
        Dim wid As Integer = image.Width
        Dim hgt As Integer = image.Height

        image_attr.SetColorMatrix(cm)
        Dim g As Graphics = Graphics.FromImage(image)
        g.DrawImage(image, rect, 0, 0, wid, _
            hgt, GraphicsUnit.Pixel, image_attr)
        g.Dispose()
    End Sub

    Private Sub AdjustContrast(ByRef image As Image)
        Dim ctr As Single = CSng((10 - CamContrast) / 5)
        Dim image_attr As New Imaging.ImageAttributes
        Dim cm As Imaging.ColorMatrix = New Imaging.ColorMatrix(New Single()() _
            { _
            New Single() {ctr, 0.0, 0.0, 0.0, 0.0}, _
            New Single() {0.0, ctr, 0.0, 0.0, 0.0}, _
            New Single() {0.0, 0.0, ctr, 0.0, 0.0}, _
            New Single() {0.0, 0.0, 0.0, 1.0, 0.0}, _
            New Single() {0.0, 0.0, 0.0, 0.0, 1.0}})
        Dim rect As Rectangle = _
            Rectangle.Round(image.GetBounds(GraphicsUnit.Pixel))
        Dim wid As Integer = image.Width
        Dim hgt As Integer = image.Height

        image_attr.SetColorMatrix(cm)
        Dim g As Graphics = Graphics.FromImage(image)
        g.DrawImage(image, rect, 0, 0, wid, _
            hgt, GraphicsUnit.Pixel, image_attr)
        g.Dispose()
    End Sub
#End Region

#Region " Target Locations "
    Public Overridable Sub AddNewTarget(ByVal targetLocation As Pose2D, Optional ByVal clearAgentsCurrentTargets As Boolean = False)
        If clearAgentsCurrentTargets Then
            ClearAllTargets()
        End If

        'This function is called when a new message is received.
        Me._TargetLocations.Enqueue(targetLocation)
    End Sub

    Public _attraction As Double = 0

    Public _areaAttraction As Double = 0

    Public _finished As Boolean = False

    Public Sub setNewTarget(ByVal agentNumber As Integer, ByVal goalpose As Point)

        If (Me.Manifold._finished = False And Me.Manifold._moving = False) Then
            Console.WriteLine("set new target is in process.")
            If (Me.Number = agentNumber) Then

                If ((((Me._CurrentPoseEstimate.X - goalpose.X) ^ 2 + (Me._CurrentPoseEstimate.Y - goalpose.Y) ^ 2) ^ 0.5) / 1000 < 2) Then
                    Me.Halt()
                    Me._attraction = 0
                Else
                    Console.WriteLine("distance : {0}", ((Me._CurrentPoseEstimate.X - goalpose.X) ^ 2 + (Me._CurrentPoseEstimate.Y - goalpose.Y) ^ 2) ^ 0.5 / 1000)
                    Console.WriteLine("angle : {0} {1}", Atan((-goalpose.Y + Me._CurrentPoseEstimate.Y) / (-goalpose.X + Me._CurrentPoseEstimate.X)) * 180 / PI, Me._rotation)
                    If (goalpose.Y - Me._CurrentPoseEstimate.Y < 0) Then
                        If (Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI <= 0) Then
                            If (Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI < Me._rotation) Then


                                Me.TurnLeft(0.2)
                                Me._attraction = 1
                            Else
                                Me.Halt()
                                'Me.Drive(0.7)
                                Me.Manifold._finished = True
                                Me._attraction = 0.25
                            End If

                        Else
                            If ((Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI - 180) < Me._rotation) Then

                                Me.TurnLeft(0.2)
                                Me._attraction = 1
                            Else
                                Me.Halt()
                                Me.Manifold._finished = True
                                'Me.Drive(0.7)
                                Me._attraction = 0.25
                            End If
                        End If

                        Me.position = "left"


                    ElseIf (goalpose.Y - Me._CurrentPoseEstimate.Y > 0) Then
                        If (Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI > 0) Then
                            If (Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI > Me._rotation) Then

                                Me._attraction = 1
                                Me.TurnRight(0.2)
                            Else
                                Me.Halt()
                                Me.Manifold._finished = True
                                'Me.Drive(0.7)
                                Me._attraction = 0.25
                            End If
                        Else
                            If ((180 + Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI) > Me._rotation) Then


                                Me.TurnRight(0.2)
                                Me._attraction = 1


                            Else
                                Me.Halt()
                                Me.Manifold._finished = True
                                'Me.Drive(0.7)
                                Me._attraction = 0.25
                            End If
                        End If
                        Me.position = "right"




                    End If

                End If

            End If

        End If

    End Sub



    Public Sub autonomousExploration(ByVal laser As LaserRangeData)

        For i As Integer = 0 To 60
            Console.WriteLine("left : {0} {1}", i, laser.Range(i))
        Next

        For i As Integer = 60 To 120
            Console.WriteLine("front : {0} {1}", i, laser.Range(i))
        Next

        For i As Integer = 120 To 180
            Console.WriteLine("right : {0} {1}", i, laser.Range(i))
        Next

    End Sub


    Public Overridable Sub ClearAllTargets()

    End Sub


    Public Sub setHeading(ByVal angle As Double)


        Console.WriteLine("[agent] set heading fafa fafa fafa!!!!!!")

        If (Me.Manifold._Astar.endArea.Y - Me._CurrentPoseEstimate.Y < 0) Then
            Me.Manifold.accurateAngle = Me._rotation + angle
            If (Me.Manifold.accurateAngle > Me._rotation) Then
                'Console.WriteLine("difference : {0}", first - last)
                Me.TurnRight(0.3)

            End If

        ElseIf (Me.Manifold._Astar.endArea.Y - Me._CurrentPoseEstimate.Y > 0) Then
            'ElseIf (Me.position = "left") Then
            Me.Manifold.accurateAngle = Me._rotation - angle
            If (Me.Manifold.accurateAngle < Me._rotation) Then
                Me.TurnLeft(0.3)

            End If

        End If


        'Me._lastAngle = Me._rotation
        Thread.Sleep(100)
    End Sub

    'here i am trying to use A* path planning algorithm but with some changes.
    'when i checked this algorithm in 2d ,it seemes that it is more likely best first search than A*.

    'Public _Astar As Astar

    'Public _moving As Boolean = False
    Public _gridCalculation As Boolean = False

    Public LeftObstacle_isNear As Integer = 0
    Public LeftObstacle_isFar As Integer = 0
    Public FrontObstacle_isNear As Integer = 0
    Public FrontObstacle_isFar As Integer = 0
    Public RightObstacle_isNear As Integer = 0
    Public RightObstacle_isFar As Integer = 0
    Public left_isnear As Boolean
    Public left_isfar As Boolean
    Public front_isnear As Boolean
    Public front_isfar As Boolean
    Public right_isnear As Boolean
    Public right_isfar As Boolean
    Public DistanceToGoal As Single
    Public ObstacleLength_Left As Integer
    Public ObstacleLength_Front As Integer
    Public ObstacleLength_Right As Integer

    Public Sub pathfinding(ByVal laser As LaserRangeData, ByVal agentNumber As Integer)
        Static Dim left_isClosed As Integer
        Static Dim front_isClosed As Integer
        Static Dim right_isClosed As Integer
        If Not (Me.Manifold.currNumber = 0) Then
            If (agentNumber = Me.Manifold.currNumber) Then
                If (Me.Manifold._finished = True And Me._gridCalculation = False) Then
                    Console.WriteLine("fafa fafa fafa!!!! grid calculation")

                    Me.Manifold._Astar.evidence.Add("init state : " + CStr(Me._CurrentPoseEstimate.X) + " " + CStr(Me._CurrentPoseEstimate.Y))
                    Me.Manifold._Astar.evidence.Add("end position : " + CStr(Me.Manifold._Astar.endArea.X) + " " + CStr(Me.Manifold._Astar.endArea.Y))
                    'this part is constructing the local grid for D* algorithm
                    Me.Manifold._Astar.addRealDistance(0, (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar.endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 7000) - Me.Manifold._Astar.endArea.Y) ^ 2) ^ 0.5)
                    Me.Manifold._Astar.addRealDistance(1, (((Me._CurrentPoseEstimate.X + 7000) - Me.Manifold._Astar.endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar.endArea.Y) ^ 2) ^ 0.5)
                    Me.Manifold._Astar.addRealDistance(2, (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar.endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y + 7000) - Me.Manifold._Astar.endArea.Y) ^ 2) ^ 0.5)
                    Me.Manifold._Astar.addRealDistance(3, (((Me._CurrentPoseEstimate.X - 7000) - Me.Manifold._Astar.endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar.endArea.Y) ^ 2) ^ 0.5)


                    Dim wayOfMoving As Integer = Me.Manifold._Astar.findMin(Me.Manifold._Astar.realDistance)
                    Me.Manifold._Astar.heading = wayOfMoving
                    'decition part of my code
                    Me.Manifold._Astar.evidence.Add(CStr(wayOfMoving))
                    Me.Manifold._Astar.evidence.Add(CStr(Me._rotation))
                    Me.Manifold._finished = True
                    Me._gridCalculation = True

                    '---------------------------------------------------------------------------------------------------------------------------------
                ElseIf (Me.Manifold._finished = True And Me._gridCalculation = True) Then
                    Console.WriteLine("fafa fafa fafa!!! moving decision")
                    If (Me.Manifold._Astar.heading = 0) Then
                        Dim left_open As Integer = 0
                        Dim left_close As Integer = 0

                        For i As Integer = 0 To 60
                            If laser.Range(i) > 3 Then
                                left_open += 1
                                If laser.Range(i) > 5 Then
                                    LeftObstacle_isFar += 1

                                End If

                                'Console.WriteLine("left is open")
                            ElseIf (laser.Range(i) < 3) Then
                                left_close += 1

                                LeftObstacle_isNear += 1
                                'Console.WriteLine("left is closed")

                            End If

                        Next

                        Dim Front_open As Integer = 0
                        Dim Front_close As Integer = 0

                        For i As Integer = 60 To 120
                            If laser.Range(i) > 3 Then
                                Front_open += 1
                                If laser.Range(i) > 5 Then
                                    FrontObstacle_isFar += 1
                                End If
                                'Console.WriteLine("left is open")
                            ElseIf (laser.Range(i) < 3) Then
                                Front_close += 1
                                FrontObstacle_isNear += 1

                                'Console.WriteLine("left is closed")

                            End If

                        Next

                        Dim right_open As Integer = 0
                        Dim right_close As Integer = 0


                        For i As Integer = 120 To 179
                            If laser.Range(i) > 3 Then
                                right_open += 1
                                If laser.Range(i) > 5 Then
                                    RightObstacle_isFar += 1

                                End If
                                'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), True)
                                'Me.Manifold._Astar.addHeuristicValue(1, (((Me._CurrentPoseEstimate.X + 5300) - Me.Manifold._Astar.endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar.endArea.Y) ^ 2) ^ 0.5)
                                'Console.WriteLine("front is open")

                            ElseIf (laser.Range(i) < 3) Then
                                right_close += 1
                                RightObstacle_isNear += 1
                            End If
                        Next


                        If LeftObstacle_isNear > 30 Then
                            left_isnear = True

                        End If

                        If LeftObstacle_isFar > 30 Then
                            left_isfar = True

                        End If

                        ObstacleLength_Left = left_close
                        If (left_close > left_open) Then
                            'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X - 3800), CInt(Me._CurrentPoseEstimate.Y - 3800)), False)
                            Me.Manifold._Astar.addHeuristicValue(0, 1000000000)
                            left_isClosed = 0
                        Else
                            'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X - 3800), CInt(Me._CurrentPoseEstimate.Y - 3800)), True)
                            'If ((((Me._CurrentPoseEstimate.X - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y + 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5 < (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5) Then
                            'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(0, (((Me._CurrentPoseEstimate.X - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y + 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5)
                            'Else
                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X - 7000)
                            Dim temp_y As Double = (Me._CurrentPoseEstimate.Y)
                            Me.Manifold._Astar.addHeuristicValue(0, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            left_isClosed = 1
                            'End If
                        End If

                        If FrontObstacle_isFar > 30 Then
                            front_isfar = True

                        End If

                        If FrontObstacle_isNear > 30 Then
                            front_isnear = True
                        End If

                        ObstacleLength_Front = Front_close
                        If (Front_close > Front_open) Then
                            'Me.Manifold._Astar.addNode(0, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 5300)), False)
                            Me.Manifold._Astar.addHeuristicValue(1, 1000000000)
                            front_isClosed = 0
                        Else
                            'Me.Manifold._Astar.addNode(0, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 5300)), True)
                            'If ((((Me._CurrentPoseEstimate.X - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y + 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5 < (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5) Then
                            'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(2, (((Me._CurrentPoseEstimate.X - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y + 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5)
                            'Else
                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X)
                            Dim temp_y As Double = (Me._CurrentPoseEstimate.Y - 7000)
                            Me.Manifold._Astar.addHeuristicValue(1, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            front_isClosed = 1
                            'End If
                        End If

                        If RightObstacle_isFar > 30 Then
                            right_isfar = True

                        End If

                        If RightObstacle_isNear > 30 Then
                            right_isnear = True
                        End If

                        ObstacleLength_Right = right_close

                        If (right_close > right_open) Then
                            'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 5300)), False)
                            Me.Manifold._Astar.addHeuristicValue(2, 1000000000)
                            right_isClosed = 0
                        Else
                            'Me.Manifold._Astar.addNode(0, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 5300)), True)
                            'If ((((Me._CurrentPoseEstimate.X - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5 < (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5) Then
                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X + 7000)
                            Dim temp_y As Double = (Me._CurrentPoseEstimate.Y)
                            Me.Manifold._Astar.addHeuristicValue(2, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            right_isClosed = 1

                        End If




                        Dim choice As Integer = Me.Manifold._Astar.findMin(Me.Manifold._Astar.HeuristicDistance)
                        Me.Manifold._Astar.evidence.Add(CStr(choice))
                        If choice = 0 Then
                            If (left_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)

                                Me.Manifold._Astar.heading = 3
                            ElseIf (left_isfar And DistanceToGoal < 5 And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.4)

                                Me.Manifold._Astar.heading = 3

                            ElseIf (left_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.2)

                                Me.Manifold._Astar.heading = 3
                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)

                                    Me.Manifold._Astar.heading = 3
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.5)

                                    Me.Manifold._Astar.heading = 3
                                End If

                            End If

                            'Console.WriteLine("front is choosen.")
                        ElseIf (choice = 1) Then
                            If (front_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.6)
                                Me.Manifold._Astar.heading = 0
                            ElseIf (front_isfar And DistanceToGoal < 5 And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.4)
                                Me.Manifold._Astar.heading = 0
                            ElseIf (front_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.2)
                                Me.Manifold._Astar.heading = 0
                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.6)
                                    Me.Manifold._Astar.heading = 0
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.5)
                                    Me.Manifold._Astar.heading = 0
                                End If

                            End If
                            'Console.WriteLine("left is choosen")

                        ElseIf (choice = 2) Then
                            If (right_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)
                                Me.Manifold._Astar.heading = 1
                            ElseIf (right_isfar And DistanceToGoal < 5 And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.4)
                                Me.Manifold._Astar.heading = 1
                            ElseIf (right_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.2)
                                Me.Manifold._Astar.heading = 1
                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)
                                    Me.Manifold._Astar.heading = 1
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.5)
                                    Me.Manifold._Astar.heading = 1
                                End If

                            End If
                            'Console.WriteLine("right is choosen")
                        Else
                            ' Me.setHeading(20)

                        End If
                        '----------------------------------------------------------------------------------------------------------------------------------------

                    ElseIf (Me.Manifold._Astar.heading = 1) Then



                        Dim left_open As Integer = 0
                        Dim left_close As Integer = 0

                        For i As Integer = 0 To 60
                            If laser.Range(i) > 3 Then
                                left_open += 1
                                If laser.Range(i) > 5 Then
                                    LeftObstacle_isFar += 1
                                End If


                                'Console.WriteLine("left is open")
                            ElseIf (laser.Range(i) < 3) Then
                                left_close += 1
                                LeftObstacle_isNear += 1
                                'Console.WriteLine("left is closed")

                            End If

                        Next

                        Dim Front_open As Integer = 0
                        Dim Front_close As Integer = 0

                        For i As Integer = 60 To 120
                            If laser.Range(i) > 3 Then
                                Front_open += 1
                                If laser.Range(i) > 5 Then
                                    FrontObstacle_isFar += 1
                                End If

                                'Console.WriteLine("left is open")
                            ElseIf (laser.Range(i) < 3) Then
                                Front_close += 1
                                FrontObstacle_isNear += 1
                                'Console.WriteLine("left is closed")

                            End If

                        Next

                        Dim right_open As Integer = 0
                        Dim right_close As Integer = 0



                        For i As Integer = 120 To 179
                            If laser.Range(i) > 3 Then
                                right_open += 1
                                If laser.Range(i) > 5 Then
                                    RightObstacle_isFar += 1
                                End If
                                'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), True)
                                'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(1, (((Me._CurrentPoseEstimate.X + 5300) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5)
                                'Console.WriteLine("front is open")

                            ElseIf (laser.Range(i) < 3) Then
                                right_close += 1
                                RightObstacle_isNear += 1
                                'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), False)
                                'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(1, 1000000000)
                                'Console.WriteLine("front is close")
                            End If
                        Next

                        If LeftObstacle_isFar > 30 Then
                            left_isfar = True

                        End If

                        If LeftObstacle_isNear > 30 Then
                            left_isnear = True
                        End If

                        ObstacleLength_Left = left_close
                        If (left_close > left_open) Then
                            'Me.Manifold._Astar.addNode(0, New Point(CInt(Me._CurrentPoseEstimate.X - 2000), CInt(Me._CurrentPoseEstimate.Y + 3800)), False)
                            Me.Manifold._Astar.addHeuristicValue(0, 1000000000)
                            left_isClosed = 0

                        Else

                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X)
                            Dim temp_y As Double = (Me._CurrentPoseEstimate.Y) - 7000
                            Me.Manifold._Astar.addHeuristicValue(0, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            left_isClosed = 1
                            'Console.WriteLine("left : {0}", Abs((((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y)) / 1000))
                        End If



                        If FrontObstacle_isFar > 30 Then
                            front_isfar = True

                        End If

                        If FrontObstacle_isNear > 30 Then
                            front_isnear = True
                        End If

                        ObstacleLength_Front = Front_close
                        If (Front_close > Front_open) Then
                            'Me.Manifold._Astar.addNode(2, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), False)
                            Me.Manifold._Astar.addHeuristicValue(1, 1000000000)
                            front_isClosed = 0
                        Else
                            'Me.Manifold._Astar.addNode(2, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), True)
                            'If ((((Me._CurrentPoseEstimate.X - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y + 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5 < (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5) Then
                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X) + 7000
                            Dim temp_y As Double = (Me._CurrentPoseEstimate.Y)
                            Me.Manifold._Astar.addHeuristicValue(1, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            front_isClosed = 1
                            'Console.WriteLine("front : {0}", Abs((((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y)) / 1000))
                        End If

                        If RightObstacle_isFar > 30 Then
                            right_isfar = True

                        End If

                        If RightObstacle_isNear > 30 Then
                            right_isnear = True
                        End If

                        ObstacleLength_Right = right_close
                        If (right_close > right_open) Then
                            'Me.Manifold._Astar.addNode(2, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), False)
                            Me.Manifold._Astar.addHeuristicValue(2, 1000000000)
                            right_isClosed = 0
                        Else
                            'Me.Manifold._Astar.addNode(2, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), True)

                            'If ((((Me._CurrentPoseEstimate.X - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5 < (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5) Then
                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X)
                            Dim temp_y As Double = (Me._CurrentPoseEstimate.Y) + 7000
                            Me.Manifold._Astar.addHeuristicValue(2, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            right_isClosed = 1
                            'Console.WriteLine("right : {0}", Abs((((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y)) / 1000))

                        End If


                        Dim choice As Integer = Me.Manifold._Astar.findMin(Me.Manifold._Astar.HeuristicDistance)
                        Me.Manifold._Astar.evidence.Add(CStr(choice))
                        If choice = 0 Then
                            If (left_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.6)
                                Me.Manifold._Astar.heading = 0

                            ElseIf (left_isfar And DistanceToGoal < 5 And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.4)
                                Me.Manifold._Astar.heading = 0

                            ElseIf (left_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.2)
                                Me.Manifold._Astar.heading = 0
                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.6)
                                    Me.Manifold._Astar.heading = 0
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.5)
                                    Me.Manifold._Astar.heading = 0
                                End If

                            End If

                        ElseIf (choice = 1) Then
                            If (front_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)
                                Me.Manifold._Astar.heading = 1

                            ElseIf (front_isfar And DistanceToGoal < 5 And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.4)
                                Me.Manifold._Astar.heading = 1

                            ElseIf (front_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.2)
                                Me.Manifold._Astar.heading = 1
                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)
                                    Me.Manifold._Astar.heading = 1
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.5)
                                    Me.Manifold._Astar.heading = 1
                                End If

                            End If

                        ElseIf (choice = 2) Then
                            If (right_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.6)
                                Me.Manifold._Astar.heading = 2

                            ElseIf (right_isfar And DistanceToGoal < 5 And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.4)
                                Me.Manifold._Astar.heading = 2

                            ElseIf (right_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.2)
                                Me.Manifold._Astar.heading = 2
                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.6)
                                    Me.Manifold._Astar.heading = 2
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.5)
                                    Me.Manifold._Astar.heading = 2
                                End If

                            End If

                        Else
                            '   Me.setHeading(20)

                        End If


                        '------------------------------------------------------------------------------------------------------------------------------------------------
                    ElseIf (Me.Manifold._Astar.heading = 2) Then



                        Dim left_open As Integer = 0
                        Dim left_close As Integer = 0

                        For i As Integer = 0 To 60
                            If laser.Range(i) > 3 Then
                                left_open += 1
                                If laser.Range(i) > 5 Then
                                    LeftObstacle_isFar += 1
                                End If
                                'Console.WriteLine("left is open")
                            ElseIf (laser.Range(i) < 3) Then
                                left_close += 1
                                LeftObstacle_isNear += 1

                                'Console.WriteLine("left is closed")

                            End If

                        Next

                        Dim Front_open As Integer = 0
                        Dim Front_close As Integer = 0

                        For i As Integer = 60 To 120
                            If laser.Range(i) > 3 Then
                                Front_open += 1
                                If laser.Range(i) > 5 Then
                                    FrontObstacle_isFar += 1
                                End If

                                'Console.WriteLine("left is open")
                            ElseIf (laser.Range(i) < 3) Then
                                Front_close += 1
                                FrontObstacle_isNear += 1
                                'Console.WriteLine("left is closed")

                            End If

                        Next

                        Dim right_open As Integer = 0
                        Dim right_close As Integer = 0



                        For i As Integer = 120 To 179
                            If laser.Range(i) > 3 Then
                                right_open += 1
                                If laser.Range(i) > 5 Then
                                    RightObstacle_isFar += 1
                                End If
                                'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), True)
                                'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(1, (((Me._CurrentPoseEstimate.X + 5300) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5)
                                'Console.WriteLine("front is open")

                            ElseIf (laser.Range(i) < 3) Then
                                right_close += 1
                                RightObstacle_isNear += 1
                                'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), False)
                                'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(1, 1000000000)
                                'Console.WriteLine("front is close")
                            End If
                        Next

                        If LeftObstacle_isFar > 30 Then
                            left_isfar = True

                        End If

                        If LeftObstacle_isNear > 30 Then
                            left_isnear = True
                        End If

                        ObstacleLength_Left = left_close
                        If (left_close > left_open) Then
                            'Me.Manifold._Astar.addNode(0, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), False)
                            Me.Manifold._Astar.addHeuristicValue(0, 1000000000)
                            left_isClosed = 0
                        Else
                            'Me.Manifold._Astar.addNode(0, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), True)
                            'If ((((Me._CurrentPoseEstimate.X - 7700) - Me.Manifold._Astar.endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5 < (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5) Then
                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X + 7000)
                            Dim temp_y As Double = Me._CurrentPoseEstimate.Y
                            Me.Manifold._Astar.addHeuristicValue(0, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            left_isClosed = 1
                            'Else
                            '   Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(0, (((Me._CurrentPoseEstimate.X - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5)
                            'End If
                        End If

                        If FrontObstacle_isFar > 30 Then
                            front_isfar = True

                        End If

                        If FrontObstacle_isNear > 30 Then
                            front_isnear = True
                        End If

                        ObstacleLength_Front = Front_close

                        If (Front_close > Front_open) Then
                            'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X + 3800), CInt(Me._CurrentPoseEstimate.Y - 3800)), False)
                            Me.Manifold._Astar.addHeuristicValue(1, 1000000000)
                            front_isClosed = 0
                        Else
                            'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X + 3800), CInt(Me._CurrentPoseEstimate.Y - 3800)), True)
                            'If ((((Me._CurrentPoseEstimate.X - 5600) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 5600) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5 < (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5) Then
                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X)
                            Dim temp_y As Double = (Me._CurrentPoseEstimate.Y + 7000)
                            Me.Manifold._Astar.addHeuristicValue(1, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            front_isClosed = 1
                            'Else
                            'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(1, (((Me._CurrentPoseEstimate.X - 5600) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 5600) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5)
                            'End If
                        End If

                        If RightObstacle_isFar > 30 Then
                            right_isfar = True

                        End If

                        If RightObstacle_isNear > 30 Then
                            right_isnear = True
                        End If

                        ObstacleLength_Right = right_close

                        If (right_close > right_open) Then
                            'Me.Manifold._Astar.addNode(2, New Point(CInt(Me._CurrentPoseEstimate.X -2000), CInt(Me._CurrentPoseEstimate.Y - 7700)), False)
                            Me.Manifold._Astar.addHeuristicValue(2, 1000000000)
                            right_isClosed = 0
                        Else
                            'Me.Manifold._Astar.addNode(2, New Point(CInt(Me._CurrentPoseEstimate.X + 3800), CInt(Me._CurrentPoseEstimate.Y + 3800)), True)
                            'If ((((Me._CurrentPoseEstimate.X - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5 < (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5) Then
                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X - 7000)
                            Dim temp_y As Double = (Me._CurrentPoseEstimate.Y)
                            Me.Manifold._Astar.addHeuristicValue(2, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            right_isClosed = 1
                            'Else
                            ' Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(2, (((Me._CurrentPoseEstimate.X - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5)
                            'End If
                        End If




                        Dim choice As Integer = Me.Manifold._Astar.findMin(Me.Manifold._Astar.HeuristicDistance)
                        Me.Manifold._Astar.evidence.Add(CStr(choice))
                        If choice = 0 Then
                            If (left_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)
                                Me.Manifold._Astar.heading = 1
                            ElseIf (left_isfar And DistanceToGoal < 5 And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.4)
                                Me.Manifold._Astar.heading = 1

                            ElseIf (left_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.2)
                                Me.Manifold._Astar.heading = 1
                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)
                                    Me.Manifold._Astar.heading = 1
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X + 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.5)
                                    Me.Manifold._Astar.heading = 1
                                End If

                            End If
                        ElseIf (choice = 1) Then
                            If (front_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.6)
                                Me.Manifold._Astar.heading = 2
                            ElseIf (front_isfar And DistanceToGoal < 5 And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.4)
                                Me.Manifold._Astar.heading = 2
                            ElseIf (front_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.2)
                                Me.Manifold._Astar.heading = 2
                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.6)
                                    Me.Manifold._Astar.heading = 2
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.5)
                                    Me.Manifold._Astar.heading = 2
                                End If

                            End If

                        ElseIf (choice = 2) Then
                            If (right_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)
                                Me.Manifold._Astar.heading = 3

                            ElseIf (right_isfar And DistanceToGoal < 5 And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.4)
                                Me.Manifold._Astar.heading = 3

                            ElseIf (right_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.2)
                                Me.Manifold._Astar.heading = 3

                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)
                                    Me.Manifold._Astar.heading = 3
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.5)
                                    Me.Manifold._Astar.heading = 3
                                End If

                            End If

                        Else
                            ' Me.setHeading(20)

                        End If

                    ElseIf (Me.Manifold._Astar.heading = 3) Then



                        Dim left_open As Integer = 0
                        Dim left_close As Integer = 0

                        For i As Integer = 0 To 60
                            If laser.Range(i) > 3 Then
                                left_open += 1
                                If laser.Range(i) > 5 Then
                                    LeftObstacle_isFar += 1
                                End If

                                'Console.WriteLine("left is open")
                            ElseIf (laser.Range(i) < 3) Then
                                left_close += 1
                                LeftObstacle_isNear += 1
                                'Console.WriteLine("left is closed")

                            End If

                        Next

                        Dim Front_open As Integer = 0
                        Dim Front_close As Integer = 0

                        For i As Integer = 60 To 120
                            If laser.Range(i) > 3 Then
                                Front_open += 1
                                If laser.Range(i) > 5 Then
                                    FrontObstacle_isFar += 1
                                End If

                                'Console.WriteLine("left is open")
                            ElseIf (laser.Range(i) < 3) Then
                                Front_close += 1
                                FrontObstacle_isNear += 1
                                'Console.WriteLine("left is closed")

                            End If

                        Next

                        Dim right_open As Integer = 0
                        Dim right_close As Integer = 0



                        For i As Integer = 120 To 179
                            If laser.Range(i) > 3 Then
                                right_open += 1
                                If laser.Range(i) > 5 Then
                                    RightObstacle_isFar += 1
                                End If
                                'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), True)
                                'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(1, (((Me._CurrentPoseEstimate.X + 5300) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5)
                                'Console.WriteLine("front is open")

                            ElseIf (laser.Range(i) < 3) Then
                                right_close += 1
                                RightObstacle_isNear += 1
                                'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), False)
                                'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(1, 1000000000)
                                'Console.WriteLine("front is close")
                            End If
                        Next

                        If LeftObstacle_isFar > 30 Then
                            left_isfar = True

                        End If

                        If LeftObstacle_isNear > 30 Then
                            left_isnear = True
                        End If

                        ObstacleLength_Left = left_close

                        If (left_close > left_open) Then
                            'Me.Manifold._Astar.addNode(0, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), False)
                            Me.Manifold._Astar.addHeuristicValue(0, 1000000000)
                            left_isClosed = 0
                        Else
                            'Me.Manifold._Astar.addNode(0, New Point(CInt(Me._CurrentPoseEstimate.X + 5300), CInt(Me._CurrentPoseEstimate.Y)), True)
                            'If ((((Me._CurrentPoseEstimate.X - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5 < (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5) Then
                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X)
                            Dim temp_y As Double = (Me._CurrentPoseEstimate.Y + 7000)
                            Me.Manifold._Astar.addHeuristicValue(0, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            left_isClosed = 1
                            'Else
                            'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(0, (((Me._CurrentPoseEstimate.X - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5)
                            'End If
                        End If


                        If FrontObstacle_isFar > 30 Then
                            front_isfar = True

                        End If

                        If FrontObstacle_isNear > 30 Then
                            front_isnear = True
                        End If

                        ObstacleLength_Front = Front_close

                        If (Front_close > Front_open) Then
                            'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X + 3800), CInt(Me._CurrentPoseEstimate.Y - 3800)), False)
                            Me.Manifold._Astar.addHeuristicValue(1, 1000000000)
                            front_isClosed = 0
                        Else
                            'Me.Manifold._Astar.addNode(1, New Point(CInt(Me._CurrentPoseEstimate.X + 3800), CInt(Me._CurrentPoseEstimate.Y - 3800)), True)
                            'If ((((Me._CurrentPoseEstimate.X - 5600) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 5600) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5 < (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5) Then
                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X - 7000)
                            Dim temp_y As Double = (Me._CurrentPoseEstimate.Y)
                            Me.Manifold._Astar.addHeuristicValue(1, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            front_isClosed = 1
                            'Else
                            'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(1, (((Me._CurrentPoseEstimate.X - 5600) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 5600) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5)
                            'End If
                        End If

                        If RightObstacle_isFar > 30 Then
                            right_isfar = True

                        End If

                        If RightObstacle_isNear > 30 Then
                            right_isnear = True
                        End If

                        ObstacleLength_Right = right_close

                        If (right_close > right_open) Then
                            'Me.Manifold._Astar.addNode(2, New Point(CInt(Me._CurrentPoseEstimate.X -2000), CInt(Me._CurrentPoseEstimate.Y - 7700)), False)
                            Me.Manifold._Astar.addHeuristicValue(2, 1000000000)
                            right_isClosed = 0
                        Else
                            'Me.Manifold._Astar.addNode(2, New Point(CInt(Me._CurrentPoseEstimate.X + 3800), CInt(Me._CurrentPoseEstimate.Y + 3800)), True)
                            'If ((((Me._CurrentPoseEstimate.X - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5 < (((Me._CurrentPoseEstimate.X) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5) Then
                            Dim temp_x As Double = (Me._CurrentPoseEstimate.X)
                            Dim temp_y As Double = (Me._CurrentPoseEstimate.Y - 7000)
                            Me.Manifold._Astar.addHeuristicValue(2, (Abs(temp_x - Me.Manifold._Astar.endArea.X) + Abs(temp_y - Me.Manifold._Astar.endArea.Y)) / 1000)
                            right_isClosed = 1
                            'Else
                            'Me.Manifold._Astar(Me.Manifold.currNumber).addHeuristicValue(2, (((Me._CurrentPoseEstimate.X - 2000) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.X) ^ 2 + ((Me._CurrentPoseEstimate.Y - 7700) - Me.Manifold._Astar(Me.Manifold.currNumber).endArea.Y) ^ 2) ^ 0.5)
                            'End If
                        End If



                        Dim choice As Integer = Me.Manifold._Astar.findMin(Me.Manifold._Astar.HeuristicDistance)
                        Me.Manifold._Astar.evidence.Add(CStr(choice))
                        If choice = 0 Then
                            If (left_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.6)
                                Me.Manifold._Astar.heading = 2
                            ElseIf (left_isfar And DistanceToGoal < 5 And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.4)
                                Me.Manifold._Astar.heading = 2
                            ElseIf (left_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.2)
                                Me.Manifold._Astar.heading = 2
                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.6)
                                    Me.Manifold._Astar.heading = 2
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y + 7000)), 0.5)
                                    Me.Manifold._Astar.heading = 2
                                End If

                            End If

                        ElseIf (choice = 1) Then
                            If (front_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)
                                Me.Manifold._Astar.heading = 3
                            ElseIf (front_isfar And DistanceToGoal < 5 And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.4)
                                Me.Manifold._Astar.heading = 3

                            ElseIf (front_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.2)
                                Me.Manifold._Astar.heading = 3
                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.6)
                                    Me.Manifold._Astar.heading = 3
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X - 7000), CInt(Me._CurrentPoseEstimate.Y)), 0.5)
                                    Me.Manifold._Astar.heading = 3
                                End If

                            End If

                        ElseIf (choice = 2) Then
                            If (right_isfar And DistanceToGoal > 5) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.6)
                                Me.Manifold._Astar.heading = 0
                            ElseIf (right_isfar And DistanceToGoal < 5 And right_isfar And DistanceToGoal > 3) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.4)
                                Me.Manifold._Astar.heading = 0
                            ElseIf (right_isnear) Then
                                Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.2)
                                Me.Manifold._Astar.heading = 0
                            Else
                                If (DistanceToGoal > 5) Then
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.6)
                                    Me.Manifold._Astar.heading = 0
                                Else
                                    Me.moveToTarget(agentNumber, New Point(CInt(Me._CurrentPoseEstimate.X), CInt(Me._CurrentPoseEstimate.Y - 7000)), 0.5)
                                    Me.Manifold._Astar.heading = 0
                                End If

                            End If

                        Else
                            ' Me.setHeading(20)

                        End If

                    End If




                    For i As Integer = 0 To 3
                        'Console.WriteLine("heuristic distance : {0}", Me.Manifold._Astar.HeuristicDistance(i))
                        Me.Manifold._Astar.evidence.Add(CStr(Me.Manifold._Astar.HeuristicDistance(i)))
                    Next
                    If (left_isClosed + front_isClosed + right_isClosed = 1) Then
                        Console.WriteLine("corridor walking is activated")
                    Else
                        Console.WriteLine("a path is found")
                    End If

                    Me.Manifold._Astar.Save("debug")
                End If

            End If
        End If

    End Sub


    Public Sub moveToTarget(ByVal agentNumber As Integer, ByVal goalpose As Point, ByVal agentSpeed As Single)
        Console.WriteLine("speed of agent : {0}", agentSpeed)
        If ((((Me._CurrentPoseEstimate.X - Me.Manifold._Astar.endArea.X) ^ 2 + (Me._CurrentPoseEstimate.Y - Me.Manifold._Astar.endArea.Y) ^ 2) ^ 0.5) / 1000 < 2) Then
            Me.Halt()
            '           Console.WriteLine("fafa fafa fafa !!!! moving to target is stopped")

        Else
            'Console.WriteLine("fafa fafa fafa!!! move to target is under target.")
            If (Me.Manifold._finished = True) Then

                If (Me.Number = agentNumber) Then

                    If ((((Me._CurrentPoseEstimate.X - goalpose.X) ^ 2 + (Me._CurrentPoseEstimate.Y - goalpose.Y) ^ 2) ^ 0.5) / 1000 < 2) Then
                        Me.Halt()
                        Me._attraction = 0



                    Else


                        Console.WriteLine("distance : {0}", ((Me._CurrentPoseEstimate.X - goalpose.X) ^ 2 + (Me._CurrentPoseEstimate.Y - goalpose.Y) ^ 2) ^ 0.5 / 1000)
                        Console.WriteLine("angle : {0} {1}", Atan((-goalpose.Y + Me._CurrentPoseEstimate.Y) / (-goalpose.X + Me._CurrentPoseEstimate.X)) * 180 / PI, Me._rotation)
                        If (goalpose.Y - Me._CurrentPoseEstimate.Y < 0) Then
                            If (Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI <= 0) Then
                                If (Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI < Me._rotation) Then

                                    If (ObstacleLength_Left > 20) Then
                                        Me.TurnLeft(0.2)
                                        Console.WriteLine("the angle is : {0} {1}", 0.2, "left")
                                    Else
                                        Me.TurnLeft(0.25)
                                        Console.WriteLine("the angle is : {0} {1}", 0.25, "left")
                                    End If


                                Else
                                    Me.Halt()
                                    Me.Drive(agentSpeed)


                                End If

                            Else
                                If ((Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI - 180) < Me._rotation) Then
                                    If (ObstacleLength_Left > 20) Then
                                        Me.TurnLeft(0.2)
                                        Console.WriteLine("the angle is : {0} {1}", 0.2, "left")
                                    Else
                                        Me.TurnLeft(0.25)
                                        Console.WriteLine("the angle is : {0} {1}", 0.25, "left")
                                    End If


                                Else
                                    Me.Halt()

                                    Me.Drive(agentSpeed)

                                End If
                            End If

                            Me.position = "left"


                        ElseIf (goalpose.Y - Me._CurrentPoseEstimate.Y > 0) Then
                            If (Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI > 0) Then
                                If (Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI > Me._rotation) Then
                                    If (ObstacleLength_Right > 20) Then
                                        Me.TurnRight(0.2)
                                        Console.WriteLine("the angle is : {0} {1}", 0.2, "right")
                                    Else
                                        Me.TurnRight(0.25)
                                        Console.WriteLine("the angle is : {0} {1}", 0.25, "right")
                                    End If

                                Else
                                    Me.Halt()

                                    Me.Drive(agentSpeed)

                                End If
                            Else
                                If ((180 + Atan((goalpose.Y - Me._CurrentPoseEstimate.Y) / (goalpose.X - Me._CurrentPoseEstimate.X)) * 180 / PI) > Me._rotation) Then
                                    If (ObstacleLength_Right > 20) Then
                                        Me.TurnRight(0.2)
                                        Console.WriteLine("the angle is : {0}", 0.2, "right")
                                    Else
                                        Me.TurnRight(0.25)
                                        Console.WriteLine("the angle is : {0} {1}", 0.25, "right")
                                    End If

                                Else
                                    Me.Halt()

                                    Me.Drive(agentSpeed)

                                End If
                            End If
                            Me.position = "right"




                        End If

                    End If
                End If
            End If
        End If
        'Thread.Sleep(50)


    End Sub


    Public Sub search(ByVal agentNumber As Integer, ByVal startPoint As Vector2, ByVal endPoint As Vector2)

        If (Me.Number = agentNumber) Then

            'If (Me._CurrentPoseEstimate.X > startPoint.X And Me._CurrentPoseEstimate.X < endPoint.X And Me._CurrentPoseEstimate.Y < startPoint.Y And Me._CurrentPoseEstimate.Y > endPoint.Y) Then
            If (Me._CurrentPoseEstimate.X > startPoint.X And Me._CurrentPoseEstimate.X < endPoint.X And Me._CurrentPoseEstimate.Y > startPoint.Y And Me._CurrentPoseEstimate.Y < endPoint.Y) Then
                Me._areaAttraction = 1

            Else
                setNewTarget(Me.Manifold.currNumber, New Point(CInt(startPoint.X + endPoint.X / 2), CInt(startPoint.Y + endPoint.Y / 2)))



            End If



            Console.WriteLine("attraction : {0}", Me._areaAttraction)
        End If
    End Sub

    Public Overridable Sub SendNewTarget(ByVal toRobot As String, ByVal target As Pose2D)

        Console.WriteLine("fafa")

    End Sub

#End Region





#Region " Change behavior "
    Overridable Sub ChangeBehaviour(ByVal newBehaviour As String)


    End Sub
#End Region

#Region " Finish "

    Overridable Sub Finish()
        Me.OnAgentStopped()
    End Sub

#End Region


End Class
