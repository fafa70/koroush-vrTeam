Imports System.Math

''' <summary>
''' Really simplistic, yet effective, steering actor that works
''' fine for most skid-steered and differential drive robots.
''' </summary>
''' <remarks></remarks>
Public Class StandardDriveActor
    Inherits DriveActor


    Private _HeadLight As Boolean = False
    Private _Forward As Single = 0
    Public Overrides ReadOnly Property ForwardSpeed() As Single
        Get
            Return _Forward
        End Get
    End Property
    Private _TurnLeft As Single = 0
    Public Overrides ReadOnly Property TurningSpeed() As Single
        Get
            Return _TurnLeft
        End Get
    End Property

    Private _CameraTilt As Double = 0


    Public Overrides Sub Drive(ByVal speed As Single)
        Me._Forward = speed
        Me._TurnLeft = 0
        Me.Move(Me._Forward, Me._TurnLeft, Me._HeadLight)
    End Sub
    Public Overrides Sub DifferentialDrive(ByVal speed As Single, ByVal turning_speed As Single)
        Me._Forward = speed
        Me._TurnLeft = turning_speed
        Me.Move(Me._Forward, Me._TurnLeft, Me._HeadLight)
    End Sub

    Public Overrides Sub Turn(ByVal left As Single)
        Me._Forward = 0
        Me._TurnLeft = left
        Me.Move(Me._Forward, Me._TurnLeft, Me._HeadLight)
    End Sub


    Public Overrides Sub ToggleHeadLight()
        Me._HeadLight = Not Me._HeadLight
        Me.Move(Me._Forward, Me._TurnLeft, Me._HeadLight)
    End Sub

    Public Overrides Sub CameraPanTilt(ByVal up As Double)
        Me._CameraTilt += up
        If Me._CameraTilt > 1 Then
            Me._CameraTilt = 1
        ElseIf Me._CameraTilt < -1 Then
            Me._CameraTilt = -1
        End If
        Me.SendUsarSimCommand(String.Format("MISPKG {{Name CameraPanTilt}} {{Link 1}} {{Value {0}}} {{Link 2}} {{Value {1}}}", _
                 0, _
                 Me._CameraTilt))
    End Sub

    Private _Up As Single = 0
    Private _MoveLeft As Single = 0
    Private _RotateLeft As Double = 0.0
    Public Overrides Sub Fly(ByVal speed As Single)
        Me._Forward = 0
        Me._TurnLeft = 0
        Me._Up = speed
        Me.Move(Me._Up, Me._Forward, Me._MoveLeft, Me._RotateLeft)
    End Sub

    Protected Sub Move(ByVal forward As Single, ByVal left As Single, ByVal headLight As Boolean)
        Me.SendUsarSimCommand(String.Format("DRIVE {{Left {0}}} {{Right {1}}} {{Light {2}}}", _
         forward - left, _
         forward + left, _
         headLight.ToString))
    End Sub

    Protected Sub Move(ByVal up As Single, ByVal forward As Single, ByVal left As Single, ByVal rot As Double)
        Me.SendUsarSimCommand(String.Format("DRIVE {{AltitudeVelocity {0}}} {{LinearVelocity {1}}} {{LateralVelocity {2}}} {{RotationalVelocity {3}}} {{Normalized {4}}}", _
         up, _
         forward, _
         left, _
         rot, _
         "False"))
    End Sub
    'DRIVE {Propeller float} {Rudder float} {SternPlane float} {Normalized bool} {Light bool}
End Class
