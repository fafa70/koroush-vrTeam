Imports System.Math

''' <summary>
''' Base class for Drive actors.
''' 
''' UsarSim knows several kinds of steering models like
''' skid-steering, Ackerman steering and differential drive.
''' 
''' </summary>
''' <remarks></remarks>
Public MustInherit Class FlyActor
    Inherits Actor

    Public Shared ReadOnly ACTORTYPE_FLY As String = "Fly"

    Public Sub New()
        MyBase.New(ACTORTYPE_FLY, ACTORTYPE_FLY)
    End Sub

    Public MustOverride Sub Fly(ByVal speed As Single, ByVal up_speed As Single)
    Public MustOverride Sub Strafe(ByVal speed As Single)
    Public MustOverride Sub DifferentialFly(ByVal speed As Single, ByVal left As Single)
    Public MustOverride Sub Turn(ByVal left As Single)
    Public MustOverride Sub CameraPanTilt(ByVal up As Double)
    Public MustOverride Sub ReleaseRFIDTagFly()
    Public MustOverride ReadOnly Property ForwardSpeed() As Single
    Public MustOverride ReadOnly Property TurningSpeed() As Single
    Public MustOverride ReadOnly Property StrafeSpeed() As Single
    Public MustOverride ReadOnly Property FlySpeed() As Single



End Class
