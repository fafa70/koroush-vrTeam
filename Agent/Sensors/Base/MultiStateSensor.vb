''' <summary>
''' A multi-state sensor holds a (limited) history of sensor readings.
''' </summary>
''' <typeparam name="TData"></typeparam>
''' <remarks></remarks>
Public MustInherit Class MultiStateSensor(Of TData As ISensorData)
    Inherits Sensor

    Private _history As Queue(Of TData)

    Protected Sub New(ByVal type As String, ByVal name As String, ByVal maxHistoryLength As Integer)
        MyBase.New(type, name)
        Me._history = New Queue(Of TData)
        Me._MaxHistoryLength = maxHistoryLength
    End Sub

    Private _MaxHistoryLength As Integer
    Public ReadOnly Property MaxHistoryLength() As Integer
        Get
            Return _MaxHistoryLength
        End Get
    End Property

    'this part causes error in image viewer
    Public Function PopData() As TData
        If _history.Count > 0 Then
        Return _history.Dequeue
        Else
            Console.Error.WriteLine("[MultiStateSensor:PopData Warning] trying to empty an empty queue")
        End If

    End Function

    Public Function PeekData() As TData
        Return _history.Peek
    End Function

    Public Function DataCount() As Integer
        Return _history.Count
    End Function

    Protected Sub EnqueueData(ByVal data As TData)
        Me._history.Enqueue(data)
        While _history.Count > Me._MaxHistoryLength
            Me._history.Dequeue()
        End While
    End Sub

    Public Sub ClearData()
        Me._history.Clear()
    End Sub

End Class
