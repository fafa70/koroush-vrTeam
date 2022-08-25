<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SaveMapDialog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim ListViewItem1 As System.Windows.Forms.ListViewItem = New System.Windows.Forms.ListViewItem(New String() {"TIF", "Yes", "Yes"}, -1)
        Dim ListViewItem2 As System.Windows.Forms.ListViewItem = New System.Windows.Forms.ListViewItem(New String() {"PNG", "No", "Yes"}, -1)
        Dim ListViewItem3 As System.Windows.Forms.ListViewItem = New System.Windows.Forms.ListViewItem(New String() {"JPG", "No", "No"}, -1)
        Dim ListViewItem4 As System.Windows.Forms.ListViewItem = New System.Windows.Forms.ListViewItem(New String() {"GIF", "No", "No"}, -1)
        Me.grpOutputFormat = New System.Windows.Forms.GroupBox
        Me.lvwFormats = New System.Windows.Forms.ListView
        Me.colFormat = New System.Windows.Forms.ColumnHeader
        Me.colGlobalMapper = New System.Windows.Forms.ColumnHeader
        Me.colGeoTiffViewer = New System.Windows.Forms.ColumnHeader
        Me.chkGeoTiff = New System.Windows.Forms.CheckBox
        Me.grpTfw = New System.Windows.Forms.GroupBox
        Me.txtShearingX = New System.Windows.Forms.TextBox
        Me.txtShearingY = New System.Windows.Forms.TextBox
        Me.txtPixelHeight = New System.Windows.Forms.TextBox
        Me.txtPixelWidth = New System.Windows.Forms.TextBox
        Me.txtOffsetY = New System.Windows.Forms.TextBox
        Me.txtOffsetX = New System.Windows.Forms.TextBox
        Me.lblShearingY = New System.Windows.Forms.Label
        Me.lblShearingX = New System.Windows.Forms.Label
        Me.lblPixelHeight = New System.Windows.Forms.Label
        Me.lblPixelWidth = New System.Windows.Forms.Label
        Me.lblOffsetY = New System.Windows.Forms.Label
        Me.lblOffsetX = New System.Windows.Forms.Label
        Me.txtName = New System.Windows.Forms.TextBox
        Me.dlgFolderBrowser = New System.Windows.Forms.FolderBrowserDialog
        Me.lblName = New System.Windows.Forms.Label
        Me.grpSave = New System.Windows.Forms.GroupBox
        Me.btnPath = New System.Windows.Forms.Button
        Me.lblPath = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.btnSave = New System.Windows.Forms.Button
        Me.barProgress = New System.Windows.Forms.ProgressBar
        Me.tabMenu = New System.Windows.Forms.TabControl
        Me.TabPage1 = New System.Windows.Forms.TabPage
        Me.gbAgentPatches = New System.Windows.Forms.GroupBox
        Me.pnlAgentPatches = New System.Windows.Forms.Panel
        Me.TabPage2 = New System.Windows.Forms.TabPage
        Me.btnOK = New System.Windows.Forms.Button
        Me.grpOutputFormat.SuspendLayout()
        Me.grpTfw.SuspendLayout()
        Me.grpSave.SuspendLayout()
        Me.tabMenu.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.gbAgentPatches.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.SuspendLayout()
        '
        'grpOutputFormat
        '
        Me.grpOutputFormat.Controls.Add(Me.lvwFormats)
        Me.grpOutputFormat.Controls.Add(Me.chkGeoTiff)
        Me.grpOutputFormat.Location = New System.Drawing.Point(6, 82)
        Me.grpOutputFormat.Name = "grpOutputFormat"
        Me.grpOutputFormat.Size = New System.Drawing.Size(280, 182)
        Me.grpOutputFormat.TabIndex = 1
        Me.grpOutputFormat.TabStop = False
        Me.grpOutputFormat.Text = "Output Formats"
        '
        'lvwFormats
        '
        Me.lvwFormats.CheckBoxes = True
        Me.lvwFormats.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colFormat, Me.colGlobalMapper, Me.colGeoTiffViewer})
        ListViewItem1.Checked = True
        ListViewItem1.StateImageIndex = 1
        ListViewItem1.Tag = "TIF"
        ListViewItem2.Checked = True
        ListViewItem2.StateImageIndex = 1
        ListViewItem2.Tag = "PNG"
        ListViewItem3.StateImageIndex = 0
        ListViewItem3.Tag = "JPG"
        ListViewItem4.StateImageIndex = 0
        ListViewItem4.Tag = "GIF"
        Me.lvwFormats.Items.AddRange(New System.Windows.Forms.ListViewItem() {ListViewItem1, ListViewItem2, ListViewItem3, ListViewItem4})
        Me.lvwFormats.Location = New System.Drawing.Point(12, 23)
        Me.lvwFormats.Name = "lvwFormats"
        Me.lvwFormats.Size = New System.Drawing.Size(256, 117)
        Me.lvwFormats.TabIndex = 5
        Me.lvwFormats.UseCompatibleStateImageBehavior = False
        Me.lvwFormats.View = System.Windows.Forms.View.Details
        '
        'colFormat
        '
        Me.colFormat.Text = "Format"
        '
        'colGlobalMapper
        '
        Me.colGlobalMapper.Text = "Global Mapper"
        Me.colGlobalMapper.Width = 88
        '
        'colGeoTiffViewer
        '
        Me.colGeoTiffViewer.Text = "GeoTiffViewer"
        Me.colGeoTiffViewer.Width = 104
        '
        'chkGeoTiff
        '
        Me.chkGeoTiff.AutoSize = True
        Me.chkGeoTiff.Location = New System.Drawing.Point(12, 153)
        Me.chkGeoTiff.Name = "chkGeoTiff"
        Me.chkGeoTiff.Size = New System.Drawing.Size(135, 17)
        Me.chkGeoTiff.TabIndex = 4
        Me.chkGeoTiff.Text = "Create GeoTIFF (slow!)"
        Me.chkGeoTiff.UseVisualStyleBackColor = True
        '
        'grpTfw
        '
        Me.grpTfw.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpTfw.Controls.Add(Me.txtShearingX)
        Me.grpTfw.Controls.Add(Me.txtShearingY)
        Me.grpTfw.Controls.Add(Me.txtPixelHeight)
        Me.grpTfw.Controls.Add(Me.txtPixelWidth)
        Me.grpTfw.Controls.Add(Me.txtOffsetY)
        Me.grpTfw.Controls.Add(Me.txtOffsetX)
        Me.grpTfw.Controls.Add(Me.lblShearingY)
        Me.grpTfw.Controls.Add(Me.lblShearingX)
        Me.grpTfw.Controls.Add(Me.lblPixelHeight)
        Me.grpTfw.Controls.Add(Me.lblPixelWidth)
        Me.grpTfw.Controls.Add(Me.lblOffsetY)
        Me.grpTfw.Controls.Add(Me.lblOffsetX)
        Me.grpTfw.Location = New System.Drawing.Point(292, 82)
        Me.grpTfw.Name = "grpTfw"
        Me.grpTfw.Size = New System.Drawing.Size(208, 182)
        Me.grpTfw.TabIndex = 2
        Me.grpTfw.TabStop = False
        Me.grpTfw.Text = "TFW Values (meters)"
        '
        'txtShearingX
        '
        Me.txtShearingX.Location = New System.Drawing.Point(100, 124)
        Me.txtShearingX.Name = "txtShearingX"
        Me.txtShearingX.ReadOnly = True
        Me.txtShearingX.Size = New System.Drawing.Size(100, 20)
        Me.txtShearingX.TabIndex = 9
        Me.txtShearingX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtShearingY
        '
        Me.txtShearingY.Location = New System.Drawing.Point(100, 150)
        Me.txtShearingY.Name = "txtShearingY"
        Me.txtShearingY.ReadOnly = True
        Me.txtShearingY.Size = New System.Drawing.Size(100, 20)
        Me.txtShearingY.TabIndex = 11
        Me.txtShearingY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtPixelHeight
        '
        Me.txtPixelHeight.Location = New System.Drawing.Point(100, 98)
        Me.txtPixelHeight.Name = "txtPixelHeight"
        Me.txtPixelHeight.ReadOnly = True
        Me.txtPixelHeight.Size = New System.Drawing.Size(100, 20)
        Me.txtPixelHeight.TabIndex = 7
        Me.txtPixelHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtPixelWidth
        '
        Me.txtPixelWidth.Location = New System.Drawing.Point(100, 72)
        Me.txtPixelWidth.Name = "txtPixelWidth"
        Me.txtPixelWidth.ReadOnly = True
        Me.txtPixelWidth.Size = New System.Drawing.Size(100, 20)
        Me.txtPixelWidth.TabIndex = 5
        Me.txtPixelWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtOffsetY
        '
        Me.txtOffsetY.Location = New System.Drawing.Point(100, 46)
        Me.txtOffsetY.Name = "txtOffsetY"
        Me.txtOffsetY.ReadOnly = True
        Me.txtOffsetY.Size = New System.Drawing.Size(100, 20)
        Me.txtOffsetY.TabIndex = 3
        Me.txtOffsetY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtOffsetX
        '
        Me.txtOffsetX.Location = New System.Drawing.Point(100, 20)
        Me.txtOffsetX.Name = "txtOffsetX"
        Me.txtOffsetX.ReadOnly = True
        Me.txtOffsetX.Size = New System.Drawing.Size(100, 20)
        Me.txtOffsetX.TabIndex = 1
        Me.txtOffsetX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'lblShearingY
        '
        Me.lblShearingY.AutoSize = True
        Me.lblShearingY.Location = New System.Drawing.Point(9, 153)
        Me.lblShearingY.Name = "lblShearingY"
        Me.lblShearingY.Size = New System.Drawing.Size(62, 13)
        Me.lblShearingY.TabIndex = 10
        Me.lblShearingY.Text = "Shearing Y:"
        '
        'lblShearingX
        '
        Me.lblShearingX.AutoSize = True
        Me.lblShearingX.Location = New System.Drawing.Point(9, 127)
        Me.lblShearingX.Name = "lblShearingX"
        Me.lblShearingX.Size = New System.Drawing.Size(62, 13)
        Me.lblShearingX.TabIndex = 8
        Me.lblShearingX.Text = "Shearing X:"
        '
        'lblPixelHeight
        '
        Me.lblPixelHeight.AutoSize = True
        Me.lblPixelHeight.Location = New System.Drawing.Point(9, 101)
        Me.lblPixelHeight.Name = "lblPixelHeight"
        Me.lblPixelHeight.Size = New System.Drawing.Size(66, 13)
        Me.lblPixelHeight.TabIndex = 6
        Me.lblPixelHeight.Text = "Pixel Height:"
        '
        'lblPixelWidth
        '
        Me.lblPixelWidth.AutoSize = True
        Me.lblPixelWidth.Location = New System.Drawing.Point(9, 75)
        Me.lblPixelWidth.Name = "lblPixelWidth"
        Me.lblPixelWidth.Size = New System.Drawing.Size(63, 13)
        Me.lblPixelWidth.TabIndex = 4
        Me.lblPixelWidth.Text = "Pixel Width:"
        '
        'lblOffsetY
        '
        Me.lblOffsetY.AutoSize = True
        Me.lblOffsetY.Location = New System.Drawing.Point(9, 49)
        Me.lblOffsetY.Name = "lblOffsetY"
        Me.lblOffsetY.Size = New System.Drawing.Size(48, 13)
        Me.lblOffsetY.TabIndex = 2
        Me.lblOffsetY.Text = "Offset Y:"
        '
        'lblOffsetX
        '
        Me.lblOffsetX.AutoSize = True
        Me.lblOffsetX.Location = New System.Drawing.Point(9, 23)
        Me.lblOffsetX.Name = "lblOffsetX"
        Me.lblOffsetX.Size = New System.Drawing.Size(48, 13)
        Me.lblOffsetX.TabIndex = 0
        Me.lblOffsetX.Text = "Offset X:"
        '
        'txtName
        '
        Me.txtName.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtName.Location = New System.Drawing.Point(79, 41)
        Me.txtName.Name = "txtName"
        Me.txtName.Size = New System.Drawing.Size(325, 20)
        Me.txtName.TabIndex = 1
        '
        'lblName
        '
        Me.lblName.AutoSize = True
        Me.lblName.Location = New System.Drawing.Point(9, 44)
        Me.lblName.Name = "lblName"
        Me.lblName.Size = New System.Drawing.Size(52, 13)
        Me.lblName.TabIndex = 0
        Me.lblName.Text = "Filename:"
        '
        'grpSave
        '
        Me.grpSave.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpSave.Controls.Add(Me.btnPath)
        Me.grpSave.Controls.Add(Me.lblPath)
        Me.grpSave.Controls.Add(Me.Label1)
        Me.grpSave.Controls.Add(Me.btnSave)
        Me.grpSave.Controls.Add(Me.lblName)
        Me.grpSave.Controls.Add(Me.txtName)
        Me.grpSave.Location = New System.Drawing.Point(6, 6)
        Me.grpSave.Name = "grpSave"
        Me.grpSave.Size = New System.Drawing.Size(494, 70)
        Me.grpSave.TabIndex = 0
        Me.grpSave.TabStop = False
        Me.grpSave.Text = "Save Map"
        '
        'btnPath
        '
        Me.btnPath.Cursor = System.Windows.Forms.Cursors.Arrow
        Me.btnPath.FlatAppearance.BorderSize = 0
        Me.btnPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnPath.Image = Global.UvARescue.Usar.[Lib].My.Resources.Resources.FolderSmall
        Me.btnPath.Location = New System.Drawing.Point(411, 13)
        Me.btnPath.Name = "btnPath"
        Me.btnPath.Size = New System.Drawing.Size(22, 24)
        Me.btnPath.TabIndex = 6
        Me.btnPath.UseVisualStyleBackColor = True
        '
        'lblPath
        '
        Me.lblPath.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.lblPath.Location = New System.Drawing.Point(79, 19)
        Me.lblPath.Name = "lblPath"
        Me.lblPath.Size = New System.Drawing.Size(326, 15)
        Me.lblPath.TabIndex = 5
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(9, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(32, 13)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Path:"
        '
        'btnSave
        '
        Me.btnSave.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnSave.Location = New System.Drawing.Point(410, 39)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(75, 23)
        Me.btnSave.TabIndex = 3
        Me.btnSave.Text = "Save Now"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'barProgress
        '
        Me.barProgress.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.barProgress.Location = New System.Drawing.Point(6, 269)
        Me.barProgress.Name = "barProgress"
        Me.barProgress.Size = New System.Drawing.Size(494, 23)
        Me.barProgress.TabIndex = 4
        '
        'tabMenu
        '
        Me.tabMenu.Controls.Add(Me.TabPage1)
        Me.tabMenu.Controls.Add(Me.TabPage2)
        Me.tabMenu.Location = New System.Drawing.Point(6, 6)
        Me.tabMenu.Name = "tabMenu"
        Me.tabMenu.SelectedIndex = 0
        Me.tabMenu.Size = New System.Drawing.Size(521, 321)
        Me.tabMenu.TabIndex = 5
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.gbAgentPatches)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(513, 295)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "TabPage1"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'gbAgentPatches
        '
        Me.gbAgentPatches.Controls.Add(Me.pnlAgentPatches)
        Me.gbAgentPatches.Location = New System.Drawing.Point(6, 6)
        Me.gbAgentPatches.Name = "gbAgentPatches"
        Me.gbAgentPatches.Padding = New System.Windows.Forms.Padding(8, 3, 3, 3)
        Me.gbAgentPatches.Size = New System.Drawing.Size(504, 283)
        Me.gbAgentPatches.TabIndex = 2
        Me.gbAgentPatches.TabStop = False
        Me.gbAgentPatches.Text = "Individual Agent Contributions"
        '
        'pnlAgentPatches
        '
        Me.pnlAgentPatches.AutoScroll = True
        Me.pnlAgentPatches.Location = New System.Drawing.Point(0, 19)
        Me.pnlAgentPatches.Name = "pnlAgentPatches"
        Me.pnlAgentPatches.Size = New System.Drawing.Size(501, 258)
        Me.pnlAgentPatches.TabIndex = 0
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.grpSave)
        Me.TabPage2.Controls.Add(Me.barProgress)
        Me.TabPage2.Controls.Add(Me.grpTfw)
        Me.TabPage2.Controls.Add(Me.grpOutputFormat)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(513, 295)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "TabPage2"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'btnOK
        '
        Me.btnOK.Location = New System.Drawing.Point(452, 333)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(75, 23)
        Me.btnOK.TabIndex = 6
        Me.btnOK.Text = "Close"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'SaveMapDialog
        '
        Me.AcceptButton = Me.btnSave
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.ClientSize = New System.Drawing.Size(533, 363)
        Me.ControlBox = False
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.tabMenu)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "SaveMapDialog"
        Me.Padding = New System.Windows.Forms.Padding(3)
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Process and Save Map"
        Me.grpOutputFormat.ResumeLayout(False)
        Me.grpOutputFormat.PerformLayout()
        Me.grpTfw.ResumeLayout(False)
        Me.grpTfw.PerformLayout()
        Me.grpSave.ResumeLayout(False)
        Me.grpSave.PerformLayout()
        Me.tabMenu.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.gbAgentPatches.ResumeLayout(False)
        Me.TabPage2.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents grpOutputFormat As System.Windows.Forms.GroupBox
    Friend WithEvents grpTfw As System.Windows.Forms.GroupBox
    Friend WithEvents lblOffsetY As System.Windows.Forms.Label
    Friend WithEvents lblOffsetX As System.Windows.Forms.Label
    Friend WithEvents txtName As System.Windows.Forms.TextBox
    Friend WithEvents dlgFolderBrowser As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents lblName As System.Windows.Forms.Label
    Friend WithEvents lblShearingY As System.Windows.Forms.Label
    Friend WithEvents lblShearingX As System.Windows.Forms.Label
    Friend WithEvents lblPixelHeight As System.Windows.Forms.Label
    Friend WithEvents lblPixelWidth As System.Windows.Forms.Label
    Friend WithEvents grpSave As System.Windows.Forms.GroupBox
    Friend WithEvents txtShearingX As System.Windows.Forms.TextBox
    Friend WithEvents txtShearingY As System.Windows.Forms.TextBox
    Friend WithEvents txtPixelHeight As System.Windows.Forms.TextBox
    Friend WithEvents txtPixelWidth As System.Windows.Forms.TextBox
    Friend WithEvents txtOffsetY As System.Windows.Forms.TextBox
    Friend WithEvents txtOffsetX As System.Windows.Forms.TextBox
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents barProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents chkGeoTiff As System.Windows.Forms.CheckBox
    Friend WithEvents lvwFormats As System.Windows.Forms.ListView
    Friend WithEvents colFormat As System.Windows.Forms.ColumnHeader
    Friend WithEvents colGlobalMapper As System.Windows.Forms.ColumnHeader
    Friend WithEvents colGeoTiffViewer As System.Windows.Forms.ColumnHeader
    Friend WithEvents tabMenu As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents gbAgentPatches As System.Windows.Forms.GroupBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblPath As System.Windows.Forms.Label
    Friend WithEvents btnPath As System.Windows.Forms.Button
    Friend WithEvents btnOK As System.Windows.Forms.Button
    Friend WithEvents pnlAgentPatches As System.Windows.Forms.Panel

End Class
