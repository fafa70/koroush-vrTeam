Imports UvARescue.Math

<Serializable()> _
Public Class GreetingMessage
    Inherits Message

    Public Sub New(ByVal agentName As String, ByVal agentPose As Pose2D, ByVal useCamConnection As Boolean)
        MyBase.New(Communication.MessageID.Greeting)
        Me.AgentName = agentName
        Me.AgentPose = agentPose
        Me.UseCamConnection = useCamConnection
    End Sub

    Public ReadOnly AgentName As String
    Public ReadOnly AgentPose As Pose2D

End Class
