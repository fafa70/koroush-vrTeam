Public MustInherit Class Actor
    Inherits Sensor

    Protected Sub New(ByVal type As String, ByVal name As String)
        MyBase.New(type, name)
    End Sub

    Protected Sub SendUsarSimCommand(ByVal cmd As String)
        If IsNothing(Me.Agent) Then Throw New InvalidOperationException("Actor needs to be mounted on an Agent first")
        Me.Agent.SendUsarSimCommand(cmd)
    End Sub

End Class
