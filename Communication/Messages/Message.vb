<Serializable()> _
Public Class Message

    Public Sub New(ByVal messageID As MessageID)
        Me.MessageID = messageID
        Me.UseCamConnection = False
    End Sub

    Public ReadOnly MessageID As MessageID
    Public UseCamConnection As Boolean

End Class

