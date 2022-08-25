Imports System.Math
Imports System.Threading
Imports UvARescue.Math
Imports UvARescue.Agent.CommAgent
Imports UvARescue.Agent.MotionControl
Imports AForge.Fuzzy
Imports UvARescue.Agent



''' <summary>
''' Really simplistic, yet effective, steering actor that works
''' fine for most skid-steered and differential drive robots.
''' </summary>
''' <remarks></remarks>
Public Class StandardDriveActor
    Inherits DriveActor



    Private _KenafFrontFlipperHeight As Integer = 0
    Private _KenafRearFlipperHeight As Integer = 0




    Private _HeadLight As Boolean = False
    Private _Forward As Single = 0
    Public Overrides ReadOnly Property ForwardSpeed() As Single
        Get
            Return _Forward
        End Get
    End Property
    Private _TurnLeft As Single = 0
    Public Overrides ReadOnly Property TurningSpeed() As Single
        Get
            Return _TurnLeft
        End Get
    End Property

    Private _CameraTilt As Double = 0


    Public Overrides Sub Drive(ByVal speed As Single)
        Me._Forward = speed
        Me._TurnLeft = 0
        Me.Move(Me._Forward, Me._TurnLeft, Me._HeadLight)
    End Sub
    Public Overrides Sub DifferentialDrive(ByVal speed As Single, ByVal turning_speed As Single)
        Me._Forward = speed
        Me._TurnLeft = turning_speed
        Me.Move(Me._Forward, Me._TurnLeft, Me._HeadLight)
    End Sub

    Public Overrides Sub Turn(ByVal left As Single)
        Me._Forward = 0
        Me._TurnLeft = left
        Me.Move(Me._Forward, Me._TurnLeft, Me._HeadLight)
    End Sub


    Public Overrides Sub ToggleHeadLight()
        Me._HeadLight = Not Me._HeadLight
        Me.Move(Me._Forward, Me._TurnLeft, Me._HeadLight)
    End Sub

    Public Overrides Sub pathPlanTo(ByVal goalPose As Pose2D)
        Me.moveTo(goalPose)
    End Sub


    Public Overrides Sub Flip()
        Me.SendUsarSimCommand("DRIVE {Flip true}")
    End Sub


    Public Overrides Sub CameraPanTilt(ByVal up As Double)
        Me._CameraTilt += up
        If Me._CameraTilt > 1 Then
            Me._CameraTilt = 1
        ElseIf Me._CameraTilt < -1 Then
            Me._CameraTilt = -1
        End If

        'Talon and TeleMax have  more turning points (TBD)
        Dim command As String

        If Me.Agent.AgentConfig.RobotModel.ToLower = "airrobot" Then

            'AirRobot has only one turningpoint, a Tilt
            ' {Order 3} means bang-bang control

            'This was working, but {Order 0} also works according to Hanne
            command = String.Format("MISPKG {{Name CameraPanTilt}} {{Link 1}} {{Value {0}}} {{Link 2}} {{Value {1}}} {{Order {2}}}", _
            -Me._CameraTilt, 0, 3)
            'opposite direction, because the camera is looking downwards

        ElseIf Me.Agent.AgentConfig.RobotModel.ToLower = "zerg" Then

            ' {Order 10} means relative angle
            'the camera on the Zerg was only moving a small angle, so let's try this

            ''This was working, but {Order 0} also works according to Hanne
            command = String.Format("MISPKG {{Name CameraPanTilt}} {{Link 1}} {{Value {0}}} {{Link 2}} {{Value {1}}} {{Order {2}}}", _
            0, Me._CameraTilt, 10)
        Else


            'Most other robots have a camera with a double turning point

            command = String.Format("MISPKG {{Name CameraPanTilt}} {{Link 1}} {{Value {0}}} {{Link 2}} {{Value {1}}}", _
            0, Me._CameraTilt)

        End If

        Me.SendUsarSimCommand(command)

    End Sub

    Private _Up As Single = 0
    Private _MoveLeft As Single = 0
    Private _RotateLeft As Double = 0.0
    Public Overrides Sub Fly(ByVal speed As Single)
        Me._Forward = 0
        Me._TurnLeft = 0
        Me._Up = speed
        Me.Move(Me._Up, Me._Forward, Me._MoveLeft, Me._RotateLeft)
    End Sub



    Private currPos As Double



    Protected Sub moveTo(ByVal pose As Pose2D)
        Dim distance As Double = Double.MaxValue
       
        Dim distance2 As Double = Double.MaxValue
        Dim goalPose As Pose2D = New Pose2D()

        goalPose.X = pose.X
        goalPose.Y = pose.Y
        goalPose.Rotation = Atan2(goalPose.Y, goalPose.X)
       

        Dim dangle As Double = goalPose.Rotation * 180 / PI
        Dim dangle2 As Double = Me.Agent.GroundTruthPose.Yaw * 180 / PI
        'fuzzy part of path planning is here


        ' Dim mainDistance As AForge.Fuzzy.LinguisticVariable = New AForge.Fuzzy.LinguisticVariable("distance", 0, 200)
        'Dim state1 As AForge.Fuzzy.TrapezoidalFunction = New AForge.Fuzzy.TrapezoidalFunction(0, 5, 10, 15)
        'Dim soclose As AForge.Fuzzy.FuzzySet = New AForge.Fuzzy.FuzzySet("soclose", state1)
        'Dim state2 As AForge.Fuzzy.TrapezoidalFunction = New AForge.Fuzzy.TrapezoidalFunction(12, 20, 30, 40)
        'Dim close As AForge.Fuzzy.FuzzySet = New AForge.Fuzzy.FuzzySet("close", state2)
        'Dim state3 As AForge.Fuzzy.TrapezoidalFunction = New AForge.Fuzzy.TrapezoidalFunction(35, 50, 70, 90)
        'Dim far As AForge.Fuzzy.FuzzySet = New AForge.Fuzzy.FuzzySet("far", state3)
        'Dim state4 As AForge.Fuzzy.TrapezoidalFunction = New AForge.Fuzzy.TrapezoidalFunction(80, 100, TrapezoidalFunction.EdgeType.Right)
        'Dim sofar As AForge.Fuzzy.FuzzySet = New AForge.Fuzzy.FuzzySet("sofar", state4)
        'mainDistance.AddLabel(soclose)
        'mainDistance.AddLabel(close)
        'mainDistance.AddLabel(far)
        'mainDistance.AddLabel(sofar)
        'For i As Integer = 0 To 15
        'soclose.GetMembership(i)
        'Next

        'For i As Integer = 12 To 40
        'close.GetMembership(i)
        'Next

        'For i As Integer = 35 To 90
        'far.GetMembership(i)
        'Next

        'For i As Integer = 80 To 200
        'sofar.GetMembership(i)
        'Next

        'Dim speed As LinguisticVariable = New LinguisticVariable("speed", 0, 2)
        'Dim soslow As SingletonFunction = New SingletonFunction(0.5)
        'Dim speedState1 As FuzzySet = New FuzzySet("soslow", soslow)
        'Dim slow As SingletonFunction = New SingletonFunction(1.0)
        'Dim speedState2 As FuzzySet = New FuzzySet("slow", slow)
        'Dim fast As SingletonFunction = New SingletonFunction(1.5)
        'Dim speedState3 As FuzzySet = New FuzzySet("fast", fast)
        'Dim sofast As SingletonFunction = New SingletonFunction(2.0)
        'Dim speedState4 As FuzzySet = New FuzzySet("sofast", sofast)
        'speed.AddLabel(speedState1)
        'speed.AddLabel(speedState2)
        'speed.AddLabel(speedState3)
        'speed.AddLabel(speedState4)
        'Dim FuzzyDatabase As Database = New Database()
        'FuzzyDatabase.AddVariable(mainDistance)
        'FuzzyDatabase.AddVariable(speed)

        'Dim _interface As InferenceSystem = New InferenceSystem(FuzzyDatabase, New CentroidDefuzzifier(1000))
        '_interface.NewRule("rule1", "IF distance IS soclose THEN speed IS soslow")
        '_interface.NewRule("rule2", "IF distance IS close THEN speed IS slow")
        '_interface.NewRule("rule3", "IF distance IS far THEN speed IS fast")
        '_interface.NewRule("rule4", "IF distance IS sofar THEN speed IS sofast")

        '_interface.SetInput("distance", CSng(distance2))
        'Dim fuzzyoutput As FuzzyOutput = _interface.ExecuteInference("speed")

        'Dim oc As FuzzyOutput.OutputConstraint = fuzzyoutput.OutputList
        'Dim counters As Integer = 0
        'For Each oc As FuzzyOutput.OutputConstraint In fuzzyoutput.OutputList
        'Console.WriteLine(oc.Label + " - " + oc.FiringStrength.ToString())
        'counters = counters + 1
        'Next
        ' Dim output As Single = oc.FiringStrength()
        'Me._behaviour = New BehaviorAgent(Me.Agent)
        'Me._motions = New MotionControl(_behaviour)
        'Dim currPose As Pose2D = New Pose2D(Me.Agent.GroundTruthPose.X, Me.Agent.GroundTruthPose.Y, Me.Agent.GroundTruthPose.Yaw)
        'Console.WriteLine(String.Format("goal angle : {0},robot angle : {1}", dangle, dangle2))
        'Console.WriteLine()
        'Console.WriteLine(String.Format("counters {0}", counters))
        'While (dangle2 - dangle > 8)

        '        Me.Agent.TurnLeft(0.2F)
        '       dangle2 = dangle2 - 0.015
        '      Console.WriteLine(String.Format("{0}", dangle2))


        'End While
        'Me.Agent.Halt()
        'While (dangle2 - dangle < -8)
        'Me.Agent.TurnRight(0.2F)
        'dangle2 = dangle2 + 0.015

        'End While
        'Me.Agent.Halt()
        ' distance = ((goalPose.X - Me.Agent.GroundTruthPose.X) ^ 2 + (goalPose.Y - Me.Agent.GroundTruthPose.Y) ^ 2) ^ 0.5
        'distance2 = ((goalPose.X - Me.Agent.GroundTruthPose.X) ^ 2 + (goalPose.Y - Me.Agent.GroundTruthPose.Y) ^ 2) ^ 0.5
        'Console.WriteLine(String.Format("last robot pose : {0} {1} {2} {3}", Me.Agent.GroundTruthPose.X, Me.Agent.GroundTruthPose.Y, dangle2, dangle))

        'If (distance > distance2 * Sin(8)) Then


        'Me.DifferentialDrive(1.0F, 0)
        'If (Me.AgentControl.IsActiveMotion(MotionType.FrontierExploration)) Then
        'nothing
        'Else

        '   Me.AgentControl.ActivateMotion(MotionType.FrontierExploration, True)
        'End If
        'While (distance > distance2 * Sin(8))
        'For i As Integer = 0 To Me._laser.FrontCount
        'If (Me.Agent.SonarSensor.FrontYaw(i) > 2.3 AndAlso Me.Agent.SonarSensor.FrontYaw(i) < 0) Then
        'Me.Agent.Halt()
        'End If

        'Me.AlertObstacle = True
        'Dim alert As String = "HOK - OBS - " + laser.Range(i).ToString
        'Me.Agent.NotifyAlertReceived(alert)
        'End If
        '        Next
        'distance = ((goalPose.X - Me.Agent.GroundTruthPose.X) ^ 2 + (goalPose.Y - Me.Agent.GroundTruthPose.Y) ^ 2) ^ 0.5

        'this part is for multithreading


        ' System.Windows.Forms.Application.DoEvents()

        'Thread.Sleep(0)

        '   End While
        'End If




        ' Me.Agent.Halt()
        '  Console.WriteLine(String.Format("last robot pose : {0} {1}", Me.Agent.GroundTruthPose.X, Me.Agent.GroundTruthPose.Y))




    End Sub

    Protected Sub Move(ByVal forward As Single, ByVal left As Single, ByVal headLight As Boolean)
        Me.SendUsarSimCommand(String.Format("DRIVE {{Left {0}}} {{Right {1}}} {{Light {2}}}", _
         forward - left, _
         forward + left, _
         headLight.ToString))

    End Sub

    Protected Sub Move(ByVal leftSpeed As Single, ByVal rightSpeed As Single)
        Me.SendUsarSimCommand(String.Format("DRIVE {{Left {0}}} {{Right {1}}}", leftSpeed, rightSpeed))
        Console.WriteLine("correct choice")
    End Sub

    Protected Sub Move(ByVal up As Single, ByVal forward As Single, ByVal left As Single, ByVal rot As Double)
        Me.SendUsarSimCommand(String.Format("DRIVE {{AltitudeVelocity {0}}} {{LinearVelocity {1}}} {{LateralVelocity {2}}} {{RotationalVelocity {3}}} {{Normalized {4}}}", _
         up, _
         forward, _
         left, _
         rot, _
         "False"))
    End Sub
    'DRIVE {Propeller float} {Rudder float} {SternPlane float} {Normalized bool} {Light bool}

#Region " Kenaf Specific "

    'Kenaf flippers can be at height -3 (standing on flipper), ..., 0 (flat), ... 3 (flipper up perpendicular to ground)

    Public Overrides Sub KenafFrontUp()
        If Me._KenafFrontFlipperHeight = 3 Then Return

        Me._KenafFrontFlipperHeight += 1

        Dim msg As String = String.Format("MULTIDRIVE {{FRFlipper {0}}} {{FLFlipper {1}}} {{RRFlipper {2}}} {{RLFlipper {3}}}", _
         -0.5 * _KenafFrontFlipperHeight, _
         -0.5 * _KenafFrontFlipperHeight, _
         0.5 * _KenafRearFlipperHeight, _
         0.5 * _KenafRearFlipperHeight)

        Console.WriteLine(msg)
        Me.SendUsarSimCommand(msg)
    End Sub

    Public Overrides Sub KenafFrontDown()
        If Me._KenafFrontFlipperHeight = -3 Then Return

        Me._KenafFrontFlipperHeight -= 1

        Me.SendUsarSimCommand(String.Format("MULTIDRIVE {{FRFlipper {0}}} {{FLFlipper {1}}} {{RRFlipper {2}}} {{RLFlipper {3}}}", _
         -0.5 * _KenafFrontFlipperHeight, _
         -0.5 * _KenafFrontFlipperHeight, _
         0.5 * _KenafRearFlipperHeight, _
         0.5 * _KenafRearFlipperHeight))
    End Sub

    Public Overrides Sub KenafRearUp()
        If Me._KenafRearFlipperHeight = 3 Then Return

        Me._KenafRearFlipperHeight += 1

        Me.SendUsarSimCommand(String.Format("MULTIDRIVE {{FRFlipper {0}}} {{FLFlipper {1}}} {{RRFlipper {2}}} {{RLFlipper {3}}}", _
         -0.5 * _KenafFrontFlipperHeight, _
         -0.5 * _KenafFrontFlipperHeight, _
         0.5 * _KenafRearFlipperHeight, _
         0.5 * _KenafRearFlipperHeight))
    End Sub

    Public Overrides Sub KenafRearDown()
        If Me._KenafRearFlipperHeight = -3 Then Return

        Me._KenafRearFlipperHeight -= 1

        Me.SendUsarSimCommand(String.Format("MULTIDRIVE {{FRFlipper {0}}} {{FLFlipper {1}}} {{RRFlipper {2}}} {{RLFlipper {3}}}", _
         -0.5 * _KenafFrontFlipperHeight, _
         -0.5 * _KenafFrontFlipperHeight, _
         0.5 * _KenafRearFlipperHeight, _
         0.5 * _KenafRearFlipperHeight))
    End Sub

#End Region


    Private _behaviour As BehaviorAgent
    Public ReadOnly Property behaviourAgent() As BehaviorAgent
        Get
            Return Me._behaviour
        End Get
    End Property

    Private _motions As MotionControl
    Public ReadOnly Property AgentControl() As MotionControl
        Get
            Return Me._motions
        End Get
    End Property

End Class
