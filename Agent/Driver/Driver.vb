Imports UvARescue.Tools

''' <summary>
''' Base Driver for Agents.
''' </summary>
''' <remarks></remarks>
Public MustInherit Class Driver
    Inherits PausableThread
    Implements IDriver

    Private _Agent As Agent

    Public Sub New(ByVal agent As Agent)
        If IsNothing(agent) Then Throw New ArgumentNullException("agent")
        Me._Agent = agent
        Me._Agent.Driver = Me
    End Sub

    ''' <summary>
    ''' The Agent will run in the Driver's thread. So between these
    ''' a single thread is assumed and hence they can access each other
    ''' freely without locking each other. If external parties need
    ''' access to the driver or agent, they should enforce thread-safety.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Agent() As Agent
        Get
            Return Me._Agent
        End Get
    End Property

    ''' <summary>
    ''' Wraps the base-class' member in order to implement the interface.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property IsRunning() As Boolean Implements IDriver.IsRunning
        Get
            'no locking, base class should have taken care of that
            Return MyBase.IsRunning
        End Get
    End Property

    ''' <summary>
    ''' Wraps the base-class' member in order to implement the interface.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property IsPaused() As Boolean Implements IDriver.IsPaused
        Get
            'no locking, base class should have taken care of that
            Return MyBase.IsPaused
        End Get
    End Property

    Public Overrides Sub Start() Implements IDriver.Start
        MyBase.Start()
        Me.Agent.OnAgentStarted()
    End Sub

    Public Overrides Sub Pause() Implements IDriver.Pause
        MyBase.Pause()
        Me.Agent.OnAgentPaused()
    End Sub

    Public Overrides Sub [Resume]() Implements IDriver.Resume
        MyBase.[Resume]()
        Me.Agent.OnAgentResumed()
    End Sub

    Public Overrides Sub [Stop]() Implements IDriver.Stop
        MyBase.Stop()
        Me.Agent.OnAgentStopped()
    End Sub

    Private _LogFile As String = ""
    Public Property LogFile() As String Implements IDriver.LogFile
        Get
            Return Me._LogFile
        End Get
        Set(ByVal value As String)
            Me._LogFile = value
        End Set
    End Property

End Class
