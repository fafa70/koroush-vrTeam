'Class: PriorityFront
'Author: Rasto Novotny
'Date: 15.11.2005
'Description: Priority is a collection like stack. However, it is able to assign a priority to each inserting item and items are popped ordered by priorities.
'Copyright: This source coe is property of author and it is not able to use it for commercial purposes without permission of the author.

''' <summary>
''' Indicates, which priority is poped first from the front
''' </summary>
''' <remarks></remarks>
Public Enum PriorityTarget As Integer
    ''' <summary>
    ''' Items with greater priority are poped first from priority front
    ''' </summary>
    ''' <remarks></remarks>
    MaximumFirts = 0
    ''' <summary>
    ''' Items with lower priority are poped first from priority front
    ''' </summary>
    ''' <remarks></remarks>
    MinimumFirst = -2
End Enum

Public Class PriorityQueue(Of TPriority, TValue)
    Implements ICollection, IEnumerable(Of TValue)

    Private intCount As Integer
    Private objFirstLevel As HeapLevel
    Private objLastLevel As HeapLevel
    Private enTarget As PriorityTarget = PriorityTarget.MinimumFirst
    Private objComparer As IComparer(Of TPriority)
    Private Version As UInteger

    ''' <summary>
    ''' Create new priority front
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        Me.objFirstLevel = New HeapLevel(Me)
        Me.objLastLevel = Me.objFirstLevel
    End Sub

    ''' <summary>
    ''' Create new priority front
    ''' </summary>
    ''' <param name="Target">indicates, which priority is poped first from the front</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal Target As PriorityTarget)
        Me.New()
        Me.enTarget = Target
    End Sub

    ''' <summary>
    ''' Create new priority front
    ''' </summary>
    ''' <param name="Comparer">Comparer used to compare priorities</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal Comparer As IComparer(Of TPriority))
        Me.New()
        Me.objComparer = Comparer
    End Sub

    ''' <summary>
    ''' Create new priority front
    ''' </summary>
    ''' <param name="Target">indicates, which priority is poped first from the front</param>
    ''' <param name="Comparer">Comparer used to compare priorities</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal Target As PriorityTarget, ByVal Comparer As IComparer(Of TPriority))
        Me.New()
        Me.enTarget = Target
        Me.objComparer = Comparer
    End Sub

    ''' <summary>
    ''' Gets value indicating, which priority is poped first from the front
    ''' </summary>
    ''' <value>indicates, which priority is poped first from the front</value>
    ''' <returns>value indicating, which priority is poped first from the front</returns>
    ''' <remarks>Default is MaximumFirst</remarks>
    Public ReadOnly Property Target() As PriorityTarget
        Get
            Return Me.enTarget
        End Get
    End Property

    ''' <summary>
    ''' Add new item into priority front
    ''' </summary>
    ''' <param name="Priority">Priority of the item</param>
    ''' <param name="Value">Item to add</param>
    ''' <remarks></remarks>
    Public Sub Push(ByVal Priority As TPriority, ByVal Value As TValue)
        'If priority is not specified then throw exception
        If Priority Is Nothing Then
            Throw New ArgumentNullException("Priority", "Priority cannot be null")
        End If

        'Create heap item and add it to the end of the heap
        Me.objLastLevel.AddItem(New HeapItem(Priority, Value))
        'Increment items count
        Me.intCount += 1

        'Hypify parent of the last item in the heap. If something was moved then heapify parent of the parent and so on...
        Dim iLevel As HeapLevel = Me.objLastLevel.PrevLevel
        Dim iIndex As Integer = Me.objLastLevel.ItemsCount - 1

        'Do until top of heap is reached
        Do Until iLevel Is Nothing
            'Set index to the parent
            iIndex >>= 1
            Dim MoveParent As Integer
            'Heapify parent
            MoveParent = iLevel.Heapify(iIndex)
            'If nothing was moved then work is done
            If MoveParent = 0 Then Exit Do
            'Move to previous level
            iLevel = iLevel.PrevLevel
        Loop

        Me.AdvanceNextVersion()
    End Sub

    ''' <summary>
    ''' Returns item with minimal or maximal priority and remove it from the priority front
    ''' </summary>
    ''' <returns>Returns item with minimal or maximal priority</returns>
    ''' <remarks></remarks>
    Public Function Pop() As TValue
        'If priority front is empty then throw exception
        If Me.intCount = 0 Then
            Throw New InvalidOperationException("Priority front is empty.")
        End If

        'Return item from top of the heap
        Dim RetVal As TValue = Me.objFirstLevel.Items(0).Value

        If Me.objFirstLevel Is Me.objLastLevel Then
            'If there exist only one level of the heap, then remove item from this level
            Me.objFirstLevel.RemoveItem()
        Else
            'Move last item of the heap to the top of the heap
            Dim LastIndex As Integer = Me.objLastLevel.ItemsCount - 1
            Me.objFirstLevel.Items(0) = Me.objLastLevel.RemoveItem()

            'Heapify top of the heap. If something was moved then heapify switched child and so on...
            Dim iLevel As HeapLevel = Me.objFirstLevel
            Dim iIndex As Integer = 0
            Dim MoveParent As Integer
            Do
                'Heapify parent
                MoveParent = iLevel.Heapify(iIndex)
                'Set index to left child
                iIndex <<= 1
                'If parent was moved right, then set index to rgiht child
                If MoveParent = 1 Then
                    iIndex = iIndex Or 1
                End If
                'Advance to next level
                iLevel = iLevel.NextLevel
                'If parent was not moved, then work is done
            Loop Until MoveParent = 0
        End If

        Me.intCount -= 1

        Me.AdvanceNextVersion()
        Return RetVal
    End Function

    ''' <summary>
    ''' Returns item with minimal or maximal priority
    ''' </summary>
    ''' <returns>Returns item with minimal or maximal priority</returns>
    ''' <remarks></remarks>
    Public Function Peek() As TValue
        'If priority front is empty then throw exception
        If Me.intCount = 0 Then
            Throw New InvalidOperationException("Priority front is empty.")
        End If

        'Return item from top of the heap
        Return Me.objFirstLevel.Items(0).Value
    End Function

    ''' <summary>
    ''' Gets number of items in priority front
    ''' </summary>
    ''' <value>number of items in priority front</value>
    ''' <returns>number of items in priority front</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Count() As Integer Implements System.Collections.ICollection.Count
        Get
            Return Me.intCount
        End Get
    End Property

    ''' <summary>
    ''' Change version number indicating that priority front has changed
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub AdvanceNextVersion()
        Me.Version += CType(1, UInteger)
        Me.Version = Me.Version Mod CType(2147483648, UInteger)
    End Sub

    ''' <summary>
    ''' Compare two priorities taking Target property into account. If Target is MinimumFirst, then lower priority is considered as greater
    ''' </summary>
    ''' <param name="Priority1">Priority 1 to compare</param>
    ''' <param name="Priority2">Priority 2 to compare</param>
    ''' <returns>lower than 0: if Priority 1 is greater
    ''' 0: if Priority 1 is equal to Priority 2
    ''' greater than 0: if Priority 2 is greater</returns>
    ''' <remarks></remarks>
    Private Function ComparePriorities(ByVal Priority1 As TPriority, ByVal Priority2 As TPriority) As Integer
        Try
            If Me.objComparer IsNot Nothing Then
                'If comparer is available then compare priorities using it
                Return Me.objComparer.Compare(Priority1, Priority2) * (Me.Target + 1)
            Else
                If TypeOf Priority1 Is IComparable(Of TPriority) Then
                    'If Priority1 is comparable with type of priority then use it to compare
                    Dim ComparablePriority As IComparable(Of TPriority)
                    ComparablePriority = CType(Priority1, IComparable(Of TPriority))
                    Return ComparablePriority.CompareTo(Priority2) * (Me.Target + 1)
                ElseIf TypeOf Priority1 Is IComparable Then
                    'If Priority1 is comparable then use it to compare
                    Dim ComparablePriority As IComparable
                    ComparablePriority = CType(Priority1, IComparable)
                    Return ComparablePriority.CompareTo(Priority2) * (Me.Target + 1)
                End If
            End If
        Catch ex As Exception
            'Throw exception if exception was thrown during comparison
            Throw New InvalidOperationException("Cannot compare priorities.", ex)
        End Try

        'Throw exception if priorities are not compareable
        Throw New InvalidOperationException("Cannot compare priorities.")
    End Function

    ''' <summary>
    ''' Return enumerator to enumerate values in priority front
    ''' </summary>
    ''' <returns>enumerator to enumerate values in priority front</returns>
    ''' <remarks>This enumerator enumerates only values (not priorities).
    ''' It does not enumerate values sorted by priority.</remarks>
    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of TValue) Implements System.Collections.Generic.IEnumerable(Of TValue).GetEnumerator
        Return New PriorityFrontEnumerator(Me)
    End Function

    ''' <summary>
    ''' Copy values from priority front to an array
    ''' </summary>
    ''' <param name="array">The one-dimensional System.Array that is the destination of the elements copied from System.Collections.ICollection. The System.Array must have zero-based indexing.</param>
    ''' <param name="index">The zero-based index in array at which copying begins.</param>
    ''' <remarks></remarks>
    Public Sub CopyTo(ByVal array As System.Array, ByVal index As Integer) Implements System.Collections.ICollection.CopyTo
        If array Is Nothing Then
            Throw New ArgumentNullException("array", "Supplied array cannot be null")
        End If
        If index < 0 Then
            Throw New ArgumentOutOfRangeException("index", "Index cannot be negative")
        End If
        If array.Rank > 1 Then
            Throw New ArgumentException("Array cannot be multidimensional")
        End If
        If index >= array.Length Then
            Throw New ArgumentException("Index is greater than lenght of array")
        End If
        If Me.Count + index > array.Length Then
            Throw New ArgumentException("The number of elements in the source System.Collections.ICollection is greater than the available space from index to the end of the destination array.")
        End If

        For Each iValue As TValue In Me
            array.SetValue(iValue, index)
            index += 1
        Next
    End Sub

    ''' <summary>
    ''' Return true if priority front is synchronized
    ''' </summary>
    ''' <value>true if priority front is synchronized</value>
    ''' <returns>true if priority front is synchronized</returns>
    ''' <remarks>Always return false</remarks>
    Public ReadOnly Property IsSynchronized() As Boolean Implements System.Collections.ICollection.IsSynchronized
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Return object used to synchronize access to collection
    ''' </summary>
    ''' <value>object used to synchronize access to collection</value>
    ''' <returns>object used to synchronize access to collection</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property SyncRoot() As Object Implements System.Collections.ICollection.SyncRoot
        Get
            Return Me
        End Get
    End Property

    Private Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return Me.GetEnumerator
    End Function

    ''' <summary>
    ''' This structure defines pair of value and it's priority
    ''' </summary>
    ''' <remarks></remarks>
    Private Structure HeapItem
        Private objPriority As TPriority
        Private objValue As TValue

        Public Sub New(ByVal Priority As TPriority, ByVal Value As TValue)
            Me.objPriority = Priority
            Me.objValue = Value
        End Sub

        Public ReadOnly Property Priority() As TPriority
            Get
                Return Me.objPriority
            End Get
        End Property

        Public ReadOnly Property Value() As TValue
            Get
                Return Me.objValue
            End Get
        End Property
    End Structure

    ''' <summary>
    ''' This class represents level in heap
    ''' </summary>
    ''' <remarks>Heap is binary tree of heap items, where each item's priority is greater than it's children. So at the top is maximum.
    ''' Binary tree is encoded as doubly linked list of arrays, where each array is level in the tree. So level 0 contains only one item and each level contains 2x more items as previous level.</remarks>
    Private Class HeapLevel
        Private objRoot As PriorityQueue(Of TPriority, TValue)
        Public ItemsCount As Integer
        Public Items() As HeapItem
        Public PrevLevel As HeapLevel
        Public NextLevel As HeapLevel

        ''' <summary>
        ''' Create root (0-level) of the heap. This level can contain only one item
        ''' </summary>
        ''' <param name="Root">Priority front to associate</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal Root As PriorityQueue(Of TPriority, TValue))
            Me.objRoot = Root
            Me.Items = New HeapItem(0) {}
        End Sub

        ''' <summary>
        ''' Create next level of the heap.
        ''' </summary>
        ''' <param name="PrevLevel">Previous level of the heap to associate</param>
        ''' <remarks></remarks>
        Private Sub New(ByVal PrevLevel As HeapLevel)
            'Associate root from previous level
            Me.objRoot = PrevLevel.Root

            'Link levels to each other
            Me.PrevLevel = PrevLevel
            PrevLevel.NextLevel = Me

            'Create array with double size
            Dim NewLevelSize As Integer = Me.PrevLevel.LevelSize
            NewLevelSize <<= 1
            Me.Items = New HeapItem(NewLevelSize - 1) {}
        End Sub

        ''' <summary>
        ''' Return associated priority level
        ''' </summary>
        ''' <value>associated priority level</value>
        ''' <returns>associated priority level</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Root() As PriorityQueue(Of TPriority, TValue)
            Get
                Return Me.objRoot
            End Get
        End Property

        ''' <summary>
        ''' Return number of items in this level
        ''' </summary>
        ''' <value>number of items in this level</value>
        ''' <returns>number of items in this level</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LevelSize() As Integer
            Get
                Return Me.Items.Length
            End Get
        End Property

        ''' <summary>
        ''' Return true if level is full and next level have to be created
        ''' </summary>
        ''' <value>true if level is full</value>
        ''' <returns>true if level is full</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsFull() As Boolean
            Get
                Return Me.ItemsCount = Me.LevelSize
            End Get
        End Property

        ''' <summary>
        ''' Add item at the end of level or create new level if this level is full
        ''' </summary>
        ''' <param name="TheItem">item to add</param>
        ''' <remarks></remarks>
        Public Sub AddItem(ByVal TheItem As HeapItem)
            If Me.IsFull Then
                'If level is full then create new level
                Dim NewLevel As New HeapLevel(Me)
                'Set new level as the last level in priority front
                Me.Root.objLastLevel = NewLevel
                'Add item to new level
                NewLevel.AddItem(TheItem)
            Else
                'Add item to the end and increment items count
                Me.Items(Me.ItemsCount) = TheItem
                Me.ItemsCount += 1
            End If
        End Sub

        ''' <summary>
        ''' Remove last item or remove this level, if it is empty
        ''' </summary>
        ''' <returns>removed heap item</returns>
        ''' <remarks></remarks>
        Public Function RemoveItem() As HeapItem
            If Me.ItemsCount = 0 Then
                'If level is empty then delete this level
                Me.PrevLevel.NextLevel = Nothing
                'Set previous level as the last level
                Me.Root.objLastLevel = Me.PrevLevel
                'Remove item from previous level
                Return Me.PrevLevel.RemoveItem()
            Else
                'Decrement items count and delete last item
                Me.ItemsCount -= 1
                Dim RetVal As HeapItem = Me.Items(Me.ItemsCount)
                Me.Items(Me.ItemsCount) = New HeapItem
                Return RetVal
            End If
        End Function

        ''' <summary>
        ''' Compare heap item with it's children and move greates item to the top.
        ''' </summary>
        ''' <param name="Index">Index of heap item, which compare</param>
        ''' <returns>-1: if left child was moved to top.
        ''' 1: if right child was moved to top.
        ''' 0: if nothing was moved</returns>
        ''' <remarks></remarks>
        Public Function Heapify(ByVal Index As Integer) As Integer
            Dim MoveParent As Integer

            'Compare with children, if there exists next level
            If Me.NextLevel IsNot Nothing Then
                'Index of left child
                Dim LeftIndex As Integer = Index << 1
                'Index of right child
                Dim RightIndex As Integer = LeftIndex Or 1
                Dim IsLeftChildGreaterThanParent, IsRightChildGreaterThanParent As Boolean
                'If exists left child then compare parent with left child
                If LeftIndex < Me.NextLevel.ItemsCount Then
                    IsLeftChildGreaterThanParent = Me.Root.ComparePriorities(Me.Items(Index).Priority, Me.NextLevel.Items(LeftIndex).Priority) < 0
                End If
                'If exists right child then compare parent with right child
                If RightIndex < Me.NextLevel.ItemsCount Then
                    IsRightChildGreaterThanParent = Me.Root.ComparePriorities(Me.Items(Index).Priority, Me.NextLevel.Items(RightIndex).Priority) < 0
                End If

                If IsLeftChildGreaterThanParent And IsRightChildGreaterThanParent Then
                    'If both children are greater than parent then compare children
                    If Me.Root.ComparePriorities(Me.NextLevel.Items(LeftIndex).Priority, Me.NextLevel.Items(RightIndex).Priority) > 0 Then
                        'If left child is greater then switch parent with left child
                        MoveParent = -1
                    Else
                        'If right child is greater then switch parent with right child
                        MoveParent = 1
                    End If
                ElseIf IsLeftChildGreaterThanParent Then
                    'If left child is greater than parent then switch parent with left child
                    MoveParent = -1
                ElseIf IsRightChildGreaterThanParent Then
                    'If right child is greater than parent then switch parent with right child
                    MoveParent = 1
                End If

                If MoveParent <> 0 Then
                    'Store parent to temp variable
                    Dim TempHT As HeapItem
                    TempHT = Me.Items(Index)
                    If MoveParent = -1 Then
                        'Move left child to top
                        Me.Items(Index) = Me.NextLevel.Items(LeftIndex)
                        'Move parent to left child position
                        Me.NextLevel.Items(LeftIndex) = TempHT
                    ElseIf MoveParent = 1 Then
                        'Move right child to top
                        Me.Items(Index) = Me.NextLevel.Items(RightIndex)
                        'Move parent to right child position
                        Me.NextLevel.Items(RightIndex) = TempHT
                    End If
                End If
            End If

            Return MoveParent
        End Function
    End Class

    ''' <summary>
    ''' Implements enumerator for priority front
    ''' </summary>
    ''' <remarks>This enumerator enumerates only values (not priorities).
    ''' It does not enumerate values sorted by priority.</remarks>
    Private Class PriorityFrontEnumerator
        Implements IEnumerator(Of TValue), IEnumerator

        Private MyPriorityFront As PriorityQueue(Of TPriority, TValue)
        Private CurrentLevel As HeapLevel
        Private CurrentIndex As Integer
        Private CurrentValue As TValue
        Private MyVersion As UInteger

        ''' <summary>
        ''' Creates new priority front enumerator
        ''' </summary>
        ''' <param name="PF">priority front to enumerate</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal PF As PriorityQueue(Of TPriority, TValue))
            Me.MyPriorityFront = PF
            Me.MyVersion = PF.Version
            Me.Reset()
        End Sub

        ''' <summary>
        ''' Returns current value
        ''' </summary>
        ''' <value>current value</value>
        ''' <returns>current value</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Current() As TValue Implements System.Collections.Generic.IEnumerator(Of TValue).Current
            Get
                Return Me.CurrentValue
            End Get
        End Property

        Private ReadOnly Property Current1() As Object Implements System.Collections.IEnumerator.Current
            Get
                Return Me.Current
            End Get
        End Property

        ''' <summary>
        ''' Advance to next value in priority front
        ''' </summary>
        ''' <returns>Return false, if end of priority front is reached; otherwise it returns true</returns>
        ''' <remarks></remarks>
        Public Function MoveNext() As Boolean Implements System.Collections.IEnumerator.MoveNext
            'If priority fron has changed then throw exception
            If Me.MyVersion <> Me.MyPriorityFront.Version Then
                Throw New InvalidOperationException("Priority front has changed")
            End If

            'If end of priority front has been reached then return false
            If Me.CurrentLevel Is Nothing Then Return False

            'Advance to next heap item
            Me.CurrentIndex += 1
            If Me.CurrentLevel.ItemsCount <= Me.CurrentIndex Then
                'If end of level was reached, then advance to next level
                Me.CurrentLevel = Me.CurrentLevel.NextLevel
                Me.CurrentIndex = 0
                'If this is last level, then return false
                If Me.CurrentLevel Is Nothing Then Return False
            End If

            'Set current enumerating value
            Me.CurrentValue = Me.CurrentLevel.Items(Me.CurrentIndex).Value
            'Return true that sucessfully advanced to next value
            Return True
        End Function

        ''' <summary>
        ''' Reset enumerator
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Reset() Implements System.Collections.IEnumerator.Reset
            Me.CurrentLevel = Me.MyPriorityFront.objFirstLevel
            Me.CurrentIndex = -1
            Me.MyVersion = Me.MyPriorityFront.Version
        End Sub

        Private disposedValue As Boolean = False        ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    'Do nothing
                End If

                'Do nothing
            End If
            Me.disposedValue = True
        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Class