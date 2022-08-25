<Serializable()> _
Public Class TextMessage
    Inherits Message

    Public Sub New(ByVal text As String)
        MyBase.New(Communication.MessageID.Text)
        Me.Text = text
    End Sub

    Public ReadOnly Text As String

End Class
