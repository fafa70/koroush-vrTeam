Public Class FollowPath
    Inherits Behavior

    Public Sub New(ByVal control As BehaviorControl)
        MyBase.New(control)
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Behaviors\FollowPath.vb::New() called")
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        Me.Control.ActivateMotion(MotionType.FollowPathTraversability, False)
    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        Me.Control.DeActivateMotion(MotionType.FollowPathTraversability)
    End Sub

    Private _LastTried As DateTime = DateTime.MinValue

    Protected Overrides Sub OnNotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)

    End Sub

End Class
