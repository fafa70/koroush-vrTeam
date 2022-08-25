Imports System.Collections.Generic

''' <summary>
''' Strong-typed template data structure for Graphs. You cannot just inherit it by 
''' itself, the Graph and the GraphNode and GraphLink classes should be inherited
''' simultaneously for all template specifiers to work.
''' </summary>
''' <typeparam name="TNode"></typeparam>
''' <typeparam name="TLink"></typeparam>
''' <remarks></remarks>
Public MustInherit Class Graph(Of TNode As GraphNode(Of TNode, TLink), TLink As GraphLink(Of TNode, TLink))
    Implements IDisposable

#Region " Constructor "

    Public Sub New()
    End Sub

    Public Overridable Sub Clear()
        For Each link As TLink In Me.Links
            If TypeOf link Is IDisposable Then
                DirectCast(link, IDisposable).Dispose()
            End If
        Next
        Me._Links.Clear()

        For Each node As TNode In Me.Nodes
            If TypeOf node Is IDisposable Then
                DirectCast(node, IDisposable).Dispose()
            End If
        Next
        Me._Nodes.Clear()
    End Sub

#End Region

#Region " IDisposable "

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free unmanaged resources when explicitly called
                Me.Clear()
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

#Region " Nodes "

    Private _Nodes As New Dictionary(Of Guid, TNode)
    Public ReadOnly Property Nodes() As IEnumerable(Of TNode)
        Get
            Return Me._Nodes.Values
        End Get
    End Property
    Public ReadOnly Property NodeCount() As Integer
        Get
            Return Me._Nodes.Count
        End Get
    End Property

    Public Function GetNode(ByVal nodeID As Guid) As TNode
        If Me._Nodes.ContainsKey(nodeID) Then
            Return Me._Nodes(nodeID)
        End If
        Return Nothing
    End Function

    Public Function HasNode(ByVal nodeID As Guid) As Boolean
        Return Me._Nodes.ContainsKey(nodeID)
    End Function

    Public Function InsertNode(ByVal node As TNode) As TNode
        If Not HasNode(node.ID) Then
            Me._Nodes.Add(node.ID, node)
            'Console.WriteLine(String.Format("Inserting Node {0}", node.ID))
            Me.OnNodeInserted(node) 'does nothing yet. Check if 
        Else
            Console.WriteLine(String.Format("Node {0} was already present", node.ID))
        End If
        Return node
    End Function

    Public Function RemoveNode(ByVal nodeID As Guid) As TNode
        Dim node As TNode = Me.GetNode(nodeID)
        If Not IsNothing(node) Then
            Me._Nodes.Remove(nodeID)
            Me.OnNodeRemoved(node)
        End If
        Return node
    End Function

    Protected Overridable Sub OnNodeInserted(ByVal node As TNode)
    End Sub
    Protected Overridable Sub OnNodeRemoved(ByVal node As TNode)
    End Sub

#End Region

#Region " Links "

    Private _Links As New Dictionary(Of Guid, TLink)
    Public ReadOnly Property Links() As IEnumerable(Of TLink)
        Get
            Return Me._Links.Values
        End Get
    End Property
    Public ReadOnly Property LinkCount() As Integer
        Get
            Return Me._Links.Count
        End Get
    End Property

    Public Function GetLink(ByVal linkID As Guid) As TLink
        If Me._Links.ContainsKey(linkID) Then
            Return Me._Links(linkID)
        End If
        Return Nothing
    End Function
    Public Function GetLink(ByVal fromNode As TNode, ByVal toNode As TNode) As TLink
        If Me.HasNode(fromNode.ID) AndAlso Me.HasNode(toNode.ID) Then
            Return fromNode.GetLink(toNode)
        End If
        Return Nothing
    End Function
    Public Function GetLink(ByVal fromNodeID As Guid, ByVal toNodeID As Guid) As TLink
        If Me.HasNode(fromNodeID) AndAlso Me.HasNode(toNodeID) Then
            Return Me.GetNode(fromNodeID).GetLink(toNodeID)
        End If
        Return Nothing
    End Function

    Public Function HasLink(ByVal linkID As Guid) As Boolean
        Return Me._Links.ContainsKey(linkID)
    End Function
    Public Function HasLink(ByVal fromNode As TNode, ByVal toNode As TNode) As Boolean
        If Me.HasNode(fromNode.ID) AndAlso Me.HasNode(toNode.ID) Then
            Return fromNode.HasLink(toNode)
        End If
        Return False
    End Function
    Public Function HasLink(ByVal fromNodeID As Guid, ByVal toNodeID As Guid) As Boolean
        If Me.HasNode(fromNodeID) AndAlso Me.HasNode(toNodeID) Then
            Return Me.GetNode(fromNodeID).HasLink(toNodeID)
        End If
        Return False
    End Function


    Public Function InsertLink(ByVal link As TLink) As TLink
        Me._Links.Add(link.ID, link)
        Me.OnLinkInserted(link)
        Return link
    End Function

    Public Function RemoveLink(ByVal linkID As Guid) As TLink
        Dim Link As TLink = Me.GetLink(linkID)
        If Not IsNothing(Link) Then
            Me._Links.Remove(linkID)
            Me.OnLinkRemoved(Link)
        End If
        Return Link
    End Function

    Protected Overridable Sub OnLinkInserted(ByVal link As TLink)
    End Sub
    Protected Overridable Sub OnLinkRemoved(ByVal link As TLink)
    End Sub

#End Region

End Class
