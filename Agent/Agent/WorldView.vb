Public Class WorldView

#Region " Constructor "

    Public Sub New(ByVal manifold As Manifold, ByVal name As String, ByVal apriori As String)

        If IsNothing(manifold) Then Throw New ArgumentNullException("manifold")
        If String.IsNullOrEmpty(name) Then Throw New ArgumentNullException("name")
        'If String.IsNullOrEmpty(apriori) Then Throw New ArgumentNullException("apriori")

        Me._UniqueID = Guid.NewGuid
        Me._Name = name

        Me._Manifold = manifold
        Me._ManifoldImage = Me.CreateManifoldImage(manifold)
        Me._ManifoldImage.AprioriFileName = apriori
        Me._ManifoldImage.ShowAprioriMobility = True


    End Sub

    Protected Overridable Function CreateManifoldImage(ByVal manifold As Manifold) As ManifoldImage
        Return New ManifoldImage(manifold, UvARescue.Tools.Constants.MAP_RESOLUTION, False)
    End Function

#End Region

#Region " WorldView Properties "

    Private _UniqueID As Guid
    Public ReadOnly Property UniqueID() As Guid
        Get
            Return _UniqueID
        End Get
    End Property

    Private _Manifold As Manifold
    Public ReadOnly Property Manifold() As Manifold
        Get
            Return Me._Manifold
        End Get
    End Property

    Private _ManifoldImage As ManifoldImage
    Public ReadOnly Property ManifoldImage() As ManifoldImage
        Get
            Return Me._ManifoldImage
        End Get
    End Property

    Private _Name As String
    Public ReadOnly Property Name() As String
        Get
            Return _Name
        End Get
    End Property
#End Region

End Class
