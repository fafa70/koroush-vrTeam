Imports UvARescue.Math

Public Class GroundTruthSensor
    Inherits SingleStateSensor(Of GroundTruthData)

    Public Shared ReadOnly SENSORTYPE_GROUNDTRUTH As String = "GroundTruth"

    Public Sub New()
        MyBase.New(New GroundTruthData, SENSORTYPE_GROUNDTRUTH, Sensor.MATCH_ANYNAME)
        If Not IsNothing(Me.Agent) AndAlso Not IsNothing(Me.Agent.AgentConfig) AndAlso Not String.IsNullOrEmpty(Me.Agent.AgentConfig.StartLocation) AndAlso Not String.IsNullOrEmpty(Me.Agent.AgentConfig.StartRotation) Then
            _CurrentPose = New Pose3D(Me.Agent.AgentConfig.StartLocation, Me.Agent.AgentConfig.StartRotation)
        Else
            _CurrentPose = New Pose3D
        End If


    End Sub

    Protected Overrides Sub ProcessMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessMessage(msgtype, msg)

        If msgtype = MessageType.Sensor Then
            Me.CurrentData.Load(msg)

            _CurrentPose.X = Me.CurrentData.X()
            _CurrentPose.Y = Me.CurrentData.Y()
            _CurrentPose.Z = Me.CurrentData.Z()

            _CurrentPose.Roll = Me.CurrentData.Roll()
            _CurrentPose.Pitch = Me.CurrentData.Pitch()
            _CurrentPose.Yaw = Me.CurrentData.Yaw()

            Me.Agent.NotifySensorUpdate(Me)
        End If

    End Sub

    Private _CurrentPose As Pose3D
    Public ReadOnly Property PoseEstimate() As Pose3D
        Get
            Return _CurrentPose
        End Get
        'The offset is already added in CurrentData functions (needs GetGeo message processing)
    End Property
End Class
