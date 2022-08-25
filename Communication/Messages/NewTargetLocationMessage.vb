Imports UvARescue.Communication
Imports UvARescue.Math
Imports UvARescue.Tools

<Serializable()> _
Public Class NewTargetLocationMessage
    Inherits BroadcastMessage

    Public Sub New(ByVal sender As String, ByVal recipient As String, ByVal target As Pose2D)
        MyBase.New(sender, recipient, Communication.MessageID.NewTargetLocation)
        Me._Target = target
    End Sub

    Public ReadOnly _Target As Pose2D

End Class

