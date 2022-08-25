Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Slam

''' <summary>
''' Base class used for USAR Agents.
''' </summary>
''' <remarks></remarks>
Public Class UsarOperatorAgent
    Inherits OperatorAgent

    Public Sub New(ByVal manifold As Manifold, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(manifold, name, agentConfig, teamConfig)
    End Sub

End Class
