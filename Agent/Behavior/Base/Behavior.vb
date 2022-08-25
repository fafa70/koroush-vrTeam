Public MustInherit Class Behavior

#Region " Constructor "

    Public Sub New(ByVal control As BehaviorControl)
        Console.WriteLine("[RIKNOTE] Agent\Behavior\Base\Behavior.vb::New() called")
        Me._Control = control
        Me._Active = False
    End Sub

#End Region

#Region " Properties "

    Private _Control As BehaviorControl
    Public ReadOnly Property Control() As BehaviorControl
        Get
            Return Me._Control
        End Get
    End Property

#End Region

#Region " Activation "

    Private _Active As Boolean
    Public ReadOnly Property Active() As Boolean
        Get
            Return Me._Active
        End Get
    End Property

    Public Sub Activate()
        Console.WriteLine("[RIKNOTE] Agent\Behavior\Base\Behavior.vb::Activate() called")
        If Not Me._Active Then
            Me._Active = True
            Me.OnActivated()
        End If
    End Sub
    Public Sub DeActivate()
        If Me._Active Then
            Me._Active = False
            Me.OnDeActivated()
        End If
    End Sub


    Protected Overridable Sub OnActivated()
    End Sub
    Protected Overridable Sub OnDeActivated()
    End Sub

#End Region

#Region " Incoming Notifications from BehaviorControl "

    Public Sub NotifySensorUpdate(ByVal sensor As Sensor)
        Me.OnNotifySensorUpdate(sensor)
    End Sub

    Public Sub NotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        Me.OnNotifyPoseEstimateUpdated(pose)
    End Sub

    Public Sub NotifyOperatorConnected()
        Me.OnNotifyOperatorConnected()
    End Sub
    Public Sub NotifyOperatorDisconnected()
        Me.OnNotifyOperatorDisconnected()
    End Sub

    Public Sub NotifyAgentConnected(ByVal agentName As String)
        Me.OnNotifyAgentConnected(agentName)
    End Sub
    Public Sub NotifyAgentDisconnected(ByVal agentName As String)
        Me.OnNotifyAgentDisconnected(agentName)
    End Sub



    Protected Overridable Sub OnNotifySensorUpdate(ByVal sensor As Sensor)
    End Sub
    Protected Overridable Sub OnNotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)
    End Sub

    Protected Overridable Sub OnNotifyOperatorConnected()
    End Sub
    Protected Overridable Sub OnNotifyOperatorDisconnected()
    End Sub

    Protected Overridable Sub OnNotifyAgentConnected(ByVal agentName As String)
    End Sub
    Protected Overridable Sub OnNotifyAgentDisconnected(ByVal agentName As String)
    End Sub

#End Region

End Class
