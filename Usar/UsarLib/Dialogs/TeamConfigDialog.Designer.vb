<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TeamConfigDialog

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
        Me.lblImageServerPort = New System.Windows.Forms.Label()
        Me.lblUsarSimPort = New System.Windows.Forms.Label()
        Me.lblUsarSimServer = New System.Windows.Forms.Label()
        Me.txtImageServerPort = New System.Windows.Forms.TextBox()
        Me.txtUsarSimPort = New System.Windows.Forms.TextBox()
        Me.txtImageServerHost = New System.Windows.Forms.TextBox()
        Me.txtUsarSimHost = New System.Windows.Forms.TextBox()
        Me.lblImageServer = New System.Windows.Forms.Label()
        Me.txtLocalhost = New System.Windows.Forms.TextBox()
        Me.lblLocalhost = New System.Windows.Forms.Label()
        Me.txtWirelessServerPort = New System.Windows.Forms.TextBox()
        Me.lblWirelessServerPort = New System.Windows.Forms.Label()
        Me.txtWirelessServerHost = New System.Windows.Forms.TextBox()
        Me.lblWirelessServer = New System.Windows.Forms.Label()
        Me.grpNetwork = New System.Windows.Forms.GroupBox()
        Me.btnGetStartPoses = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtBackupFrequency = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.chkboxBackup = New System.Windows.Forms.CheckBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.ComboBox1 = New System.Windows.Forms.ComboBox()
        Me.ComboBox2 = New System.Windows.Forms.ComboBox()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.grpNetwork.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblImageServerPort
        '
        Me.lblImageServerPort.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblImageServerPort.AutoSize = True
        Me.lblImageServerPort.Location = New System.Drawing.Point(150, 77)
        Me.lblImageServerPort.Name = "lblImageServerPort"
        Me.lblImageServerPort.Size = New System.Drawing.Size(29, 13)
        Me.lblImageServerPort.TabIndex = 8
        Me.lblImageServerPort.Text = "Port:"
        '
        'lblUsarSimPort
        '
        Me.lblUsarSimPort.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblUsarSimPort.AutoSize = True
        Me.lblUsarSimPort.Location = New System.Drawing.Point(159, 52)
        Me.lblUsarSimPort.Name = "lblUsarSimPort"
        Me.lblUsarSimPort.Size = New System.Drawing.Size(29, 13)
        Me.lblUsarSimPort.TabIndex = 4
        Me.lblUsarSimPort.Text = "Port:"
        '
        'lblUsarSimServer
        '
        Me.lblUsarSimServer.AutoSize = True
        Me.lblUsarSimServer.Location = New System.Drawing.Point(3, 52)
        Me.lblUsarSimServer.Name = "lblUsarSimServer"
        Me.lblUsarSimServer.Size = New System.Drawing.Size(83, 13)
        Me.lblUsarSimServer.TabIndex = 2
        Me.lblUsarSimServer.Text = "UsarSim Server:"
        '
        'txtImageServerPort
        '
        Me.txtImageServerPort.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtImageServerPort.Location = New System.Drawing.Point(182, 75)
        Me.txtImageServerPort.Name = "txtImageServerPort"
        Me.txtImageServerPort.Size = New System.Drawing.Size(103, 20)
        Me.txtImageServerPort.TabIndex = 9
        Me.txtImageServerPort.WordWrap = False
        '
        'txtUsarSimPort
        '
        Me.txtUsarSimPort.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtUsarSimPort.Location = New System.Drawing.Point(186, 45)
        Me.txtUsarSimPort.Name = "txtUsarSimPort"
        Me.txtUsarSimPort.Size = New System.Drawing.Size(101, 20)
        Me.txtUsarSimPort.TabIndex = 5
        Me.txtUsarSimPort.WordWrap = False
        '
        'txtImageServerHost
        '
        Me.txtImageServerHost.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtImageServerHost.Location = New System.Drawing.Point(90, 74)
        Me.txtImageServerHost.Name = "txtImageServerHost"
        Me.txtImageServerHost.Size = New System.Drawing.Size(57, 20)
        Me.txtImageServerHost.TabIndex = 7
        Me.txtImageServerHost.WordWrap = False
        '
        'txtUsarSimHost
        '
        Me.txtUsarSimHost.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtUsarSimHost.Location = New System.Drawing.Point(90, 49)
        Me.txtUsarSimHost.Name = "txtUsarSimHost"
        Me.txtUsarSimHost.Size = New System.Drawing.Size(57, 20)
        Me.txtUsarSimHost.TabIndex = 3
        Me.txtUsarSimHost.WordWrap = False
        '
        'lblImageServer
        '
        Me.lblImageServer.AutoSize = True
        Me.lblImageServer.Location = New System.Drawing.Point(6, 78)
        Me.lblImageServer.Name = "lblImageServer"
        Me.lblImageServer.Size = New System.Drawing.Size(73, 13)
        Me.lblImageServer.TabIndex = 6
        Me.lblImageServer.Text = "Image Server:"
        '
        'txtLocalhost
        '
        Me.txtLocalhost.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLocalhost.Location = New System.Drawing.Point(87, 19)
        Me.txtLocalhost.Name = "txtLocalhost"
        Me.txtLocalhost.Size = New System.Drawing.Size(104, 20)
        Me.txtLocalhost.TabIndex = 1
        Me.txtLocalhost.WordWrap = False
        '
        'lblLocalhost
        '
        Me.lblLocalhost.AutoSize = True
        Me.lblLocalhost.Location = New System.Drawing.Point(12, 22)
        Me.lblLocalhost.Name = "lblLocalhost"
        Me.lblLocalhost.Size = New System.Drawing.Size(69, 13)
        Me.lblLocalhost.TabIndex = 0
        Me.lblLocalhost.Text = "Localhost IP:"
        '
        'txtWirelessServerPort
        '
        Me.txtWirelessServerPort.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtWirelessServerPort.Location = New System.Drawing.Point(186, 99)
        Me.txtWirelessServerPort.Name = "txtWirelessServerPort"
        Me.txtWirelessServerPort.Size = New System.Drawing.Size(99, 20)
        Me.txtWirelessServerPort.TabIndex = 13
        Me.txtWirelessServerPort.WordWrap = False
        '
        'lblWirelessServerPort
        '
        Me.lblWirelessServerPort.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblWirelessServerPort.AutoSize = True
        Me.lblWirelessServerPort.Location = New System.Drawing.Point(150, 102)
        Me.lblWirelessServerPort.Name = "lblWirelessServerPort"
        Me.lblWirelessServerPort.Size = New System.Drawing.Size(29, 13)
        Me.lblWirelessServerPort.TabIndex = 12
        Me.lblWirelessServerPort.Text = "Port:"
        '
        'txtWirelessServerHost
        '
        Me.txtWirelessServerHost.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtWirelessServerHost.Location = New System.Drawing.Point(90, 102)
        Me.txtWirelessServerHost.Name = "txtWirelessServerHost"
        Me.txtWirelessServerHost.Size = New System.Drawing.Size(57, 20)
        Me.txtWirelessServerHost.TabIndex = 11
        Me.txtWirelessServerHost.WordWrap = False
        '
        'lblWirelessServer
        '
        Me.lblWirelessServer.AutoSize = True
        Me.lblWirelessServer.Location = New System.Drawing.Point(3, 102)
        Me.lblWirelessServer.Name = "lblWirelessServer"
        Me.lblWirelessServer.Size = New System.Drawing.Size(84, 13)
        Me.lblWirelessServer.TabIndex = 10
        Me.lblWirelessServer.Text = "Wireless Server:"
        '
        'grpNetwork
        '
        Me.grpNetwork.Controls.Add(Me.btnGetStartPoses)
        Me.grpNetwork.Controls.Add(Me.lblLocalhost)
        Me.grpNetwork.Controls.Add(Me.lblWirelessServerPort)
        Me.grpNetwork.Controls.Add(Me.txtUsarSimHost)
        Me.grpNetwork.Controls.Add(Me.txtImageServerHost)
        Me.grpNetwork.Controls.Add(Me.lblImageServerPort)
        Me.grpNetwork.Controls.Add(Me.txtLocalhost)
        Me.grpNetwork.Controls.Add(Me.lblUsarSimPort)
        Me.grpNetwork.Controls.Add(Me.txtWirelessServerHost)
        Me.grpNetwork.Controls.Add(Me.lblWirelessServer)
        Me.grpNetwork.Controls.Add(Me.txtUsarSimPort)
        Me.grpNetwork.Controls.Add(Me.lblImageServer)
        Me.grpNetwork.Controls.Add(Me.txtImageServerPort)
        Me.grpNetwork.Controls.Add(Me.txtWirelessServerPort)
        Me.grpNetwork.Controls.Add(Me.lblUsarSimServer)
        Me.grpNetwork.Location = New System.Drawing.Point(7, 7)
        Me.grpNetwork.Name = "grpNetwork"
        Me.grpNetwork.Size = New System.Drawing.Size(293, 129)
        Me.grpNetwork.TabIndex = 1000
        Me.grpNetwork.TabStop = False
        Me.grpNetwork.Text = "Network"
        '
        'btnGetStartPoses
        '
        Me.btnGetStartPoses.Location = New System.Drawing.Point(198, 17)
        Me.btnGetStartPoses.Name = "btnGetStartPoses"
        Me.btnGetStartPoses.Size = New System.Drawing.Size(89, 23)
        Me.btnGetStartPoses.TabIndex = 14
        Me.btnGetStartPoses.Text = "Get Start Poses"
        Me.btnGetStartPoses.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtBackupFrequency)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.chkboxBackup)
        Me.GroupBox1.Location = New System.Drawing.Point(2, 142)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(292, 47)
        Me.GroupBox1.TabIndex = 1002
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Backups"
        '
        'txtBackupFrequency
        '
        Me.txtBackupFrequency.Location = New System.Drawing.Point(101, 17)
        Me.txtBackupFrequency.Name = "txtBackupFrequency"
        Me.txtBackupFrequency.Size = New System.Drawing.Size(35, 20)
        Me.txtBackupFrequency.TabIndex = 2
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(142, 20)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(47, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "seconds"
        '
        'chkboxBackup
        '
        Me.chkboxBackup.AutoSize = True
        Me.chkboxBackup.Checked = True
        Me.chkboxBackup.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkboxBackup.Location = New System.Drawing.Point(11, 19)
        Me.chkboxBackup.Name = "chkboxBackup"
        Me.chkboxBackup.Size = New System.Drawing.Size(95, 17)
        Me.chkboxBackup.TabIndex = 0
        Me.chkboxBackup.Text = "Backup every "
        Me.chkboxBackup.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        Me.Panel1.Location = New System.Drawing.Point(300, 35)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(758, 606)
        Me.Panel1.TabIndex = 1004
        '
        'ComboBox1
        '
        Me.ComboBox1.FormattingEnabled = True
        Me.ComboBox1.Location = New System.Drawing.Point(380, 9)
        Me.ComboBox1.Name = "ComboBox1"
        Me.ComboBox1.Size = New System.Drawing.Size(121, 21)
        Me.ComboBox1.TabIndex = 14
        '
        'ComboBox2
        '
        Me.ComboBox2.FormattingEnabled = True
        Me.ComboBox2.Location = New System.Drawing.Point(672, 8)
        Me.ComboBox2.Name = "ComboBox2"
        Me.ComboBox2.Size = New System.Drawing.Size(121, 21)
        Me.ComboBox2.TabIndex = 1006
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(545, 9)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(100, 20)
        Me.TextBox1.TabIndex = 1007
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(817, 6)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 1008
        Me.Button1.Text = "spawn"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(927, 5)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(75, 23)
        Me.Button2.TabIndex = 1009
        Me.Button2.Text = "ok"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'TeamConfigDialog
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1062, 648)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.ComboBox2)
        Me.Controls.Add(Me.ComboBox1)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.grpNetwork)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "TeamConfigDialog"
        Me.Padding = New System.Windows.Forms.Padding(4)
        Me.Text = "Team Configuration"
        Me.grpNetwork.ResumeLayout(False)
        Me.grpNetwork.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblImageServerPort As System.Windows.Forms.Label
    Friend WithEvents lblUsarSimPort As System.Windows.Forms.Label
    Friend WithEvents lblUsarSimServer As System.Windows.Forms.Label
    Friend WithEvents txtImageServerPort As System.Windows.Forms.TextBox
    Friend WithEvents txtUsarSimPort As System.Windows.Forms.TextBox
    Friend WithEvents txtImageServerHost As System.Windows.Forms.TextBox
    Friend WithEvents txtUsarSimHost As System.Windows.Forms.TextBox
    Friend WithEvents lblImageServer As System.Windows.Forms.Label
    Friend WithEvents txtLocalhost As System.Windows.Forms.TextBox
    Friend WithEvents lblLocalhost As System.Windows.Forms.Label
    Friend WithEvents txtWirelessServerPort As System.Windows.Forms.TextBox
    Friend WithEvents lblWirelessServerPort As System.Windows.Forms.Label
    Friend WithEvents txtWirelessServerHost As System.Windows.Forms.TextBox
    Friend WithEvents lblWirelessServer As System.Windows.Forms.Label
    Friend WithEvents grpNetwork As System.Windows.Forms.GroupBox
    Friend WithEvents btnGetStartPoses As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents chkboxBackup As System.Windows.Forms.CheckBox
    Friend WithEvents txtBackupFrequency As System.Windows.Forms.TextBox
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents ComboBox1 As System.Windows.Forms.ComboBox
    Friend WithEvents ComboBox2 As System.Windows.Forms.ComboBox
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Button2 As System.Windows.Forms.Button
End Class
