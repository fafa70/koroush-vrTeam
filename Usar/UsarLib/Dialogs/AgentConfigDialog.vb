Imports UvARescue.Agent
Imports UvARescue.Slam
Imports UvARescue.Tools

Imports System.IO
Imports System.Windows.Forms

Public Class AgentConfigDialog
    Inherits ConfigDialog


    Protected Enum AllowedRobotModels

        ComStation
        Nao
        Nomad
        NomadWithSick
        Talon
        P2AT
        OmniP2AT
        P3AT
        Zerg

        ATRV
        ATRVJr
        ATRVJr3D
        kenaf
        Matilda
        Element
        Rugbot
        Hummer
        P2DX
        OmniP2DX
        Telemax

        AirRobot

        ERS

    End Enum

    Protected Enum SupportedScanMatchers

        WSM
        IDC
        MbICP
        ICP

        MultiICP

        QuadWSM
        QuadIDC
        QuadICP

    End Enum

    Protected Enum SupportedLogFileFormats

        UsarSim
        Player
        Carmen
        Cogniron
        Matlab
        NomadWithHokuyo
        Longwood

    End Enum

    Protected Enum SkinDetectorMode
        Detection
        Teaching
    End Enum

    Protected Enum BehaviorMode
        ' see BehaviorControl.BehaviorType
        TeleOperation
        ConservativeTeleOp
        AutonomousExploration
        DriveSquare
        DriveCircle

        ExploreTerrain
        FollowPath
        ' RIKNOTE: added for assignment 2 and 3
        FollowCorridorBehaviorType
        ObstacleAvoidanceBehaviorType
        FollowingBehavior
        DeploymentTest
    End Enum

    Protected Enum DmMappingAllowedStartPoses
        DmMapping 'Level name as reported by UsarSim
        PlayerStart
        robot1
        robot2
        Robot3
    End Enum

    Protected Enum DmRobotTeleopRobrechtAllowedStartPoses
        DmRobotTeleopRobrecht_v4 'Level name as reported by UsarSim
        Robot1
        Robot2
        Robot3
        Robot4
        BaseStation
        Target1
        Target2
        Target3
        Target4
    End Enum

 

    Protected Enum DmRoboCupTrainingAllowedStartPoses
        DmRoboCupTraining 'Level name as reported by UsarSim
        RobotStart1
        RobotStart2
        RobotStart3
        RobotStart4
        Position1
        Position2
        Position3
        Position4
    End Enum

    Protected Enum DmMaze6x6AllowedStartPoses
        Maze6x6 'Level name as reported by UsarSim
        Robot_0
        Robot_1
        Robot_2
        Robot_3
        Robot_4
        Robot_5
        Robot_6
        Robot_7
        Robot_8
        Robot_9
        Robot_10
        Robot_11
        Robot_12
        Robot_13
        Robot_14
        Robot_15
    End Enum

    Protected Enum Maze12x12Yellow5AllowedStartPoses
        Maze12x12Yellow5 'Level name as reported by UsarSim
        Robot_0
        Robot_1
        Robot_2
        Robot_3
        Robot_4
        Robot_6
        Robot_8
        Robot_10
        Robot_12
        Robot_14
        Robot_15
    End Enum

    'IrOpenMapping?
    'Robot2 16.9923,2.4600,1.4720 0.0000,0.0000,-4.7124 Robot1 16.9760,0.6680,1.4720 0.0000,0.0000,-4.7124 Robot4 4.4640,36.8680,1.4720 0.0000,0.0000,-1.5708 Robot3 4.4640,35.0760,1.4720 0.0000,0.0000,-1.5708 Goal1 1.5254,36.0546,1.4720 0.0000,0.0000,0.0000 Goal2 5.1351,36.0150,1.4720 0.0000,0.0000,0.0000 Goal3 7.7262,36.0150,1.4720 0.0000,0.0000,0.0000 Goal4 11.0689,36.0150,1.4720 0.0000,0.0000,0.0000 Goal5 14.2039,35.9864,1.4720 0.0000,0.0000,0.0000 Goal6 17.6652,35.9864,1.4720 0.0000,0.0000,0.0000 Goal7 20.5134,35.9864,1.4720 0.0000,0.0000,0.0000 Goal8 23.8265,35.9864,1.4720 0.0000,0.0000,0.0000 Goal16 1.5254,29.8640,1.4720 0.0000,0.0000,0.0000 Goal15 5.1351,29.7765,1.4720 0.0000,0.0000,0.0000 Goal14 7.7262,29.7765,1.4720 0.0000,0.0000,0.0000 Goal13 11.0689,29.7765,1.4720 0.0000,0.0000,0.0000 Goal12 14.2039,29.7478,1.4720 0.0000,0.0000,0.0000 Goal11 17.6652,29.7478,1.4720 0.0000,0.0000,0.0000 Goal10 20.5134,29.7478,1.4720 0.0000,0.0000,0.0000 Goal9 23.8265,29.7478,1.4720 0.0000,0.0000,0.0000 Goal17 1.5254,23.3975,1.4166 0.0000,0.0000,0.0000 Goal18 5.1351,23.3579,1.4720 0.0000,0.0000,0.0000 Goal19 7.8712,23.3434,1.2881 0.0000,0.0000,0.0000 Goal20 11.0689,23.3579,1.4720 0.0000,0.0000,0.0000 Goal21 14.2039,23.3293,1.4720 0.0000,0.0000,0.0000 Goal22 17.6652,23.3293,1.4720 0.0000,0.0000,0.0000 Goal23 20.5134,23.3293,1.4720 0.0000,0.0000,0.0000 Goal24 23.8265,23.3293,1.4720 0.0000,0.0000,0.0000 Goal32 1.5254,17.3988,1.3622 0.0000,0.0000,0.0000 Goal31 5.1351,17.2202,1.4217 0.0000,0.0000,0.0000 Goal30 7.7262,17.2143,1.3941 0.0000,0.0000,0.0000 Goal29 11.0689,17.3593,1.3437 0.0000,0.0000,0.0000 Goal28 14.2039,17.3306,1.4720 0.0000,0.0000,0.0000 Goal27 17.6652,17.3306,1.4720 0.0000,0.0000,0.0000 Goal26 20.5134,17.3306,1.4720 0.0000,0.0000,0.0000 Goal25 23.8265,17.3306,1.4720 0.0000,0.0000,0.0000 Goal33 1.5254,11.1603,1.4720 0.0000,0.0000,0.0000 Goal34 5.1351,11.1207,-0.0345 0.0000,0.0000,0.0000 Goal35 7.7262,11.1207,1.4720 0.0000,0.0000,0.0000 Goal36 11.0689,11.1207,1.4306 0.0000,0.0000,0.0000 Goal51 14.2039,11.0921,1.4720 0.0000,0.0000,0.0000 Goal52 17.6652,11.0921,1.4720 0.0000,0.0000,0.0000 Goal53 20.5134,11.0921,1.4720 0.0000,0.0000,0.0000 Goal55 23.8265,11.0921,1.4720 0.0000,0.0000,0.0000 Goal42 1.5254,4.9817,1.4720 0.0000,0.0000,0.0000 Goal41 5.1351,4.9421,1.3289 0.0000,0.0000,0.0000 Goal40 7.7262,4.9421,1.4295 0.0000,0.0000,0.0000 Goal39 11.0689,4.9421,1.4720 0.0000,0.0000,0.0000 Goal44 14.2039,4.9135,1.4720 0.0000,0.0000,0.0000 Goal45 17.6652,4.9135,1.4720 0.0000,0.0000,0.0000 Goal46 20.5134,4.9135,1.4720 0.0000,0.0000,0.0000 Goal47 23.8265,4.9135,1.4720 0.0000,0.0000,0.0000
    Protected Enum IrOpenMappingAllowedStartPoses
        IrOpenMapping
        Robot1
        Robot2
        Robot3
        Robot4
    End Enum

    Protected Enum RoboCup2011day1darkSmokeAllowedStartPoses
        RoboCup2011day1darkSmoke 'Level name as reported by UsarSim
        ComStation
        Robot1
        Robot2
    End Enum

    Protected Enum AllowedStartPoses
        UnknownMap
    End Enum

    Public Sub New(ByVal config As AgentConfig)
        MyBase.New(config)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        Dim tmpTeamConfig As TeamConfig = New TeamConfig(My.Settings.LastTeamConfigFile)
        tmpTeamConfig.LevelName = TeamConfigDialog.GetLevelName(tmpTeamConfig.UsarSimHost, tmpTeamConfig.UsarSimPort)
        tmpTeamConfig.StartPoses = TeamConfigDialog.GetStartPoses(tmpTeamConfig.UsarSimHost, tmpTeamConfig.UsarSimPort)
        tmpTeamConfig.Save(My.Settings.LastTeamConfigFile)

        'load enums into combos
        ' RIKNOTE: converts the BehaviorMode enums to strings in the AgentConfig pulldown menu
        Me.LoadEnumIntoComboBox(GetType(AllowedRobotModels), Me.cboModel)
        Me.LoadEnumIntoComboBox(GetType(BehaviorMode), Me.cboBehaviorMode)
        Me.LoadEnumIntoComboBox(GetType(SupportedLogFileFormats), Me.cboLogfileFormat)
        Me.LoadEnumIntoComboBox(GetType(MappingMode), Me.cboMappingMode)
        Me.LoadEnumIntoComboBox(GetType(SupportedScanMatchers), Me.cboScanMatcher)
        Me.LoadEnumIntoComboBox(GetType(PoseEstimationSeedMode), Me.cboSeedMode)
        Me.LoadEnumIntoComboBox(GetType(SkinDetectorMode), Me.cboSkinDetMode)
        Me.LoadEnumIntoComboBox(GetType(ImageAnalysis.SkinDetectorTeacherMode), Me.cboTeachMode)
        'VS: commented out below as not used.
        'Select Case tmpTeamConfig.LevelName
        '    Case "DM-RobotTeleopRobrecht_v4"
        '        Me.LoadEnumIntoComboBox(GetType(DmRobotTeleopRobrechtAllowedStartPoses), Me.cboStartPoseName)
        '    Case "Maze6x6"
        '        Me.LoadEnumIntoComboBox(GetType(DmMaze6x6AllowedStartPoses), Me.cboStartPoseName)
        '    Case "Maze12x12Yellow5"
        '        Me.LoadEnumIntoComboBox(GetType(Maze12x12Yellow5AllowedStartPoses), Me.cboStartPoseName)
        '    Case "IrOpenMapping"
        '        Me.LoadEnumIntoComboBox(GetType(IrOpenMappingAllowedStartPoses), Me.cboStartPoseName)
        '    Case "RoboCup2011day1darkSmoke"
        '        Me.LoadEnumIntoComboBox(GetType(RoboCup2011day1darkSmokeAllowedStartPoses), Me.cboStartPoseName)
        '    Case Else

        '        Me.LoadEnumIntoComboBox(GetType(AllowedStartPoses), Me.cboStartPoseName)

        'End Select

    End Sub

    Private Sub LoadEnumIntoComboBox(ByVal enumType As Type, ByVal combo As ComboBox)

        Dim names As Array = System.Enum.GetNames(enumType)
        Dim values As Array = System.Enum.GetValues(enumType)

        Dim table As New DataTable
        table.Columns.Add("Name", GetType(String))
        table.Columns.Add("Value", enumType)

        For i As Integer = 0 To names.Length - 1
            table.Rows.Add(names.GetValue(i), values.GetValue(i))
        Next

        With combo
            .DataSource = table
            .DisplayMember = "Name"
            .ValueMember = "Value"
        End With

    End Sub

    Private Sub ShowEnumValueInComboBox(ByVal enumType As Type, ByVal valueToSelect As String, ByVal combo As ComboBox)
        Dim values As Array = System.Enum.GetValues(enumType)
        For Each value As Object In values
            If value.ToString = valueToSelect Then
                combo.SelectedValue = value
                Exit For
            End If
        Next
    End Sub

    Public ReadOnly Property AgentConfig() As AgentConfig
        Get
            Return DirectCast(MyBase.Config, AgentConfig)
        End Get
    End Property


    Protected Overrides Sub OnOpened(ByVal filename As String)
        MyBase.OnOpened(filename)
        My.Settings.LastAgentConfigFile = filename
        My.Settings.Save()
    End Sub
    Protected Overrides Sub OnSaved(ByVal filename As String)
        MyBase.OnSaved(filename)
        My.Settings.LastAgentConfigFile = filename
        My.Settings.Save()
    End Sub


    Protected Overrides Sub SyncGuiWithConfig()
        MyBase.SyncGuiWithConfig()
        With Me.AgentConfig

            Me.Text = String.Format("Agent Configuration - [{0}]", .AgentName)

            Me.ShowEnumValueInComboBox(GetType(AllowedRobotModels), .RobotModel, Me.cboModel)

            Me.txtLocation.Text = .StartLocation
            Me.txtRotation.Text = .StartRotation
            'VS: Load the items into the dropdown lists
            Dim line As String
            Dim reader As StreamReader
            Try
                reader = _
                    New StreamReader("startposes.txt")
                Try
                    Do
                        line = reader.ReadLine
                        If (line.Substring(0, 5) = "Playe") Or (line.Substring(0, 5) = "Robot") _
                            Or (line.Substring(0, 5) = "ComSt") Then
                            cboStartPoseName.Items.Add(line)
                            line = reader.ReadLine
                            txtLocation.Items.Add(line)
                            line = reader.ReadLine
                            txtRotation.Items.Add(line)
                        End If
                    Loop Until reader.Peek = -1
                Catch

                Finally
                    reader.Close()
                End Try
            Catch
            Finally
            End Try


            Me.chkSpawnFromCommander.Checked = .SpawnFromCommander
            Me.numAgentNumber.Value = .AgentNumber

            If Not String.IsNullOrEmpty(.AgentType) Then
                Select Case .AgentType.ToLower
                    Case "operator"
                        Me.optUsarOperator.Checked = True
                    Case "slam"
                        Me.optUsarSlam.Checked = True
                    Case "follow"
                        Me.optUsarFollow.Checked = True
                    Case "skin"
                        Me.optUsarSkinDet.Checked = True
                    Case Else
                        Me.optUsarBase.Checked = True
                End Select
            End If

            'Me.optPlayback.Checked = .LogPlayback
            'Me.lnkLogfile.Text = .LogFile
            'Me.ShowEnumValueInComboBox(GetType(SupportedLogFileFormats), .LogFileFormat, Me.cboLogfileFormat)

            'Me.optLive.Checked = Not .LogPlayback
            'Me.chkUseLogger.Checked = .UseLogger
            Me.chkUseImageServer.Checked = .UseImageServer
            Me.chkUseMultiView.Checked = .UseMultiView
            Me.txtMultiViewPanels.Text = .MultiViewPanels

            Me.ShowEnumValueInComboBox(GetType(BehaviorMode), .BehaviorMode, Me.cboBehaviorMode)
            Me.TxtBehaviorBalance.Text = CStr(.BehaviorBalance)
            ' Me.PathGoalBox.Text = .PathPlanGoal
            Me.ShowEnumValueInComboBox(GetType(MappingMode), .MappingMode, Me.cboMappingMode)
            Me.ShowEnumValueInComboBox(GetType(SupportedScanMatchers), .ScanMatcher, Me.cboScanMatcher)
            Me.ShowEnumValueInComboBox(GetType(PoseEstimationSeedMode), .SeedMode, Me.cboSeedMode)
            Me.chkNoise.Checked = .UseNoise
            Me.txtNoiseSigma.Text = CStr(.NoiseSigma)

            Me.ShowEnumValueInComboBox(GetType(SkinDetectorMode), .SkinDetectorMode, Me.cboSkinDetMode)
            'Me.ShowEnumValueInComboBox(GetType(ImageAnalysis.SkinDetectorTeacherMode), .TeacherMode, Me.cboTeachMode)

            'Me.txtDetTheta.Text = .DetectorTheta
        End With
    End Sub

    Protected Overrides Sub SyncConfigWithGui()
        MyBase.SyncConfigWithGui()
        With Me.AgentConfig

            If Me.optUsarOperator.Checked Then
                .AgentType = "operator"
            ElseIf Me.optUsarSlam.Checked Then
                .AgentType = "slam"
            ElseIf Me.optUsarSkinDet.Checked Then
                .AgentType = "skin"
            ElseIf Me.optUsarFollow.Checked Then
                .AgentType = "follow"
            Else
                .AgentType = "base"
            End If

            .RobotModel = CType(Me.cboModel.SelectedValue, AllowedRobotModels).ToString
            .StartLocation = Me.txtLocation.Text
            .StartRotation = Me.txtRotation.Text
            .SpawnFromCommander = Me.chkSpawnFromCommander.Checked
            .AgentNumber = CInt(Me.numAgentNumber.Value)
            '.PathPlanGoal = Me.PathGoalBox.Text

            ' .LogPlayback = Me.optPlayback.Checked
            '.LogFile = Me.lnkLogfile.Text
            '.LogFileFormat = CType(Me.cboLogfileFormat.SelectedValue, SupportedLogFileFormats).ToString

            ' .UseLogger = Me.chkUseLogger.Checked
            .UseImageServer = Me.chkUseImageServer.Checked
            .UseMultiView = Me.chkUseMultiView.Checked
            .MultiViewPanels = Me.txtMultiViewPanels.Text

            .MappingMode = CType(Me.cboMappingMode.SelectedValue, MappingMode).ToString
            .BehaviorMode = CType(Me.cboBehaviorMode.SelectedValue, BehaviorMode).ToString
            If IsNumeric(Me.TxtBehaviorBalance.Text) Then
                .BehaviorBalance = Double.Parse(Me.TxtBehaviorBalance.Text)
            End If

            .ScanMatcher = CType(Me.cboScanMatcher.SelectedValue, SupportedScanMatchers).ToString
            '.SeedMode = CType(Me.cboSeedMode.SelectedValue, PoseEstimationSeedMode).ToString
            .UseNoise = Me.chkNoise.Checked
            If IsNumeric(Me.txtNoiseSigma.Text) Then
                .NoiseSigma = Double.Parse(Me.txtNoiseSigma.Text)
            End If

            .SkinDetectorMode = CType(Me.cboSkinDetMode.SelectedValue, SkinDetectorMode).ToString
            '.TeacherMode = CType(Me.cboTeachMode.SelectedValue, ImageAnalysis.SkinDetectorTeacherMode).ToString
            '.DetectorTheta = Me.txtDetTheta.Text
        End With
    End Sub


    Private Sub optUsarOperator_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles optUsarOperator.CheckedChanged
        If Me.optUsarOperator.Checked Then
            Me.chkSpawnFromCommander.Checked = True
            Me.cboBehaviorMode.Enabled = False
        End If
    End Sub


    Private Sub optUsarSlam_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles optUsarSlam.CheckedChanged
        Me.grpSlamConfig.Enabled = Me.optUsarSlam.Checked OrElse Me.optUsarSkinDet.Checked
        Me.cboBehaviorMode.Enabled = True
    End Sub

    Private Sub optUsarSkinDet_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optUsarSkinDet.CheckedChanged
        Me.grpSlamConfig.Enabled = Me.optUsarSlam.Checked OrElse Me.optUsarSkinDet.Checked
        Me.grpSkinDet.Enabled = Me.optUsarSkinDet.Checked
        If Me.txtDetTheta.Text = "" Then
            Me.txtDetTheta.Text = "50"
        End If
        Me.cboBehaviorMode.Enabled = True
    End Sub

    Private Sub optUsarFollow_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optUsarFollow.CheckedChanged
        Me.cboBehaviorMode.Enabled = True
    End Sub
    Private Sub optUsarBase_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optUsarBase.CheckedChanged
        Me.cboBehaviorMode.Enabled = True
    End Sub



    Private Sub optPlayback_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optPlayback.CheckedChanged
        Me.pnlPlayback.Enabled = Me.optPlayback.Checked
    End Sub

    Private Sub optLive_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optLive.CheckedChanged
        Me.pnlLive.Enabled = Me.optLive.Checked
    End Sub

    Private Sub lnkLogfile_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles lnkLogfile.LinkClicked
        Dim dlg As New System.Windows.Forms.OpenFileDialog
        Select Case dlg.ShowDialog
            Case Windows.Forms.DialogResult.OK
                Me.lnkLogfile.Text = dlg.FileName
        End Select
    End Sub
    Private Sub lnkLogfile_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkLogfile.TextChanged
        If String.IsNullOrEmpty(Me.lnkLogfile.Text) Then
            Me.lnkLogfile.Text = "<Click to Select ...>"
        End If
    End Sub

    Private Sub chkUseImageServer_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkUseImageServer.CheckedChanged
        Me.pnlMultiView.Enabled = Me.chkUseImageServer.Checked
    End Sub

    Private Sub chkUseMultiView_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkUseMultiView.CheckedChanged
        Me.lblMultiViewPanels.Enabled = Me.chkUseMultiView.Checked
        Me.txtMultiViewPanels.Enabled = Me.chkUseMultiView.Checked
    End Sub

    Private Sub chkNoise_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkNoise.CheckedChanged
        Me.txtNoiseSigma.Enabled = Me.chkNoise.Checked
    End Sub

    Private Sub Label1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblBehaviorBalance.Click

    End Sub

    Private Sub cboBehaviorMode_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboBehaviorMode.SelectedIndexChanged
        Me.TxtBehaviorBalance.Enabled = (Me.cboBehaviorMode.SelectedIndex = BehaviorMode.AutonomousExploration)
        Me.PathGoalBox.Enabled = (Me.cboBehaviorMode.SelectedIndex = BehaviorMode.FollowPath)
    End Sub

    Private Sub cboStartPoseNameSelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboStartPoseName.SelectedIndexChanged

        Try
            txtLocation.SelectedIndex = cboStartPoseName.SelectedIndex
            txtRotation.SelectedIndex = cboStartPoseName.SelectedIndex
        Catch
        Finally
        End Try

        'VS: Commented out code below as it wasn't used anyway
        'Dim values As Array = System.Enum.GetValues(GetType(AllowedStartPoses))

        'If Me.cboStartPoseName.SelectedIndex = 0 Then
        '    'Me.cboStartPoseName.Enabled = False
        '    'Return
        '    Console.WriteLine("No PoseName selected yet")
        '    Return

        'End If

        'Dim tmpTeamConfig As TeamConfig = New TeamConfig(My.Settings.LastTeamConfigFile)

        'If IsNothing(tmpTeamConfig.StartPoses) Then
        '    Console.WriteLine("No Start position loaded from map")
        '    Me.cboStartPoseName.SelectedIndex = 0
        '    Me.cboStartPoseName.Enabled = False

        '    Return
        'End If

        'Dim PoseName As String = cboStartPoseName.Text

        ''If Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.PlayerStart AndAlso Me.cboStartPoseName.Enabled = True Then
        ''    PoseName = cboStartPoseName.SelectedText
        ''ElseIf Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.robot1 Then
        ''    PoseName = AllowedStartPoses.robot1.ToString
        ''ElseIf Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.robot2 Then
        ''    PoseName = AllowedStartPoses.robot2.ToString
        ''ElseIf Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.Robot3 Then
        ''    PoseName = AllowedStartPoses.Robot3.ToString
        ''ElseIf Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.RobotStart1 Then
        ''    PoseName = AllowedStartPoses.RobotStart1.ToString
        ''ElseIf Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.RobotStart2 Then
        ''    PoseName = AllowedStartPoses.RobotStart2.ToString
        ''ElseIf Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.RobotStart3 Then
        ''    PoseName = AllowedStartPoses.RobotStart3.ToString
        ''ElseIf Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.RobotStart4 Then
        ''    PoseName = AllowedStartPoses.RobotStart4.ToString
        ''ElseIf Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.Position1 Then
        ''    PoseName = AllowedStartPoses.Position1.ToString
        ''ElseIf Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.Position2 Then
        ''    PoseName = AllowedStartPoses.Position2.ToString
        ''ElseIf Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.Position3 Then
        ''    PoseName = AllowedStartPoses.Position3.ToString
        ''ElseIf Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.Position4 Then
        ''    PoseName = AllowedStartPoses.Position4.ToString
        ''Else
        ''    Console.WriteLine("Other GETSTARTPOS to be done")
        ''    Me.cboStartPoseName.SelectedIndex = AllowedStartPoses.PlayerStart
        ''    Return
        ''End If

        'If tmpTeamConfig.StartPoses.Contains(PoseName) Then
        '    Console.WriteLine(String.Format("Change to {0}", PoseName))
        '    Dim parts As New Specialized.StringCollection
        '    Dim i As Integer = 0
        '    Dim j As Integer = tmpTeamConfig.StartPoses.IndexOf(PoseName)
        '    Dim part As String = Nothing

        '    i = tmpTeamConfig.StartPoses.IndexOf(" ", j)
        '    If i >= 0 Then
        '        j = tmpTeamConfig.StartPoses.IndexOf(" ", i + 1)
        '        If j >= 0 Then
        '            part = tmpTeamConfig.StartPoses.Substring(i + 1, j - i - 1)
        '            Me.txtLocation.Text = part
        '            parts.Add(part) 'first part is location

        '            i = tmpTeamConfig.StartPoses.IndexOf(" ", j)
        '            If i >= 0 Then
        '                j = tmpTeamConfig.StartPoses.IndexOf(" ", i + 1)
        '                If j >= 0 Then
        '                    part = tmpTeamConfig.StartPoses.Substring(i + 1, j - i - 1)
        '                    Me.txtRotation.Text = part
        '                    parts.Add(part) 'second part is orientation
        '                Else
        '                    part = tmpTeamConfig.StartPoses.Substring(i + 1)
        '                    If part.Contains(",") AndAlso part.Length > 5 Then 'no space at the end of the string
        '                        Me.txtRotation.Text = part
        '                        parts.Add(part) 'second part is orientation
        '                    Else
        '                        Console.WriteLine(String.Format("Could not get end of orientation of {0}", PoseName))
        '                        Me.cboStartPoseName.SelectedIndex = 0
        '                        Me.cboStartPoseName.Enabled = False
        '                        Return
        '                    End If
        '                End If
        '            Else
        '                Console.WriteLine(String.Format("Could not get start of orientation of {0}", PoseName))
        '                Me.cboStartPoseName.SelectedIndex = 0
        '                Me.cboStartPoseName.Enabled = False
        '                Return
        '            End If
        '        Else
        '            Console.WriteLine(String.Format("Could not get end of location of {0}", PoseName))
        '            Me.cboStartPoseName.SelectedIndex = 0
        '            Me.cboStartPoseName.Enabled = False
        '            Return
        '        End If
        '    Else
        '        Console.WriteLine(String.Format("Could not get start of location of {0}", PoseName))
        '        Me.cboStartPoseName.SelectedIndex = 0
        '        Me.cboStartPoseName.Enabled = False
        '        Return
        '    End If
        '    'Success 
        '    Me.cboStartPoseName.Enabled = True

        '    Me.SyncConfigWithGui() 'Done when saved / closed
        'Else
        '    Console.WriteLine(String.Format("Could not change to {0}", PoseName))
        '    Me.cboStartPoseName.SelectedIndex = 0
        '    Me.cboStartPoseName.Enabled = False

        'End If

    End Sub


    Private Sub cboMappingMode_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboMappingMode.SelectedIndexChanged

        Me.cboScanMatcher.Enabled = (Me.cboMappingMode.SelectedIndex = UvARescue.Slam.MappingMode.ScanMatching)
        If Me.cboScanMatcher.Enabled Then
            Me.lblSeedMode.Text = "Pose Seed Mode:"
        Else
            Me.lblSeedMode.Text = "Pose Sensor:"
        End If

        'If Not IsNothing(Me.AgentConfig) Then
        'Me.AgentConfig.MappingMode = CType(Me.cboMappingMode.SelectedValue, MappingMode).ToString
        'Me.cboScanMatcher.Enabled = (Me.AgentConfig.MappingMode = UvARescue.Slam.MappingMode.ScanMatching.ToString)
        'End If

    End Sub


    Private Sub txtLocation_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtLocation.SelectedIndexChanged

    End Sub

    Private Sub cboModel_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboModel.SelectedIndexChanged

    End Sub

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub txtMultiViewPanels_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtMultiViewPanels.TextChanged

    End Sub
End Class
