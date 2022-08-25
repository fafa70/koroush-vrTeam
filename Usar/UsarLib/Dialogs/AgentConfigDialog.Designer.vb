<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AgentConfigDialog

    'UserControl overrides dispose to clean up the component list.
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
        Me.cboModel = New System.Windows.Forms.ComboBox()
        Me.lblRobotModel = New System.Windows.Forms.Label()
        Me.lblStartLocation = New System.Windows.Forms.Label()
        Me.lblStartRotation = New System.Windows.Forms.Label()
        Me.lblMeter1 = New System.Windows.Forms.Label()
        Me.lblRadians = New System.Windows.Forms.Label()
        Me.txtLocation = New System.Windows.Forms.ComboBox()
        Me.txtRotation = New System.Windows.Forms.ComboBox()
        Me.optLive = New System.Windows.Forms.RadioButton()
        Me.optPlayback = New System.Windows.Forms.RadioButton()
        Me.pnlLive = New System.Windows.Forms.Panel()
        Me.chkUseImageServer = New System.Windows.Forms.CheckBox()
        Me.chkUseLogger = New System.Windows.Forms.CheckBox()
        Me.pnlPlayback = New System.Windows.Forms.Panel()
        Me.lnkLogfile = New System.Windows.Forms.LinkLabel()
        Me.lblLogfileFormat = New System.Windows.Forms.Label()
        Me.cboLogfileFormat = New System.Windows.Forms.ComboBox()
        Me.optUsarBase = New System.Windows.Forms.RadioButton()
        Me.optUsarFollow = New System.Windows.Forms.RadioButton()
        Me.optUsarSlam = New System.Windows.Forms.RadioButton()
        Me.cboSeedMode = New System.Windows.Forms.ComboBox()
        Me.cboMappingMode = New System.Windows.Forms.ComboBox()
        Me.cboScanMatcher = New System.Windows.Forms.ComboBox()
        Me.lblSeedMode = New System.Windows.Forms.Label()
        Me.lblScanMatcher = New System.Windows.Forms.Label()
        Me.lblMappingMode = New System.Windows.Forms.Label()
        Me.grpSpawnConfig = New System.Windows.Forms.GroupBox()
        Me.cboStartPoseName = New System.Windows.Forms.ComboBox()
        Me.numAgentNumber = New System.Windows.Forms.NumericUpDown()
        Me.lblAgentNumber = New System.Windows.Forms.Label()
        Me.chkSpawnFromCommander = New System.Windows.Forms.CheckBox()
        Me.grpAgentType = New System.Windows.Forms.GroupBox()
        Me.optUsarSkinDet = New System.Windows.Forms.RadioButton()
        Me.optUsarOperator = New System.Windows.Forms.RadioButton()
        Me.grpSlamConfig = New System.Windows.Forms.GroupBox()
        Me.txtNoiseSigma = New System.Windows.Forms.TextBox()
        Me.chkNoise = New System.Windows.Forms.CheckBox()
        Me.lblMeter2 = New System.Windows.Forms.Label()
        Me.grpDriver = New System.Windows.Forms.GroupBox()
        Me.pnlMultiView = New System.Windows.Forms.Panel()
        Me.chkUseMultiView = New System.Windows.Forms.CheckBox()
        Me.lblMultiViewPanels = New System.Windows.Forms.Label()
        Me.txtMultiViewPanels = New System.Windows.Forms.TextBox()
        Me.grpSkinDet = New System.Windows.Forms.GroupBox()
        Me.txtDetTheta = New System.Windows.Forms.TextBox()
        Me.cboTeachMode = New System.Windows.Forms.ComboBox()
        Me.lblSkinDetMode = New System.Windows.Forms.Label()
        Me.lblDetTheta = New System.Windows.Forms.Label()
        Me.cboSkinDetMode = New System.Windows.Forms.ComboBox()
        Me.lblTeachMode = New System.Windows.Forms.Label()
        Me.grpBehaviorConfig = New System.Windows.Forms.GroupBox()
        Me.lblPathGoal = New System.Windows.Forms.Label()
        Me.lblTurnSpeed = New System.Windows.Forms.Label()
        Me.PathGoalBox = New System.Windows.Forms.TextBox()
        Me.TxtBehaviorBalance = New System.Windows.Forms.TextBox()
        Me.lblBehaviorBalance = New System.Windows.Forms.Label()
        Me.lblBehaviorMode = New System.Windows.Forms.Label()
        Me.cboBehaviorMode = New System.Windows.Forms.ComboBox()
        Me.pnlLive.SuspendLayout()
        Me.pnlPlayback.SuspendLayout()
        Me.grpSpawnConfig.SuspendLayout()
        CType(Me.numAgentNumber, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpAgentType.SuspendLayout()
        Me.grpSlamConfig.SuspendLayout()
        Me.grpDriver.SuspendLayout()
        Me.pnlMultiView.SuspendLayout()
        Me.grpSkinDet.SuspendLayout()
        Me.grpBehaviorConfig.SuspendLayout()
        Me.SuspendLayout()
        '
        'cboModel
        '
        Me.cboModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboModel.FormattingEnabled = True
        Me.cboModel.Location = New System.Drawing.Point(129, 19)
        Me.cboModel.Name = "cboModel"
        Me.cboModel.Size = New System.Drawing.Size(135, 21)
        Me.cboModel.TabIndex = 1
        '
        'lblRobotModel
        '
        Me.lblRobotModel.AutoSize = True
        Me.lblRobotModel.Location = New System.Drawing.Point(12, 22)
        Me.lblRobotModel.Name = "lblRobotModel"
        Me.lblRobotModel.Size = New System.Drawing.Size(71, 13)
        Me.lblRobotModel.TabIndex = 0
        Me.lblRobotModel.Text = "Robot Model:"
        '
        'lblStartLocation
        '
        Me.lblStartLocation.AutoSize = True
        Me.lblStartLocation.Location = New System.Drawing.Point(12, 72)
        Me.lblStartLocation.Name = "lblStartLocation"
        Me.lblStartLocation.Size = New System.Drawing.Size(76, 13)
        Me.lblStartLocation.TabIndex = 4
        Me.lblStartLocation.Text = "Start Location:"
        '
        'lblStartRotation
        '
        Me.lblStartRotation.AutoSize = True
        Me.lblStartRotation.Location = New System.Drawing.Point(12, 97)
        Me.lblStartRotation.Name = "lblStartRotation"
        Me.lblStartRotation.Size = New System.Drawing.Size(75, 13)
        Me.lblStartRotation.TabIndex = 7
        Me.lblStartRotation.Text = "Start Rotation:"
        '
        'lblMeter1
        '
        Me.lblMeter1.AutoSize = True
        Me.lblMeter1.Location = New System.Drawing.Point(270, 72)
        Me.lblMeter1.Name = "lblMeter1"
        Me.lblMeter1.Size = New System.Drawing.Size(15, 13)
        Me.lblMeter1.TabIndex = 6
        Me.lblMeter1.Text = "m"
        '
        'lblRadians
        '
        Me.lblRadians.AutoSize = True
        Me.lblRadians.Location = New System.Drawing.Point(270, 96)
        Me.lblRadians.Name = "lblRadians"
        Me.lblRadians.Size = New System.Drawing.Size(22, 13)
        Me.lblRadians.TabIndex = 9
        Me.lblRadians.Text = "rad"
        '
        'txtLocation
        '
        Me.txtLocation.Location = New System.Drawing.Point(129, 69)
        Me.txtLocation.Name = "txtLocation"
        Me.txtLocation.Size = New System.Drawing.Size(135, 21)
        Me.txtLocation.TabIndex = 5
        '
        'txtRotation
        '
        Me.txtRotation.Location = New System.Drawing.Point(129, 94)
        Me.txtRotation.Name = "txtRotation"
        Me.txtRotation.Size = New System.Drawing.Size(135, 21)
        Me.txtRotation.TabIndex = 8
        '
        'optLive
        '
        Me.optLive.AutoSize = True
        Me.optLive.Location = New System.Drawing.Point(16, 111)
        Me.optLive.Name = "optLive"
        Me.optLive.Size = New System.Drawing.Size(178, 17)
        Me.optLive.TabIndex = 2
        Me.optLive.Text = "Live Mode - Connect to UsarSim"
        Me.optLive.UseVisualStyleBackColor = True
        '
        'optPlayback
        '
        Me.optPlayback.AutoSize = True
        Me.optPlayback.Checked = True
        Me.optPlayback.Location = New System.Drawing.Point(12, 22)
        Me.optPlayback.Name = "optPlayback"
        Me.optPlayback.Size = New System.Drawing.Size(237, 17)
        Me.optPlayback.TabIndex = 0
        Me.optPlayback.TabStop = True
        Me.optPlayback.Text = "Log Playback Mode - Run offline from Logfile"
        Me.optPlayback.UseVisualStyleBackColor = True
        '
        'pnlLive
        '
        Me.pnlLive.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlLive.Controls.Add(Me.chkUseImageServer)
        Me.pnlLive.Controls.Add(Me.chkUseLogger)
        Me.pnlLive.Enabled = False
        Me.pnlLive.Location = New System.Drawing.Point(30, 134)
        Me.pnlLive.Name = "pnlLive"
        Me.pnlLive.Size = New System.Drawing.Size(378, 73)
        Me.pnlLive.TabIndex = 3
        '
        'chkUseImageServer
        '
        Me.chkUseImageServer.AutoSize = True
        Me.chkUseImageServer.Location = New System.Drawing.Point(9, 31)
        Me.chkUseImageServer.Name = "chkUseImageServer"
        Me.chkUseImageServer.Size = New System.Drawing.Size(111, 17)
        Me.chkUseImageServer.TabIndex = 1
        Me.chkUseImageServer.Text = "Use Image Server"
        Me.chkUseImageServer.UseVisualStyleBackColor = True
        '
        'chkUseLogger
        '
        Me.chkUseLogger.AutoSize = True
        Me.chkUseLogger.Checked = True
        Me.chkUseLogger.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkUseLogger.Location = New System.Drawing.Point(9, 8)
        Me.chkUseLogger.Name = "chkUseLogger"
        Me.chkUseLogger.Size = New System.Drawing.Size(91, 17)
        Me.chkUseLogger.TabIndex = 0
        Me.chkUseLogger.Text = "Create Logfile"
        Me.chkUseLogger.UseVisualStyleBackColor = True
        '
        'pnlPlayback
        '
        Me.pnlPlayback.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlPlayback.Controls.Add(Me.lnkLogfile)
        Me.pnlPlayback.Controls.Add(Me.lblLogfileFormat)
        Me.pnlPlayback.Controls.Add(Me.cboLogfileFormat)
        Me.pnlPlayback.Location = New System.Drawing.Point(30, 42)
        Me.pnlPlayback.Name = "pnlPlayback"
        Me.pnlPlayback.Size = New System.Drawing.Size(372, 63)
        Me.pnlPlayback.TabIndex = 1
        '
        'lnkLogfile
        '
        Me.lnkLogfile.Location = New System.Drawing.Point(9, 3)
        Me.lnkLogfile.Margin = New System.Windows.Forms.Padding(3)
        Me.lnkLogfile.Name = "lnkLogfile"
        Me.lnkLogfile.Size = New System.Drawing.Size(360, 31)
        Me.lnkLogfile.TabIndex = 0
        Me.lnkLogfile.TabStop = True
        Me.lnkLogfile.Text = "<Click to Select Logfile ...>"
        '
        'lblLogfileFormat
        '
        Me.lblLogfileFormat.AutoSize = True
        Me.lblLogfileFormat.Location = New System.Drawing.Point(6, 42)
        Me.lblLogfileFormat.Name = "lblLogfileFormat"
        Me.lblLogfileFormat.Size = New System.Drawing.Size(76, 13)
        Me.lblLogfileFormat.TabIndex = 1
        Me.lblLogfileFormat.Text = "Logfile Format:"
        '
        'cboLogfileFormat
        '
        Me.cboLogfileFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboLogfileFormat.FormattingEnabled = True
        Me.cboLogfileFormat.Location = New System.Drawing.Point(97, 39)
        Me.cboLogfileFormat.Name = "cboLogfileFormat"
        Me.cboLogfileFormat.Size = New System.Drawing.Size(121, 21)
        Me.cboLogfileFormat.TabIndex = 2
        '
        'optUsarBase
        '
        Me.optUsarBase.AutoSize = True
        Me.optUsarBase.Location = New System.Drawing.Point(12, 114)
        Me.optUsarBase.Name = "optUsarBase"
        Me.optUsarBase.Size = New System.Drawing.Size(74, 17)
        Me.optUsarBase.TabIndex = 4
        Me.optUsarBase.TabStop = True
        Me.optUsarBase.Text = "Usar Base"
        Me.optUsarBase.UseVisualStyleBackColor = True
        '
        'optUsarFollow
        '
        Me.optUsarFollow.AutoSize = True
        Me.optUsarFollow.Location = New System.Drawing.Point(12, 91)
        Me.optUsarFollow.Name = "optUsarFollow"
        Me.optUsarFollow.Size = New System.Drawing.Size(80, 17)
        Me.optUsarFollow.TabIndex = 3
        Me.optUsarFollow.TabStop = True
        Me.optUsarFollow.Text = "Usar Follow"
        Me.optUsarFollow.UseVisualStyleBackColor = True
        '
        'optUsarSlam
        '
        Me.optUsarSlam.AutoSize = True
        Me.optUsarSlam.Checked = True
        Me.optUsarSlam.Location = New System.Drawing.Point(12, 45)
        Me.optUsarSlam.Name = "optUsarSlam"
        Me.optUsarSlam.Size = New System.Drawing.Size(79, 17)
        Me.optUsarSlam.TabIndex = 1
        Me.optUsarSlam.TabStop = True
        Me.optUsarSlam.Text = "Usar SLAM"
        Me.optUsarSlam.UseVisualStyleBackColor = True
        '
        'cboSeedMode
        '
        Me.cboSeedMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboSeedMode.FormattingEnabled = True
        Me.cboSeedMode.Location = New System.Drawing.Point(129, 69)
        Me.cboSeedMode.Name = "cboSeedMode"
        Me.cboSeedMode.Size = New System.Drawing.Size(135, 21)
        Me.cboSeedMode.TabIndex = 5
        '
        'cboMappingMode
        '
        Me.cboMappingMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboMappingMode.FormattingEnabled = True
        Me.cboMappingMode.Location = New System.Drawing.Point(129, 19)
        Me.cboMappingMode.Name = "cboMappingMode"
        Me.cboMappingMode.Size = New System.Drawing.Size(135, 21)
        Me.cboMappingMode.TabIndex = 1
        '
        'cboScanMatcher
        '
        Me.cboScanMatcher.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboScanMatcher.FormattingEnabled = True
        Me.cboScanMatcher.Location = New System.Drawing.Point(129, 44)
        Me.cboScanMatcher.Name = "cboScanMatcher"
        Me.cboScanMatcher.Size = New System.Drawing.Size(135, 21)
        Me.cboScanMatcher.TabIndex = 3
        '
        'lblSeedMode
        '
        Me.lblSeedMode.AutoSize = True
        Me.lblSeedMode.Location = New System.Drawing.Point(12, 70)
        Me.lblSeedMode.Name = "lblSeedMode"
        Me.lblSeedMode.Size = New System.Drawing.Size(92, 13)
        Me.lblSeedMode.TabIndex = 4
        Me.lblSeedMode.Text = "Pose Seed Mode:"
        '
        'lblScanMatcher
        '
        Me.lblScanMatcher.AutoSize = True
        Me.lblScanMatcher.Location = New System.Drawing.Point(12, 47)
        Me.lblScanMatcher.Name = "lblScanMatcher"
        Me.lblScanMatcher.Size = New System.Drawing.Size(77, 13)
        Me.lblScanMatcher.TabIndex = 2
        Me.lblScanMatcher.Text = "Scan Matcher:"
        '
        'lblMappingMode
        '
        Me.lblMappingMode.AutoSize = True
        Me.lblMappingMode.Location = New System.Drawing.Point(12, 22)
        Me.lblMappingMode.Name = "lblMappingMode"
        Me.lblMappingMode.Size = New System.Drawing.Size(96, 13)
        Me.lblMappingMode.TabIndex = 0
        Me.lblMappingMode.Text = "Localization Mode:"
        '
        'grpSpawnConfig
        '
        Me.grpSpawnConfig.Controls.Add(Me.cboStartPoseName)
        Me.grpSpawnConfig.Controls.Add(Me.numAgentNumber)
        Me.grpSpawnConfig.Controls.Add(Me.lblAgentNumber)
        Me.grpSpawnConfig.Controls.Add(Me.chkSpawnFromCommander)
        Me.grpSpawnConfig.Controls.Add(Me.lblRobotModel)
        Me.grpSpawnConfig.Controls.Add(Me.cboModel)
        Me.grpSpawnConfig.Controls.Add(Me.lblStartLocation)
        Me.grpSpawnConfig.Controls.Add(Me.lblMeter1)
        Me.grpSpawnConfig.Controls.Add(Me.lblRadians)
        Me.grpSpawnConfig.Controls.Add(Me.lblStartRotation)
        Me.grpSpawnConfig.Controls.Add(Me.txtLocation)
        Me.grpSpawnConfig.Controls.Add(Me.txtRotation)
        Me.grpSpawnConfig.Location = New System.Drawing.Point(2, 74)
        Me.grpSpawnConfig.Name = "grpSpawnConfig"
        Me.grpSpawnConfig.Size = New System.Drawing.Size(414, 125)
        Me.grpSpawnConfig.TabIndex = 0
        Me.grpSpawnConfig.TabStop = False
        Me.grpSpawnConfig.Text = "Spawn Configuration"
        '
        'cboStartPoseName
        '
        Me.cboStartPoseName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboStartPoseName.FormattingEnabled = True
        Me.cboStartPoseName.Location = New System.Drawing.Point(298, 68)
        Me.cboStartPoseName.Name = "cboStartPoseName"
        Me.cboStartPoseName.Size = New System.Drawing.Size(110, 21)
        Me.cboStartPoseName.TabIndex = 11
        '
        'numAgentNumber
        '
        Me.numAgentNumber.Location = New System.Drawing.Point(129, 44)
        Me.numAgentNumber.Maximum = New Decimal(New Integer() {24, 0, 0, 0})
        Me.numAgentNumber.Name = "numAgentNumber"
        Me.numAgentNumber.Size = New System.Drawing.Size(135, 20)
        Me.numAgentNumber.TabIndex = 3
        '
        'lblAgentNumber
        '
        Me.lblAgentNumber.AutoSize = True
        Me.lblAgentNumber.Location = New System.Drawing.Point(12, 47)
        Me.lblAgentNumber.Name = "lblAgentNumber"
        Me.lblAgentNumber.Size = New System.Drawing.Size(79, 13)
        Me.lblAgentNumber.TabIndex = 2
        Me.lblAgentNumber.Text = "Robot Number:"
        '
        'chkSpawnFromCommander
        '
        Me.chkSpawnFromCommander.BackColor = System.Drawing.Color.Red
        Me.chkSpawnFromCommander.CheckAlign = System.Drawing.ContentAlignment.TopLeft
        Me.chkSpawnFromCommander.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkSpawnFromCommander.ForeColor = System.Drawing.Color.White
        Me.chkSpawnFromCommander.Location = New System.Drawing.Point(298, 19)
        Me.chkSpawnFromCommander.Name = "chkSpawnFromCommander"
        Me.chkSpawnFromCommander.Size = New System.Drawing.Size(110, 45)
        Me.chkSpawnFromCommander.TabIndex = 10
        Me.chkSpawnFromCommander.Text = "Spawn from Commander"
        Me.chkSpawnFromCommander.TextAlign = System.Drawing.ContentAlignment.TopLeft
        Me.chkSpawnFromCommander.UseVisualStyleBackColor = False
        '
        'grpAgentType
        '
        Me.grpAgentType.Controls.Add(Me.optUsarSkinDet)
        Me.grpAgentType.Controls.Add(Me.optUsarOperator)
        Me.grpAgentType.Controls.Add(Me.optUsarBase)
        Me.grpAgentType.Controls.Add(Me.optUsarSlam)
        Me.grpAgentType.Controls.Add(Me.optUsarFollow)
        Me.grpAgentType.Location = New System.Drawing.Point(311, 205)
        Me.grpAgentType.Name = "grpAgentType"
        Me.grpAgentType.Size = New System.Drawing.Size(105, 322)
        Me.grpAgentType.TabIndex = 1
        Me.grpAgentType.TabStop = False
        Me.grpAgentType.Text = "Agent Type"
        '
        'optUsarSkinDet
        '
        Me.optUsarSkinDet.AutoSize = True
        Me.optUsarSkinDet.Location = New System.Drawing.Point(12, 68)
        Me.optUsarSkinDet.Name = "optUsarSkinDet"
        Me.optUsarSkinDet.Size = New System.Drawing.Size(71, 17)
        Me.optUsarSkinDet.TabIndex = 2
        Me.optUsarSkinDet.TabStop = True
        Me.optUsarSkinDet.Text = "Usar Skin"
        Me.optUsarSkinDet.UseVisualStyleBackColor = True
        '
        'optUsarOperator
        '
        Me.optUsarOperator.AutoSize = True
        Me.optUsarOperator.Location = New System.Drawing.Point(12, 22)
        Me.optUsarOperator.Name = "optUsarOperator"
        Me.optUsarOperator.Size = New System.Drawing.Size(91, 17)
        Me.optUsarOperator.TabIndex = 0
        Me.optUsarOperator.TabStop = True
        Me.optUsarOperator.Text = "Usar Operator"
        Me.optUsarOperator.UseVisualStyleBackColor = True
        '
        'grpSlamConfig
        '
        Me.grpSlamConfig.Controls.Add(Me.txtNoiseSigma)
        Me.grpSlamConfig.Controls.Add(Me.chkNoise)
        Me.grpSlamConfig.Controls.Add(Me.cboScanMatcher)
        Me.grpSlamConfig.Controls.Add(Me.lblMappingMode)
        Me.grpSlamConfig.Controls.Add(Me.lblSeedMode)
        Me.grpSlamConfig.Controls.Add(Me.cboMappingMode)
        Me.grpSlamConfig.Controls.Add(Me.lblMeter2)
        Me.grpSlamConfig.Controls.Add(Me.cboSeedMode)
        Me.grpSlamConfig.Controls.Add(Me.lblScanMatcher)
        Me.grpSlamConfig.Location = New System.Drawing.Point(3, 296)
        Me.grpSlamConfig.Name = "grpSlamConfig"
        Me.grpSlamConfig.Size = New System.Drawing.Size(302, 125)
        Me.grpSlamConfig.TabIndex = 2
        Me.grpSlamConfig.TabStop = False
        Me.grpSlamConfig.Text = "Slam Configuration"
        '
        'txtNoiseSigma
        '
        Me.txtNoiseSigma.Enabled = False
        Me.txtNoiseSigma.Location = New System.Drawing.Point(218, 94)
        Me.txtNoiseSigma.Name = "txtNoiseSigma"
        Me.txtNoiseSigma.Size = New System.Drawing.Size(46, 20)
        Me.txtNoiseSigma.TabIndex = 7
        '
        'chkNoise
        '
        Me.chkNoise.AutoSize = True
        Me.chkNoise.Location = New System.Drawing.Point(15, 96)
        Me.chkNoise.Name = "chkNoise"
        Me.chkNoise.Size = New System.Drawing.Size(180, 17)
        Me.chkNoise.TabIndex = 6
        Me.chkNoise.Text = "Add Zero-Mean Gaussian Noise:"
        Me.chkNoise.UseVisualStyleBackColor = True
        '
        'lblMeter2
        '
        Me.lblMeter2.AutoSize = True
        Me.lblMeter2.Location = New System.Drawing.Point(270, 97)
        Me.lblMeter2.Name = "lblMeter2"
        Me.lblMeter2.Size = New System.Drawing.Size(15, 13)
        Me.lblMeter2.TabIndex = 8
        Me.lblMeter2.Text = "m"
        '
        'grpDriver
        '
        Me.grpDriver.Controls.Add(Me.pnlMultiView)
        Me.grpDriver.Controls.Add(Me.optPlayback)
        Me.grpDriver.Controls.Add(Me.pnlPlayback)
        Me.grpDriver.Controls.Add(Me.optLive)
        Me.grpDriver.Controls.Add(Me.pnlLive)
        Me.grpDriver.Location = New System.Drawing.Point(3, 533)
        Me.grpDriver.Name = "grpDriver"
        Me.grpDriver.Size = New System.Drawing.Size(414, 213)
        Me.grpDriver.TabIndex = 4
        Me.grpDriver.TabStop = False
        Me.grpDriver.Text = "Driver Type"
        '
        'pnlMultiView
        '
        Me.pnlMultiView.Controls.Add(Me.chkUseMultiView)
        Me.pnlMultiView.Controls.Add(Me.lblMultiViewPanels)
        Me.pnlMultiView.Controls.Add(Me.txtMultiViewPanels)
        Me.pnlMultiView.Enabled = False
        Me.pnlMultiView.Location = New System.Drawing.Point(158, 134)
        Me.pnlMultiView.Name = "pnlMultiView"
        Me.pnlMultiView.Size = New System.Drawing.Size(241, 58)
        Me.pnlMultiView.TabIndex = 8
        '
        'chkUseMultiView
        '
        Me.chkUseMultiView.AutoSize = True
        Me.chkUseMultiView.Location = New System.Drawing.Point(3, 8)
        Me.chkUseMultiView.Name = "chkUseMultiView"
        Me.chkUseMultiView.Size = New System.Drawing.Size(93, 17)
        Me.chkUseMultiView.TabIndex = 1
        Me.chkUseMultiView.Text = "Use MultiView"
        Me.chkUseMultiView.UseVisualStyleBackColor = True
        '
        'lblMultiViewPanels
        '
        Me.lblMultiViewPanels.AutoSize = True
        Me.lblMultiViewPanels.Enabled = False
        Me.lblMultiViewPanels.Location = New System.Drawing.Point(29, 32)
        Me.lblMultiViewPanels.Name = "lblMultiViewPanels"
        Me.lblMultiViewPanels.Size = New System.Drawing.Size(42, 13)
        Me.lblMultiViewPanels.TabIndex = 7
        Me.lblMultiViewPanels.Text = "Panels:"
        '
        'txtMultiViewPanels
        '
        Me.txtMultiViewPanels.Enabled = False
        Me.txtMultiViewPanels.Location = New System.Drawing.Point(99, 29)
        Me.txtMultiViewPanels.Name = "txtMultiViewPanels"
        Me.txtMultiViewPanels.Size = New System.Drawing.Size(135, 20)
        Me.txtMultiViewPanels.TabIndex = 6
        Me.txtMultiViewPanels.WordWrap = False
        '
        'grpSkinDet
        '
        Me.grpSkinDet.Controls.Add(Me.txtDetTheta)
        Me.grpSkinDet.Controls.Add(Me.cboTeachMode)
        Me.grpSkinDet.Controls.Add(Me.lblSkinDetMode)
        Me.grpSkinDet.Controls.Add(Me.lblDetTheta)
        Me.grpSkinDet.Controls.Add(Me.cboSkinDetMode)
        Me.grpSkinDet.Controls.Add(Me.lblTeachMode)
        Me.grpSkinDet.Enabled = False
        Me.grpSkinDet.Location = New System.Drawing.Point(3, 427)
        Me.grpSkinDet.Name = "grpSkinDet"
        Me.grpSkinDet.Size = New System.Drawing.Size(302, 100)
        Me.grpSkinDet.TabIndex = 3
        Me.grpSkinDet.TabStop = False
        Me.grpSkinDet.Text = "Skindetector Configuration"
        '
        'txtDetTheta
        '
        Me.txtDetTheta.Location = New System.Drawing.Point(129, 68)
        Me.txtDetTheta.Name = "txtDetTheta"
        Me.txtDetTheta.Size = New System.Drawing.Size(135, 20)
        Me.txtDetTheta.TabIndex = 5
        Me.txtDetTheta.WordWrap = False
        '
        'cboTeachMode
        '
        Me.cboTeachMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboTeachMode.FormattingEnabled = True
        Me.cboTeachMode.Location = New System.Drawing.Point(129, 44)
        Me.cboTeachMode.Name = "cboTeachMode"
        Me.cboTeachMode.Size = New System.Drawing.Size(135, 21)
        Me.cboTeachMode.TabIndex = 3
        '
        'lblSkinDetMode
        '
        Me.lblSkinDetMode.AutoSize = True
        Me.lblSkinDetMode.Location = New System.Drawing.Point(12, 22)
        Me.lblSkinDetMode.Name = "lblSkinDetMode"
        Me.lblSkinDetMode.Size = New System.Drawing.Size(100, 13)
        Me.lblSkinDetMode.TabIndex = 0
        Me.lblSkinDetMode.Text = "Skindetector Mode:"
        '
        'lblDetTheta
        '
        Me.lblDetTheta.AutoSize = True
        Me.lblDetTheta.Location = New System.Drawing.Point(12, 71)
        Me.lblDetTheta.Name = "lblDetTheta"
        Me.lblDetTheta.Size = New System.Drawing.Size(82, 13)
        Me.lblDetTheta.TabIndex = 4
        Me.lblDetTheta.Text = "Detector Theta:"
        '
        'cboSkinDetMode
        '
        Me.cboSkinDetMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboSkinDetMode.FormattingEnabled = True
        Me.cboSkinDetMode.Location = New System.Drawing.Point(129, 19)
        Me.cboSkinDetMode.Name = "cboSkinDetMode"
        Me.cboSkinDetMode.Size = New System.Drawing.Size(135, 21)
        Me.cboSkinDetMode.TabIndex = 1
        '
        'lblTeachMode
        '
        Me.lblTeachMode.AutoSize = True
        Me.lblTeachMode.Location = New System.Drawing.Point(12, 47)
        Me.lblTeachMode.Name = "lblTeachMode"
        Me.lblTeachMode.Size = New System.Drawing.Size(80, 13)
        Me.lblTeachMode.TabIndex = 2
        Me.lblTeachMode.Text = "Teacher Mode:"
        '
        'grpBehaviorConfig
        '
        Me.grpBehaviorConfig.Controls.Add(Me.lblPathGoal)
        Me.grpBehaviorConfig.Controls.Add(Me.lblTurnSpeed)
        Me.grpBehaviorConfig.Controls.Add(Me.PathGoalBox)
        Me.grpBehaviorConfig.Controls.Add(Me.TxtBehaviorBalance)
        Me.grpBehaviorConfig.Controls.Add(Me.lblBehaviorBalance)
        Me.grpBehaviorConfig.Controls.Add(Me.lblBehaviorMode)
        Me.grpBehaviorConfig.Controls.Add(Me.cboBehaviorMode)
        Me.grpBehaviorConfig.Location = New System.Drawing.Point(4, 205)
        Me.grpBehaviorConfig.Name = "grpBehaviorConfig"
        Me.grpBehaviorConfig.Size = New System.Drawing.Size(302, 85)
        Me.grpBehaviorConfig.TabIndex = 1000
        Me.grpBehaviorConfig.TabStop = False
        Me.grpBehaviorConfig.Text = "Behavior Configuration"
        '
        'lblPathGoal
        '
        Me.lblPathGoal.AutoSize = True
        Me.lblPathGoal.Location = New System.Drawing.Point(10, 70)
        Me.lblPathGoal.Name = "lblPathGoal"
        Me.lblPathGoal.Size = New System.Drawing.Size(57, 13)
        Me.lblPathGoal.TabIndex = 9
        Me.lblPathGoal.Text = "Path Goal:"
        '
        'lblTurnSpeed
        '
        Me.lblTurnSpeed.AutoSize = True
        Me.lblTurnSpeed.Location = New System.Drawing.Point(268, 45)
        Me.lblTurnSpeed.Name = "lblTurnSpeed"
        Me.lblTurnSpeed.Size = New System.Drawing.Size(32, 13)
        Me.lblTurnSpeed.TabIndex = 10
        Me.lblTurnSpeed.Text = "rad/s"
        '
        'PathGoalBox
        '
        Me.PathGoalBox.Location = New System.Drawing.Point(129, 65)
        Me.PathGoalBox.Name = "PathGoalBox"
        Me.PathGoalBox.Size = New System.Drawing.Size(135, 20)
        Me.PathGoalBox.TabIndex = 8
        Me.PathGoalBox.WordWrap = False
        '
        'TxtBehaviorBalance
        '
        Me.TxtBehaviorBalance.Location = New System.Drawing.Point(129, 42)
        Me.TxtBehaviorBalance.Name = "TxtBehaviorBalance"
        Me.TxtBehaviorBalance.Size = New System.Drawing.Size(135, 20)
        Me.TxtBehaviorBalance.TabIndex = 7
        Me.TxtBehaviorBalance.WordWrap = False
        '
        'lblBehaviorBalance
        '
        Me.lblBehaviorBalance.AutoSize = True
        Me.lblBehaviorBalance.Location = New System.Drawing.Point(10, 45)
        Me.lblBehaviorBalance.Name = "lblBehaviorBalance"
        Me.lblBehaviorBalance.Size = New System.Drawing.Size(100, 13)
        Me.lblBehaviorBalance.TabIndex = 6
        Me.lblBehaviorBalance.Text = "Cost/Gain Balance:"
        '
        'lblBehaviorMode
        '
        Me.lblBehaviorMode.AutoSize = True
        Me.lblBehaviorMode.Location = New System.Drawing.Point(12, 22)
        Me.lblBehaviorMode.Name = "lblBehaviorMode"
        Me.lblBehaviorMode.Size = New System.Drawing.Size(52, 13)
        Me.lblBehaviorMode.TabIndex = 0
        Me.lblBehaviorMode.Text = "Behavior:"
        '
        'cboBehaviorMode
        '
        Me.cboBehaviorMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboBehaviorMode.FormattingEnabled = True
        Me.cboBehaviorMode.Location = New System.Drawing.Point(129, 19)
        Me.cboBehaviorMode.Name = "cboBehaviorMode"
        Me.cboBehaviorMode.Size = New System.Drawing.Size(135, 21)
        Me.cboBehaviorMode.TabIndex = 1
        '
        'AgentConfigDialog
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(416, 752)
        Me.Controls.Add(Me.grpBehaviorConfig)
        Me.Controls.Add(Me.grpSlamConfig)
        Me.Controls.Add(Me.grpSkinDet)
        Me.Controls.Add(Me.grpAgentType)
        Me.Controls.Add(Me.grpSpawnConfig)
        Me.Controls.Add(Me.grpDriver)
        Me.Name = "AgentConfigDialog"
        Me.Text = "Agent Configuration"
        Me.pnlLive.ResumeLayout(False)
        Me.pnlLive.PerformLayout()
        Me.pnlPlayback.ResumeLayout(False)
        Me.pnlPlayback.PerformLayout()
        Me.grpSpawnConfig.ResumeLayout(False)
        Me.grpSpawnConfig.PerformLayout()
        CType(Me.numAgentNumber, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpAgentType.ResumeLayout(False)
        Me.grpAgentType.PerformLayout()
        Me.grpSlamConfig.ResumeLayout(False)
        Me.grpSlamConfig.PerformLayout()
        Me.grpDriver.ResumeLayout(False)
        Me.grpDriver.PerformLayout()
        Me.pnlMultiView.ResumeLayout(False)
        Me.pnlMultiView.PerformLayout()
        Me.grpSkinDet.ResumeLayout(False)
        Me.grpSkinDet.PerformLayout()
        Me.grpBehaviorConfig.ResumeLayout(False)
        Me.grpBehaviorConfig.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents cboModel As System.Windows.Forms.ComboBox
    Friend WithEvents lblRobotModel As System.Windows.Forms.Label
    Friend WithEvents lblStartLocation As System.Windows.Forms.Label
    Friend WithEvents lblStartRotation As System.Windows.Forms.Label
    Friend WithEvents lblMeter1 As System.Windows.Forms.Label
    Friend WithEvents lblRadians As System.Windows.Forms.Label
    Friend WithEvents txtLocation As System.Windows.Forms.ComboBox
    Friend WithEvents txtRotation As System.Windows.Forms.ComboBox
    Friend WithEvents optLive As System.Windows.Forms.RadioButton
    Friend WithEvents optPlayback As System.Windows.Forms.RadioButton
    Friend WithEvents pnlLive As System.Windows.Forms.Panel
    Friend WithEvents chkUseLogger As System.Windows.Forms.CheckBox
    Friend WithEvents pnlPlayback As System.Windows.Forms.Panel
    Friend WithEvents lnkLogfile As System.Windows.Forms.LinkLabel
    Friend WithEvents chkUseImageServer As System.Windows.Forms.CheckBox
    Friend WithEvents optUsarBase As System.Windows.Forms.RadioButton
    Friend WithEvents optUsarFollow As System.Windows.Forms.RadioButton
    Friend WithEvents optUsarSlam As System.Windows.Forms.RadioButton
    Friend WithEvents cboSeedMode As System.Windows.Forms.ComboBox
    Friend WithEvents cboMappingMode As System.Windows.Forms.ComboBox
    Friend WithEvents lblMappingMode As System.Windows.Forms.Label
    Friend WithEvents lblSeedMode As System.Windows.Forms.Label
    Friend WithEvents cboScanMatcher As System.Windows.Forms.ComboBox
    Friend WithEvents lblScanMatcher As System.Windows.Forms.Label
    Friend WithEvents cboLogfileFormat As System.Windows.Forms.ComboBox
    Friend WithEvents lblLogfileFormat As System.Windows.Forms.Label
    Friend WithEvents grpSpawnConfig As System.Windows.Forms.GroupBox
    Friend WithEvents grpAgentType As System.Windows.Forms.GroupBox
    Friend WithEvents grpSlamConfig As System.Windows.Forms.GroupBox
    Friend WithEvents grpDriver As System.Windows.Forms.GroupBox
    Friend WithEvents chkSpawnFromCommander As System.Windows.Forms.CheckBox
    Friend WithEvents chkUseMultiView As System.Windows.Forms.CheckBox
    Friend WithEvents lblAgentNumber As System.Windows.Forms.Label
    Friend WithEvents numAgentNumber As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblMultiViewPanels As System.Windows.Forms.Label
    Friend WithEvents txtMultiViewPanels As System.Windows.Forms.TextBox
    Friend WithEvents pnlMultiView As System.Windows.Forms.Panel
    Friend WithEvents grpSkinDet As System.Windows.Forms.GroupBox
    Friend WithEvents cboTeachMode As System.Windows.Forms.ComboBox
    Friend WithEvents lblSkinDetMode As System.Windows.Forms.Label
    Friend WithEvents lblDetTheta As System.Windows.Forms.Label
    Friend WithEvents cboSkinDetMode As System.Windows.Forms.ComboBox
    Friend WithEvents lblTeachMode As System.Windows.Forms.Label
    Friend WithEvents txtDetTheta As System.Windows.Forms.TextBox
    Friend WithEvents optUsarSkinDet As System.Windows.Forms.RadioButton
    Friend WithEvents optUsarOperator As System.Windows.Forms.RadioButton
    Friend WithEvents txtNoiseSigma As System.Windows.Forms.TextBox
    Friend WithEvents chkNoise As System.Windows.Forms.CheckBox
    Friend WithEvents lblMeter2 As System.Windows.Forms.Label
    Friend WithEvents grpBehaviorConfig As System.Windows.Forms.GroupBox
    Friend WithEvents lblBehaviorMode As System.Windows.Forms.Label
    Friend WithEvents cboBehaviorMode As System.Windows.Forms.ComboBox
    Friend WithEvents TxtBehaviorBalance As System.Windows.Forms.TextBox
    Friend WithEvents lblBehaviorBalance As System.Windows.Forms.Label
    Friend WithEvents lblTurnSpeed As System.Windows.Forms.Label
    Friend WithEvents lblPathGoal As System.Windows.Forms.Label
    Friend WithEvents PathGoalBox As System.Windows.Forms.TextBox
    Friend WithEvents cboStartPoseName As System.Windows.Forms.ComboBox

End Class
