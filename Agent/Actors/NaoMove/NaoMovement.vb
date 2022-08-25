Public MustInherit Class NaoMovement
    Inherits Actor
    Public Shared ReadOnly Nao_Movement As String = "Nao"
    Public Sub New()
        MyBase.New(Nao_Movement, Nao_Movement)
    End Sub

    Public MustOverride Sub HeadYaw(ByVal angle As Single)
    Public MustOverride Sub HeadPitch(ByVal angle As Single)
    Public MustOverride Sub move()

End Class
