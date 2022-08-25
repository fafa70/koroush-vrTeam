<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TeamController

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnAdd = New System.Windows.Forms.Button()
        Me.btnRemove = New System.Windows.Forms.Button()
        Me.btnNetwork = New System.Windows.Forms.Button()
        Me.btnReady = New System.Windows.Forms.Button()
        Me.pnlOnline = New System.Windows.Forms.Panel()
        Me.lblTime = New System.Windows.Forms.Label()
        Me.btnProceed = New System.Windows.Forms.Button()
        Me.pnlSelectNamingConvention = New System.Windows.Forms.FlowLayoutPanel()
        Me.pnlBack = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.btnBack = New System.Windows.Forms.Button()
        Me.pnlAgents = New System.Windows.Forms.Panel()
        Me.Timer = New System.Windows.Forms.Timer(Me.components)
        Me.lblRunStatus = New System.Windows.Forms.Label()
        Me.pnlButtons.SuspendLayout()
        Me.pnlOnline.SuspendLayout()
        Me.pnlBack.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlButtons
        '
        Me.pnlButtons.BackColor = System.Drawing.Color.LightGray
        Me.pnlButtons.Controls.Add(Me.btnAdd)
        Me.pnlButtons.Controls.Add(Me.btnRemove)
        Me.pnlButtons.Controls.Add(Me.btnNetwork)
        Me.pnlButtons.Controls.Add(Me.btnReady)
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlButtons.Location = New System.Drawing.Point(0, 345)
        Me.pnlButtons.Name = "pnlButtons"
        Me.pnlButtons.Size = New System.Drawing.Size(263, 50)
        Me.pnlButtons.TabIndex = 2
        Me.pnlButtons.WrapContents = False
        '
        'btnAdd
        '
        Me.btnAdd.BackColor = System.Drawing.Color.DarkGray
        Me.btnAdd.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnAdd.Image = Global.UvARescue.Usar.[Lib].My.Resources.Resources.Add
        Me.btnAdd.Location = New System.Drawing.Point(216, 3)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(44, 44)
        Me.btnAdd.TabIndex = 0
        Me.btnAdd.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.btnAdd.UseVisualStyleBackColor = False
        '
        'btnRemove
        '
        Me.btnRemove.BackColor = System.Drawing.Color.DarkGray
        Me.btnRemove.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnRemove.Image = Global.UvARescue.Usar.[Lib].My.Resources.Resources.Remove
        Me.btnRemove.Location = New System.Drawing.Point(166, 3)
        Me.btnRemove.Name = "btnRemove"
        Me.btnRemove.Size = New System.Drawing.Size(44, 44)
        Me.btnRemove.TabIndex = 0
        Me.btnRemove.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.btnRemove.UseVisualStyleBackColor = False
        '
        'btnNetwork
        '
        Me.btnNetwork.BackColor = System.Drawing.Color.DarkGray
        Me.btnNetwork.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnNetwork.Image = Global.UvARescue.Usar.[Lib].My.Resources.Resources.Network
        Me.btnNetwork.Location = New System.Drawing.Point(116, 3)
        Me.btnNetwork.Name = "btnNetwork"
        Me.btnNetwork.Size = New System.Drawing.Size(44, 44)
        Me.btnNetwork.TabIndex = 0
        Me.btnNetwork.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.btnNetwork.UseVisualStyleBackColor = False
        '
        'btnReady
        '
        Me.btnReady.BackColor = System.Drawing.Color.DarkGray
        Me.btnReady.Enabled = False
        Me.btnReady.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnReady.Image = Global.UvARescue.Usar.[Lib].My.Resources.Resources.Ready
        Me.btnReady.Location = New System.Drawing.Point(66, 3)
        Me.btnReady.Name = "btnReady"
        Me.btnReady.Size = New System.Drawing.Size(44, 44)
        Me.btnReady.TabIndex = 0
        Me.btnReady.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.btnReady.UseVisualStyleBackColor = False
        '
        'pnlOnline
        '
        Me.pnlOnline.BackColor = System.Drawing.Color.LightGray
        Me.pnlOnline.Controls.Add(Me.lblTime)
        Me.pnlOnline.Controls.Add(Me.btnProceed)
        Me.pnlOnline.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlOnline.Location = New System.Drawing.Point(0, 23)
        Me.pnlOnline.Name = "pnlOnline"
        Me.pnlOnline.Size = New System.Drawing.Size(263, 50)
        Me.pnlOnline.TabIndex = 4
        '
        'lblTime
        '
        Me.lblTime.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblTime.BackColor = System.Drawing.Color.LightGray
        Me.lblTime.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTime.ForeColor = System.Drawing.Color.White
        Me.lblTime.Location = New System.Drawing.Point(3, 3)
        Me.lblTime.Margin = New System.Windows.Forms.Padding(3)
        Me.lblTime.Name = "lblTime"
        Me.lblTime.Size = New System.Drawing.Size(153, 44)
        Me.lblTime.TabIndex = 1
        Me.lblTime.Text = "20:00"
        Me.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnProceed
        '
        Me.btnProceed.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnProceed.BackColor = System.Drawing.Color.Coral
        Me.btnProceed.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnProceed.Image = Global.UvARescue.Usar.[Lib].My.Resources.Resources.Report
        Me.btnProceed.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnProceed.Location = New System.Drawing.Point(166, 3)
        Me.btnProceed.Name = "btnProceed"
        Me.btnProceed.Size = New System.Drawing.Size(94, 44)
        Me.btnProceed.TabIndex = 0
        Me.btnProceed.Text = "Done / Report"
        Me.btnProceed.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.btnProceed.UseVisualStyleBackColor = False
        '
        'pnlSelectNamingConvention
        '
        Me.pnlSelectNamingConvention.BackColor = System.Drawing.Color.LightGray
        Me.pnlSelectNamingConvention.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlSelectNamingConvention.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlSelectNamingConvention.Location = New System.Drawing.Point(0, 73)
        Me.pnlSelectNamingConvention.Name = "pnlSelectNamingConvention"
        Me.pnlSelectNamingConvention.Size = New System.Drawing.Size(263, 52)
        Me.pnlSelectNamingConvention.TabIndex = 8
        '
        'pnlBack
        '
        Me.pnlBack.BackColor = System.Drawing.Color.LightGray
        Me.pnlBack.Controls.Add(Me.btnStart)
        Me.pnlBack.Controls.Add(Me.btnBack)
        Me.pnlBack.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlBack.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlBack.Location = New System.Drawing.Point(0, 295)
        Me.pnlBack.Name = "pnlBack"
        Me.pnlBack.Size = New System.Drawing.Size(263, 50)
        Me.pnlBack.TabIndex = 10
        Me.pnlBack.WrapContents = False
        '
        'btnStart
        '
        Me.btnStart.BackColor = System.Drawing.Color.DarkGray
        Me.btnStart.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnStart.Image = Global.UvARescue.Usar.[Lib].My.Resources.Resources.Start
        Me.btnStart.Location = New System.Drawing.Point(166, 3)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(94, 41)
        Me.btnStart.TabIndex = 0
        Me.btnStart.Text = "Start"
        Me.btnStart.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage
        Me.btnStart.UseVisualStyleBackColor = False
        '
        'btnBack
        '
        Me.btnBack.BackColor = System.Drawing.Color.DarkGray
        Me.btnBack.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBack.Image = Global.UvARescue.Usar.[Lib].My.Resources.Resources.Back
        Me.btnBack.Location = New System.Drawing.Point(66, 3)
        Me.btnBack.Name = "btnBack"
        Me.btnBack.Size = New System.Drawing.Size(94, 44)
        Me.btnBack.TabIndex = 0
        Me.btnBack.Text = "Back"
        Me.btnBack.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.btnBack.UseVisualStyleBackColor = False
        '
        'pnlAgents
        '
        Me.pnlAgents.AutoScroll = True
        Me.pnlAgents.BackColor = System.Drawing.Color.LightGray
        Me.pnlAgents.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlAgents.Location = New System.Drawing.Point(0, 125)
        Me.pnlAgents.Name = "pnlAgents"
        Me.pnlAgents.Size = New System.Drawing.Size(263, 170)
        Me.pnlAgents.TabIndex = 11
        '
        'Timer
        '
        Me.Timer.Interval = 1000
        '
        'lblRunStatus
        '
        Me.lblRunStatus.BackColor = System.Drawing.Color.LightGray
        Me.lblRunStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblRunStatus.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblRunStatus.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lblRunStatus.ForeColor = System.Drawing.Color.Black
        Me.lblRunStatus.Location = New System.Drawing.Point(0, 0)
        Me.lblRunStatus.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.lblRunStatus.Name = "lblRunStatus"
        Me.lblRunStatus.Size = New System.Drawing.Size(263, 23)
        Me.lblRunStatus.TabIndex = 12
        Me.lblRunStatus.Text = "RunStatus"
        Me.lblRunStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'TeamController
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Black
        Me.Controls.Add(Me.pnlAgents)
        Me.Controls.Add(Me.pnlBack)
        Me.Controls.Add(Me.pnlSelectNamingConvention)
        Me.Controls.Add(Me.pnlOnline)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.lblRunStatus)
        Me.Name = "TeamController"
        Me.Size = New System.Drawing.Size(263, 395)
        Me.pnlButtons.ResumeLayout(False)
        Me.pnlOnline.ResumeLayout(False)
        Me.pnlBack.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnAdd As System.Windows.Forms.Button
    Friend WithEvents btnRemove As System.Windows.Forms.Button
    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents pnlOnline As System.Windows.Forms.Panel
    Friend WithEvents lblTime As System.Windows.Forms.Label
    Friend WithEvents pnlSelectNamingConvention As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnReady As System.Windows.Forms.Button
    Friend WithEvents pnlBack As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnStart As System.Windows.Forms.Button
    Friend WithEvents pnlAgents As System.Windows.Forms.Panel
    Friend WithEvents btnBack As System.Windows.Forms.Button
    Friend WithEvents btnProceed As System.Windows.Forms.Button
    Friend WithEvents Timer As System.Windows.Forms.Timer
    Friend WithEvents lblRunStatus As System.Windows.Forms.Label
    Friend WithEvents btnNetwork As System.Windows.Forms.Button

End Class
