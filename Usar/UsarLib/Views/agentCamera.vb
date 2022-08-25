Imports System.Drawing
Imports System.IO
Imports System.Math
Imports System.Windows.Forms
Imports System.Text
Imports UvARescue.Agent
Imports UvARescue.Slam
Imports UvARescue.Tools
Imports UvARescue.Math
Imports UvARescue.ImageAnalysis



Public Class agentCamera
    Inherits Windows.Forms.Form
    Private currAgent As Integer
    Private WithEvents zoom As Button = New Button()
    Private WithEvents zoomOut As Button = New Button()
    Private mainPanel As Panel = New Panel
    Private listOfPanels As New List(Of Panel)
    Private isZooming As Boolean = False
    Private isZoomingOut As Boolean = False

    Sub New()
        Me.ClientSize = New System.Drawing.Size(420, 240)
        With mainPanel
            .Size = New Drawing.Size(320, 240)
            .BackgroundImage = Image.FromFile("cyrus-the-great-ancient.ir2.jpg")
        End With
        Me.zoom.Left = 320
        Me.zoom.Text = "zoom in"
        Me.zoom.Size = New Size(100, 125)
        Me.DesktopLocation = New Point(1250, 1250)

        Me.zoomOut.Left = 320
        Me.zoomOut.Top = 125
        Me.zoomOut.Size = New Size(100, 118)
        Me.zoomOut.Text = "zoom out"

        Me.Controls.Add(mainPanel)
        Me.Controls.Add(zoom)
        Me.Controls.Add(zoomOut)
        Me.Show()
    End Sub

    Public Sub initPanel(ByVal agentnumber As Integer)
        Me.listOfPanels.Add(New Panel With {.Size = New System.Drawing.Size(320, 240)})
    End Sub


    Public Sub showCurrCamera(ByVal image As Image, ByVal agentNumber As Integer)
        If (Me.isZooming) Then
            Dim zoomImage As New Bitmap(image, image.Width * zoomValue, image.Height * zoomValue)
            Me.listOfPanels(agentNumber - 1).BackgroundImage = zoomImage

        ElseIf (Me.isZoomingOut) Then
            Dim zoomImage As New Bitmap(image, CInt(image.Width * zoomOutValue), CInt(image.Height * zoomOutValue))

            Me.listOfPanels(agentNumber - 1).BackgroundImage = zoomImage

        ElseIf (Me.isZooming = False And Me.isZoomingOut = False) Then
            Me.listOfPanels(agentNumber - 1).BackgroundImage = image

        End If

    End Sub


    Public Sub setCurrCamera(ByVal agentNumber As Integer)
        Me.currAgent = agentNumber

        mainPanel.BackgroundImage = listOfPanels(agentNumber - 1).BackgroundImage
        'Console.WriteLine("get image repeatly!!!!")



    End Sub

    Private zoomValue As Integer = 2
    Private zoomOutValue As Double = 0.5
    Private Sub zoom_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles zoom.Click

        If (Me.isZooming) Then
            Me.isZooming = False
        Else
            Me.isZooming = True
        End If

    End Sub

    Private Sub zoomOut_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles zoomOut.Click
        If (Me.isZoomingOut) Then
            Me.isZoomingOut = False
        Else
            Me.isZoomingOut = True
        End If
    End Sub


End Class
