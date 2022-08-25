' Motion that explores terrain of unknown traversability
' Motion is part of behaviour ExploreTraversability
Public Class ExploreTerrain
    Inherits Motion
    'fafa writes new exploration method for making base faster
    Protected Enum states
        walkForward
        turnLeft
        turnRight
    End Enum
    Private currState As states
    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
    End Sub




End Class
