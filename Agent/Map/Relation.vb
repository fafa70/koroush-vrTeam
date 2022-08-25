Imports UvARescue.Math
Imports UvARescue.Tools

''' <summary>
''' Stores the odometry estimate between two patches with the associated uncertainty.
''' </summary>
''' <remarks></remarks>
Public Class Relation
    Inherits GraphLink(Of Patch, Relation)

#Region " Constructor "

    Public Sub New(ByVal manifold As Manifold, ByVal fromPatch As Patch, ByVal toPatch As Patch, ByVal odometry As Pose2D, ByVal covariance As Matrix3)
        MyBase.New(manifold, Guid.NewGuid, fromPatch, toPatch)
        Me._Timestamp = Now
        Me._OdometryPose = odometry
        Me._Covariance = covariance
    End Sub

    Protected Sub New(ByVal manifold As Manifold, ByVal relationID As Guid, ByVal fromPatch As Patch, ByVal toPatch As Patch, ByVal odometry As Pose2D, ByVal covariance As Matrix3)
        MyBase.New(manifold, relationID, fromPatch, toPatch)
        Me._Timestamp = Now
        Me._OdometryPose = odometry
        Me._Covariance = covariance
    End Sub

#End Region

#Region " Memento "

    <Serializable()> _
    Protected Class RelationMemento
        Implements IMemento

        Public Sub New(ByVal relation As Relation)
            Me._RelationID = relation.ID

            If Not IsNothing(relation.FromNode) Then
                Me._FromPatchID = relation.FromNode.ID
            Else
                Me._FromPatchID = Guid.Empty
            End If

            If Not IsNothing(relation.ToNode) Then
                Me._ToPatchID = relation.ToNode.ID
            Else
                Me._ToPatchID = Guid.Empty
            End If

            Me._Odometry = relation._OdometryPose
            Me._Covariance = relation._Covariance
            Me._Timestamp = relation.Timestamp

        End Sub

        Private _RelationID As Guid
        Private _FromPatchID As Guid
        Private _ToPatchID As Guid

        Private _Odometry As Pose2D
        Private _Covariance As Matrix3


        Private _Timestamp As DateTime


        Public Sub Restore(ByVal manifold As Manifold) Implements IMemento.Restore
            If Not manifold.HasLink(Me._RelationID) Then

                Dim fromPatch As Patch = DirectCast(IIf(manifold.HasNode(Me._FromPatchID), manifold.GetNode(Me._FromPatchID), Nothing), Patch)
                Dim toPatch As Patch = DirectCast(IIf(manifold.HasNode(Me._ToPatchID), manifold.GetNode(Me._ToPatchID), Nothing), Patch)

                If Not IsNothing(fromPatch) AndAlso Not IsNothing(toPatch) Then

                    Dim relation As New Relation(manifold, Me._RelationID, fromPatch, toPatch, Me._Odometry, Me._Covariance)

                    'IMPORTANT
                    'do not update timestamp, every manifold must assign its own
                    'timestamps to relations in order to make manifold sharing
                    'between more than 2 robots work!
                    'relation._Timestamp = Me._Timestamp

                    manifold.Extend(relation)

                ElseIf IsNothing(fromPatch) Then

                    Console.WriteLine(String.Format("[Map] - relation recorded at {0} from unknown FromPatch at {1})", Me._Timestamp, Me._Odometry.ToString))

                ElseIf IsNothing(toPatch) Then

                    Console.WriteLine(String.Format("[Map] - relation recorded at {0} to unknown ToPatch at {1})", Me._Timestamp, Me._Odometry.ToString))
                Else

                    Console.WriteLine(String.Format("[Map] - relation recorded at {0} from unknown FromPatch to unknown ToPatch at {1})", Me._Timestamp, Me._Odometry.ToString))

                End If

            Else
                Console.WriteLine(String.Format("[Map] - relation recorded at {0} already exist)", Me._Timestamp))

            End If
        End Sub

    End Class

    Public Function CreateMemento() As IMemento
        Return New RelationMemento(Me)
    End Function

#End Region

#Region " Properties "

    Private _Timestamp As DateTime
    Public ReadOnly Property Timestamp() As DateTime
        Get
            Return Me._Timestamp
        End Get
    End Property

    Private _OdometryPose As Pose2D
    Public ReadOnly Property OdometryPose() As Pose2D
        Get
            Return Me._OdometryPose
        End Get
    End Property

    Private _Covariance As Matrix3
    Public ReadOnly Property Covariance() As Matrix3
        Get
            Return Me._Covariance
        End Get
    End Property

#End Region

End Class
