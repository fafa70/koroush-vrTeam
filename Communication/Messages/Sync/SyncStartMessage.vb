Imports UvARescue.Math

<Serializable()> _
Public Class SyncStartMessage
    Inherits Message

    Public Sub New(ByVal currentPose As Pose2D)
        MyBase.New(Communication.MessageID.SyncStart)
        Me.CurrentPose = currentPose
    End Sub

    Public ReadOnly CurrentPose As Pose2D

End Class
