Imports System.Math

''' <summary>
''' Really simplistic, yet effective, steering actor that works
''' fine for most skid-steered and differential drive robots.
''' </summary>
''' <remarks></remarks>
Public Class StandardFlyActor
    Inherits FlyActor

    Private _HeadLight As Boolean = False
    Private _Up As Single = 0
    Private _Forward As Single = 0
    Private _MoveLeft As Single = 0
    Private _TurnLeft As Single = 0
    Private _CameraTilt As Double = 0

    Public Overrides ReadOnly Property ForwardSpeed() As Single
        Get
            Return _Forward
        End Get
    End Property
    Public Overrides ReadOnly Property TurningSpeed() As Single
        Get
            Return _TurnLeft
        End Get
    End Property

    Public Overrides ReadOnly Property StrafeSpeed() As Single
        Get
            Return _MoveLeft
        End Get
    End Property

    Public Overrides ReadOnly Property FlySpeed() As Single
        Get
            Return _Up
        End Get
    End Property


    Public Overrides Sub Fly(ByVal speed As Single, ByVal up_speed As Single)
        Me._Up = up_speed
        Me._Forward = speed
        Me._MoveLeft = 0
        Me._TurnLeft = 0
        Me.Move(Me._Up, Me._Forward, Me._MoveLeft, Me._TurnLeft)
    End Sub
    Public Overrides Sub Strafe(ByVal speed As Single)
        Me._Up = 0
        Me._Forward = 0
        Me._MoveLeft = speed
        Me._TurnLeft = 0
        Me.Move(Me._Up, Me._Forward, Me._MoveLeft, Me._TurnLeft)
    End Sub
    Public Overrides Sub DifferentialFly(ByVal speed As Single, ByVal turning_speed As Single)
        Me._Up = 0
        Me._Forward = speed
        Me._MoveLeft = 0
        Me._TurnLeft = turning_speed
        Me.Move(Me._Up, Me._Forward, Me._MoveLeft, Me._TurnLeft)
    End Sub

    Public Overrides Sub Turn(ByVal left As Single)
        Me._Up = 0
        Me._Forward = 0
        Me._MoveLeft = 0
        Me._TurnLeft = left
        Me.Move(Me._Up, Me._Forward, Me._MoveLeft, Me._TurnLeft)
    End Sub


    Public Overrides Sub CameraPanTilt(ByVal up As Double)
        Console.WriteLine(String.Format("MISPKG {{Name CameraPanTilt}} {{Link 1}} {{Value {0}}}", Me._CameraTilt))
        Me._CameraTilt += up
        If Me._CameraTilt > 3 Then
            Me._CameraTilt = 3
        ElseIf Me._CameraTilt < -3 Then
            Me._CameraTilt = -3
        End If
        Me.SendUsarSimCommand(String.Format("MISPKG {{Name CameraPanTilt}} {{Link 1}} {{Value {0}}}", _
                 Me._CameraTilt))
    End Sub

    Protected Sub Move(ByVal up As Single, ByVal forward As Single, ByVal left As Single, ByVal rot As Double)
        Me.SendUsarSimCommand(String.Format("DRIVE {{AltitudeVelocity {0}}} {{LinearVelocity {1}}} {{LateralVelocity {2}}} {{RotationalVelocity {3}}} {{Normalized {4}}}", _
         up, _
         forward, _
         left, _
         rot, _
         "False"))
    End Sub

    Public Overrides Sub ReleaseRFIDTagFly()
        Dim cmd As New Text.StringBuilder
        ''cmd.Append("SET ")
        ''cmd.AppendFormat("{{Type {0}}} ", "RFIDReleaser")
        ''cmd.AppendFormat("{{Name {0}}} ", "Gun")
        ''cmd.AppendFormat("{{Opcode {0}}}", "TagsRemaining")
        ''Me.SendUsarSimCommand(cmd.ToString)

        cmd.Append("SET ")
        cmd.AppendFormat("{{Type {0}}} ", "RFIDReleaser")
        cmd.AppendFormat("{{Name {0}}} ", "Gun")
        cmd.AppendFormat("{{Opcode {0}}}", "Release")
        Me.SendUsarSimCommand(cmd.ToString)

        ''cmd.Append("SET ")
        ''cmd.AppendFormat("{{Type {0}}} ", "RFIDReleaser")
        ''cmd.AppendFormat("{{Name {0}}} ", "Gun")
        ''cmd.AppendFormat("{{Opcode {0}}}", "TagsRemaining")
        ''Me.SendUsarSimCommand(cmd.ToString)

    End Sub
End Class
