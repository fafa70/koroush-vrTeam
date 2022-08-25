Public Class BenchMarker

#Region " Constructor "

    Public Sub New()
        Me._Timers = New Dictionary(Of Guid, TimerInfo)
        Me._Counters = New Dictionary(Of Guid, CounterInfo)
    End Sub

    Public Sub Reset()
        Me.StopAllTimers()
        For Each timer As TimerInfo In Me._Timers.Values
            timer.Reset()
        Next
        For Each counter As CounterInfo In Me._Counters.Values
            counter.Reset()
        Next
    End Sub

    Public Sub Clear()
        Me._Timers.Clear()
        Me._Counters.Clear()
    End Sub

#End Region

#Region " Timers "

    Private _Timers As Dictionary(Of Guid, TimerInfo)

    Public Function RegisterTimer(ByVal startImmediately As Boolean) As Guid
        Dim timer As New TimerInfo()
        Dim timerID As Guid = Guid.NewGuid
        Me._Timers(timerID) = timer
        If startImmediately Then
            Me.Start(timerID)
        End If
        Return timerID
    End Function

    Public Sub Start(ByVal timerID As Guid)
        If Not Me._Timers.ContainsKey(timerID) Then Throw New ArgumentException("timerID")
        Dim timer As TimerInfo = Me._Timers(timerID)
        timer.Start()
    End Sub

    Public Function [Stop](ByVal timerID As Guid) As TimeSpan
        If Not Me._Timers.ContainsKey(timerID) Then Throw New ArgumentException("timerID")
        Dim timer As TimerInfo = Me._Timers(timerID)
        timer.Stop()
        Return timer.GetDuration
    End Function


    Public Function Pause(ByVal timerID As Guid) As TimeSpan
        If Not Me._Timers.ContainsKey(timerID) Then Throw New ArgumentException("timerID")
        Dim timer As TimerInfo = Me._Timers(timerID)
        timer.Pause()
        Return timer.GetDuration
    End Function

    Public Function [Resume](ByVal timerID As Guid) As TimeSpan
        If Not Me._Timers.ContainsKey(timerID) Then Throw New ArgumentException("timerID")
        Dim timer As TimerInfo = Me._Timers(timerID)
        timer.Resume()
        Return timer.GetDuration
    End Function

    Public Function GetDuration(ByVal timerID As Guid) As TimeSpan
        If Not Me._Timers.ContainsKey(timerID) Then Throw New ArgumentException("timerID")
        Dim timer As TimerInfo = Me._Timers(timerID)
        Return timer.GetDuration
    End Function

    Public Sub StopAllTimers()
        For Each timer As TimerInfo In Me._Timers.Values
            timer.Stop()
        Next
    End Sub

#End Region

#Region " Counters "

    Private _Counters As Dictionary(Of Guid, CounterInfo)

    Public Function RegisterCounter() As Guid
        Dim counter As New CounterInfo()
        Dim counterID As Guid = Guid.NewGuid
        Me._Counters(counterID) = counter
        Return counterID
    End Function

    Public Function Increment(ByVal counterID As Guid) As Integer
        If Not Me._Counters.ContainsKey(counterID) Then Throw New ArgumentException("counterID")
        Dim counter As CounterInfo = Me._Counters(counterID)
        counter.Increment()
        Return counter.Count
    End Function

    Public Function Decrement(ByVal counterID As Guid) As Integer
        If Not Me._Counters.ContainsKey(counterID) Then Throw New ArgumentException("counterID")
        Dim counter As CounterInfo = Me._Counters(counterID)
        counter.Decrement()
        Return counter.Count
    End Function

    Public Function GetCount(ByVal counterID As Guid) As Integer
        If Not Me._Counters.ContainsKey(counterID) Then Throw New ArgumentException("counterID")
        Dim counter As CounterInfo = Me._Counters(counterID)
        Return counter.Count
    End Function


#End Region


#Region " Inner Classes "

    Private Class TimerInfo

        Private _start As DateTime
        Private _stop As DateTime

        Private _inactive As TimeSpan
        Private _pause As DateTime
        Private _resume As DateTime

        Private _started As Boolean
        Private _running As Boolean
        Private _stopped As Boolean

        Public Sub New()
            Me._started = False
            Me._running = False
            Me._stopped = False
            Me._inactive = TimeSpan.Zero
        End Sub

        Public Sub Reset()
            If Me._running Then
                Me.Stop()
            End If
            Me._started = False
            Me._running = False
            Me._stopped = False
            Me._inactive = TimeSpan.Zero
        End Sub

        Public Function GetDuration() As TimeSpan
            If Not Me._started Then
                Return TimeSpan.Zero
            Else
                Dim until As DateTime
                If Me._stopped Then
                    until = Me._stop
                ElseIf Not Me._running Then
                    until = Me._pause
                Else
                    until = DateTime.Now
                End If
                If until < Me._start Then Throw New InvalidOperationException("until is earlier than start")
                Return until.Subtract(Me._start).Subtract(Me._inactive)
            End If
        End Function



        Friend Sub Start()
            If Me._stopped Then Throw New InvalidOperationException("Already stopped")
            If Me._started Then Throw New InvalidOperationException("Already started")
            If Me._running Then Throw New InvalidOperationException("Already running")
            Me._start = DateTime.Now
            Me._started = True
            Me._running = True
        End Sub

        Friend Sub [Resume]()
            If Me._stopped Then Throw New InvalidOperationException("Already stopped")
            If Me._running Then Throw New InvalidOperationException("Already running")
            If Not Me._started Then
                Me.Start()
            Else
                Me._resume = DateTime.Now
                Me._inactive = Me._inactive.Add(Me._resume.Subtract(Me._pause))
                Me._running = True
            End If
        End Sub

        Friend Sub Pause()
            If Me._stopped Then Throw New InvalidOperationException("Already stopped")
            If Not Me._running Then Throw New InvalidOperationException("Already paused")
            Me._pause = DateTime.Now
            Me._running = False
        End Sub

        Friend Sub [Stop]()
            If Not Me._stopped Then
                If Me._running Then
                    Me.Pause()
                End If
                Me._stop = DateTime.Now
                Me._stopped = True
            End If
        End Sub

    End Class

    Private Class CounterInfo

        Private _count As Integer

        Public Sub New()
            Me._count = 0
        End Sub

        Public Sub Reset()
            Me._count = 0
        End Sub

        Public ReadOnly Property Count() As Integer
            Get
                Return Me._count
            End Get
        End Property

        Friend Sub Increment()
            Me._count += 1
        End Sub

        Friend Sub Decrement()
            Me._count -= 1
        End Sub

    End Class

#End Region

End Class
