''' <summary>
''' Driver Interface.
''' Is intended to encapsulate the data-flow between the agent 
''' and external sources.
''' 
''' The LogDriver is used to have an agent process logged sensor data.
''' The LifeDriver is used for agents that connect to a usarsim server.
''' </summary>
''' <remarks></remarks>
Public Interface IDriver

    ''' <summary>
    ''' This property should return True as long as the thread should continue
    ''' execution. Typically, there is a while-loop that checks this value
    ''' in every iteration.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ReadOnly Property IsRunning() As Boolean

    ''' <summary>
    ''' Should return True when the thread is to be paused (put in 
    ''' waiting state). 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ReadOnly Property IsPaused() As Boolean

    Sub Start()
    Sub Pause()
    Sub [Resume]()
    Sub [Stop]()

    ''' <summary>
    ''' The name of the file which is read or written to. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Property LogFile() As String


End Interface

