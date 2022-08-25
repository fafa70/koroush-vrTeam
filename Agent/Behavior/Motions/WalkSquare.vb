Imports System.Math

Imports UvARescue.Math

Public Class WalkSquare
    Inherits Motion

    Protected random As New System.Random(DateTime.Now.Millisecond)

    Protected Enum Movement

        None

        HeadNorth
        TurnEast
        TurnWest
        Retreat

    End Enum


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

    Protected Overrides Sub ProcessSensorUpdate(ByVal sensor As Sensor)
        MyBase.ProcessSensorUpdate(sensor)

        If sensor.SensorType = LaserRangeSensor.SENSORTYPE_RANGESCANNER Then
            Me.ProcessLaserRangeData(sensor.SensorName, DirectCast(sensor, LaserRangeSensor).PeekData)

        ElseIf sensor.SensorType = SonarSensor.SENSORTYPE_SONAR Then
            Me.ProcessSonarData(DirectCast(sensor, SonarSensor).CurrentData)

        ElseIf sensor.SensorType = InsSensor.SENSORTYPE_INS Then
            Me.ProcessInsData(DirectCast(sensor, InsSensor).CurrentData)

        ElseIf sensor.SensorType = OdometrySensor.SENSORTYPE_ODOMETRY Then
            Me.ProcessOdometryData(DirectCast(sensor, OdometrySensor).CurrentData)

        ElseIf sensor.SensorType = GroundTruthSensor.SENSORTYPE_GROUNDTRUTH Then
            Me.ProcessGroundTruthData(DirectCast(sensor, GroundTruthSensor).CurrentData)

        ElseIf sensor.SensorType = CameraSensor.SENSORTYPE_CAMERA Then
            Me.ProcessCameraData(DirectCast(sensor, CameraSensor).PopData)

        End If

    End Sub

    Private _CurrentMovement As Movement

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
        Else
            Me._CurrentMovement = Movement.TurnEast
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

    Private _CurrentInit As Vector2
    Private _CurrentGoal As Vector2
    'Private _CurrentOrientation As Double

    Protected Overridable Sub ProcessOdometryData(ByVal sensed As OdometryData)
        'check if we should select a new target
        Dim bNewTarget As Boolean = False
        Dim bNewPlan As Boolean = False
        Dim pose As New Pose2D(sensed.X, sensed.Y, sensed.Theta)

        If IsNothing(Me._CurrentGoal) Then
            'we don't have a target yet
            bNewTarget = True
            '            'Me._CurrentOrientation = pose.Rotation
        Else
            'check if we are done with the current target
            Dim tdpose As Vector2 = Me._CurrentGoal - pose.Position
            Dim tdist As Double = (tdpose.X ^ 2 + tdpose.Y ^ 2) ^ 0.5
            bNewTarget = tdist < 0.1 '10cm near goal

            'compute displacement from where I started on this path
            Dim idpose As Vector2 = pose.Position - Me._CurrentInit
            Dim idist As Double = (idpose.X ^ 2 + idpose.Y ^ 2) ^ 0.5
            bNewPlan = idist > 2 '2 m travelled

        End If

        If bNewTarget Or bNewPlan Then

            With Me.Control.Agent
                .Halt()
            End With

            If IsNothing(Me._CurrentGoal) Then

                Me._CurrentGoal = New Vector2(2, 0)
                ' first time straight ahead
                'If pose.GetNormalizedRotation > -0.7854 And pose.GetNormalizedRotation <= +0.7854 Then
                'Me._CurrentGoal = New Vector2(pose.Position.X + 2, pose.Position.Y)
                'ElseIf pose.GetNormalizedRotation > +0.7854 And pose.GetNormalizedRotation <= +2.3562 Then
                'Me._CurrentGoal = New Vector2(pose.Position.X, pose.Position.Y + 2)
                'ElseIf pose.GetNormalizedRotation <= -0.7854 And pose.GetNormalizedRotation > -2.3562 Then
                'Me._CurrentGoal = New Vector2(pose.Position.X, pose.Position.Y - 2)
                'Else 'If pose.GetNormalizedRotation > +2.3562 Or pose.GetNormalizedRotation <= -2.3562 Then
                'Me._CurrentGoal = New Vector2(pose.Position.X - 2, pose.Position.Y)
                'End If
            Else
                If Me._CurrentGoal.X = 2 And Me._CurrentGoal.Y = 0 Then
                    Me._CurrentGoal = New Vector2(2, 2)
                ElseIf Me._CurrentGoal.X = 2 And Me._CurrentGoal.Y = 2 Then
                    Me._CurrentGoal = New Vector2(0, 2)
                ElseIf Me._CurrentGoal.X = 0 And Me._CurrentGoal.Y = 2 Then
                    Me._CurrentGoal = New Vector2(0, 0)
                Else 'If Me._CurrentGoal.X = 0 And Me._CurrentGoal.Y = 0 Then
                    Me._CurrentGoal = New Vector2(2, 0)

                    ' turn 90 degrees
                    'If pose.GetNormalizedRotation > -0.7854 And pose.GetNormalizedRotation <= +0.7854 Then
                    'Me._CurrentGoal = New Vector2(pose.Position.X, pose.Position.Y + 2)
                    'ElseIf pose.GetNormalizedRotation > +0.7854 And pose.GetNormalizedRotation <= +2.3562 Then
                    'Me._CurrentGoal = New Vector2(pose.Position.X - 2, pose.Position.Y)
                    'ElseIf pose.GetNormalizedRotation <= -0.7854 And pose.GetNormalizedRotation > -2.3562 Then
                    'Me._CurrentGoal = New Vector2(pose.Position.X + 2, pose.Position.Y)
                    'Else 'If pose.GetNormalizedRotation > +2.3562 Or pose.GetNormalizedRotation <= -2.3562 Then
                    'Me._CurrentGoal = New Vector2(pose.Position.X, pose.Position.Y - 2)
                End If

            End If
            Me._CurrentInit = New Vector2(pose.Position.X, pose.Position.Y)
            Console.WriteLine(String.Format("[WalkSquare] Reached {0:f2},{1:f2}", Me._CurrentInit.X, Me._CurrentInit.Y))

            Console.WriteLine(String.Format("[WalkSquare] NewGoal {0:f2},{1:f2}", Me._CurrentGoal.X, Me._CurrentGoal.Y))

        End If

        Dim dgoal As Vector2 = Me._CurrentGoal - pose.Position

        Dim dradians As Double = Atan2(dgoal.Y, dgoal.X) - pose.Rotation
        Dim dangle As Double = dradians / PI * 180

        While dangle >= 180
            dangle -= 360
        End While
        While dangle < -180
            dangle += 360
        End While

        With Me.Control.Agent
            If Abs(dangle) < 5 Then
                .Drive(1.5F)
            ElseIf dangle > 0 Then
                .TurnRight(0.3)
                Console.WriteLine(String.Format("[WalkSquare] TurnRight {0:f2}", dangle))

            ElseIf dangle < 0 Then
                .TurnLeft(0.3)
                Console.WriteLine(String.Format("[WalkSquare] TurnLeft {0:f2}", dangle))

            End If
        End With


    End Sub

    Protected Overridable Sub ProcessInsData(ByVal ins As InsData)
    End Sub

    Protected Overridable Sub ProcessGroundTruthData(ByVal sensed As GroundTruthData)
        Dim pose As New Pose2D(sensed.X, sensed.Y, sensed.Yaw)
        Console.WriteLine(String.Format("[WalkSquare] GroundTruth {0:f2},{1:f2},{2:f2}", pose.X, pose.Y, pose.Rotation))
    End Sub

    Protected Overridable Sub ProcessCameraData(ByVal camData As CameraData)
        ' process one out of ten images
        If camData.Sequence Mod 10 = 0 Then
            Dim red, green, blue As Byte
            Dim luma As Byte
            Dim index As Integer = 300
            Dim imLength As Integer = CInt(camData.RawData.Length / 3)

            red = camData.RawData(index * 3)
            green = camData.RawData(index * 3 + 1)
            blue = camData.RawData(index * 3 + 2)

            luma = CByte(0.299 * red + 0.587 * green + 0.144 * blue) 'luma is Y in YUV colorspace

            Console.WriteLine(String.Format("[WalkSquare] luma {0}", luma))


        End If
    End Sub

End Class
