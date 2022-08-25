<Serializable()> _
Public Class BehaviorChangeMessage
    Inherits BroadcastMessage

    Public Sub New(ByVal sender As String, ByVal recipient As String, ByVal newBehavior As String)
        MyBase.New(sender, recipient, Communication.MessageID.BehaviorChange)
        Me.Behavior = newBehavior
    End Sub

    Public ReadOnly Behavior As String

End Class

