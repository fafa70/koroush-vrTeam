''' <summary>
''' When this behavior is activated the Agent will not initiate any exploration
''' actions by itself. All it will do is 
'''    - try to get back in range of the ComStation as soon as the connection with the Operator is lost
'''    - stop if it senses an obstacle straight ahead using either laser or sonar, and only continue
'''      if operating person confirms
'''    - stop if there is a major change in incline and also wait for confirmation
''' </summary>
''' <remarks></remarks>
Public Class ConservativeTeleOp
    Inherits Behavior

    Public Sub New(ByVal control As BehaviorControl)
        MyBase.New(control)
        'Console.WriteLine("[JDHNOTE] Agent\Behavior\Behaviors\ConservativeTeleOp.vb::New() called")
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()

        Console.WriteLine(String.Format("[Behavior {0}] - Activated", Me.GetType))

        Me.Control.ActivateMotion(MotionType.ConservativeTeleOpMotion, True)

    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        Me.Control.DeActivateAllMotions()
    End Sub

    Protected Overrides Sub OnNotifyOperatorConnected()
        MyBase.OnNotifyOperatorConnected()
        Me.Control.ActivateMotion(MotionType.ConservativeTeleOpMotion, True)
    End Sub

    Protected Overrides Sub OnNotifyOperatorDisconnected()
        MyBase.OnNotifyOperatorDisconnected()
        Me.Control.ActivateMotion(MotionType.Retreat, True)
    End Sub

End Class
