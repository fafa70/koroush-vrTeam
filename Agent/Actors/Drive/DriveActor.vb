Imports System.Math

Imports UvARescue.Math
''' <summary>
''' Base class for Drive actors.
''' 
''' UsarSim knows several kinds of steering models like
''' skid-steering, Ackerman steering and differential drive.
''' 
''' </summary>
''' <remarks></remarks>
Public MustInherit Class DriveActor
    Inherits Actor

    Public Shared ReadOnly ACTORTYPE_DRIVE As String = "Drive"
    Private _Motions As New Dictionary(Of MotionType, Motion)
    Public Sub New()
        MyBase.New(ACTORTYPE_DRIVE, ACTORTYPE_DRIVE)

    End Sub

    Public MustOverride Sub Drive(ByVal speed As Single)
    Public MustOverride Sub pathPlanTo(ByVal goalPose As Pose2D)
    Public MustOverride Sub DifferentialDrive(ByVal speed As Single, ByVal left As Single)
    Public MustOverride Sub Turn(ByVal left As Single)
    Public MustOverride Sub CameraPanTilt(ByVal up As Double)
    Public MustOverride Sub Fly(ByVal speed As Single)
    Public MustOverride Sub ToggleHeadLight()
    Public MustOverride ReadOnly Property ForwardSpeed() As Single
    Public MustOverride ReadOnly Property TurningSpeed() As Single
    Public MustOverride Sub Flip()

    Public MustOverride Sub KenafFrontUp()
    Public MustOverride Sub KenafFrontDown()
    Public MustOverride Sub KenafRearUp()
    Public MustOverride Sub KenafRearDown()

    Public Sub ReleaseRFIDTag()
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
