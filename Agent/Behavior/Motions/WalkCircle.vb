Imports System.Math

Imports UvARescue.Math

Public Class WalkCircle
    Inherits Motion

    Protected random As New System.Random(DateTime.Now.Millisecond)

    Protected Enum Movement

        None

        HeadNorth
        TurnEast
        TurnWest
        Retreat

    End Enum


    Private _CurrentMovement As Movement

    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        Me._CurrentMovement = Movement.TurnEast
        Me.Control.Agent.DifferentialDrive(2.0F, CType(Me.Control.Agent.BehaviorBalance, Single))

    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        Me._CurrentMovement = Movement.None
        Me.Control.Agent.Halt()
    End Sub

    Protected Overrides Sub ProcessPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        MyBase.ProcessPoseEstimateUpdated(pose)

        With Me.Control.Agent
            Select Case Me._CurrentMovement

                Case Movement.None
                    .Halt()

                Case Movement.HeadNorth
                    .Drive(1.0F)

                Case Movement.TurnWest
                    .DifferentialDrive(2.0F, -CType(Me.Control.Agent.BehaviorBalance, Single))
                Case Movement.TurnEast
                    .DifferentialDrive(2.0F, +CType(Me.Control.Agent.BehaviorBalance, Single))

                Case Movement.Retreat
                    .Reverse(0.5F)

            End Select

        End With

    End Sub
    Protected Overrides Sub ProcessSensorUpdate(ByVal sensor As Sensor)
        MyBase.ProcessSensorUpdate(sensor)

        If sensor.SensorType = LaserRangeSensor.SENSORTYPE_RANGESCANNER Then
            Me.ProcessLaserRangeData(DirectCast(sensor, LaserRangeSensor).PeekData)
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


    Protected Overridable Sub ProcessLaserRangeData(ByVal laser As LaserRangeData)
    End Sub

    Protected Overridable Sub ProcessSonarData(ByVal sonar As SonarData)
    End Sub


    Protected Overridable Sub ProcessOdometryData(ByVal sensed As OdometryData)
    End Sub

    Protected Overridable Sub ProcessInsData(ByVal ins As InsData)
    End Sub

    Protected Overridable Sub ProcessGroundTruthData(ByVal sensed As GroundTruthData)
        Dim pose As New Pose2D(sensed.X, sensed.Y, sensed.Yaw)
        Console.WriteLine(String.Format("[WalkCircle] GroundTruth {0:f2},{1:f2},{2:f2}", pose.X, pose.Y, pose.Rotation))
    End Sub

    Protected Overridable Sub ProcessCameraData(ByVal camData As CameraData)
    End Sub

End Class
