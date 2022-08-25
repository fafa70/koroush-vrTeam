Imports UvARescue.Math

<Serializable()> _
Public Class SyncCommitMessage
    Inherits Message

    Public Sub New(ByVal timestamp As DateTime, ByVal currentPose As Pose2D)
        MyBase.New(Communication.MessageID.SyncCommit)
        Me.Timestamp = timestamp
        Me.CurrentPose = currentPose
    End Sub

    Public ReadOnly Timestamp As DateTime
    Public ReadOnly CurrentPose As Pose2D

End Class
