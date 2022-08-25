'
' RIKNOTE: this class and file added for assignment 2
'
Public Class FollowCorridorBehavior
    Inherits Behavior

    Public Sub New(ByVal control As BehaviorControl)
        MyBase.New(control)
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Behaviors\FollowCorridorBehavior.vb::New() called")
    End Sub

    Protected Overrides Sub OnActivated()
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Behaviors\FollowCorridorBehavior.vb::OnActivated() called")
        MyBase.OnActivated()
        Me.Control.ActivateMotion(MotionType.CorridorWalk, True)
    End Sub

    Protected Overrides Sub OnDeActivated()
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Behaviors\FollowCorridorBehavior.vb::OnDeActivated() called")
        MyBase.OnDeActivated()
        Me.Control.DeActivateMotion(MotionType.CorridorWalk)
    End Sub

End Class
