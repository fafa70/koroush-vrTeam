' JDHNOTE:  Created 13 May 2008 by Julian de Hoog
'           Extended 10 June 2008 by JDH
'           Committed 13 June 2008 by JDH v 1273

Imports System.Math
Imports UvARescue.Math

Public Class AvoidTeamMate
    Inherits Motion

    Private Enum Quadrant
        FrontRight
        FrontLeft
        RearRight
        RearLeft
    End Enum

    ' unit conversion constants
    Private ReadOnly _RAD2DEG As Double = 180.0 / PI
    Private ReadOnly _DEG2RAD As Double = PI / 180.0


    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
    End Sub

    Protected Overrides Sub OnActivated()
        Console.WriteLine("[JDHNOTE] Agent\Behavior\Motions\AvoidTeamMate.vb::OnActivated() called")
        MyBase.OnActivated()
    End Sub

    Protected Overrides Sub OnDeActivated()
        Console.WriteLine("[JDHNOTE] Agent\Behavior\Motions\AvoidTeamMate.vb::OnDeActivated() called")
        MyBase.OnDeActivated()
    End Sub



    Protected Overrides Sub ProcessSensorUpdate(ByVal sensor As Sensor)
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
    End Sub

    Protected Overridable Sub ProcessSonarData(ByVal sonar As SonarData)
        'Checks whether I am actually too close to a TeamMate using sonar, not poseUpdates
        'If yes, and if I am in the way, then avoids TeamMate by getting out of the way.

        Dim distToRobot As Double = Double.MaxValue     'Distance between me and robot to be avoided
        Dim avoidRobotNum As Integer                    'Number of robot to be avoided
        Dim avoidRobotPose As Pose2D = New Pose2D       'Pose2D of robot to be avoided
        Dim foundRobot As Boolean = False

        Dim avoidRobotQuadrant As Quadrant          'Quadrant that robot to be avoided is in, relative to my rotation

        Dim angleMeToOther As Double                'Angle of the vector from Me to TeamMate
        Dim angleMyRotToOther As Double             'Angle between my rotation and vector from me to TeamMate
        Dim angleOtherToMe As Double                'Angle of vector from TeamMate to Me
        Dim angleOtherRotToMe As Double             'Angle between TeamMate's rotation and vector from him to me
        Dim closestSonarReading As Double

        With Me.Control.Agent
            'Determine which robot is to be avoided according to PoseUpdates (we can ignore ComStation)
            For i As Integer = 1 To (.Number - 1)
                'Calculate distance between the two robots
                If ._TeamPoses.ContainsKey(._TeamMembers(i)) Then
                    avoidRobotPose = ._TeamPoses.Item(._TeamMembers(i))
                    distToRobot = ((.CurrentPoseEstimate.X - avoidRobotPose.X) ^ 2 + (.CurrentPoseEstimate.Y - avoidRobotPose.Y) ^ 2) ^ 0.5
                End If

                If (distToRobot < 1000) Then
                    'Determine number of robot to be avoided
                    foundRobot = True
                    avoidRobotNum = i
                    Exit For
                End If
            Next

            'If no robot to be avoided is found, then return to frontier exploration
            If (Not foundRobot) Then
                Me.Control.ActivateMotion(MotionType.FrontierExploration, True)
                Exit Sub
            End If

            'Determine some useful angles
            angleMeToOther = Atan2(avoidRobotPose.Y - .CurrentPoseEstimate.Y, avoidRobotPose.X - .CurrentPoseEstimate.X)
            angleMyRotToOther = angleMeToOther - .CurrentPoseEstimate.Rotation
            'Normalize angle to range (-pi pi]
            While (angleMyRotToOther > PI)
                angleMyRotToOther -= (2 * PI)
            End While
            While (angleMyRotToOther <= -PI)
                angleMyRotToOther += (2 * PI)
            End While
            angleOtherToMe = Atan2(.CurrentPoseEstimate.Y - avoidRobotPose.Y, .CurrentPoseEstimate.X - avoidRobotPose.X)
            angleOtherRotToMe = angleOtherToMe - avoidRobotPose.Rotation
            'Normalize angle to range (-pi pi]
            While (angleOtherRotToMe > PI)
                angleOtherRotToMe -= (2 * PI)
            End While
            While (angleOtherRotToMe <= -PI)
                angleOtherRotToMe += (2 * PI)
            End While

            'To get a sense of distance to robot that's more accurate than the PoseUpdates,
            'use the sonar sensors on the side where we think the avoidable robot is
            closestSonarReading = Double.MaxValue

            '(NOTE! Coordinate system in USARSim seems to be flipped from standard math - x axis is backward)
            'Check my back right quadrant
            If (angleMyRotToOther > PI / 2) Then
                avoidRobotQuadrant = Quadrant.RearRight
                For i As Integer = 1 To sonar.RearCount
                    If (Me.Control.Agent.SonarSensor.RearYaw(i) > +1.57 AndAlso sonar.Rear(i) < closestSonarReading) Then
                        closestSonarReading = sonar.Rear(i)
                    End If
                Next

                'check my front right quadrant
            ElseIf (angleMyRotToOther > 0) Then
                avoidRobotQuadrant = Quadrant.FrontRight
                For i As Integer = 1 To sonar.FrontCount
                    If (Me.Control.Agent.SonarSensor.FrontYaw(i) > 0.0 AndAlso sonar.Front(i) < closestSonarReading) Then
                        closestSonarReading = sonar.Front(i)
                    End If
                Next

                'check my front left quadrant
            ElseIf (angleMyRotToOther > -PI / 2) Then
                avoidRobotQuadrant = Quadrant.FrontLeft
                For i As Integer = 1 To sonar.FrontCount
                    If (Me.Control.Agent.SonarSensor.FrontYaw(i) < 0.0 AndAlso sonar.Front(i) < closestSonarReading) Then
                        closestSonarReading = sonar.Front(i)
                    End If
                Next

                'Check my back left quadrant
            Else
                avoidRobotQuadrant = Quadrant.RearLeft
                For i As Integer = 1 To sonar.RearCount
                    If (Me.Control.Agent.SonarSensor.RearYaw(i) < -1.57 AndAlso sonar.Rear(i) < closestSonarReading) Then
                        closestSonarReading = sonar.Rear(i)
                    End If
                Next
            End If

            'If the TeamMate is far enough away according to sonar, proceed with frontier exploration
            If (closestSonarReading > 2.0) Then
                Me.Control.ActivateMotion(MotionType.FrontierExploration, True)
                Exit Sub
            End If

            'If this point reached, we know TeamMate is close and must be avoided.

            'If robot to be avoided is ahead of me
            '  if facing me, then get out of way,
            '  if not facing me, wait until it's safely far away.
            If (avoidRobotQuadrant = Quadrant.FrontLeft) Or (avoidRobotQuadrant = Quadrant.FrontRight) Then
                If (angleOtherRotToMe > PI / 2) Then
                    .Halt()
                ElseIf (angleOtherRotToMe > 0) Then
                    .DifferentialDrive(-1.2F, -0.8F)
                ElseIf (angleOtherRotToMe > -PI / 2) Then
                    .DifferentialDrive(-1.2F, 0.8F)
                Else
                    .Halt()
                End If

                'If robot to be avoided is behind me
                '  if facing me, then get out of way,
                '  if not facing me, proceed as normal with frontier exploration.
            Else
                If (angleOtherRotToMe > PI / 2) Then
                    Me.Control.ActivateMotion(MotionType.FrontierExploration, True)
                ElseIf (angleOtherRotToMe > 0) Then
                    .DifferentialDrive(1.2F, -0.8F)
                ElseIf (angleOtherRotToMe > -PI / 2) Then
                    .DifferentialDrive(1.2F, 0.8F)
                Else
                    Me.Control.ActivateMotion(MotionType.FrontierExploration, True)
                End If

            End If
        End With
    End Sub

    Protected Overridable Sub ProcessGroundTruthData(ByVal current_data As GroundTruthData)
    End Sub

    Protected Overridable Sub ProcessInsData(ByVal ins As InsData)
    End Sub
End Class
