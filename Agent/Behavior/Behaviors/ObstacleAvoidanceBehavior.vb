' RIKNOTE: LOCALLY CREATED VERSION ON TUE 26-02-2008 
' BECAUSE OFFICIAL ONE CREATED ON WED 20-02-2008 NOT
' ADDED TO SVN REPO YET
Public Class ObstacleAvoidanceBehavior
    Inherits Behavior

    Public Sub New(ByVal control As BehaviorControl)
        MyBase.New(control)
        Console.WriteLine("[RIKNOTE] Agent\Behavior\Behaviors\ObstacleAvoidanceBehavior.vb::New() called")
    End Sub

    Protected Overrides Sub OnActivated()
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Behaviors\ObstacleAvoidanceBehavior.vb::OnActivated() called")
        MyBase.OnActivated()
        Me.Control.ActivateMotion(MotionType.ObstacleAvoidance, True)
    End Sub

    Protected Overrides Sub OnDeActivated()
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Behaviors\ObstacleAvoidanceBehavior.vb::OnDeActivated() called")
        MyBase.OnDeActivated()
        Me.Control.DeActivateMotion(MotionType.ObstacleAvoidance)
    End Sub

End Class
