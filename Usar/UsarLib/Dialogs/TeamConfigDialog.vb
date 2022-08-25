Imports UvARescue.Agent
Imports UvARescue.Tools
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports System.Drawing

Public Class TeamConfigDialog
    Inherits ConfigDialog


    Public Sub New(ByVal config As TeamConfig, ByVal manifold As Manifold)
        MyBase.New(config)
        'Me._Manifold = manifold
        Me._teamconfig = config
        Me.Manifold1 = manifold

        'This call is required by the Windows Form Designer.
        InitializeComponent()

    End Sub

    Private _UsarOperator As UsarOperatorAgent
    Public ReadOnly Property UsarOperator() As UsarOperatorAgent
        Get
            Return Me._UsarOperator
        End Get
    End Property


    Private Manifold1 As Manifold
    Public ReadOnly Property Manifold() As Manifold
        Get
            Return Me.Manifold1
        End Get
    End Property


    Private currAgent() As String

    Private _teamconfig As TeamConfig
    Public ReadOnly Property TeamConfig() As TeamConfig
        Get
            Return DirectCast(MyBase.Config, TeamConfig)
        End Get
    End Property

   


    Public ReadOnly Property AgentConfig() As AgentConfig
        Get
            Return DirectCast(MyBase.Config, AgentConfig)
        End Get
    End Property



    Protected Overrides Sub OnOpened(ByVal filename As String)
        MyBase.OnOpened(filename)
        My.Settings.LastTeamConfigFile = filename
        My.Settings.Save()
    End Sub
    Protected Overrides Sub OnSaved(ByVal filename As String)
        MyBase.OnSaved(filename)
        My.Settings.LastTeamConfigFile = filename
        My.Settings.Save()
    End Sub

    'Public Property OperatorName() As String
    '   Get
    '      Return Me.txtOperatorName.Text
    ' End Get
    'Set(ByVal value As String)
    '   Me.txtOperatorName.Text = value
    'End Set
    'End Property

    'Public Property TeamMembers() As String
    '   Get
    '      Return Me.txtTeamMembers.Text
    ' End Get
    'Set(ByVal value As String)
    '   Me.txtTeamMembers.Text = value
    ' End Set
    'End Property

    Protected Overrides Sub SyncGuiWithConfig()
        MyBase.SyncGuiWithConfig()
        With Me.TeamConfig

            ' Me.txtLevelName.Text = .LevelName
            ' Me.txtStartPoses.Text = .StartPoses

            Me.txtLocalhost.Text = "127.0.0.1"
            Me.txtUsarSimHost.Text = "127.0.0.1"
            Me.txtUsarSimPort.Text = CStr(.UsarSimPort)
            Me.txtImageServerHost.Text = "127.0.0.1"
            Me.txtImageServerPort.Text = CStr(.ImageServerPort)
            Me.txtWirelessServerHost.Text = "127.0.0.1"
            Me.txtWirelessServerPort.Text = "50000"
            Me.chkboxBackup.Checked = .CreateBackups
            Me.txtBackupFrequency.Text = CStr(.BackupFrequency)
        End With
    End Sub

    Protected Overrides Sub SyncConfigWithGui()
        MyBase.SyncConfigWithGui()
        With Me.TeamConfig

            '.LevelName = Me.txtLevelName.Text
            '.StartPoses = Me.txtStartPoses.Text

            ' .OperatorName = Me.txtOperatorName.Text
            '.TeamMembers = Me.txtTeamMembers.Text

            .LocalHost = Me.txtLocalhost.Text

            .UsarSimHost = Me.txtUsarSimHost.Text
            If IsNumeric(Me.txtUsarSimPort.Text) Then
                .UsarSimPort = CInt(Double.Parse(Me.txtUsarSimPort.Text))
            End If

            .ImageServerHost = Me.txtImageServerHost.Text
            If IsNumeric(Me.txtImageServerPort.Text) Then
                .ImageServerPort = CInt(Double.Parse(Me.txtImageServerPort.Text))
            End If

            .WirelessServerHost = Me.txtWirelessServerHost.Text
            If IsNumeric(Me.txtWirelessServerPort.Text) Then
                .WirelessServerPort = CInt(Double.Parse(Me.txtWirelessServerPort.Text))
            End If

            .CreateBackups = Me.chkboxBackup.Checked
            If (IsNumeric(Me.txtBackupFrequency.Text)) Then
                .BackupFrequency = CInt(Double.Parse(Me.txtBackupFrequency.Text))
            End If

        End With
        Me.TeamConfig.LevelName = GetLevelName(Me.txtUsarSimHost.Text, Integer.Parse(Me.txtUsarSimPort.Text))
        Me.TeamConfig.StartPoses = GetStartPoses(Me.txtUsarSimHost.Text, Integer.Parse(Me.txtUsarSimPort.Text))
        Me.SyncGuiWithConfig()
    End Sub





    Public Shared Function GetStartPoses(ByVal host As String, ByVal port As Integer) As String

        Try
            Dim returnValue As String = Nothing
            Dim nmbStartPoses As Integer = -1
            Dim usarSimConnection As New TcpMessagingConnection




            usarSimConnection.Connect(host, port)
            'wait for the DeadMatch answer.

            Threading.Thread.Sleep(TimeSpan.FromSeconds(1))

            If Not usarSimConnection.IsConnected Then
                Console.WriteLine("Couldn't query UsarSim server at host '" + host + "' for StartPoses.")
                Return Nothing
            End If


            Dim msgs As Specialized.StringCollection = usarSimConnection.ReceiveMessages(4096)
            For Each msg As String In msgs
                'NFO {StartPoses 3} {BlueGoal -2.55,-0.00,-0.19 0.00,-0.03,-0.01 MiddleField 0.01,-0.00,-0.19 0.00,0.00,0.00 YellowGoal 2.57,-0.01,-0.19 0.00,0.00,3.13}
                If String.IsNullOrEmpty(msg) Then
                    Continue For
                End If

                Dim msgtype As MessageType = MessageType.Unknown
                Dim parts As New Specialized.StringCollection

                'extract msgtype first
                Dim i As Integer = 0
                Dim j As Integer = msg.IndexOf(" ")
                Dim part As String = Nothing
                If j > 0 Then
                    part = msg.Substring(i, j - i)
                End If
                If part.ToUpper = "NFO" Then
                    msgtype = MessageType.Info
                Else
                    Continue For
                End If

                i = msg.IndexOf("{", j)
                While i >= 0
                    j = msg.IndexOf("}", i)
                    If j >= 0 Then
                        part = msg.Substring(i + 1, j - i - 1)
                        parts.Add(part)

                        i = msg.IndexOf("{", j)

                    Else
                        'done
                        Exit While

                    End If
                    If part.StartsWith("Gametype") Then
                        Continue For
                    End If
                    If part.StartsWith("StartPoses") Then
                        nmbStartPoses = Integer.Parse(part.Substring(10)) 'nmbStartPoses not saved
                    Else
                        returnValue = part
                    End If
                    'Now SyncGuiWithConfig in AgentConfig
                End While


            Next

            ' Send the request

            Dim cmd As New StringBuilder()
            cmd.Append("GETSTARTPOSES")
            cmd.Append(Environment.NewLine) 'newline! very important!

            'simply use the standard interface for sending commands.
            usarSimConnection.Send(cmd.ToString)

            Threading.Thread.Sleep(TimeSpan.FromSeconds(1))

            msgs = usarSimConnection.ReceiveMessages(4096)
            For Each msg As String In msgs
                'NFO {StartPoses 3} {BlueGoal -2.55,-0.00,-0.19 0.00,-0.03,-0.01 MiddleField 0.01,-0.00,-0.19 0.00,0.00,0.00 YellowGoal 2.57,-0.01,-0.19 0.00,0.00,3.13}
                If String.IsNullOrEmpty(msg) Then
                    Continue For
                End If

                Dim msgtype As MessageType = MessageType.Unknown
                Dim parts As New Specialized.StringCollection

                'extract msgtype first
                Dim i As Integer = 0
                Dim j As Integer = msg.IndexOf(" ")
                Dim part As String = Nothing
                If j > 0 Then
                    part = msg.Substring(i, j - i)
                End If
                If part.ToUpper = "NFO" Then
                    msgtype = MessageType.Info
                Else
                    Continue For
                End If

                i = msg.IndexOf("{", j)
                While i >= 0
                    j = msg.IndexOf("}", i)
                    If j >= 0 Then
                        part = msg.Substring(i + 1, j - i - 1)
                        parts.Add(part)

                        i = msg.IndexOf("{", j)

                    Else
                        'done
                        Exit While

                    End If

                    If part.StartsWith("Gametype") Then
                        Continue For
                    End If

                    'For DM-Mapping_250: "PlayerStart 0.75,-7.28,-0.44 0.00,0.00,0.00 robot1 3.63,10.80,-0.18 0.00,0.00,-1.59 robot2 1.91,10.00,-0.18 0.00,0.00,0.00 Robot3 3.85,9.17,-0.18 0.00,0.00,1.66"	String

                    If part.StartsWith("StartPoses") Then
                        nmbStartPoses = Integer.Parse(part.Substring(10)) 'nmbStartPoses not saved
                    Else
                        returnValue = part
                    End If
                    'Now SyncGuiWithConfig in AgentConfig


                End While


            Next


            'AVI: search the parts for key StartPoses, and loop through all Poses

            'AVI: Make enumeration, which can be used as drop down for each robot.

            'VS: Split startposes into lines and save to a txt file for later use
            Dim returnValueLines As String() = returnValue.Split(CChar(" "))
            Dim writer As StreamWriter = _
            New StreamWriter("startposes.txt")
            Dim word As String
            For Each word In returnValueLines
                writer.WriteLine(word)
            Next
            writer.Close()


            usarSimConnection.Disconnect()
            Return returnValue

        Catch ex As Exception
            MsgBox("Error occurred during GetStartPoses" & vbNewLine & ex.Message, MsgBoxStyle.OkOnly, MsgBoxStyle.Exclamation)
            Return Nothing
        End Try
    End Function

    Public Shared Function GetLevelName(ByVal host As String, ByVal port As Integer) As String

        Try
            Dim returnValue As String = Nothing
            Dim usarSimConnection As New TcpMessagingConnection

            usarSimConnection.Connect(host, port)

            Threading.Thread.Sleep(TimeSpan.FromSeconds(1))

            If Not usarSimConnection.IsConnected Then
                Console.WriteLine("Couldn't query UsarSim server at host '" + host + "' for Level name.")
                Return Nothing
            End If


            Dim msgs As Specialized.StringCollection = usarSimConnection.ReceiveMessages(4096)
            For Each msg As String In msgs
                'NFO {StartPoses 3} {BlueGoal -2.55,-0.00,-0.19 0.00,-0.03,-0.01 MiddleField 0.01,-0.00,-0.19 0.00,0.00,0.00 YellowGoal 2.57,-0.01,-0.19 0.00,0.00,3.13}
                If String.IsNullOrEmpty(msg) Then
                    Continue For
                End If

                Dim msgtype As MessageType = MessageType.Unknown
                Dim parts As New Specialized.StringCollection

                'extract msgtype first
                Dim i As Integer = 0
                Dim j As Integer = msg.IndexOf(" ")
                Dim part As String = Nothing
                If j > 0 Then
                    part = msg.Substring(i, j - i)
                End If
                If part.ToUpper = "NFO" Then
                    msgtype = MessageType.Info
                Else
                    Continue For
                End If

                i = msg.IndexOf("{", j)
                While i >= 0
                    j = msg.IndexOf("}", i)
                    If j >= 0 Then
                        part = msg.Substring(i + 1, j - i - 1)
                        parts.Add(part)

                        i = msg.IndexOf("{", j)

                    Else
                        'done
                        Exit While

                    End If

                    If part.StartsWith("Level") Then
                        returnValue = part.Substring(6)
                    End If
                    'Now SyncGuiWithConfig in AgentConfig
                End While


            Next

            'AVI: search the parts for key StartPoses, and loop through all Poses

            'AVI: Make enumeration, which can be used as drop down for each robot.

            usarSimConnection.Disconnect()
            Return returnValue

        Catch ex As Exception
            MsgBox("Error occurred during GetLevelName" & vbNewLine & ex.Message, MsgBoxStyle.OkOnly, MsgBoxStyle.Exclamation)
            Return Nothing
        End Try
    End Function

    Private Sub chkboxBackup_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkboxBackup.CheckedChanged
        If chkboxBackup.Checked Then
            Me.txtBackupFrequency.Enabled = True
        Else
            Me.txtBackupFrequency.Enabled = False
        End If
    End Sub

    Private Sub txtOperatorName_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private _fillEllipse As Boolean = False

    Private spawnlocations As New List(Of String)


    Private Sub Panel1_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Panel1.Paint


        Dim textHolder As String = GetStartPoses(Me.txtUsarSimHost.Text, Integer.Parse(Me.txtUsarSimPort.Text)).ToLower
        Dim replace As String = ""


        Dim pattern As String = "([a-z]+[0-9]*)+"

        Console.WriteLine(textHolder)

        Dim rgx As New Regex(pattern)
        Dim result As String = rgx.Replace(textHolder, replace)
        'Console.WriteLine("first change: {0}", result)

        Dim pattern2 As String = "_[0-9]"
        Dim rgx2 As New Regex(pattern2)
        Dim result2 As String = rgx2.Replace(result, replace)
        'Console.WriteLine("second change: {0}", result2)

        Dim pattern3 As String = "\s+"
        Dim rgx3 As New Regex(pattern3)
        Dim result3 As String = rgx3.Replace(result2, " ")
        'Console.WriteLine(result3)

        Console.WriteLine("the last result:{0}", result3)

        Dim parts() As String = Strings.Split(result3, " ")

        Console.WriteLine("the first element:{0}", parts(0))
        Dim table As New DataTable
        table.Columns.Add("poses", GetType(String))

        Dim table2 As New DataTable
        table2.Columns.Add("robotType", GetType(String))


        Dim temp() As String
        Dim temp_x As String
        Dim temp_y As String
        Dim temp_z As String
        Dim font As Font = New Font(SystemFonts.DefaultFont.FontFamily.Name, 5)


        For j As Integer = 0 To parts.Length - 1
            Dim i As Integer = 0
            If (j Mod 2 = 1) Then
                temp = Strings.Split(parts(j), ",")
                temp_x = temp(0)
                temp_y = temp(1)
                temp_z = temp(2)
                Console.WriteLine("agent{0} : {1} {2} {3}", j, temp_x, temp_y, temp_z)
                e.Graphics.DrawEllipse(Drawing.Pens.Black, Single.Parse(temp_x) * 15 + 400, Single.Parse(temp_y) * 15 + 380, 10, 10)

                table.Rows.Add(parts(j))

                e.Graphics.DrawString(parts(j), font, Brushes.Black, Single.Parse(temp_x) * 15 + 400, Single.Parse(temp_y) * 15 + 400)


            End If

        Next

        
        With ComboBox1
            .DataSource = table
            .DisplayMember = "poses"

        End With

        table2.Rows.Add("ComStation")
        table2.Rows.Add("Nao")
        table2.Rows.Add("P3AT")
        table2.Rows.Add("kenaf")
        table2.Rows.Add("AirRobot")

        With ComboBox2
            .DataSource = table2
            .DisplayMember = "robotType"
        End With

        If Me._fillEllipse Then
            For Each member As String In spawnlocations
                Dim localtemp() As String = Strings.Split(member, ",")
                e.Graphics.FillEllipse(System.Drawing.Brushes.Black, Single.Parse(localtemp(0)) * 15 + 400, Single.Parse(localtemp(1)) * 15 + 380, 10, 10)
            Next

        End If






    End Sub

    Private Sub btnOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub


    Private Sub txtTeamMembers_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub


    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged

    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox2.SelectedIndexChanged

    End Sub

    Private configString As New StringBuilder
    Private p As Windows.Forms.PaintEventArgs
    Public ReadOnly Property paintTo() As Windows.Forms.PaintEventArgs
        Get
            Return Me.p
        End Get
    End Property





    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        ' Dim cmd As New StringBuilder()
        'Dim connection As New TcpMessagingConnection
        'connection.Connect(Me.txtUsarSimHost.Text, Integer.Parse(Me.txtUsarSimPort.Text))

        'cmd.Append("INIT ")
        'If (ComboBox2.Text() = "ComStation") Then
        'cmd.AppendFormat("{{ClassName {0}}} ", "USARBot.P3AT")
        'Else
        'cmd.AppendFormat("{{ClassName {0}}} ", "USARBot." + ComboBox2.Text())
        'End If

        '        cmd.AppendFormat("{{Name {0}}} ", TextBox1.Text())

        'cmd.AppendFormat("{{Location {0}}} ", ComboBox1.Text())
        'cmd.AppendFormat("{{Rotation {0}}} ", "0,0,0")
        'cmd.Append(Environment.NewLine)
        'connection.Send(cmd.ToString)
        Dim agentconf As AgentConfig = New AgentConfig(TextBox1.Text())

        agentconf.AgentName = TextBox1.Text()
        agentconf.AgentNumber = CInt(TextBox1.Text())
        agentconf.AgentType = "slam"
        agentconf.RobotModel = ComboBox2.Text()
        agentconf.MappingMode = "ScanMatching"
        agentconf.BehaviorMode = "TeleOperation"
        agentconf.BehaviorBalance = 1.0
        agentconf.StartLocation = ComboBox1.Text()
        agentconf.StartRotation = "0,0,0"
        agentconf.UseMultiView = True
        agentconf.ScanMatcher = "WSM"
        agentconf.UseImageServer = True
        agentconf.SpawnFromCommander = True
        agentconf.UseNoise = False
        agentconf.NoiseSigma = 0.0
        agentconf.SeedMode = "None"
        agentconf.SkinDetectorMode = "Detection"
        agentconf.TeacherMode = "SkinOnly"
        agentconf.LogFileFormat = "UsarSim"
        agentconf.UseLogger = False
        agentconf.LogPlayback = False
        agentconf.MultiViewPanels = TextBox1.Text()

        agentconf.Save("newmap")
        If (Me.configString.Length = 0) Then
            Me.configString.Append(TextBox1.Text())
        Else
            Me.configString.Append("," + TextBox1.Text())
        End If

        My.Settings.LastAgentConfigFile = "newmap"
        currAgent = Strings.Split(ComboBox1.Text(), ",")
        Me.spawnlocations.Add(ComboBox1.Text())
        Me._fillEllipse = True
        Panel1.Invalidate()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Console.WriteLine(My.Settings.LastTeamConfigFile)
        Dim tmpTeamConfig As TeamConfig = New TeamConfig(My.Settings.LastTeamConfigFile)
        tmpTeamConfig.LevelName = TeamConfigDialog.GetLevelName(tmpTeamConfig.UsarSimHost, tmpTeamConfig.UsarSimPort)
        tmpTeamConfig.StartPoses = TeamConfigDialog.GetStartPoses(tmpTeamConfig.UsarSimHost, tmpTeamConfig.UsarSimPort)
        tmpTeamConfig.TeamMembers = Me.configString.ToString


        tmpTeamConfig.Save(My.Settings.LastTeamConfigFile)
        Me.Close()
    End Sub

    Private Sub txtUsarSimHost_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtUsarSimHost.TextChanged

    End Sub

    Private Sub btnGetStartPoses_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGetStartPoses.Click

    End Sub

    Private Sub fillTheElipse(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs)


        e.Graphics.FillEllipse(System.Drawing.Brushes.Black, Single.Parse(currAgent(0)), Single.Parse(currAgent(1)), 10, 10)
    End Sub

    




End Class



