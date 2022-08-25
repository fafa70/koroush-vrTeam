<Serializable()> _
Public Class SyncMasterMessage
    Inherits SyncDataMessage

    Public Sub New()
        MyBase.New(Communication.MessageID.SyncMaster)
    End Sub

End Class
