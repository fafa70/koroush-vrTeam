Imports System.Math
Imports UvARescue.Math

Public Class RandomWalk_Following
    Inherits Motion

    Protected random As New System.Random(DateTime.Now.Millisecond)

    Protected Enum Movement
        None
        HeadNorth
        TurnEast
        TurnWest
        Retreat

    End Enum

    'constructor
    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        Me._CurrentMovement = Movement.None
    End Sub

    Protected Overrides Sub OnDeActivated()
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
                        'Console.WriteLine("RandomWalk_Following: calculating distance to myself")
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
                        Me.Control.ActivateMotion(MotionType.ObstacleAvoidance_Following, True)
                    Else
                        Me.Control.ActivateMotion(MotionType.AvoidTeamMate_Following, True)
                    End If
                    Exit Sub
                End If
            Next
        End With
    End Sub

    Protected Overrides Sub ProcessSensorUpdate(ByVal sensor As Sensor)
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\RandomWalk.vb::ProcessSensorUpdate() called")
        MyBase.ProcessSensorUpdate(sensor)

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

                Me.Control.ActivateMotion(MotionType.AvoidVictim_Following, True) 'explicit return, to ExploreFrontier

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
            Console.WriteLine(String.Format("[RandomWalk_Following] - Something: tilting ({0:f2} , {1:f2}) rad -> Retreat", NormalizeAngle(current_data.Roll), NormalizeAngle(current_data.Pitch)))

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


        Dim fovNorth As Double = 120 / 180 * PI 'frontal 120 degrees
        Dim fovWest As Double = 60 / 180 * PI 'left 60 degrees
        Dim fovEast As Double = 60 / 180 * PI 'right 60 degrees

        Dim minNorth As Double = Double.MaxValue
        Dim avgWest As Double = 0
        Dim avgEast As Double = 0


        'helper vars
        Dim length As Integer = laser.Range.Length - 1
        Dim start As Double
        Dim until As Double
        Dim count As Integer


        'determine min distance in northern direction
        start = (laser.FieldOfView - fovNorth) / 2
        until = laser.FieldOfView - start
        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            minNorth = Min(minNorth, laser.Range(i))
        Next

        'determine average dist on the left-front side
        start = (laser.FieldOfView - PI) / 2
        until = start + fovWest
        count = 0
        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            avgWest += laser.Range(i)
            count += 1
        Next
        avgWest /= count


        'determine average dist on the right-front side
        until = laser.FieldOfView - (laser.FieldOfView - PI) / 2
        start = until - fovEast
        count = 0
        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            avgEast += laser.Range(i)
            count += 1
        Next
        avgEast /= count

        Dim canGoNorth As Boolean = minNorth > 0.55
        Dim westLooksPromising As Boolean
        Dim eastLooksPromising As Boolean

        If avgEast > 0.3 AndAlso avgWest > 0.3 Then
            eastLooksPromising = avgEast > avgWest
            westLooksPromising = avgWest > avgEast
        Else
            eastLooksPromising = avgEast > 0.3
            westLooksPromising = avgWest > 0.3
        End If

        If canGoNorth Then
            Me._CurrentMovement = Movement.HeadNorth
        ElseIf westLooksPromising AndAlso eastLooksPromising Then
            If random.Next(0, 1) = 0 Then
                Me._CurrentMovement = Movement.TurnWest
            Else
                Me._CurrentMovement = Movement.TurnEast
            End If
        ElseIf westLooksPromising Then
            Me._CurrentMovement = Movement.TurnWest
        ElseIf eastLooksPromising Then
            Me._CurrentMovement = Movement.TurnEast
        Else
            'Retreat is not an option
            Me._CurrentMovement = Movement.None
            'Do something smart
            Console.WriteLine(String.Format("[RandomWalk_Following] No options left (minNorth {0} m, avgEast {1} m, avgEast {2} m)", minNorth, avgEast, avgEast))
            Me.Control.ActivateMotion(MotionType.ObstacleAvoidance_Following, True)
        End If

        With Me.Control.Agent
            Select Case Me._CurrentMovement

                Case Movement.None
                    .Halt()

                Case Movement.HeadNorth
                    .Drive(1.0F)

                Case Movement.TurnWest
                    .TurnLeft(0.4F)
                Case Movement.TurnEast
                    .TurnRight(0.4F)

                Case Movement.Retreat
                    .Reverse(0.5F)

            End Select

        End With

    End Sub

    Protected Overridable Sub ProcessSonarData(ByVal sonar As SonarData)
    End Sub

    Protected Overridable Sub ProcessInsData(ByVal ins As InsData)
    End Sub

End Class
