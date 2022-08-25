Imports UvARescue.Math

Public Class OdometrySensor
    Inherits SingleStateSensor(Of OdometryData)

    Public Shared ReadOnly SENSORTYPE_ODOMETRY As String = "Odometry"

    Public Sub New()
        MyBase.New(New OdometryData, SENSORTYPE_ODOMETRY, Sensor.MATCH_ANYNAME)
        If Not IsNothing(Me.Agent) AndAlso Not IsNothing(Me.Agent.AgentConfig) AndAlso Not String.IsNullOrEmpty(Me.Agent.AgentConfig.StartLocation) AndAlso Not String.IsNullOrEmpty(Me.Agent.AgentConfig.StartRotation) Then
            _CurrentPose = New Pose2D(Me.Agent.AgentConfig.StartLocation, Me.Agent.AgentConfig.StartRotation)
        Else
            _CurrentPose = New Pose2D
        End If

    End Sub

    Public Sub Reset(ByVal x As Double, ByVal y As Double, ByVal theta As Double)
        Me.CurrentData.ResetOffsets(x, y, theta)
    End Sub

    Protected Overrides Sub ProcessMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessMessage(msgtype, msg)

        If msgtype = MessageType.Sensor Then
            Me.CurrentData.Load(msg)

            'store those values in CurrentPose (including offset)
            _CurrentPose.X = Me.CurrentData.X()
            _CurrentPose.Y = Me.CurrentData.Y()
            _CurrentPose.Rotation = Me.CurrentData.Theta()

            Me.Agent.NotifySensorUpdate(Me)
        End If

    End Sub

    Private _CurrentPose As Pose2D
    Public ReadOnly Property PoseEstimate() As Pose2D
        Get
            Return _CurrentPose
        End Get
        'The offset is already added in CurrentData functions (needs GetGeo message processing)
    End Property


End Class
