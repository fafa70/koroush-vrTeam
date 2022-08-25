Public Class FollowWaypoint
    Inherits Behavior

    Public Sub New(ByVal control As BehaviorControl)
        MyBase.New(control)
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        Me.Control.ActivateMotion(MotionType.Following, False)
    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        Me.Control.DeActivateMotion(MotionType.Following)
    End Sub

    Private _LastTried As DateTime = DateTime.MinValue

    ' COULD BE CHANGED
    Protected Overrides Sub OnNotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        MyBase.OnNotifyPoseEstimateUpdated(pose)

        If Not Me.Control.IsActiveMotion(MotionType.Following) Then
            'perform another motion for two minutes, and try to follow again
            ' RIKNOTE: supposedly this depends on signal strength somewhere
            ' forming a dependency between BehaviorAgent and CommAgent, fix
            If Now - Me._LastTried > TimeSpan.FromSeconds(60) Then
                Me.Control.ActivateMotion(MotionType.Following, True)
                Me._LastTried = Now
            End If
        End If


    End Sub

End Class
