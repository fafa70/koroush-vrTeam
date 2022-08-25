Imports System.Math
Imports UvARescue.Math

Public Class CorridorWalk_Following
    Inherits Motion

    ' unit conversion constants (copied from OA)
    Private ReadOnly _RAD2DEG As Double = 180.0 / PI
    Private ReadOnly _DEG2RAD As Double = PI / 180.0

    Protected random As New System.Random(DateTime.Now.Millisecond)

    Protected Enum Movement
        None
        HeadNorth
        HeadNorthWest
        HeadNorthEast
        Retreat
    End Enum


    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\CorridorWalk.vb::New() called")
    End Sub

    Protected Overrides Sub OnActivated()
        Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\CorridorWalk_Following.vb::OnActivated() called")
        MyBase.OnActivated()
        Me._CurrentMovement = Movement.None
    End Sub

    Protected Overrides Sub OnDeActivated()
        Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\CorridorWalk_Following.vb::OnDeActivated() called")
        MyBase.OnDeActivated()
        Me._CurrentMovement = Movement.None
    End Sub

    Protected Overrides Sub ProcessPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        MyBase.ProcessPoseEstimateUpdated(pose)

        'Check whether robot is too close to a TeamMate
        Dim distToRobot As Double = Double.MaxValue
        Dim teammatePose As Pose2D

        With Me.Control.Agent
            'In loop for all agents having lower number 
            '(Robots with higher numbers give way to those with lower numbers.
            ' This order chosen so that robots do not run into the ComStation,
            ' which will have the lowest number)            
            For i As Integer = 0 To (.Number - 1)
                'Calculate distance between the two robots
                If ._TeamPoses.ContainsKey(._TeamMembers(i)) Then
                    If ._TeamMembers(i) = .Name Then
                        'Console.WriteLine("CorridorWalk_Following: calculating distance to myself")
                    Else
                        teammatePose = ._TeamPoses.Item(._TeamMembers(i))
                        distToRobot = ((pose.X - teammatePose.X) ^ 2 + (pose.Y - teammatePose.Y) ^ 2) ^ 0.5
                    End If
                End If

                'If too close
                If (distToRobot < 1000) Then
                    .Halt()
                    Console.WriteLine("[{0}] Too close to {1}!  Avoiding...", .Name, ._TeamMembers(i))
                    If (._TeamMembers(i).Equals(Me.Control.Agent._OperatorName)) Then
                        Me.Control.ActivateMotion(MotionType.RandomWalk_Following, True)
                    Else
                        Me.Control.ActivateMotion(MotionType.AvoidTeamMate_Following, True)
                    End If
                    Exit Sub
                End If
            Next
        End With
    End Sub



    Private _Counter As Integer = 5
    Protected Overrides Sub ProcessSensorUpdate(ByVal sensor As Sensor)
        ' RIKNOTE: pass the update to our Motion parent as well
        MyBase.ProcessSensorUpdate(sensor)

        ' RIKNOTE: this Motion type uses only the laser range data for now (and
        ' the groundtruth data if our roll or pitch angle becomes too great?)
        ' but should probably just rely on the sonar updates
        If sensor.SensorType = LaserRangeSensor.SENSORTYPE_RANGESCANNER Then
            Me.ProcessLaserRangeData(sensor.SensorName, DirectCast(sensor, LaserRangeSensor).PeekData)
        ElseIf sensor.SensorType = SonarSensor.SENSORTYPE_SONAR Then
            Me.ProcessSonarData(DirectCast(sensor, SonarSensor).CurrentData)
        ElseIf sensor.SensorType = InsSensor.SENSORTYPE_INS Then
            Me.ProcessInsData(DirectCast(sensor, InsSensor).CurrentData)
        ElseIf sensor.SensorType = GroundTruthSensor.SENSORTYPE_GROUNDTRUTH Then
            Me.ProcessGroundTruthData(DirectCast(sensor, GroundTruthSensor).CurrentData)
        ElseIf sensor.SensorType = VictimSensor.SENSORTYPE_VICTIM Then
            Me.ProcessVictimData(DirectCast(sensor, VictimSensor).CurrentData)
        End If

    End Sub

    Private _CurrentMovement As Movement

    Protected Overridable Sub ProcessVictimData(ByVal current_data As VictimData)

        'Check whether robot is too close to a Victim
        Dim distToPart As Double = Double.MaxValue
        Dim pose As Pose2D = Me.Control.Agent.CurrentPoseEstimate

        Dim factor As Single = 1000 'to convert from m to mm

        'With Me.Control.Agent

        For Each part As VictimData.VictimPart In current_data.Parts

            If part.PartName = "chest" Or part.PartName = "head" Or part.PartName = "leg" Or part.PartName = "hand" Then

                'note that the part is observed in local coords wrt the current pose of the robot
                'transform to global coordinate wrt pose using a Pose2D object with dummy rotation
                Dim pglobal As Vector2 = New Pose2D(CType(part.X * factor, Single), CType(part.Y * factor, Single), 0).ToGlobal(pose).Position

                distToPart = ((pose.X - pglobal.X) ^ 2 + (pose.Y - pglobal.Y) ^ 2) ^ 0.5

            End If

            If (distToPart < 1000) Then
                Me.Control.Agent.Halt()
                Console.WriteLine("[{0}] Too close to {1}!  Avoiding...", Me.Control.Agent.Name, part.PartName)

                Me.Control.ActivateMotion(MotionType.AvoidVictim_Following, True) 'explicit return, to FrontierExploration (success) or ObstacleAvoidance (failure)

                Continue For 'one part too close is enough
            End If


        Next
        'End With

    End Sub

    Protected Overridable Sub ProcessGroundTruthData(ByVal current_data As GroundTruthData)
        If Abs(NormalizeAngle(current_data.Roll)) > 0.1 OrElse Abs(NormalizeAngle(current_data.Pitch)) > 0.3 Then
            With Me.Control.Agent
                _CurrentMovement = Movement.Retreat
                .Reverse(0.5F)
            End With
            Console.WriteLine(String.Format("[CorridorWalk_Following] - Something: tilting ({0:f2} , {1:f2}) rad -> Retreat", NormalizeAngle(current_data.Roll), NormalizeAngle(current_data.Pitch)))

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



    Protected Overridable Sub ProcessLaserRangeData(ByVal sensorName As String, ByVal laser As LaserRangeData)
        'It is possible that a sensor is mounted on a tilt.  Not interested in that here.
        If (sensorName = "TiltedScanner") Then
            Exit Sub
        End If


        Dim fov As Double = 22.5 / Me._RAD2DEG

        'Dim minWest As Double = Double.MaxValue            ' -101.25 till -78.75 deg
        Dim minWestNorthWest As Double = Double.MaxValue    '  -78.75 till -56.25 deg
        Dim minNorthWest As Double = Double.MaxValue        '  -56.25 till -33.75 deg
        Dim minNorthNorthWest As Double = Double.MaxValue   '  -33.75 till -11.25 deg
        Dim minNorth As Double = Double.MaxValue            '  -11.25 till +11.25 deg
        Dim minNorthNorthEast As Double = Double.MaxValue
        Dim minNorthEast As Double = Double.MaxValue
        Dim minEastNorthEast As Double = Double.MaxValue
        'Corridors typical dimensions
        Dim minDistance As Double = 1.0
        Dim maxDistance As Double = 0.1


        'Dim minEast As Double = Double.MaxValue

        'helper vars
        Dim length As Integer = laser.Range.Length - 1      ' index of last laser beam (180)
        Dim start As Double
        Dim until As Double



        'determine min distance in minWestNorthWest direction
        start = fov / 2
        until = start + fov

        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            minWestNorthWest = Min(minWestNorthWest, laser.Range(i))
        Next
        If minWestNorthWest < minDistance Then
            minDistance = minWestNorthWest
        End If
        If minWestNorthWest > maxDistance Then
            maxDistance = minWestNorthWest
        End If

        'determine min distance in minNorthWest direction
        start = until
        until = start + fov
        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            minNorthWest = Min(minNorthWest, laser.Range(i))
        Next
        If minNorthWest < minDistance Then
            minDistance = minNorthWest
        End If
        If minNorthWest > maxDistance Then
            maxDistance = minNorthWest
        End If

        'determine min distance in minNorthNorthWest direction
        start = until
        until = start + fov
        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            minNorthNorthWest = Min(minNorthNorthWest, laser.Range(i))
        Next
        If minNorthNorthWest < minDistance Then
            minDistance = minNorthNorthWest
        End If
        If minNorthNorthWest > maxDistance Then
            maxDistance = minNorthNorthWest
        End If

        'determine min distance in minNorth direction
        start = until
        until = start + fov
        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            minNorth = Min(minNorth, laser.Range(i))
        Next
        If minNorth < minDistance Then
            minDistance = minNorth
        End If
        If minNorth > maxDistance Then
            maxDistance = minNorth
        End If

        'determine min distance in minNorthNorthEast direction
        start = until
        until = start + fov
        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            minNorthNorthEast = Min(minNorthNorthEast, laser.Range(i))
        Next
        If minNorthNorthEast < minDistance Then
            minDistance = minNorthNorthEast
        End If
        If minNorthNorthEast > maxDistance Then
            maxDistance = minNorthNorthEast
        End If

        'determine min distance in minNorthEast direction
        start = until
        until = start + fov
        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            minNorthEast = Min(minNorthEast, laser.Range(i))
        Next
        If minNorthEast < minDistance Then
            minDistance = minNorthNorthEast
        End If
        If minNorthEast > maxDistance Then
            maxDistance = minNorthEast
        End If

        'determine min distance in minEastNorthEast direction
        start = until
        until = start + fov
        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            minEastNorthEast = Min(minEastNorthEast, laser.Range(i))
        Next
        If minEastNorthEast < minDistance Then
            minDistance = minEastNorthEast
        End If
        If minEastNorthEast > maxDistance Then
            maxDistance = minEastNorthEast
        End If




        Dim left As Single = 0.3F 'turning speed
        Dim right As Single = -0.3F 'turning speed

        If maxDistance = 0.1 Then
            If Me._CurrentMovement = Movement.Retreat Then
                'Retreat is already triggered by Tilt-sensor
            Else
                Me._CurrentMovement = Movement.None
                'Do something smart
                Me.Control.ActivateMotion(MotionType.Following, True)
            End If

        ElseIf maxDistance = minNorth Then
            Me._CurrentMovement = Movement.HeadNorth
        ElseIf maxDistance = minNorthNorthWest Then
            Me._CurrentMovement = Movement.HeadNorthWest
            left = 0.1F
        ElseIf maxDistance = minNorthNorthEast Then
            Me._CurrentMovement = Movement.HeadNorthEast
            right = -0.1F
        ElseIf maxDistance = minNorthWest Then
            Me._CurrentMovement = Movement.HeadNorthWest
            left = 0.2F
        ElseIf maxDistance = minNorthEast Then
            Me._CurrentMovement = Movement.HeadNorthEast
            right = -0.2F
        ElseIf maxDistance = minWestNorthWest Then
            Me._CurrentMovement = Movement.HeadNorthWest
            left = 0.3F
        ElseIf maxDistance = minEastNorthEast Then
            Me._CurrentMovement = Movement.HeadNorthEast
            right = -0.3F
        End If

        If minDistance = 1.0 Then
            'Out of the Corridor
            Me._CurrentMovement = Movement.None
            'Go for a stroll
            Console.WriteLine(String.Format("CorridorWalk_Following: out of the corridor (minDistance = {0} m), no longer following wall", minDistance))
            Me.Control.ActivateMotion(MotionType.RandomWalk_Following, True)
        End If

        With Me.Control.Agent
            Select Case Me._CurrentMovement
                Case Movement.None
                    .Halt()
                Case Movement.HeadNorth
                    .Drive(0.8F)
                Case Movement.HeadNorthWest
                    .DifferentialDrive(0.6F, left)
                Case Movement.HeadNorthEast
                    .DifferentialDrive(0.6F, right)
                Case Movement.Retreat
                    .Reverse(0.5F)
            End Select
        End With

    End Sub





    Protected Overridable Sub ProcessInsData(ByVal ins As InsData)
        ' RIKNOTE: no-op, we only use the laser (and sonar?) sensors
    End Sub



    ' RIKNOTE: code used by Max & Bayu for 2006 competition (which
    ' uses only sonar rather than only laser data) ported from C++
    ' to VB, see Agent\Sensors\Data\SonarData.vb for the SonarData
    ' class def
    ' RIKNOTE: Arnoud already did this in ProcessLaserRangeData()
    Protected Overridable Sub ProcessSonarData(ByVal sonar As SonarData)
        Dim fCount As Integer = sonar.FrontCount()
        Dim rCount As Integer = sonar.RearCount()

        ' RIKNOTE: these need to be defined as class members
        Dim isRetreating As Boolean
        Dim justRetreated As Boolean
        Dim startRetreat As Long

        ' Dim frontSonarData(fCount) As Double
        ' Dim rearSonarData(rCount) As Double

        ' RIKNOTE: sonar keys are one-based (range [1, 8], not [0, 7])
        ' For idx As Integer = 1 To fCount
        '    frontSonarData(idx) = sonar.Front(idx)
        '    Console.WriteLine("    [RIKNOTE] Agent\Behavior\Motions\CorridorWalk.vb::ProcessSonarData(), frontSonarData[{0}] = {1}", idx, sonar.Front(idx))
        ' Next idx
        ' For idx As Integer = 1 To rCount
        '    rearSonarData(idx) = sonar.Rear(idx)
        '    Console.WriteLine("    [RIKNOTE] Agent\Behavior\Motions\CorridorWalk.vb::ProcessSonarData(), rearSonarData[{0}] = {1}", idx, sonar.Rear(idx))
        ' Next idx

        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\CorridorWalk.vb::ProcessSonarData() called")
        'Console.WriteLine("    [RIKNOTE] Agent\Behavior\Motions\CorridorWalk.vb::ProcessSonarData(), fCount = {0}, rCount = {1}", fCount, rCount)

        'Should be depended on Me.Control.Agent.SonarSensor.Yaw, but currently not directly clear to me how.

        If Not IsNothing(Me.Control.Agent) AndAlso Me.Control.Agent.RobotModel.ToLower = "airrobot" Then
            Dim minF As Double = sonar.Front(3)
            minF = System.Math.Min(minF, sonar.Front(2))
            minF = System.Math.Min(minF, sonar.Front(4))

            'no rear sonar

            With sonar
                If ((Not isRetreating) And (minF < 0.2F Or .Front(3) > 0.6F Or .Front(2) > 0.6F Or .Front(3) < 0.15F Or .Front(2) < 0.15F)) Then
                    ' RIKNOTE: start retreating
                    ' RIKNOTE: in C++ clock() returns number of clock ticks elapsed since process start
                    ' but in VB this is problematic (needs the GetTickCount function from kernel32, etc)
                    ' so just convert the current system time to ticks instead (variable is still unused
                    ' anyway)
                    isRetreating = True
                    startRetreat = Now().Ticks()
                    'The AirRobot has only 5 sensors looking to the front. Other choices are possible
                End If
            End With
        ElseIf Not IsNothing(Me.Control.Agent) AndAlso Me.Control.Agent.RobotModel.ToLower = "atrvjr" Then
            Dim minF As Double = sonar.Front(4)
            minF = System.Math.Min(minF, sonar.Front(6))
            minF = System.Math.Min(minF, sonar.Front(8))
            minF = System.Math.Min(minF, sonar.Front(10))

            Dim minR As Double = sonar.Rear(1)
            minR = System.Math.Min(minR, sonar.Rear(2))
            minR = System.Math.Min(minR, sonar.Rear(3))
            minR = System.Math.Min(minR, sonar.Rear(4))

            With sonar
                If (isRetreating And (minR < 0.2F Or .Rear(1) > 0.6F Or .Rear(4) > 0.6F Or .Rear(1) < 0.15F Or .Rear(4) < 0.15F)) Then
                    ' RIKNOTE: stop retreating
                    isRetreating = False
                    justRetreated = True
                    'hope that this works for an ATVRJr: Rear 1 and 4 are looking with an angle of +/- 1.57 to the left/right
                    'with an P2AT, sonar Rear(3) and Rear(6) are looking with an angle of +/- 2.62
                    'Yet, there is no other choice, there are no sonar looking slightly to rear-left or right.
                End If
                If ((Not isRetreating) And (minF < 0.2F Or .Front(4) > 0.6F Or .Front(10) > 0.6F Or .Front(4) < 0.15F Or .Front(10) < 0.15F)) Then
                    ' RIKNOTE: start retreating
                    ' RIKNOTE: in C++ clock() returns number of clock ticks elapsed since process start
                    ' but in VB this is problematic (needs the GetTickCount function from kernel32, etc)
                    ' so just convert the current system time to ticks instead (variable is still unused
                    ' anyway)
                    isRetreating = True
                    startRetreat = Now().Ticks()
                    'The ATVRJr has many sensors looking to the front. Other choices are possible
                End If
            End With

        Else
            'default P2AT configuration
            Dim minF As Double = sonar.Front(2)
            minF = System.Math.Min(minF, sonar.Front(4))
            minF = System.Math.Min(minF, sonar.Front(5))
            minF = System.Math.Min(minF, sonar.Front(7))

            Dim minR As Double = sonar.Rear(2)
            minR = System.Math.Min(minR, sonar.Rear(4))
            minR = System.Math.Min(minR, sonar.Rear(5))
            minR = System.Math.Min(minR, sonar.Rear(7))



            With sonar
                If (isRetreating And (minR < 0.2F Or .Rear(3) > 0.6F Or .Rear(6) > 0.6F Or .Rear(3) < 0.15F Or .Rear(6) < 0.15F)) Then
                    ' RIKNOTE: stop retreating
                    isRetreating = False
                    justRetreated = True
                End If
                If ((Not isRetreating) And (minF < 0.2F Or .Front(3) > 0.6F Or .Front(6) > 0.6F Or .Front(3) < 0.15F Or .Front(6) < 0.15F)) Then
                    ' RIKNOTE: start retreating
                    ' RIKNOTE: in C++ clock() returns number of clock ticks elapsed since process start
                    ' but in VB this is problematic (needs the GetTickCount function from kernel32, etc)
                    ' so just convert the current system time to ticks instead (variable is still unused
                    ' anyway)
                    isRetreating = True
                    startRetreat = Now().Ticks()
                End If
            End With
        End If
    End Sub

    ' RIKNOTE: original C++ implementation of ProcessSonarData()
    'void MotionExplore::process(const SonarData& sonar) {
    '   // qDebug() << "sonar F: " << sonar.F1 << sonar.F2 << sonar.F3 << sonar.F4 << sonar.F5 << sonar.F6 << sonar.F7 << sonar.F8;
    '   // qDebug() << "sonar R: " << sonar.R1 << sonar.R2 << sonar.R3 << sonar.R4 << sonar.R5 << sonar.R6 << sonar.R7 << sonar.R8;
    '
    '   double minF = sonar.F2;
    '   if (sonar.F4 < minF) minF = sonar.F4;
    '   if (sonar.F5 < minF) minF = sonar.F5;
    '   if (sonar.F7 < minF) minF = sonar.F7;
    '
    '   double minR = sonar.R2;
    '   if (sonar.R4 < minR) minR = sonar.R4;
    '   if (sonar.R5 < minR) minR = sonar.R5;
    '   if (sonar.R7 < minR) minR = sonar.R7;
    '
    '   if (isRetreating && (minR < .2 || sonar.R3 > .6 || sonar.R6 > .6 || sonar.R3 < .15 || sonar.R6 < .15)) {
    '       // stop retreating and turn
    '       isRetreating = false;
    '       justRetreated = true;
    '
    '       //qDebug() << "******* SONAR stopped retreat";
    '   }
    '
    '   if (!isRetreating && (minF < .2 || sonar.F3 > .6 || sonar.F6 > .6 || sonar.F3 < .15 || sonar.F6 < .15)) {
    '       isRetreating = true;
    '       startRetreat = clock();
    '
    '       qDebug() << "******* SONAR initiated retreat";
    '   }
    '
    '   this->move();
    '}

End Class
