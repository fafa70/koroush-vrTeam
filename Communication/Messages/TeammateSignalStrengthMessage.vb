<Serializable()> _
Public Class TeammateSignalStrengthMessage
    Inherits BroadcastMessage

    Public Sub New(ByVal sender As String, ByVal recipient As String, ByVal newTeammate As String, ByVal newSignalStrength As Double)
        MyBase.New(sender, recipient, Communication.MessageID.TeammateSignalStrength)
        Me.Teammate = newTeammate
        Me.SignalStrength = newSignalStrength
    End Sub

    Public ReadOnly Teammate As String
    Public ReadOnly SignalStrength As Double

End Class

