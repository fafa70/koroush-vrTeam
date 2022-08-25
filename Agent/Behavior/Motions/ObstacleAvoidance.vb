' RIKNOTE: LOCALLY CREATED VERSION ON TUE 26-02-2008 
' BECAUSE OFFICIAL ONE CREATED ON WED 20-02-2008 NOT
' ADDED TO SVN REPO YET

Imports System.Math
Imports UvARescue.Math

Public Class ObstacleAvoidance
    Inherits Motion

    Private Enum Movement
        None
        Forward
        TurnR
        TurnL
    End Enum

    ' unit conversion constants
    Private ReadOnly _RAD2DEG As Double = 180.0 / PI
    Private ReadOnly _DEG2RAD As Double = PI / 180.0
    Private _agent As Agent
    ' state variables
    Private _CurrentMovement As Movement
    Private _IsTurning As Boolean
    Private _MinTurnAngleRad As Double

    Private _CurrentRoll As Double
    Private _CurrentPitch As Double
    Private _CurrentYaw As Double
    Private _OldYaw As Double


    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
    End Sub

    Protected Overrides Sub OnActivated()
        Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\ObstacleAvoidance.vb::OnActivated() called")
        MyBase.OnActivated()

        Me._IsTurning = False
        Me._CurrentMovement = Movement.None
    End Sub

    Protected Overrides Sub OnDeActivated()
        Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\ObstacleAvoidance.vb::OnDeActivated() called")
        MyBase.OnDeActivated()
        Me._CurrentMovement = Movement.None
    End Sub


    






    Protected Overrides Sub ProcessSensorUpdate(ByVal sensor As Sensor)
        ' RIKNOTE: pass the update to our Motion parent as well
        MyBase.ProcessSensorUpdate(sensor)

        If sensor.SensorType = LaserRangeSensor.SENSORTYPE_RANGESCANNER Then
            Me.ProcessLaserRangeData(sensor.SensorName, DirectCast(sensor, LaserRangeSensor).PeekData)
        ElseIf sensor.SensorType = SonarSensor.SENSORTYPE_SONAR Then
            Me.ProcessSonarData(DirectCast(sensor, SonarSensor).CurrentData)
        ElseIf sensor.SensorType = InsSensor.SENSORTYPE_INS Then
            Me.ProcessInsData(DirectCast(sensor, InsSensor).CurrentData)
        ElseIf sensor.SensorType = GroundTruthSensor.SENSORTYPE_GROUNDTRUTH Then
            Me.ProcessGroundTruthData(DirectCast(sensor, GroundTruthSensor).CurrentData)
        End If

    End Sub



    Protected Overridable Sub ProcessLaserRangeData(ByVal sensorName As String, ByVal laser As LaserRangeData)
        Return 'do not print all rangeData
        Dim elapsedSinceLastAlert As TimeSpan = DateTime.Now - Me.Control.Agent.MotionTimeStamp
        Console.WriteLine("FOV: {0}, RES: {1}, MINRANGE: {2}, MAXRANGE: {3}", laser.FieldOfView, laser.Resolution, laser.MinRange, laser.MaxRange)
        If (sensorName = "TiltedScanner") Then
            'The loop below is to examine Hokuyo Data in case we want to change the tilt
            'For i As Integer = 0 To laser.Range.Length
            ' Console.WriteLine("{0} {1}", i, laser.Range(i))
            'Next

            'The values below must be adjusted if the Hokuyo is mounted higher/lower/at different angle
            ' Current values are used by:
            ' Sensors=(ItemClass=class'USARModels.Hokuyo',ItemName="TiltedScanner",Position=(X=0.1735,Y=0.0,Z=-0.383),Direction=(Y=-0.32,Z=0.0,X=0.0))

            For i As Integer = 120 To 150
                If (laser.Range(i) > 2.3 OrElse laser.Range(i) < 1) And elapsedSinceLastAlert.Seconds > 2 Then
                    Me.Control.Agent.DifferentialDrive(0.1, 0.3)
                    'Me.AlertObstacle = True
                    Dim alert As String = "HOK - OBS - " + laser.Range(i).ToString
                    Me.Control.Agent.NotifyAlertReceived(alert)
                End If
            Next
            Exit Sub
        End If

        Dim minFrontRange As Double = Double.MaxValue
        For i As Integer = 80 To 100
            minFrontRange = Min(minFrontRange, laser.Range(i))
        Next

        While minFrontRange < 2 And elapsedSinceLastAlert.Seconds > 2
            Me.Control.Agent.TurnLeft(0.3)
            'Me.AlertObstacle = True
            Dim alert As String = "LSR - OBS - " + minFrontRange.ToString
            Me.Control.Agent.NotifyAlertReceived(alert)
        End While
    End Sub

    Protected Overridable Sub ProcessSonarData(ByVal sonar As SonarData)
        Dim slightly_left As Double
        Dim slightly_right As Double
        Dim minSonarDist As Double = 2.0
        Dim turnSpeedL As Single = 0.5F
        Dim turnSpeedR As Single = -0.5F

        For i As Integer = 1 To sonar.FrontCount
            If Me.Control.Agent.SonarSensor.FrontYaw(i) > -0.3 AndAlso Me.Control.Agent.SonarSensor.FrontYaw(i) < 0.0 Then
                slightly_left = sonar.Front(i)
            End If
        Next

        For i As Integer = 1 To sonar.FrontCount
            If Me.Control.Agent.SonarSensor.FrontYaw(i) < +0.3 AndAlso Me.Control.Agent.SonarSensor.FrontYaw(i) > 0.0 Then
                slightly_right = sonar.Front(i)
            End If
        Next

        ' Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\ObstacleAvoidance.vb::ProcessSonarData() called, F4: {0}, F5: {1}", f4, f5)
        ' NOTE: TURN FASTER IF ANGLE GREATER? SLOW DOWN WHEN GETTING CLOSER TO OBSTACLE?

        If (Not Me._IsTurning) Then
            Me._CurrentMovement = Movement.Forward
            Me._OldYaw = Me._CurrentYaw

            If (slightly_left < minSonarDist And slightly_right < minSonarDist) Then
                ' obstacle dead ahead: must turn 90 degrees in random? direction
                ' store current yaw so ProcessInsData() knows when to stop
                Me._MinTurnAngleRad = 90.0 * Me._DEG2RAD
                Me._OldYaw = Me._CurrentYaw
                Me._IsTurning = True
                Me._CurrentMovement = Movement.TurnL

                Console.WriteLine(System.Environment.NewLine)
                Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\ObstacleAvoidance.vb::ProcessSonarData(), turning 90 degrees")
                Console.WriteLine(System.Environment.NewLine)
            ElseIf (slightly_left < minSonarDist) Then
                ' obstacle ahead left: must turn in right direction "a bit"
                ' store current yaw so ProcessInsData() knows when to stop
                Me._MinTurnAngleRad = 45.0 * Me._DEG2RAD
                Me._OldYaw = Me._CurrentYaw
                Me._IsTurning = True
                Me._CurrentMovement = Movement.TurnR

                Console.WriteLine(System.Environment.NewLine)
                Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\ObstacleAvoidance.vb::ProcessSonarData(), turning 45 degrees R")
                Console.WriteLine(System.Environment.NewLine)
            ElseIf (slightly_right < minSonarDist) Then
                ' obstacle ahead right: must turn in left direction "a bit"
                ' store current yaw so ProcessInsData() knows when to stop
                Me._MinTurnAngleRad = 45.0 * Me._DEG2RAD
                Me._OldYaw = Me._CurrentYaw
                Me._IsTurning = True
                Me._CurrentMovement = Movement.TurnL

                Console.WriteLine(System.Environment.NewLine)
                Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\ObstacleAvoidance.vb::ProcessSonarData(), turning 45 degrees L")
                Console.WriteLine(System.Environment.NewLine)
            End If
        End If

        With Me.Control.Agent
            Select Case Me._CurrentMovement
                Case Movement.None
                    .Halt()
                Case Movement.Forward
                    .Drive(2.0F)
                Case Movement.TurnL
                    ' speed, turning speed
                    .DifferentialDrive(0.6F, turnSpeedL)
                Case Movement.TurnR
                    ' speed, turning speed
                    .DifferentialDrive(0.6F, turnSpeedR)
            End Select
        End With
    End Sub


    Protected Overridable Sub ProcessGroundTruthData(ByVal current_data As GroundTruthData)
        ' update the current angles
        Me._CurrentRoll = current_data.Roll()
        Me._CurrentPitch = current_data.Pitch()
        Me._CurrentYaw = current_data.Yaw()

        ' Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\ObstacleAvoidance.vb::ProcessGroundTruthData() called, current yaw: {0}, old yaw: {1}", Me._CurrentYaw, Me._OldYaw)

        ' deal with the 0 <==> 2PI angle flip
        If (Me._IsTurning) Then
            If (Me._CurrentMovement = Movement.TurnL And Me._CurrentYaw > Me._OldYaw) Then
                ' we are turning left (decreasing yaw) but
                ' CurrentYaw passed from 0 to 2PI radians,
                ' compensate
                Me._MinTurnAngleRad -= Me._OldYaw
                Me._OldYaw = PI * 2
            End If
            If (Me._CurrentMovement = Movement.TurnR And Me._CurrentYaw < Me._OldYaw) Then
                ' we are turning right (increasing yaw) but
                ' CurrentYaw  passed from 2PI to 0 radians,
                ' compensate
                Me._MinTurnAngleRad -= ((PI * 2) - Me._OldYaw)
                Me._OldYaw = 0.0
            End If

            Dim deltaYaw As Double = Abs(Me._CurrentYaw - Me._OldYaw)

            If (deltaYaw >= Me._MinTurnAngleRad) Then
                ' abort the current turn
                Me._IsTurning = False
            End If
        End If

        'Arnoud 29 July 2008

        If Abs(NormalizeAngle(current_data.Roll)) > 0.1 OrElse Abs(NormalizeAngle(current_data.Pitch)) > 0.3 Then
            With Me.Control.Agent
                .Reverse(0.5F) 'or halt()
                _CurrentMovement = Movement.None
                Me.Control.ActivateMotion(MotionType.RandomWalk_Following, True) 'haven't seen the obstacle with sonar, lets try it with laserscans
            End With
            Console.WriteLine(String.Format("[RandomWalk] - Something: tilting ({0:f2} , {1:f2}) rad -> Retreat and RandomWalk", NormalizeAngle(current_data.Roll), NormalizeAngle(current_data.Pitch)))

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


    Protected Overridable Sub ProcessInsData(ByVal ins As InsData)
        ' RIKNOTE: THIS IS NEVER CALLED? MUST SET "DEAD RECKONING" ETC
    End Sub
End Class
