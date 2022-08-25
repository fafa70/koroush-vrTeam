Imports UvARescue.Math

<Serializable()> _
Public Class PoseUpdateMessage
    Inherits BroadcastMessage

    Public Sub New(ByVal sender As String, ByVal pose As Pose2D, ByVal connectedTo() As String)
        MyBase.New(sender, Communication.MessageID.PoseUpdate)
        Me.Pose = pose
        Me.ConnectedTo = connectedTo
    End Sub

    Public ReadOnly Pose As Pose2D
    Public ReadOnly ConnectedTo() As String

End Class
