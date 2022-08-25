Imports System.Math

''' <summary>
''' Meant to make a team of robots create as extensive a comm network as possible
''' (Created in response to the "Deployment Test" at RoboCup German Open 2009)
''' </summary>
''' <remarks></remarks>

Public Class DeploymentMotion
    Inherits Motion

    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
    End Sub

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        Me.Control.Agent.Halt()
    End Sub

End Class