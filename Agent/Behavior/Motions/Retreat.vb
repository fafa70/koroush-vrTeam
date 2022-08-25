''' <summary>
''' Will initiate an immediate retreat by changing to reverse gear.
''' </summary>
''' <remarks></remarks>
Public Class Retreat
    Inherits Motion

    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        Me.Control.Agent.Reverse(1.0)
    End Sub

End Class
