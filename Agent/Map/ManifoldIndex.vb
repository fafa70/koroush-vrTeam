Imports UvARescue.Math
Imports UvARescue.Tools

Public Class ManifoldIndex
    Inherits QuadTree(Of Vector2WithCovariance)

    Private _Manifold As Manifold

    Public Sub New(ByVal manifold As Manifold)
        Me._Manifold = manifold
    End Sub

    Private _PatchOrigins As New QuadTree(Of Vector2WithUniqueID)

    Public Sub Add(ByVal patch As Patch)

        Dim origin As Vector2WithUniqueID = New Vector2WithUniqueID(patch.EstimatedOrigin.Position, patch.ID)
        Me._PatchOrigins.Insert(origin)

        Dim points() As Vector2 = patch.GlobalPoints
        Dim covariances() As Matrix2 = patch.Covariances
        Dim filter() As Boolean = patch.Filter

        For i As Integer = 0 To points.Length - 1
            If Not filter(i) Then
                Dim vector As Vector2WithCovariance = New Vector2WithCovariance(points(i), covariances(i))
                Me.Insert(vector)
                vector = Nothing
            End If
        Next
        origin = Nothing

    End Sub

    Public Function FindNearestPatch(ByVal target As Vector2, ByVal maxDistance As Single) As Patch
        Dim nearest As Vector2WithUniqueID = Me._PatchOrigins.FindNearestNeighbour(target, maxDistance)
        If Not IsNothing(nearest) Then
            Return Me._Manifold.GetNode(nearest.UniqueID)
        End If
        Return Nothing
    End Function

End Class



Public Class Vector2WithCovariance
    Inherits Vector2

    Public Sub New(ByVal vector As Vector2, ByVal covariance As Matrix2)
        MyBase.New(vector)
        Me._Covariance = covariance
    End Sub

    Private _Covariance As Matrix2
    Public ReadOnly Property Covariance() As Matrix2
        Get
            Return Me._Covariance
        End Get
    End Property

End Class

Public Class Vector2WithUniqueID
    Inherits Vector2

    Public Sub New(ByVal vector As Vector2, ByVal UniqueID As Guid)
        MyBase.New(vector)
        Me._UniqueID = UniqueID
    End Sub

    Private _UniqueID As Guid
    Public ReadOnly Property UniqueID() As Guid
        Get
            Return Me._UniqueID
        End Get
    End Property

End Class
