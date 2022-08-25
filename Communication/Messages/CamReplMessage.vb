Imports UvARescue.Math

<Serializable()> _
Public Class CamReplMessage
    Inherits BroadcastMessage

    Public Sub New(ByVal sender As String, ByVal recipient As String, ByVal camIm As System.Drawing.Bitmap)
        MyBase.New(sender, recipient, Communication.MessageID.CamRepl)

        Me._camIm = camIm

    End Sub

    Public ReadOnly _camIm As System.Drawing.Bitmap

End Class
