Public MustInherit Class GraphLink(Of TNode As GraphNode(Of TNode, TLink), TLink As GraphLink(Of TNode, TLink))
    Implements IDisposable

#Region " Constructor "

    Public Sub New(ByVal graph As Graph(Of TNode, TLink), ByVal fromNode As TNode, ByVal toNode As TNode)
        Me.New(graph, Guid.NewGuid, fromNode, toNode)
    End Sub

    Public Sub New(ByVal graph As Graph(Of TNode, TLink), ByVal uniqueID As Guid, ByVal fromNode As TNode, ByVal toNode As TNode)
        Me._Graph = graph
        Me._ID = uniqueID
        Me._fromNode = fromNode
        Me._toNode = toNode

        Me._fromNode.InternalAddLink(DirectCast(Me, TLink), GraphLinkDirection.Outgoing)
        Me._toNode.InternalAddLink(DirectCast(Me, TLink), GraphLinkDirection.Incoming)

    End Sub

#End Region

#Region " IDisposable Support "

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free unmanaged resources when explicitly called
                Me._toNode.InternalRemoveLink(DirectCast(Me, TLink), GraphLinkDirection.Incoming)
                Me._fromNode.InternalRemoveLink(DirectCast(Me, TLink), GraphLinkDirection.Outgoing)
            End If

            ' TODO: free shared unmanaged resources
        End If
        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

#Region " Properties "

    Private _Graph As Graph(Of TNode, TLink)
    Public ReadOnly Property Graph() As Graph(Of TNode, TLink)
        Get
            Return Me._Graph
        End Get
    End Property

    Private _ID As Guid
    Public ReadOnly Property ID() As Guid
        Get
            Return Me._ID
        End Get
    End Property



    Private _fromNode As TNode
    Public ReadOnly Property FromNode() As TNode
        Get
            Return Me._fromNode
        End Get
    End Property

    Private _toNode As TNode
    Public ReadOnly Property ToNode() As TNode
        Get
            Return Me._toNode
        End Get
    End Property

#End Region

#Region " Functions "

    Public Function GetOtherNode(ByVal node As TNode) As TNode
        If Not IsNothing(Me._fromNode) AndAlso node.ID = Me._fromNode.ID Then
            Return Me._toNode
        ElseIf Not IsNothing(Me._toNode) AndAlso node.ID = Me._toNode.ID Then
            Return Me._fromNode
        End If
        Return Nothing
    End Function

#End Region

End Class
