Public Class DriveCircle
    Inherits Behavior

    Public Sub New(ByVal control As BehaviorControl)
        MyBase.New(control)
        ' Console.WriteLine("[RIKNOTE] Agent\Behavior\Behaviors\DriveCircle.vb::New() called")
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        Me.Control.ActivateMotion(MotionType.WalkCircle, True)
    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        Me.Control.DeActivateMotion(MotionType.WalkCircle)
    End Sub

End Class
