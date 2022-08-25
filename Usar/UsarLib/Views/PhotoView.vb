Public Class PhotoView

    Public Sub New(ByVal controller As PhotoLibraryController)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me._PhotoLibraryController = controller

    End Sub

    Private _PhotoLibraryController As PhotoLibraryController


    Public Property Photo() As System.Drawing.Image
        Get
            Return Me.imgPhoto.Image
        End Get
        Set(ByVal value As System.Drawing.Image)
            Me.imgPhoto.Image = value
        End Set
    End Property

    Private Sub imgPhoto_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles imgPhoto.Click

    End Sub
End Class
