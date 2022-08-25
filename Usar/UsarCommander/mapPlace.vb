Imports System.IO

Imports UvARescue.Agent
Imports UvARescue.Usar.Lib
Imports UvARescue.Tools


Public Class mapPlace
    Inherits Windows.Forms.Form

    Private mapController As MapController


    Sub New(ByVal image As ManifoldImage)

        Dim button1 As Button = New Button

       

        Dim _manifoldImage As ManifoldImage = image
        Me.Dock = DockStyle.Fill
        Me.mapController = New MapController(New ManifoldView(image))
        Me.mapController.Dock = DockStyle.Fill
        Me.Controls.Add(Me.mapController)
        Me.Controls.SetChildIndex(Me.mapController, 0)
        Me.ClientSize = New System.Drawing.Size(500, 500)

        Show()


    End Sub

End Class
