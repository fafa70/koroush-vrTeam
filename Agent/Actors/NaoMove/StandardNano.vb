Public Class StandardNano
    Inherits NaoMovement

    Private NaoAngle As Single
    Public Overrides Sub HeadPitch(ByVal angle As Single)
        Me.NaoAngle = angle
        Me.turn(Me.NaoAngle)

    End Sub

    Public Overrides Sub HeadYaw(ByVal angle As Single)
        Me.NaoAngle = angle
        Me.turn2(Me.NaoAngle)
    End Sub

    Public Overrides Sub move()
        Console.WriteLine("under construction")
    End Sub

    Protected Sub turn(ByVal angle As Single)
        Console.WriteLine("turn")
        Me.NaoAngle = angle
        Me.SendUsarSimCommand(String.Format("SET {{Type Joint}} {{Name HeadPitch}} {{Opcode Angle}} {{Params {0}}}", Me.NaoAngle))
    End Sub

    Protected Sub turn2(ByVal angle As Single)
        Console.WriteLine("turn2")
        Me.NaoAngle = angle
        Me.SendUsarSimCommand(String.Format("SET {{Type Joint}} {{Name HeadYaw}} {{Opcode Angle}} {{Params {0}}}", Me.NaoAngle))
    End Sub


    ''Private Sub SendUsarSimCommand(ByVal p1 As String, ByVal angle As Single)
    '  If IsNothing(Me.Agent) Then Throw New InvalidOperationException("Actor needs to be mounted on an Agent first")
    ' Me.Agent.SendUsarSimCommand(cmd)
    ' End Sub

    ' Private Sub SendUsarSimCommand(ByVal p1 As String)
    '    Throw New NotImplementedException("fafa")
    'End Sub


End Class
