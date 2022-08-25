''' <summary>
''' A (limited) queue of Graphical Object.
''' </summary>
''' <typeparam name="TData"></typeparam>
''' <remarks>Copy/Edit of MultiStateSensor, without Sensor inheritance.</remarks>
Public MustInherit Class DataQueue(Of TData As IGraphicalObjectData)

    Private _history As Queue(Of TData)

    Protected Sub New(ByVal type As String, ByVal name As String, ByVal maxHistoryLength As Integer)
        Me._history = New Queue(Of TData)
        Me._MaxHistoryLength = maxHistoryLength
    End Sub

    Private _MaxHistoryLength As Integer
    Public ReadOnly Property MaxHistoryLength() As Integer
        Get
            Return _MaxHistoryLength
        End Get
    End Property


    Public Function PopData() As TData
        Return _history.Dequeue
    End Function

    Public Function PeekData() As TData
        Return _history.Peek
    End Function

    Public Function DataCount() As Integer
        Return _history.Count
    End Function

    Public Sub EnqueueData(ByVal data As TData)
        Me._history.Enqueue(data)
        While _history.Count > Me._MaxHistoryLength
            Me._history.Dequeue()
        End While
    End Sub

    Public Sub ClearData()
        Me._history.Clear()
    End Sub

End Class
