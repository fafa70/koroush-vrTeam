Imports UvARescue.Tools

Imports System.IO
Imports System.Windows.Forms


Public Class ConfigDialog

    Private _config1 As Agent.TeamConfig
    Public _manifold As Agent.Manifold

    Private Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Protected Sub New(ByVal config As Config)
        Me.New()

        If IsNothing(config) Then Throw New ArgumentNullException("config")
        Me._Config = config

        'make sure we have the latest changes
        Me._Config.Reload()

    End Sub

   

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)
        If Not Me.DesignMode Then
            Me.SyncGuiWithConfig()
        End If
    End Sub

    Private _Config As Config
    Public ReadOnly Property Config() As Config
        Get
            Return Me._Config
        End Get
    End Property


    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.SyncConfigWithGui()
        Me.Save(Me._Config.FileName)
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Reload()
    End Sub



    Private Sub Open()
        Dim dialog As New OpenFileDialog
        With dialog

            Dim folder As String = My.Settings.LastConfigFolder
            If Not String.IsNullOrEmpty(folder) Then
                .InitialDirectory = folder
            End If

            Select Case .ShowDialog
                Case Windows.Forms.DialogResult.OK
                    Try
                        Me._Config.Load(.FileName)
                        Me.SyncGuiWithConfig()
                        Me.OnOpened(.FileName)

                    Catch ex As Exception
                        MsgBox("Error loading config file.")

                    End Try

            End Select
        End With
    End Sub
    Private Sub Reload()
        Try
            Me._Config.Reload()
            Me.SyncGuiWithConfig()
        Catch ex As Exception
            MsgBox("Error loading config file.")
        End Try
    End Sub
    Private Sub Save(ByVal filename As String)
        If String.IsNullOrEmpty(filename) Then
            Me.SaveAs()
        Else
            Try
                Me.SyncConfigWithGui()
                Me._Config.Save(filename)
                Me.OnSaved(filename)

            Catch ex As Exception
                MsgBox("Error saving config file.")

            End Try
        End If
    End Sub
    Private Sub SaveAs()
        Dim dialog As New SaveFileDialog
        With dialog

            Dim folder As String = My.Settings.LastConfigFolder
            If Not String.IsNullOrEmpty(folder) Then
                .InitialDirectory = folder
            End If

            Select Case .ShowDialog
                Case Windows.Forms.DialogResult.OK
                    Me.Save(Path.GetFullPath(.FileName))

            End Select

        End With
    End Sub

    Protected Overridable Sub OnOpened(ByVal filename As String)
        'Me.txtConfigFile.Text = Path.GetFileName(filename)
        My.Settings.LastConfigFolder = Directory.GetCurrentDirectory()
        My.Settings.Save()
    End Sub
    Protected Overridable Sub OnSaved(ByVal filename As String)
        'Me.txtConfigFile.Text = Path.GetFileName(filename)
        My.Settings.LastConfigFolder = Directory.GetCurrentDirectory()
        My.Settings.Save()
    End Sub

    Private Sub btnOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Open()
    End Sub
    Private Sub btnReload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Reload()
    End Sub
    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Save(Me._Config.FileName)
    End Sub
    Private Sub btnSaveCopy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.SaveAs()
    End Sub


    Protected Overridable Sub SyncGuiWithConfig()
        'Me.txtConfigFile.Text = Path.GetFileName(Me._Config.FileName)
    End Sub
    Protected Overridable Sub SyncConfigWithGui()
    End Sub

    Private Sub ConfigDialog_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub
End Class