Imports System.Drawing

Public Class PhotoLibraryController

    Public Sub AddPhoto(ByVal photo As Image)
        Dim view As New PhotoView(Me)
        view.Photo = photo
        Me.pnlPhotos.Controls.Add(view)
    End Sub

    Public Sub RemovePhoto(ByVal view As PhotoView)
        Me.Controls.Remove(view)
    End Sub

End Class
