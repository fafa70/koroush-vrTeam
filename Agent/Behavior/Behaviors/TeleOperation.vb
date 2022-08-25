''' <summary>
''' When this behavior is activated the Agent will not initiate any exploration
''' actions by itself. All it will do is try to get back in range of the ComStation
''' as soon as the connection with the Operator is lost.
''' </summary>
''' <remarks></remarks>
Public Class TeleOperation
    Inherits Behavior

    Public Sub New(ByVal control As BehaviorControl)
        MyBase.New(control)
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Behaviors\TeleOperation.vb::New() called")
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()

        Console.WriteLine(String.Format("[Behavior {0}] - Activated", Me.GetType))

        Me.Control.ActivateMotion(MotionType.NoMotion, True)

    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        Me.Control.DeActivateAllMotions()
    End Sub

    Protected Overrides Sub OnNotifyOperatorConnected()
        MyBase.OnNotifyOperatorConnected()
        Me.Control.ActivateMotion(MotionType.NoMotion, True)
    End Sub

    Protected Overrides Sub OnNotifyOperatorDisconnected()
        MyBase.OnNotifyOperatorDisconnected()
        Me.Control.ActivateMotion(MotionType.Retreat, True)
    End Sub

End Class
