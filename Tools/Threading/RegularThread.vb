Imports System.Threading
Imports System.Globalization

Public MustInherit Class RegularThread

    Private wrapped As Thread

    Public Sub New()
        If Not CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = NumberFormatInfo.InvariantInfo.NumberDecimalSeparator Then
            'This prevents the WssDevice from receiving huge pathloss
            Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US", False)
            Console.WriteLine("[RegularThread]: CurrentCulture is now {0}.", CultureInfo.CurrentCulture.Name)
        End If
        Me.wrapped = New Thread(AddressOf Me.Run)
    End Sub


    Protected MustOverride Sub Run()



#Region " Starting and Stopping the wrapped Thread "

    Private mutex As New Object

    Private _IsRunning As Boolean = False
    Public Overridable ReadOnly Property IsRunning() As Boolean
        Get
            'both UI and agent read this property, use mutex
            SyncLock Me.mutex
                Return _IsRunning
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Starts the wrapped thread. The Run member is referenced as the
    ''' the main executive routine of this thread.
    ''' 
    ''' Please note that by default the wrapped thread will run with
    ''' lower priority than the UI in order to ensure UI responsiveness.
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Start()

        SyncLock Me.mutex
            Me._IsRunning = True
        End SyncLock

        'starting the wrapped thread twice will raise an exception already
        'on the Thread object, so no need for error checking here.
        'Me.wrapped.Priority = Threading.ThreadPriority.BelowNormal
        Me.wrapped.Start()

        If Thread.CurrentThread.Name = Nothing AndAlso Me.wrapped.Name = Nothing Then
            Debug.WriteLine("[RegularThread] Nameless Thread Started by Nameless Thread")
        ElseIf Thread.CurrentThread.Name = Nothing Then
            Debug.WriteLine(String.Format("[RegularThread] Thread {0} Started by Nameless Thread", Me.wrapped.Name))
        ElseIf Me.wrapped.Name = Nothing Then
            Debug.WriteLine(String.Format("[RegularThread] Nameless Thread Started by Thread {0}", Thread.CurrentThread.Name))
        Else
            Debug.WriteLine(String.Format("[RegularThread] {0} Started Thread {1}", Thread.CurrentThread.Name, Me.wrapped.Name))
        End If

    End Sub

    ''' <summary>
    ''' Stops the wrapped thread. 
    ''' 
    ''' The wrapped thread will be politely asked to stop by setting
    ''' IsRunning to false. After 10 seconds of no reaction the 
    ''' wrapped thread will be forced to abort.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub [Stop]()
        If Thread.CurrentThread.Name = Nothing AndAlso Me.wrapped.Name = Nothing Then
            Debug.WriteLine("[RegularThread] Nameless Thread Stopped by Nameless Thread")
        ElseIf Thread.CurrentThread.Name = Nothing Then
            Debug.WriteLine(String.Format("[RegularThread] Thread {0} Stopped by Nameless Thread", Me.wrapped.Name))
        ElseIf Me.wrapped.Name = Nothing Then
            Debug.WriteLine(String.Format("[RegularThread] Nameless Thread Stopped by Thread {0}", Thread.CurrentThread.Name))
        Else
            Debug.WriteLine(String.Format("[RegularThread] {0} Stopped Thread {1}", Thread.CurrentThread.Name, Me.wrapped.Name))
        End If

        If Me.IsRunning Then
            SyncLock Me.mutex
                Me._IsRunning = False
            End SyncLock

            If Not Me.wrapped.Join(TimeSpan.FromSeconds(1)) Then
                Me.wrapped.Abort()
            End If
        End If

    End Sub

#End Region

End Class
