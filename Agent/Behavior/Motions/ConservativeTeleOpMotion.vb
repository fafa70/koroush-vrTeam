Imports system.Math

''' <summary>
''' Motion for Conservative TeleOp behavior
''' </summary>
''' <remarks></remarks>

Public Class ConservativeTeleOpMotion
    Inherits Motion

    Private AlertObstacle As Boolean = False
    Private AlertIncline As Boolean = False

    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        Me.Control.Agent.Halt()
    End Sub

    Protected Overrides Sub ProcessSensorUpdate(ByVal sensor As Sensor)
        'Console.WriteLine("teloptmot ProcessSensorUpdate {0}", Date.Now)

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
        Dim elapsedSinceLastAlert As TimeSpan = DateTime.Now - Me.Control.Agent.MotionTimeStamp

        'It is possible that a sensor is mounted on a tilt.  We can use that to detect obstacles that the sick
        'can't find, and also to detect holes / cliff-like drops.
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
                    Me.AlertObstacle = True
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
            Me.AlertObstacle = True
            Dim alert As String = "LSR - OBS - " + minFrontRange.ToString
            Me.Control.Agent.NotifyAlertReceived(alert)
        End While
    End Sub

    Protected Overridable Sub ProcessSonarData(ByVal sonar As SonarData)
        'Should check if a LaserRange is mounted
        If Me.Control.Agent.IsMounted(LaserRangeSensor.SENSORTYPE_RANGESCANNER) Then
            ' No halt on sonar when a more reliable info is present
            ' Maybe do a check for FOV!
            Return
        End If

        Dim minFrontRange As Double = Double.MaxValue
        Dim maxRange As Double = 0.0 'If not moving, no Halt

        If Me.Control.Agent.ForwardSpeed > 0 Then
            maxRange = Abs(Me.Control.Agent.ForwardSpeed) 'Default 2 m / s

        For i As Integer = 1 To sonar.FrontCount
            If Me.Control.Agent.SonarSensor.FrontYaw(i) > -0.524 AndAlso Me.Control.Agent.SonarSensor.FrontYaw(i) < +0.524 Then
                minFrontRange = Min(minFrontRange, sonar.Front(i))
                    If Me.Control.Agent.RobotModel.ToLower = "airrobot" Then
                        maxRange = Me.Control.Agent.SonarSensor.FrontMaxRange(i) - 1.2
            End If
                End If
        Next

        ElseIf Me.Control.Agent.StrafeSpeed > 0 Then 'At the moment, only for AirRobot can Strafe

            maxRange = Abs(Me.Control.Agent.StrafeSpeed) 'Default 2 m / s

            For i As Integer = 1 To sonar.FrontCount
                If Me.Control.Agent.SonarSensor.FrontYaw(i) > +0.524 Then 'Going to the left
                    minFrontRange = Min(minFrontRange, sonar.Front(i))
                    If Me.Control.Agent.RobotModel.ToLower = "airrobot" Then
                        maxRange = Me.Control.Agent.SonarSensor.FrontMaxRange(i) - 1.2
                    End If

                End If
            Next

        ElseIf Me.Control.Agent.StrafeSpeed < 0 Then

            maxRange = Abs(Me.Control.Agent.StrafeSpeed) 'Default 2 m / s

            For i As Integer = 1 To sonar.FrontCount
                If Me.Control.Agent.SonarSensor.FrontYaw(i) < -0.524 Then 'Going to the right
                    minFrontRange = Min(minFrontRange, sonar.Front(i))
                    If Me.Control.Agent.RobotModel.ToLower = "airrobot" Then
                        maxRange = Me.Control.Agent.SonarSensor.FrontMaxRange(i) - 1.2
                    End If

                End If
            Next

        ElseIf Me.Control.Agent.FlySpeed > 0 Then 'For sure, only for AirRobot can Fly

            maxRange = Abs(Me.Control.Agent.FlySpeed) 'Default 2 m / s

            For i As Integer = 1 To sonar.FrontCount
                If Me.Control.Agent.SonarSensor.FrontPitch(i) > PI / 4 Then 'Looking up
                    minFrontRange = Min(minFrontRange, sonar.Front(i))
                    maxRange = Me.Control.Agent.SonarSensor.FrontMaxRange(i) - 1.2
                End If
            Next

        ElseIf Me.Control.Agent.FlySpeed < 0 Then

            maxRange = Abs(Me.Control.Agent.FlySpeed) 'Default 2 m / s

            For i As Integer = 1 To sonar.FrontCount
                If Me.Control.Agent.SonarSensor.FrontPitch(i) < -(PI / 4) Then 'Looking down
                    minFrontRange = Min(minFrontRange, sonar.Front(i))
                    maxRange = Me.Control.Agent.SonarSensor.FrontMaxRange(i) - 1.2
                End If
            Next

        End If


        Dim elapsedSinceLastAlert As TimeSpan = DateTime.Now - Me.Control.Agent.MotionTimeStamp

        If minFrontRange < maxRange AndAlso elapsedSinceLastAlert.Seconds > 2 Then
            Me.Control.Agent.Halt()
            Me.AlertObstacle = True
            Dim alert As String = "SNR - OBS - " + minFrontRange.ToString
            Me.Control.Agent.NotifyAlertReceived(alert)
        End If
    End Sub

    Protected Overridable Sub ProcessGroundTruthData(ByVal current_data As GroundTruthData)
    End Sub

    Protected Overridable Sub ProcessInsData(ByVal ins As InsData)
    End Sub

End Class
