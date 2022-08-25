<Serializable()> _
Public Class ActorCommandMessage
    Inherits BroadcastMessage

    Public Sub New(ByVal sender As String, ByVal recipient As String, ByVal command As String)
        MyBase.New(sender, recipient, Communication.MessageID.ActorCommand)
        Me.Command = command
    End Sub

    Public ReadOnly Command As String

End Class
