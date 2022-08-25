Public Class DeploymentBehavior
    Inherits Behavior

    Public Sub New(ByVal control As BehaviorControl)
        MyBase.New(control)
        ' Console.WriteLine("[RIKNOTE] Agent\Behavior\Behaviors\DeploymentBehavior.vb::New() called")
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        Me.Control.ActivateMotion(MotionType.DeploymentMotion, True)
    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        Me.Control.DeActivateMotion(MotionType.DeploymentMotion)
    End Sub

End Class
