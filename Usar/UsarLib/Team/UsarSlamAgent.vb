Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Slam

''' <summary>
''' Specific Agent class used for matches.
''' </summary>
''' <remarks></remarks>
Public Class UsarSlamAgent
    Inherits UsarAgent

    Public Sub New(ByVal slam As ISlamStrategy, ByVal manifold As Manifold, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(Manifold, name, agentConfig, teamConfig)

        If IsNothing(slam) Then Throw New ArgumentNullException("slam")
        Me._Slam = slam
        Me._Slam.ApplyConfig(agentConfig)

    End Sub

    Protected _Slam As ISlamStrategy

    ''' <summary>
    ''' Forwards sensor readings to Slam.
    ''' </summary>
    ''' <param name="sensor"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub NotifySensorUpdate(ByVal sensor As Sensor)
        MyBase.NotifySensorUpdate(sensor)
        Me._Slam.ProcessSensorUpdate(sensor)
    End Sub

End Class
