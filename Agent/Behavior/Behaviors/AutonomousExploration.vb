Public Class AutonomousExploration
    Inherits Behavior

    Public Sub New(ByVal control As BehaviorControl)
        MyBase.New(control)
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Behaviors\AutonomousExploration.vb::New() called")
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        Me.Control.ActivateMotion(MotionType.FrontierExploration, False)
    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        Me.Control.DeActivateMotion(MotionType.FrontierExploration)
    End Sub

    Private _LastTried As DateTime = DateTime.MinValue


    Protected Overrides Sub OnNotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        MyBase.OnNotifyPoseEstimateUpdated(pose)

        If Not Me.Control.IsActiveMotion(MotionType.FrontierExploration) Then

            'perform another motion for two minutes, and try to explore again
            ' RIKNOTE: supposedly this depends on signal strength somewhere
            ' forming a dependency between BehaviorAgent and CommAgent, fix
            If Now - Me._LastTried > TimeSpan.FromSeconds(120) Then
                Me.Control.ActivateMotion(MotionType.FrontierExploration, True)
                Me._LastTried = Now
            End If
        End If


    End Sub

End Class
