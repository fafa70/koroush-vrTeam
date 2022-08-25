Imports System.Threading

''' <summary>
''' This class wraps a Thread in such a way that the main execution
''' can be paused without (!) busy-waiting to occur. So, pausing this
''' thread actually should yield the benefit of having resources being 
''' freed up for other threads.
''' </summary>
''' <remarks></remarks>
Public MustInherit Class PausableThread
    Inherits RegularThread

    'to lock objects that are potentially shared with the UI-thread.

    Public Sub New()
        MyBase.New()
    End Sub


#Region " Starting and Stopping the wrapped Thread "

    Public Overrides Sub [Stop]()

        If Me.IsPaused Then
            'unlock the waitHandle first
            Me.Resume()
        End If

        MyBase.Stop()

    End Sub

#End Region

#Region " Pausing and Resuming the Thread "

    Private mutex As New Object

    Private _IsPaused As Boolean = False
    Public Overridable ReadOnly Property IsPaused() As Boolean
        Get
            'this var is shared with UI, use mutex
            SyncLock Me.mutex
                Return _IsPaused
            End SyncLock
        End Get
    End Property


    Private waitHandle As New AutoResetEvent(False)


    ''' <summary>
    ''' Just sets the IsPaused flag to true. This does not 
    ''' actually pause the wrapped thread yet. Pausing will occur 
    ''' only as soon as CheckForPause is invoked.
    ''' 
    ''' It will always be an external (other) thread that invokes
    ''' Pause and Resume on this thread. However, the actual pausing 
    ''' should NOT happen on these, but on the wrapped thread. 
    ''' That's why pausing will only occur as soon as
    ''' the wrapped thread calls CheckForPause.
    ''' 
    ''' As a consequence, pausing will only have any effect if 
    ''' the Run method actually invokes CheckForPause from time to 
    ''' time.
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Pause()
        'just set the paused flag.
        SyncLock Me.mutex
            Me._IsPaused = True
        End SyncLock
    End Sub

    ''' <summary>
    ''' This member should be invoked by subclasses as part of
    ''' their Run routine. If the paused flag was set to true 
    ''' by an external thread (through invoking Pause) then the wrapped
    ''' thread will enter a (non-busy) waiting state. The wrapped 
    ''' thread continues execution when an external thread calls 
    ''' Resume.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CheckForPause()
        If Me.IsPaused Then
            'the wait handle will be activated on the wrapped thread
            'since it is (should be) the one invoking this check

            'on a sidenote, it IS possible that IsPaused was set to false between 
            'the check several lines up and invoking WaitOne below. This is fine though
            'since in that case WaitOne will return immediately.

            'if the waithandle was set, the invoking thread will be paused 
            '(possibly indefinitely since we don't specify a timeout) right here
            Me.waitHandle.WaitOne()

            'here, after the waithandle, the handle have been reset automatically

        End If
    End Sub

    ''' <summary>
    ''' Will resume the execution of the wrapped thread.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub [Resume]()
        If Me.IsPaused Then

            'unset the paused flag
            SyncLock Me.mutex
                Me._IsPaused = False
            End SyncLock

            'signal the waitHandle to unblock the wrapped thread 
            Me.waitHandle.Set()

        End If
    End Sub

#End Region

End Class
