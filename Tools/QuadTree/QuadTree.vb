Imports System.Drawing

Imports UvARescue.Math


Public Class QuadTree(Of T As Vector2)

    Protected Mutex As New Object
    Protected Shared ReadOnly MaxNumPoints As Integer = 5

    Public Sub New()
        Me._RootNode = New QuadRootNode
    End Sub

    Private _RootNode As QuadRootNode

    Public Sub Insert(ByVal point As T)
        If IsNothing(point) OrElse Double.IsNaN(point.X) OrElse Double.IsInfinity(point.X) OrElse Double.IsNaN(point.Y) OrElse Double.IsInfinity(point.Y) Then
            Console.Error.WriteLine("[QuadRootNode:Insert Warning] didn't insert NaN point")
            Return
        End If
        SyncLock Me.Mutex
            Me._RootNode.Insert(point)
        End SyncLock
    End Sub

    Public Sub Remove(ByVal point As T)
        SyncLock Me.Mutex
        End SyncLock
    End Sub

    Public Function FindNearestNeighbour(ByVal target As Vector2, ByVal maxDistance As Single) As T

        SyncLock Me.Mutex

            'inputs
            Dim point As New PointF(CType(target.X, Single), CType(target.Y, Single))
            Dim radius As Single = maxDistance * 2
            Dim window As New RectangleF(point.X - maxDistance, point.Y - maxDistance, radius, radius)

            'outputs
            Dim nearest As T = Nothing
            Dim dist As Double = maxDistance

            If Me._RootNode.FindNearestNeighbour(point, window, nearest, dist) Then
                If dist < maxDistance Then
                    Return nearest
                End If
            End If

            Return Nothing

        End SyncLock

    End Function


    Protected MustInherit Class QuadNode

        Protected Shared ReadOnly NW As Integer = 0
        Protected Shared ReadOnly SW As Integer = 1
        Protected Shared ReadOnly NE As Integer = 2
        Protected Shared ReadOnly SE As Integer = 3

        Public Sub New(ByVal parent As QuadInternalNode, ByVal bounds As RectangleF)

            Me.Parent = parent
            Me.Bounds = bounds

            With Me.Bounds
                Me.Center = New PointF(.Left + (.Width / 2), .Top + (.Height / 2))
            End With

        End Sub

        Protected Friend Parent As QuadInternalNode
        Protected Friend Bounds As RectangleF
        Protected Friend Center As PointF

        Protected Friend Function Insert(ByVal points() As T) As QuadNode
            Dim node As QuadNode = Me
            For Each point As T In points
                node = node.Insert(point)
            Next
            Return node
        End Function

        Protected Friend MustOverride Function Insert(ByVal point As T) As QuadNode

        Protected Friend MustOverride Function FindSmallestContainingNode(ByVal rect As RectangleF) As QuadNode
        Protected Friend MustOverride Sub FindNearestNeighbour(ByVal target As PointF, ByRef window As RectangleF, ByRef nearest As T, ByRef sqdist As Double)

    End Class

    Protected Class QuadRootNode
        Inherits QuadInternalNode

        Public Sub New()
            MyBase.New(Nothing, New RectangleF(-2000, -2000, 4000, 4000))
        End Sub

        Protected Friend Overrides Function Insert(ByVal point As T) As QuadTree(Of T).QuadNode
            If Me.Bounds.Contains(CType(point.X, Single), CType(point.Y, Single)) Then
                'all is fine, proceed as usual
                Return MyBase.Insert(point)

            Else
                'we need to grow first

                'create a new internal node with my current bounds and children
                Dim newNode As New QuadInternalNode(Me, Me.Bounds, Me._Children)

                'then figure out where this new node should be placed
                'and what my new bounds will be
                Dim newIndex As Integer
                Dim newBounds As RectangleF

                With Me.Bounds

                    Dim newOffset As New PointF(.X, .Y)
                    Dim newSize As New SizeF(.Width * 2, .Height * 2)

                    If point.X < .Left Then
                        If point.Y < .Top Then
                            newIndex = SE
                            newOffset.X -= .Width
                            newOffset.Y -= .Height
                        Else
                            newIndex = NE
                            newOffset.X -= .Width
                        End If
                    Else
                        If point.Y < .Top Then
                            newIndex = SW
                            newOffset.Y -= .Height
                        Else
                            newIndex = NW
                        End If
                    End If

                    newBounds = New RectangleF(newOffset, newSize)

                End With

                'update bounds 
                Me.Bounds = newBounds

                'setup children according to newBounds
                With Me.Bounds
                    Try
                        'recompute center first
                        Me.Center = New PointF(.Left + (.Width / 2), .Top + (.Height / 2))

                        Dim width As Single = .Width / 2
                        Dim height As Single = .Height / 2

                        Me._Children(NW) = New QuadLeafNode(Me, New RectangleF(.Left, .Top, width, height))
                        Me._Children(SW) = New QuadLeafNode(Me, New RectangleF(.Left, Me.Center.Y, width, height))
                        Me._Children(NE) = New QuadLeafNode(Me, New RectangleF(Me.Center.X, .Top, width, height))
                        Me._Children(SE) = New QuadLeafNode(Me, New RectangleF(Me.Center.X, Me.Center.Y, width, height))
                    Catch ex As Exception
                        Console.Error.WriteLine("[QuadRootNode:Insert Error] newBounds out of Bound.")
                        Console.Error.WriteLine(ex.Message & vbNewLine & ex.StackTrace)

                    End Try

                End With

                'copy newNode into newIndex
                Me._Children(newIndex) = newNode

                'done, now try again
                Return Me.Insert(point)

            End If
        End Function

        Protected Friend Overloads Function FindNearestNeighbour(ByVal target As System.Drawing.PointF, ByVal window As RectangleF, ByRef nearest As T, ByRef distance As Double) As Boolean

            Dim start As QuadNode = Me.FindSmallestContainingNode(window)

            Dim sqdist As Double = Double.MaxValue
            start.FindNearestNeighbour(target, window, nearest, sqdist)

            distance = sqdist ^ 0.5

            Return Not IsNothing(nearest)

        End Function

    End Class

    Protected Class QuadInternalNode
        Inherits QuadNode

        Public Sub New(ByVal parent As QuadInternalNode, ByVal bounds As RectangleF)
            MyBase.New(parent, bounds)

            With Me.Bounds
                Try

                    Dim width As Single = .Width / 2
                    Dim height As Single = .Height / 2
                    Me._Children(NW) = New QuadLeafNode(Me, New RectangleF(.Left, .Top, width, height))
                    Me._Children(SW) = New QuadLeafNode(Me, New RectangleF(.Left, Me.Center.Y, width, height))
                    Me._Children(NE) = New QuadLeafNode(Me, New RectangleF(Me.Center.X, .Top, width, height))
                    Me._Children(SE) = New QuadLeafNode(Me, New RectangleF(Me.Center.X, Me.Center.Y, width, height))
                Catch ex As Exception
                    Console.Error.WriteLine("[QuadInternalNode:New Error] unhandled error.")
                    Console.Error.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
                End Try
            End With

        End Sub

        Friend Sub New(ByVal parent As QuadInternalNode, ByVal bounds As RectangleF, ByVal children() As QuadNode)
            MyBase.New(parent, bounds)

            Dim index As Integer = 0
            For Each child As QuadNode In children
                Me._Children(index) = child
                child.Parent = Me
                index += 1
            Next

        End Sub


        Protected _Children(3) As QuadNode

        Protected Function GetIndexOfNearestChildNode(ByVal x As Double, ByVal y As Double) As Integer
            Dim minidx As Integer = -1

            With Me.Center
                If x < .X Then
                    If y < Me.Center.Y Then
                        Return NW
                    Else
                        Return SW
                    End If
                Else
                    If y < Me.Center.Y Then
                        Return NE
                    Else
                        Return SE
                    End If
                End If
            End With

            Return minidx
        End Function

        Protected Friend Overrides Function Insert(ByVal point As T) As QuadTree(Of T).QuadNode
            Dim index As Integer = Me.GetIndexOfNearestChildNode(point.X, point.Y)
            Me._Children(index) = Me._Children(index).Insert(point)
            Return Me
        End Function

        Protected Friend Overrides Function FindSmallestContainingNode(ByVal rect As System.Drawing.RectangleF) As QuadTree(Of T).QuadNode
            Dim index As Integer = Me.GetIndexOfNearestChildNode(rect.X, rect.Y)
            Dim child As QuadNode = Me._Children(index)
            If child.Bounds.Contains(rect) Then
                Return child.FindSmallestContainingNode(rect)
            Else
                Return Me
            End If
        End Function


        Protected Friend Overrides Sub FindNearestNeighbour(ByVal target As System.Drawing.PointF, ByRef window As System.Drawing.RectangleF, ByRef nearest As T, ByRef sqdist As Double)

            'first search in the child that is nearest the target
            'usually this will shrink the search-window to such small dimensions already so that
            'we can skip a lot of processing later on

            Dim index As Integer = Me.GetIndexOfNearestChildNode(target.X, target.Y)
            Dim child As QuadNode = Me._Children(index)
            child.FindNearestNeighbour(target, window, nearest, sqdist)

            If child.Bounds.Contains(window) Then
                'the search window is fully contained by the child and thus
                'no further searching is necessary, break off now
                Exit Sub

            Else
                'check remaining children
                For i As Integer = 0 To Me._Children.Length - 1

                    'skip the child that we already processed
                    If i = index Then Continue For

                    child = Me._Children(i)

                    If child.Bounds.IntersectsWith(window) Then
                        'this child might have a point that is even nearer, check it
                        child.FindNearestNeighbour(target, window, nearest, sqdist)
                    End If

                Next

            End If
        End Sub

    End Class

    Protected Class QuadLeafNode
        Inherits QuadNode

        Public Sub New(ByVal parent As QuadInternalNode, ByVal bounds As RectangleF)
            MyBase.New(parent, bounds)
        End Sub


        Private _Points As New List(Of T)

        Protected Function IsFull() As Boolean
            Return Me._Points.Count >= MaxNumPoints
        End Function

        Protected Friend Overrides Function Insert(ByVal point As T) As QuadTree(Of T).QuadNode

            If Not Me.IsFull Then
                'simply insert the point
                Me._Points.Add(point)
                Return Me

            Else
                If Me.Bounds.Width > 1 Then
                    'cells are square, so height will also be > 1

                    'split
                    Dim node As QuadNode = New QuadInternalNode(Me.Parent, Me.Bounds)
                    node = node.Insert(Me._Points.ToArray) 're-insert current points
                    node = node.Insert(point) 'insert the new point
                    Return node

                Else
                    'we will not get any smaller than this
                    'pretend the point was inserted
                    Return Me

                End If

            End If
        End Function

        Protected Friend Overrides Function FindSmallestContainingNode(ByVal rect As System.Drawing.RectangleF) As QuadTree(Of T).QuadNode
            Return Me
        End Function


        Protected Friend Overloads Overrides Sub FindNearestNeighbour(ByVal target As System.Drawing.PointF, ByRef window As System.Drawing.RectangleF, ByRef nearest As T, ByRef sqdist As Double)

            Dim foundOne As Boolean = False

            For Each point As T In Me._Points
                Dim dist As Double = (point.X - target.X) ^ 2 + (point.Y - target.Y) ^ 2
                If dist < sqdist Then
                    sqdist = dist
                    nearest = point
                    foundOne = True
                End If
            Next

            If foundOne Then

                'adjust search window
                Dim dist As Double = sqdist ^ 0.5

                window = New RectangleF( _
                    CType(target.X - dist, Single), _
                    CType(target.Y - dist, Single), _
                    CType(2 * dist, Single), _
                    CType(2 * dist, Single))

            End If

        End Sub

    End Class

End Class
