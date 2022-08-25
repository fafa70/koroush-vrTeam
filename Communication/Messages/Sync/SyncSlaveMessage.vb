<Serializable()> _
Public Class SyncSlaveMessage
    Inherits SyncDataMessage

    Public Sub New()
        MyBase.New(Communication.MessageID.SyncSlave)
    End Sub

End Class
