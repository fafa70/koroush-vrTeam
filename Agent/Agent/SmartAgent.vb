Public Class SmartAgent
    Inherits CommAgent

    Public Sub New(ByVal manifold As Manifold, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(manifold, name, agentConfig, teamConfig)

        Me._MotionControl = New MotionControl(Me)
        Me._BehaviorControl = New BehaviorControl(Me, Me._MotionControl)

        Me._BehaviorControl.SwitchToBehavior(BehaviorType.AutonomousExploration)

    End Sub

    Private _BehaviorControl As BehaviorControl
    Public ReadOnly Property BehaviorControl() As BehaviorControl
        Get
            Return Me._BehaviorControl
        End Get
    End Property

    Private _MotionControl As MotionControl
    Public ReadOnly Property MotionControl() As MotionControl
        Get
            Return Me._MotionControl
        End Get
    End Property

    Protected Friend Overrides Sub NotifySensorUpdate(ByVal sensor As Sensor)
        MyBase.NotifySensorUpdate(sensor)

        Me._BehaviorControl.NotifySensorUpdate(sensor)
        Me._MotionControl.NotifySensorUpdate(sensor)

    End Sub

End Class
