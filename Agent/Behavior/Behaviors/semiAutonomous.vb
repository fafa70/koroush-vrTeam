Public Class semiAutonomous
    Inherits Behavior
    Public Sub New(ByVal control As BehaviorControl)
        MyBase.New(control)
    End Sub


    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        Me.Control.ActivateMotion(MotionType.FrontierExploration, True)
    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        Me.Control.DeActivateAllMotions()
    End Sub
    Protected Overrides Sub OnNotifyOperatorConnected()
        MyBase.OnNotifyOperatorConnected()
        Me.Control.ActivateMotion(MotionType.FrontierExploration, True)
    End Sub

    Protected Overrides Sub OnNotifyOperatorDisconnected()
        MyBase.OnNotifyOperatorDisconnected()
        Me.Control.ActivateMotion(MotionType.Retreat, True)
    End Sub



    Private _LastTried As DateTime = DateTime.MinValue

    Protected Overrides Sub OnNotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        MyBase.OnNotifyPoseEstimateUpdated(pose)

        If Not Me.Control.IsActiveMotion(MotionType.semiBehaviour) Then

            'perform another motion for two minutes, and try to explore again
            ' RIKNOTE: supposedly this depends on signal strength somewhere
            ' forming a dependency between BehaviorAgent and CommAgent, fix
            If Now - Me._LastTried > TimeSpan.FromSeconds(120) Then
                Me.Control.ActivateMotion(MotionType.semiBehaviour, True)
                Me._LastTried = Now
            End If
        End If


    End Sub


End Class
