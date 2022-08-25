Imports UvARescue.Math
Imports UvARescue.Communication
Imports System.Math
Public Class semiBehaviour
    Inherits Motion
    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
    End Sub
    
    Private currPos As Pose2D
    Private goalAngle As Double
    Private robotAngle As Double

    

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        With Control.Agent
            .Drive(1.5F)
        End With
        'Me.Control.ActivateMotion(MotionType.CorridorWalk, True)
    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        Console.WriteLine("semi is deactivated")
        Me.Control.ActivateMotion(MotionType.NoMotion, True)

    End Sub

    Private Sub moveTo(ByVal pose As Pose2D, ByVal agentName As String)
        Dim distance As Double = Double.MaxValue
        goalAngle = Atan2(Me.currPos.Y, Me.currPos.X)
        robotAngle = currPos.Rotation
        With Me.Control.Agent
            Me.currPos = ._TeamPoses(agentName)
            distance = ((Me.currPos.X - pose.X) ^ 2 + (Me.currPos.Y - pose.Y) ^ 2) ^ 0.5

        End With
        If (distance < 1000) Then
            Console.WriteLine("semiBehaviour::stop")
            Me.Control.ActivateMotion(MotionType.NoMotion, True)
        Else
            Me.walkto(goalAngle)
        End If

    End Sub


    Private Sub walkto(ByVal angle As Double)
        Dim drobotAngle As Double = robotAngle / PI * 180
        Dim dangle As Double = angle / PI * 180
        With Me.Control.Agent
            If (drobotAngle = dangle) Then
                .Drive(1.5F)
            Else
                .TurnLeft(CSng(dangle - drobotAngle))
                .Drive(1.5F)
            End If
        End With


    End Sub


End Class
