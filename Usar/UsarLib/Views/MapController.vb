Imports System.Drawing
Imports System.Windows.Forms

Imports UvARescue.Slam
Imports UvARescue.Tools
Imports UvARescue.Agent

Public Class MapController

#Region " Constructor"

    Private _ManifoldView As ManifoldView
    Public ReadOnly Property ManifoldView() As ManifoldView
        Get
            Return Me._ManifoldView
        End Get
    End Property


    Public Sub New(ByVal view As ManifoldView)
        MyBase.New()

        If IsNothing(view) Then Throw New ArgumentNullException("view")
        Me._ManifoldView = view

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        view.Dock = Windows.Forms.DockStyle.Fill
        Me.pnlManifold.Controls.Add(view)

        'restore user settings
        '   Me.chkApriori.Checked = My.Settings.ShowApriori
        '  Me.chkAprioriMobility.Checked = My.Settings.ShowAprioriMobility
        ' Me.chkAprioriVictims.Checked = My.Settings.ShowAprioriVictims
        'Me.chkAprioriComm.Checked = My.Settings.ShowAprioriComm

        ' Me.chkFreeSpace.Checked = My.Settings.ShowFreeSpace
        ' Me.chkSafeSpace.Checked = My.Settings.ShowSafeSpace
        ' Me.chkClearSpace.Checked = My.Settings.ShowClearSpace
        ' Me.chkVictims.Checked = My.Settings.ShowVictims
        '  Me.chkFrontiers.Checked = My.Settings.ShowFrontiers
        '   Me.chkObstacles.Checked = My.Settings.ShowObstacles
        '  Me.chkNotes.Checked = My.Settings.ShowNotes
        ' Me.chkComms.Checked = My.Settings.ShowComms
        'Me.chkAgents.Checked = My.Settings.ShowAgents
        'Me.chkOmniMap.Checked = My.Settings.ShowOmniMap
        'Me.chkMobility.Checked = My.Settings.ShowMobility '(MRT)

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try

            '   My.Settings.ShowApriori = Me.chkApriori.Checked
            '  My.Settings.ShowAprioriMobility = Me.chkAprioriMobility.Checked
            ' My.Settings.ShowAprioriVictims = Me.chkAprioriVictims.Checked
            'My.Settings.ShowAprioriComm = Me.chkAprioriComm.Checked
            'My.Settings.ShowFreeSpace = Me.chkFreeSpace.Checked
            'My.Settings.ShowSafeSpace = Me.chkSafeSpace.Checked
            'My.Settings.ShowClearSpace = Me.chkClearSpace.Checked
            'My.Settings.ShowVictims = Me.chkVictims.Checked
            'My.Settings.ShowObstacles = Me.chkObstacles.Checked
            'My.Settings.ShowFrontiers = Me.chkFrontiers.Checked
            'My.Settings.ShowNotes = Me.chkNotes.Checked
            'My.Settings.ShowComms = Me.chkComms.Checked
            'My.Settings.ShowAgents = Me.chkAgents.Checked
            'Me.chkOmniMap.Checked = My.Settings.ShowOmniMap
            'My.Settings.ShowMobility = Me.chkMobility.Checked '(MRT)

            My.Settings.Save()

            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If

        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region

#Region " Actions "

    Private Sub btnSaveMap_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        'Save current image
        Dim oldImage As ManifoldImage = Me._ManifoldView.Image

        'Allow manipulation of current image by SaveMap dialog
        Dim dlg As New SaveMapDialog(Me.TeamConfig, Me.ManifoldView)

        While Not dlg.ShowDialog = Windows.Forms.DialogResult.Abort
        End While

        'Retrieve image that existed pre-SaveMap dialog and display
        Me._ManifoldView.Image = oldImage
        Me._ManifoldView.Invalidate()
    End Sub

    Private Sub btnLoadApriori_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim dlg As New OpenFileDialog
        dlg.RestoreDirectory = True
        Select Case dlg.ShowDialog
            Case Windows.Forms.DialogResult.OK
                'Me._ManifoldView.Image.AprioriFileName = dlg.FileName
                Me._ManifoldView.Invalidate()
        End Select
    End Sub

    Private Sub btnExtractFrontiers_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        'Me._ManifoldView.Image.RecomputeFrontiers()
        ' Me._ManifoldView.Invalidate()
    End Sub

    Private Sub btnZoomIn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ZoomIn()
    End Sub

    Private Sub btnZoomOut_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ZoomOut()
    End Sub

    Private Sub btnZoomAuto_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me._ManifoldView.AutoZoom()
    End Sub

#End Region

#Region " Enable/disable map save button "
    ' Public Sub EnableMapSave(ByRef enable As Boolean)
    '    If (enable) Then
    '       Me.btnSaveMap.Enabled = True
    '  Else
    '     Me.btnSaveMap.Enabled = False
    ' End If
    'End Sub
#End Region

#Region " Flags "

    Private Sub chkApriori_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ShowAprioriLayer = True
        Me._ManifoldView.Invalidate()
    End Sub
    Private Sub chkAprioriMobility_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.Image.ShowAprioriMobility = True
        Me._ManifoldView.Invalidate()
    End Sub
    Private Sub chkAprioriVictims_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.Image.ShowAprioriVictims = True
        Me._ManifoldView.Invalidate()
    End Sub
    Private Sub chkAprioriComm_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.Image.ShowAprioriComm = True
        Me._ManifoldView.Invalidate()
    End Sub



    Private Sub chkFreeSpace_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ShowFreeSpaceLayer = True
    End Sub
    Private Sub chkSafeSpace_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ShowSafeSpaceLayer = True
    End Sub
    Private Sub chkClearSpace_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ShowClearSpaceLayer = True
    End Sub

    Private Sub chkFrontiers_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ShowFrontierLayer = True
    End Sub
    Private Sub chkVictims_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ShowVictimsLayer = True
    End Sub
    Private Sub chkObstacles_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ShowObstaclesLayer = True
    End Sub
    Private Sub chkNotes_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ShowNotesLayer = True
    End Sub
    Private Sub chkComms_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ShowCommsLayer = True
    End Sub
    Private Sub chkAgents_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Me._ManifoldView.ShowAgentsLayer = True
    End Sub

#End Region

#Region " Team Config "
    Private _TeamConfig As TeamConfig
    Public ReadOnly Property TeamConfig() As TeamConfig
        Get
            Return Me._TeamConfig
        End Get
    End Property


    Public Sub SetTeamConfig(ByVal teamConfig As TeamConfig)
        Me._TeamConfig = teamConfig
    End Sub
#End Region


    Private Sub pnlManifold_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles pnlManifold.Paint

    End Sub

    Private Sub chkAgents_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub chkFreeSpace_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub
End Class
