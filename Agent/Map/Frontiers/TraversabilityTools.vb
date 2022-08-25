' Class TraversabilityTools
' Contains tools needed for path planning using traversibility information.
' Part of the 2008/2009 Honours project where we look a traversability maps.
' Author: Maarten van der Velden, Bram Huijten & Wouter Josemans

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports System.Collections

Imports AForge
Imports AForge.Math
Imports AForge.ImaginG
Imports AForge.Imaging.Filters

Public Class TraversabilityTools
    Inherits FrontierTools

    ' Computes a path using the A* algorithm
    Public Function ComputePathPlanTraversability(ByVal occupancy As Bitmap, ByVal traversability As Bitmap, ByVal start As Point, ByVal target As Point) As List(Of Point)

        Dim bits As BitmapData = occupancy.LockBits(New Rectangle(0, 0, occupancy.Width, occupancy.Height), ImageLockMode.ReadOnly, occupancy.PixelFormat)
        Dim trav As BitmapData = traversability.LockBits(New Rectangle(0, 0, traversability.Width, traversability.Height), ImageLockMode.ReadWrite, traversability.PixelFormat)

        Console.WriteLine("[Pathplanner] Planning path from ({0},{1}) to ({2},{3})!", start.X, start.Y, target.X, target.Y)

        ' PQueue that will contain the to be considered nodes
        Dim open As New PriorityQueue(Of Double, PathNode)



            If bits.Height > 1 Then
                If (target.X < 0 OrElse target.Y < 0 OrElse target.X > bits.Width OrElse target.Y > bits.Height) Then
                    Console.WriteLine("[Pathplanner] Target is out of bounds!")
                Return New List(Of Point)
                End If

                ' If this point is an obstacle, terminate immediately
                If MyBase.ComputeDistanceToObstacle(target, bits) = 0 Then
                    Console.WriteLine("[Pathplanner] Target is obstacle!")
                    Console.WriteLine(String.Format("ComputePathPlanTraversability: WARNING target {0},{1} not reachable ", target.X, target.Y))
                    target = Me.ExpandEndPoint(start, target, bits)
                    Console.WriteLine(String.Format("ComputePathPlanTraversability: NEW target {0},{1} ", target.X, target.Y))
                End If

            If Me.ComputeDistanceToObstacle(start, bits) = 0 Then
                Console.WriteLine(String.Format("ComputePathPlan: WARNING start-point {0},{1} not reachable ", start.X, start.Y))
                start = Me.ExpandStartPoint(start, target, bits)
                Console.WriteLine(String.Format("ComputePathPlan: NEW start-point {0},{1}", start.X, start.Y))

            End If


            Else
                Console.WriteLine("[Pathplanner] No occupancy data available! Path may run through obstacles.")
            End If




            ' List that will contain the already considered points
            Dim closed As New List(Of Point)
            ' The "current node", the node that is being considered at every stage of the algorithm
            Dim curNode As New PathNode
            ' Indicates if we have found a path or not
            Dim FoundPath As Boolean = False
            ' Used to iterate 
            Dim i As Integer
            ' Will contain the neighbours of a node
            Dim neighbours As List(Of Point)
            ' Stores a single neighbour to simplify processing
            Dim neighbour As PathNode
            ' Create the first pathnode
            Dim node As New PathNode(0, start)
            ' Push the node into the priority queue
            open.Push(0, node)
            Dim displayCount As Integer = 0

            ' While there are elements left in open to consider, loop
            While (open.Count > 0)

                ' Take the first element of the open priorityqueue. This is always the path with the lowest cost.
                curNode = open.Pop

                ' Specify that we have already seen this point
                closed.Add(curNode.Head)
                displayCount += 1
                If displayCount = 1000 Then
                    Console.WriteLine("    [PathPlanner] Visited {0} Nodes! Current distance to goal: {1}", closed.Count, distance(target, curNode.Head))
                    displayCount = 0
                End If

                ' Check if we have reached the target
                If curNode.Head.X = target.X And curNode.Head.Y = target.Y Then
                    FoundPath = True
                    Exit While
                End If

                ' Get the neighbouring points
                neighbours = FindNeighbours(curNode, bits, closed)

                ' Add all neighbouring points to the open list
                For i = 0 To neighbours.Count - 1
                    ' Create a node for this neighbour
                    neighbour = New PathNode(curNode.Cost, curNode.Path)

                    ' Add the neighbour to this node
                    neighbour.Add(neighbours(i))

                    ' Get the total cost (g(x) + h(x))
                    neighbour.Cost = curNode.RealCost + g(curNode.Head, neighbour.Head, trav) + h(neighbour.Head, target) ^ 2

                    ' Store the real cost as well (the cost for driving from the start to the current position)
                    neighbour.RealCost = neighbour.Cost - h(neighbour.Head, target) ^ 3

                    ' Add the node to the queue
                    open.Push(neighbour.Cost, neighbour)
                Next

                Console.WriteLine(String.Format("PP costs {0}", curNode.RealCost))
            End While

            occupancy.UnlockBits(bits)
            'traversability.UnlockBits(trav)

            ' Return the path, if we found one. Returns an empty list if no path was found. Check for this in the calling function!
            If (FoundPath) Then
                Return curNode.Path
            Else
                Return New List(Of Point)
            End If


    End Function

    ' Finds neighbours of node. Filters neighbours that are blocked by obstacles and 
    Public Function FindNeighbours(ByVal node As PathNode, ByRef bits As BitmapData, ByRef closed As List(Of Point)) As List(Of Point)
        Dim curpoint As Point = node.Head
        Dim x As Integer, y As Integer
        Dim result As New List(Of Point)
        Dim nxtpoint As Point
        For y = curpoint.Y - 1 To curpoint.Y + 1
            For x = curpoint.X - 1 To curpoint.X + 1
                nxtpoint = New Point(x, y)

                ' If we've already processed this point, don't add it again
                If (closed.Contains(nxtpoint)) Then
                    Continue For
                End If

                ' Avoid planning past map boundaries
                If bits.Height > 1 Then
                    If (x < 0 OrElse y < 0 OrElse x > bits.Width OrElse y > bits.Height) Then
                        Continue For
                    End If


                    ' If this point is an obstacle, don't use it to plan
                    If MyBase.ComputeDistanceToObstacle(nxtpoint, bits) = 0 Then
                        Continue For
                    End If
                End If

                ' If none of the above apply, then it is safe to add the point to the result
                result.Add(nxtpoint)
            Next
        Next

        Return result

    End Function

    Function findSmallest(ByVal l As List(Of PathNode)) As PathNode
        Dim i As Integer
        Dim r As PathNode = l(0)
        For i = 1 To l.Count - 1
            If l(i).Cost < r.Cost Then
                r = l(i)
            End If
        Next
        Return r
    End Function

    ' Inner class describing a path node. Contains information such as the path from the start 
    ' to the node, and the cost of that path.
    Public Class PathNode

        Public Sub New()
            Me._Cost = 0
            Me._RealCost = 0
            Me._Path = New List(Of Point)
        End Sub

        Public Sub New(ByVal cost As Double, ByVal path As List(Of Point))
            Me._Cost = cost
            Me._Path = New List(Of Point)(path)
        End Sub

        Public Sub New(ByVal cost As Double, ByVal p As Point)
            Me._RealCost = 0
            Me._Cost = cost
            Me._Path = New List(Of Point)
            Me._Path.Add(p)
        End Sub

        Private _Cost As Double

        Public Property Cost() As Double
            Set(ByVal value As Double)
                Me._Cost = value
            End Set
            Get
                Return Me._Cost
            End Get
        End Property

        Private _RealCost As Double

        Public Property RealCost() As Double
            Get
                Return Me._RealCost
            End Get
            Set(ByVal value As Double)
                Me._RealCost = value
            End Set
        End Property

        Private _Path As List(Of Point)
        Public ReadOnly Property Path() As List(Of Point)
            Get
                Return Me._Path
            End Get
        End Property

        Public Function Head() As Point
            Return Me._Path(Me._Path.Count - 1)
        End Function

        Public Sub Add(ByVal p As Point)
            Me._Path.Add(p)
        End Sub
    End Class

    ' Calculate the distance between two points
    Private Function distance(ByVal a As Point, ByVal b As Point) As Double
        Return Sqrt(((b.X - a.X) ^ 2) + ((b.Y - a.Y) ^ 2))
    End Function

    ' Calculate estimated cost (now simply the distance between two points)
    Private Function h(ByVal a As Point, ByVal b As Point) As Double
        Return distance(a, b)
    End Function

    ' Calculate the real cost for the path so far
    Private Function g(ByVal a As Point, ByVal b As Point, ByRef trav As BitmapData) As Double
        Return distance(a, b) / getTraversability(a, b, trav)
    End Function

    ' Get traversability at certain index
    Private Function getTraversability(ByVal a As Point, ByVal b As Point, ByRef trav As BitmapData) As Double
        Return 1
    End Function

    Private Function ExpandStartPoint(ByVal start As Point, ByVal target As Point, ByRef bits As BitmapData) As Point

        Dim curpoint As Point = start
        Dim nxtpoint As Point, x As Integer, y As Integer
        Dim differenceX As Double = target.X - start.X
        Dim differenceY As Double = target.Y - start.Y
        Dim history As New List(Of Point)(New Point() {start})



        If differenceX > differenceY Then
            y = curpoint.Y
            For x = curpoint.X To curpoint.X + CInt(differenceX / 10) Step Sign(differenceX)

                nxtpoint = New Point(x, y)

                If history.Contains(nxtpoint) Then
                    'already processed
                    Continue For

                Else
                    history.Insert(0, nxtpoint)
                    If x < 0 OrElse y < 0 OrElse x > bits.Width OrElse y > bits.Height Then
                        'avoid planning beyond map bounds
                        Continue For
                    End If
                End If

                If Me.ComputeDistanceToObstacle(nxtpoint, bits) = 0 Then
                    'this point is an obstacle
                    Continue For
                Else
                    Return nxtpoint
                End If


            Next
        Else
            x = curpoint.Y
            For y = curpoint.Y To curpoint.Y + CInt(differenceY / 10) Step Sign(differenceY)

                nxtpoint = New Point(x, y)

                If history.Contains(nxtpoint) Then
                    'already processed
                    Continue For

                Else
                    history.Insert(0, nxtpoint)
                    If x < 0 OrElse y < 0 OrElse x > bits.Width OrElse y > bits.Height Then
                        'avoid planning beyond map bounds
                        Continue For
                    End If
                End If

                If Me.ComputeDistanceToObstacle(nxtpoint, bits) = 0 Then
                    'this point is an obstacle
                    Continue For
                Else
                    Return nxtpoint
                End If


            Next
        End If


        Return Nothing

    End Function

    Private Function ExpandEndPoint(ByVal start As Point, ByVal target As Point, ByRef bits As BitmapData) As Point

        Dim curpoint As Point = target
        Dim nxtpoint As Point, x As Integer, y As Integer
        Dim differenceX As Double = target.X - start.X
        Dim differenceY As Double = target.Y - start.Y
        Dim history As New List(Of Point)(New Point() {target})



        If Abs(differenceX) > Abs(differenceY) Then
            y = curpoint.Y
            For x = curpoint.X To (curpoint.X - CInt(differenceX / 10)) Step -Sign(differenceX)

                nxtpoint = New Point(x, y)

                If history.Contains(nxtpoint) Then
                    'already processed
                    Continue For

                Else
                    history.Insert(0, nxtpoint)
                    If x < 0 OrElse y < 0 OrElse x > bits.Width OrElse y > bits.Height Then
                        'avoid planning beyond map bounds
                        Continue For
                    End If
                End If

                If Me.ComputeDistanceToObstacle(nxtpoint, bits) = 0 Then
                    'this point is an obstacle
                    Continue For
                Else
                    Return nxtpoint
                End If


            Next
        Else
            x = curpoint.X
            For y = curpoint.Y To (curpoint.Y - CInt(differenceY / 10)) Step -Sign(differenceY)

                nxtpoint = New Point(x, y)

                If history.Contains(nxtpoint) Then
                    'already processed
                    Continue For

                Else
                    history.Insert(0, nxtpoint)
                    If x < 0 OrElse y < 0 OrElse x > bits.Width OrElse y > bits.Height Then
                        'avoid planning beyond map bounds
                        Continue For
                    End If
                End If

                If Me.ComputeDistanceToObstacle(nxtpoint, bits) = 0 Then
                    'this point is an obstacle
                    Continue For
                Else
                    Return nxtpoint
                End If


            Next
        End If


        Return Nothing

    End Function

End Class
