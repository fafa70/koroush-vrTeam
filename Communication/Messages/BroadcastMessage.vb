<Serializable()> _
Public MustInherit Class BroadcastMessage
    Inherits Message

    Public Sub New(ByVal sender As String, ByVal messageID As MessageID)
        MyBase.New(messageID)

        Me.Timestamp = DateTime.Now
        Me.ToEveryone = True
        Me.Sender = sender
        Me.Recipient = "*"

        Me._DeliveryTrace = New List(Of String)
        Me._DeliveryTrace.Add(sender)

    End Sub

    Public Sub New(ByVal sender As String, ByVal recipient As String, ByVal messageID As MessageID)
        MyBase.New(messageID)

        Me.Timestamp = DateTime.Now
        Me.ToEveryone = False
        Me.Sender = sender
        Me.Recipient = recipient

        Me._DeliveryTrace = New List(Of String)
        Me._DeliveryTrace.Add(sender)

    End Sub

    Public ReadOnly Timestamp As DateTime

    Public ReadOnly ToEveryone As Boolean
    Public ReadOnly Sender As String
    Public ReadOnly Recipient As String


    'to keep track of whom this message has been delivered to already
    'this is to avoid endless resends
    Private _DeliveryTrace As List(Of String)

    Public ReadOnly Property DeliveryTrace() As IEnumerable(Of String)
        Get
            Return Me._DeliveryTrace
        End Get
    End Property

    Public Sub MarkDelivered(ByVal name As String)
        If Not _DeliveryTrace.Contains(name) Then
            _DeliveryTrace.Add(name)
        End If
    End Sub

    Public Function IsDeliveredTo(ByVal name As String) As Boolean
        Return Me._DeliveryTrace.Contains(name)
    End Function

End Class

