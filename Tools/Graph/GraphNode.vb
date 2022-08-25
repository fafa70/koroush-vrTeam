Imports System.Math

Public MustInherit Class GraphNode(Of TNode As GraphNode(Of TNode, TLink), TLink As GraphLink(Of TNode, TLink))

#Region " Constructor "

    Public Sub New(ByVal graph As Graph(Of TNode, TLink))
        Me.New(graph, Guid.NewGuid)
    End Sub

    Public Sub New(ByVal graph As Graph(Of TNode, TLink), ByVal uniqueID As Guid)
        Me._Graph = graph
        Me._ID = uniqueID
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

#End Region

#Region " Level Management "

    Public Event LevelChanged As EventHandler(Of EventArgs(Of Integer))

    Private _Level As Integer = 0
    Public ReadOnly Property Level() As Integer
        Get
            Return Me._Level
        End Get
    End Property

    Private _IsSettingLevel As Boolean = False
    Private Sub SetLevel(ByVal level As Integer)
        If level <> Me._Level Then
            If Me._IsSettingLevel Then
                'avoid infinite loops
            Else
                Me._IsSettingLevel = True
                Me._Level = level
                Me.OnLevelChanged()
                Me._IsSettingLevel = False
            End If
        End If
    End Sub
    Protected Overridable Sub OnLevelChanged()
        RaiseEvent LevelChanged(Me, New EventArgs(Of Integer)(Me.Level))
    End Sub

    Private Sub OnFromNodeChangedLevel(ByVal sender As Object, ByVal e As EventArgs(Of Integer))
        Me.RecheckLevel()
    End Sub

    Private Sub RecheckLevel()
        If Me.HasFromNodes Then
            Dim myDesiredLevel As Integer = 0
            For Each fromNode As TNode In Me.FromNodes
                myDesiredLevel = Max(myDesiredLevel, fromNode.Level + 1)
            Next
            Me.SetLevel(myDesiredLevel)

        Else
            Me.SetLevel(0)

        End If
    End Sub

    Private Sub EnsureMinimumLevel(ByVal minimum As Integer)
        If minimum > Me.Level Then
            Me.SetLevel(minimum)
        End If
    End Sub



    Protected Overridable Sub OnFromNodeAdded(ByVal fromNode As TNode)
        Me.EnsureMinimumLevel(fromNode.Level + 1)
        AddHandler fromNode.LevelChanged, AddressOf Me.OnFromNodeChangedLevel
    End Sub
    Protected Overridable Sub OnFromNodeRemoved(ByVal fromNode As TNode)
        RemoveHandler fromNode.LevelChanged, AddressOf Me.OnFromNodeChangedLevel
        Me.RecheckLevel()
    End Sub
    Protected Overridable Sub OnToNodeAdded(ByVal toNode As TNode)
    End Sub
    Protected Overridable Sub OnToNodeRemoved(ByVal toNode As TNode)
    End Sub

#End Region

#Region " Link and Node Registration "

    Friend Sub InternalAddLink(ByVal link As TLink, ByVal direction As GraphLinkDirection)
        Dim node As TNode = link.GetOtherNode(DirectCast(Me, TNode))
        Me._Links.Add(link.ID, DirectCast(link, TLink))
        Me._Nodes.Add(node.ID, node)
        Select Case direction
            Case GraphLinkDirection.Incoming
                Me._IncomingLinks.Add(link.ID, link)
                Me._FromNodes.Add(node.ID, node)
            Case GraphLinkDirection.Outgoing
                Me._OutgoingLinks.Add(link.ID, link)
                Me._ToNodes.Add(node.ID, node)
        End Select
        Me.OnLinkAdded(link, node, direction)
    End Sub
    Friend Sub InternalRemoveLink(ByVal link As TLink, ByVal direction As GraphLinkDirection)
        Dim node As TNode = link.GetOtherNode(DirectCast(Me, TNode))
        Select Case direction
            Case GraphLinkDirection.Incoming
                Me._IncomingLinks.Remove(link.ID)
                Me._FromNodes.Remove(node.ID)
            Case GraphLinkDirection.Outgoing
                Me._OutgoingLinks.Remove(link.ID)
                Me._ToNodes.Remove(node.ID)
        End Select
        Me._Nodes.Remove(node.ID)
        Me._Links.Remove(link.ID)
        Me.OnLinkRemoved(link, node, direction)
    End Sub

    Protected Overridable Sub OnLinkAdded(ByVal link As TLink, ByVal otherNode As TNode, ByVal direction As GraphLinkDirection)
        Select Case direction
            Case GraphLinkDirection.Incoming
                Me.OnIncomingLinkAdded(link, otherNode)
            Case GraphLinkDirection.Outgoing
                Me.OnOutgoingLinkAdded(link, otherNode)
        End Select
    End Sub
    Protected Overridable Sub OnIncomingLinkAdded(ByVal link As TLink, ByVal fromNode As TNode)
        Me.OnFromNodeAdded(fromNode)
    End Sub
    Protected Overridable Sub OnOutgoingLinkAdded(ByVal link As TLink, ByVal toNode As TNode)
        Me.OnToNodeAdded(toNode)
    End Sub

    Protected Overridable Sub OnLinkRemoved(ByVal link As TLink, ByVal otherNode As TNode, ByVal direction As GraphLinkDirection)
        Select Case direction
            Case GraphLinkDirection.Incoming
                Me.OnIncomingLinkRemoved(link, otherNode)
            Case GraphLinkDirection.Outgoing
                Me.OnOutgoingLinkRemoved(link, otherNode)
        End Select
    End Sub
    Protected Overridable Sub OnIncomingLinkRemoved(ByVal link As TLink, ByVal fromNode As TNode)
        Me.OnFromNodeRemoved(fromNode)
    End Sub
    Protected Overridable Sub OnOutgoingLinkRemoved(ByVal link As TLink, ByVal toNode As TNode)
        Me.OnToNodeRemoved(toNode)
    End Sub

#End Region

#Region " Links "

    Private _Links As New Dictionary(Of Guid, TLink)
    Public ReadOnly Property Links() As IEnumerable(Of TLink)
        Get
            Return Me._Links.Values
        End Get
    End Property
    Private _IncomingLinks As New Dictionary(Of Guid, TLink)
    Public ReadOnly Property IncomingLinks() As IEnumerable(Of TLink)
        Get
            Return Me._IncomingLinks.Values
        End Get
    End Property
    Private _OutgoingLinks As New Dictionary(Of Guid, TLink)
    Public ReadOnly Property OutgoingLinks() As IEnumerable(Of TLink)
        Get
            Return Me._OutgoingLinks.Values
        End Get
    End Property


    Public Function GetLink(ByVal otherNode As TNode) As TLink
        Return Me.GetLink(otherNode.ID)
    End Function
    Public Function GetLink(ByVal otherNodeID As Guid) As TLink
        If Me.HasLink(otherNodeID) Then
            Dim node As TNode
            For Each link As TLink In Me._Links.Values
                node = link.GetOtherNode(DirectCast(Me, TNode))
                If node.ID = otherNodeID Then
                    Return link
                End If
            Next
        End If
        Return Nothing
    End Function
    Public Function GetIncomingLink(ByVal fromNode As TNode) As TLink
        Return Me.GetLink(fromNode.ID)
    End Function
    Public Function GetIncomingLink(ByVal fromNodeID As Guid) As TLink
        Return Me.GetLink(fromNodeID)
    End Function
    Public Function GetOutgoingLink(ByVal toNode As TNode) As TLink
        Return Me.GetLink(toNode.ID)
    End Function
    Public Function GetOutgoingLink(ByVal toNodeID As Guid) As TLink
        Return Me.GetLink(toNodeID)
    End Function

    Public Function HasLink(ByVal otherNode As TNode) As Boolean
        Return Me.HasLink(otherNode.ID)
    End Function
    Public Function HasLink(ByVal otherNodeID As Guid) As Boolean
        Return Me._Nodes.ContainsKey(otherNodeID)
    End Function
    Public Function HasIncomingLink(ByVal fromNode As TNode) As Boolean
        Return Me.HasIncomingLink(fromNode.ID)
    End Function
    Public Function HasIncomingLink(ByVal fromNodeID As Guid) As Boolean
        Return Me._FromNodes.ContainsKey(fromNodeID)
    End Function
    Public Function HasOutgoingLink(ByVal toNode As TNode) As Boolean
        Return Me.HasOutgoingLink(toNode.ID)
    End Function
    Public Function HasOutgoingLink(ByVal toNodeID As Guid) As Boolean
        Return Me._ToNodes.ContainsKey(toNodeID)
    End Function

#End Region

#Region " Nodes "

    Private _Nodes As New Dictionary(Of Guid, TNode)
    Public ReadOnly Property Nodes() As IEnumerable(Of TNode)
        Get
            Return Me._Nodes.Values
        End Get
    End Property

    Private _FromNodes As New Dictionary(Of Guid, TNode)
    Public ReadOnly Property FromNodes() As IEnumerable(Of TNode)
        Get
            Return Me._FromNodes.Values
        End Get
    End Property
    Public Function HasFromNodes() As Boolean
        Return Me._FromNodes.Count > 0
    End Function

    Private _ToNodes As New Dictionary(Of Guid, TNode)
    Public ReadOnly Property ToNodes() As IEnumerable(Of TNode)
        Get
            Return Me._ToNodes.Values
        End Get
    End Property
    Public Function HasToNodes() As Boolean
        Return Me._ToNodes.Count > 0
    End Function

#End Region

#Region " Query "

    Public Delegate Function QueryFilter(ByVal node As TNode) As Boolean

    Public Overridable Function Query(ByVal direction As GraphQueryDirection, ByVal recursive As Boolean, Optional ByVal filterHandler As QueryFilter = Nothing) As List(Of TNode)

        Dim history As New List(Of TNode)
        Dim frontier As New List(Of TNode)
        history.Add(DirectCast(Me, TNode))
        frontier.Add(DirectCast(Me, TNode))

        frontier = Me.Expand(frontier, history, direction, filterHandler)
        If recursive Then
            While frontier.Count > 0
                frontier = Me.Expand(frontier, history, direction, filterHandler)
            End While
        End If

        Return history

    End Function

    Protected Overridable Function Expand(ByVal frontier As List(Of TNode), ByRef history As List(Of TNode), ByVal direction As GraphQueryDirection, ByVal filterHandler As QueryFilter) As List(Of TNode)
        Dim oNextFrontier As New List(Of TNode)
        For Each oNode As TNode In frontier

            Dim oNodes As IEnumerable(Of TNode) = Nothing
            Select Case direction
                Case GraphQueryDirection.Both
                    oNodes = oNode.Nodes
                Case GraphQueryDirection.BackwardsOnly
                    oNodes = oNode.FromNodes
                Case GraphQueryDirection.ForwardsOnly
                    oNodes = oNode.ToNodes
            End Select

            If Not IsNothing(oNodes) Then
                For Each oNextNode As TNode In oNodes
                    If Not history.Contains(oNextNode) Then
                        Dim bFiltered As Boolean = False
                        If Not IsNothing(filterHandler) Then
                            bFiltered = filterHandler(oNextNode)
                        End If
                        If Not bFiltered Then
                            oNextFrontier.Add(oNextNode)
                            history.Add(oNextNode)
                        End If
                    End If
                Next
            End If

        Next

        Return oNextFrontier

    End Function

#End Region

End Class
