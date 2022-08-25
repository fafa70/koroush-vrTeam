Imports UvARescue.Math

<Serializable()> _
Public Class CamReqMessage
    Inherits BroadcastMessage

    Public Sub New(ByVal fromRobot As String, ByVal toRobot As String)
        MyBase.New(fromRobot, toRobot, Communication.MessageID.CamReq)
    End Sub

End Class
