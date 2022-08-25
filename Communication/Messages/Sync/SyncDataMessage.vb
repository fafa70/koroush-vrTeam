<Serializable()> _
Public MustInherit Class SyncDataMessage
    Inherits Message

    Public Sub New(ByVal messageID As MessageID)
        MyBase.New(messageID)
        Me._CommitTimestamp = DateTime.MinValue
        Me._Data = New List(Of Object)
    End Sub

    Private _CommitTimestamp As DateTime
    Public Property CommitTimestamp() As DateTime
        Get
            Return Me._CommitTimestamp
        End Get
        Set(ByVal value As DateTime)
            Me._CommitTimestamp = value
        End Set
    End Property

    Private _Data As List(Of Object)
    Public ReadOnly Property Data() As List(Of Object)
        Get
            Return Me._Data
        End Get
    End Property

End Class
