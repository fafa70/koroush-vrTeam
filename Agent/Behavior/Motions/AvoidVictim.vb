' Arnoud:  Created 29 July 2008 by Arnoud Visser
'           Inspired by AvoidTeamMate by JDH


Imports System.Math
Imports UvARescue.Math

Public Class AvoidVictim
    Inherits Motion

    Protected Enum Movement
        None
        HeadNorth
        TurnEast
        TurnWest
        Retreat

    End Enum

    ' unit conversion constants
    Private ReadOnly _RAD2DEG As Double = 180.0 / PI
    Private ReadOnly _DEG2RAD As Double = PI / 180.0

    Private AvoidLeft As Double = -PI / 2
    Private AvoidRight As Double = +PI / 2


    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
    End Sub

    Private _CurrentMovement As Movement


    Protected Overrides Sub OnActivated()
        Console.WriteLine("[AvoidVictim::OnActivated() called")
        MyBase.OnActivated()
        Me._CurrentMovement = Movement.None
    End Sub

    Protected Overrides Sub OnDeActivated()
        Console.WriteLine("[AvoidVictim::OnDeActivated() called")
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
        ElseIf sensor.SensorType = GroundTruthSensor.SENSORTYPE_GROUNDTRUTH Then
            Me.ProcessGroundTruthData(DirectCast(sensor, GroundTruthSensor).CurrentData)
        ElseIf sensor.SensorType = VictimSensor.SENSORTYPE_VICTIM Then
            Me.ProcessVictimData(DirectCast(sensor, VictimSensor).CurrentData)
        End If
    End Sub

    Protected Overridable Sub ProcessVictimData(ByVal current_data As VictimData)


        'Check whether robot is too close to a Victim
        Dim distToPart As Double = Double.MaxValue
        Dim distToVictim As Double = Double.MaxValue
        Dim pose As Pose2D = Me.Control.Agent.CurrentPoseEstimate

        Dim factor As Single = 1000 'to convert from m to mm

        Dim angleMeToOther As Double                'Angle of the vector from Me to NearestPart
        Dim angleMyRotToOther As Double             'Angle between my rotation and vector from me to NearestPart


        With Me.Control.Agent

            For Each part As VictimData.VictimPart In current_data.Parts

                If part.PartName = "chest" Or part.PartName = "head" Or part.PartName = "leg" Or part.PartName = "hand" Then

                    'note that the part is observed in local coords wrt the current pose of the robot
                    'transform to global coordinate wrt pose using a Pose2D object with dummy rotation
                    Dim pglobal As Vector2 = New Pose2D(CType(part.X * factor, Single), CType(part.Y * factor, Single), 0).ToGlobal(pose).Position

                    distToPart = ((pose.X - pglobal.X) ^ 2 + (pose.Y - pglobal.Y) ^ 2) ^ 0.5

                    Console.WriteLine(String.Format("[AvoidVictim::VictimPart {0} at {1} m", part.PartName, distToPart / 1000))

                    If distToPart < distToVictim Then
                        distToVictim = distToPart
                    End If
                    If distToPart < 2000 Then
                        'Determine some useful angles
                        angleMeToOther = Atan2(pglobal.Y - .CurrentPoseEstimate.Y, pglobal.X - .CurrentPoseEstimate.X)
                        angleMyRotToOther = angleMeToOther - .CurrentPoseEstimate.Rotation
                        'Normalize angle to range (-pi pi]
                        While (angleMyRotToOther > PI)
                            angleMyRotToOther -= (2 * PI)
                        End While
                        While (angleMyRotToOther <= -PI)
                            angleMyRotToOther += (2 * PI)
                        End While
                        If angleMyRotToOther + PI / 36 > AvoidLeft Then
                            AvoidLeft = angleMyRotToOther + PI
                        End If
                        If angleMyRotToOther - PI / 36 > AvoidRight Then
                            AvoidRight = angleMyRotToOther + PI
                        End If
                    End If

                End If

            Next

            If (Not distToVictim = Double.MaxValue AndAlso distToVictim > 2000) Then
                'Far enough, back to FrontierExploration
                Me.Control.Agent.Halt()
                Me.Control.ActivateMotion(MotionType.FrontierExploration, True)
            End If


        End With

    End Sub
    Protected Overridable Sub ProcessLaserRangeData(ByVal sensorName As String, ByVal laser As LaserRangeData)
        'It is possible that a sensor is mounted on a tilt.  Not interested in that here.
        If (sensorName = "TiltedScanner") Then
            Exit Sub
        End If



        Dim fovWest As Double = 60 / 180 * PI 'left 60 degrees
        Dim fovEast As Double = 60 / 180 * PI 'right 60 degrees

        Dim avgWest As Double = 0
        Dim avgEast As Double = 0


        'helper vars
        Dim length As Integer = laser.Range.Length - 1
        Dim start As Double
        Dim until As Double
        Dim count As Integer


        'determine min distance in both direction
        start = laser.FieldOfView / 2 + AvoidLeft
        until = Min(laser.FieldOfView, start + fovWest)
        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            avgWest += laser.Range(i)
            count += 1
        Next
        avgWest /= count

        until = laser.FieldOfView / 2 + AvoidRight
        start = Max(0, until - fovEast)
        For i As Integer = Max(0, CInt(start / laser.Resolution)) To Min(CInt(until / laser.Resolution), length)
            avgEast += laser.Range(i)
            count += 1
        Next
        avgEast /= count

        Dim westLooksPromising As Boolean
        Dim eastLooksPromising As Boolean

        If avgEast > 0.3 AndAlso avgWest > 0.3 Then
            eastLooksPromising = avgEast > avgWest
            westLooksPromising = avgWest > avgEast
        Else
            eastLooksPromising = avgEast > 0.3
            westLooksPromising = avgWest > 0.3
        End If

        If westLooksPromising AndAlso eastLooksPromising Then
            If Abs(AvoidLeft) < Abs(AvoidRight) Then 'make the smallest turn
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
            Me.Control.ActivateMotion(MotionType.ObstacleAvoidance, True)
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

    Protected Overridable Sub ProcessGroundTruthData(ByVal current_data As GroundTruthData)
    End Sub

    Protected Overridable Sub ProcessInsData(ByVal ins As InsData)
    End Sub
End Class
