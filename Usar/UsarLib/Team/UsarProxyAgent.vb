Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Slam

''' <summary>
''' Specific Agent class used for matches.
''' </summary>
''' <remarks></remarks>
Public Class UsarProxyAgent
    Inherits ProxyAgent

    Public Sub New(ByVal manifold As Manifold, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(manifold, name, agentConfig, teamConfig)
    End Sub

End Class
