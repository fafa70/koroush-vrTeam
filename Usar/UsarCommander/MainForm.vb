Imports System.IO

Imports UvARescue.Agent
Imports UvARescue.Usar.Lib
Imports UvARescue.Tools




Public Class MainForm
    Inherits System.Windows.Forms.Form

    Private _config As TeamConfig = New TeamConfig("newmap")
    Private image As ManifoldImage
    Private teamController As TeamController
    Private mapController As MapController
    Private _mapPlace As mapPlace
    Private _cameraBox As cameraBox = New cameraBox
    Private _agentCamera As agentCamera = New agentCamera
    Private _mainController As mainController = New mainController


    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        'Console.WriteLine("[RIKNOTE] UsarCommander\MainForm.vb::OnLoad() called")
        'set the tread's name (very convenient when debugging)
        Threading.Thread.CurrentThread.Name = "UI"


        'restore last form bounds
        With My.Settings
            Me.Left = .LastFormLeft
            Me.Top = .LastFormTop
            Me.Width = .LastFormWidth
            Me.Height = .LastFormHeight
        End With

        'setup UI
        Dim manifold As New Manifold
        Me.image = New ManifoldImage(manifold, UvARescue.Tools.Constants.MAP_RESOLUTION, True)
        Me._mapPlace = New mapPlace(Me.image)

        Me.mapController = New MapController(New ManifoldView(image))
        'Me.mapController.Show()
        Me.mapController.Dock = DockStyle.Fill
        ' Me.pnlMap.Controls.Add(Me.mapController)
        'Me.pnlMap.Controls.SetChildIndex(Me.mapController, 0)


        Me.teamController = New TeamController(manifold, Me.mapController, _cameraBox, _agentCamera, _mainController)
        teamController.Dock = DockStyle.Fill
        Me.pnlController.Controls.Add(Me.teamController)
        Me.pnlController.Controls.SetChildIndex(Me.teamController, 0)
        Show()
        'Me._cameraBox.Show()
        Dim teamconfig As TeamConfigDialog = New TeamConfigDialog(Me._config, manifold)
        teamconfig.ShowDialog(Me)
    End Sub

    Protected Overrides Sub OnClosing(ByVal e As System.ComponentModel.CancelEventArgs)
        Dim quit As Boolean = Me.teamController.QueryCanQuitApplication
        If Not quit Then
            e.Cancel = True

        Else
            'save last form bounds
            With My.Settings
                If Not Me.WindowState = FormWindowState.Minimized Then
                    .LastFormLeft = Me.Left
                    .LastFormTop = Me.Top
                    .LastFormWidth = Me.Width
                    .LastFormHeight = Me.Height
                End If
            End With
        End If

        My.Settings.Save()

        MyBase.OnClosing(e)

    End Sub



    Private Sub mnuFileExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Close()
    End Sub

    Private Sub MainForm_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown

        If e.KeyCode = Keys.W Then

            e.Handled = True

            'executes the code in the buttons _Click event.
            'btn.PerformClick()

        ElseIf e.KeyCode = Keys.Q Then

            'one of the reasons I set handled to true is so it won't beep whenever you press enter
            'in the textbox
            e.Handled = True

            MessageBox.Show("The text in the Textbox is: Pressed the Enter")

        End If

    End Sub

    Private Sub stbStatus_ItemClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles stbStatus.ItemClicked

    End Sub

    Private Sub MainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Activate()
    End Sub

    Private Sub pnlMap_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs)

    End Sub
End Class
