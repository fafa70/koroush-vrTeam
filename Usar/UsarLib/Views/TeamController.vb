Imports System.IO
Imports System.Math
Imports System.Text

Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Slam
Imports UvARescue.Usar.Lib
Imports UvARescue.Tools

Imports System.Drawing
Imports System.Drawing.Imaging

Imports System.Windows.Forms

Public Enum NamingConvention
    Numbers
    GreekHeroes
    PlanetNames
    BondActors
    MatrixStyle
End Enum


Public Enum TimerStyle
    ShowTimeLeft
    ShowTimeElapsed
End Enum



Public Class TeamController
    Inherits UserControl

    Private _MapController As MapController
    Private _FrontierTools As FrontierTools = New FrontierTools

#Region " Constructor / Destructor "

    Public Sub New(ByVal manifold As Manifold, ByVal mapController As MapController, ByVal camerabox As cameraBox, ByVal cameraAgent As agentCamera, ByVal mainController As mainController)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me._TeamConfig = New TeamConfig(My.Settings.LastTeamConfigFile)
        ' Dim dialog As New TeamConfigDialog(Me._TeamConfig)
        Me._Manifold = manifold
        'fafa
        ' Me._ManifoldImage = New ManifoldImage(manifold, UvARescue.Tools.Constants.MAP_RESOLUTION, False)
        'Me._ManifoldImage = mapController.ManifoldView.Image

        _agentCamera = cameraAgent

        Me._MapController = mapController
        Me._MapController.SetTeamConfig(Me._TeamConfig)

        Me._camerabox = camerabox
        Me._mainController = mainController

        Me._NamingConvention = [Lib].NamingConvention.Numbers
        Me._TimerStyle = TimerStyle.ShowTimeLeft

        Me.SetRunStatus(UsarRunStatus.Preparing)

        'dialog.ShowDialog(Me)

    End Sub

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

#End Region


#Region " Naming Convention Gimmick "

    Private _NamingConvention As NamingConvention
    Protected Property NamingConvention() As NamingConvention
        Get
            Return Me._NamingConvention
        End Get
        Set(ByVal value As NamingConvention)
            If Not value = Me._NamingConvention Then
                Dim oldValue As NamingConvention = Me._NamingConvention
                Me._NamingConvention = value
                Me.OnNamingConventionChanged(oldValue, value)
            End If
        End Set
    End Property

    Protected Overridable Sub OnNamingConventionChanged(ByVal oldValue As NamingConvention, ByVal newValue As NamingConvention)
        With Me.pnlAgents
            'traverse in reverse order
            Dim index As Integer = .Controls.Count - 1
            For Each control As Control In .Controls
                If TypeOf control Is AgentController Then
                    DirectCast(control, AgentController).AgentName = Me.GetName(Me.NamingConvention, index)
                    index -= 1
                End If
            Next
        End With
    End Sub

    Private Function GetName(ByVal convention As NamingConvention, ByVal index As Integer) As String

        'the first name (index=0) should be the Operator name 
        Dim names() As String = Nothing
        Select Case convention
            Case NamingConvention.Numbers
                names = New String() {"1", "2", "3", "4", "5", "6", "7", "8", "9"}
                '       Case NamingConvention.MatrixStyle
                '          names = New String() {"Operator", "Neo", "Morpheus", "Trinity", "Tank", "Dozer", "Apoc", "Mouse", "Cypher"}
                '     Case NamingConvention.BondActors
                '        names = New String() {"MoneyPenny", "Sean", "George", "Roger", "Timothy", "Pierce", "Daniel"}
                '   Case NamingConvention.GreekHeroes
                '      names = New String() {"Zeus", "Hercules", "Achilles", "Theseus", "Odysseus", "Perseus"}
                ' Case NamingConvention.PlanetNames
                '    names = New String() {"Sun", "Mercury", "Venus", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune", "Pluto"}
        End Select

        'return the i-th name if it exists, otherwise invent a name
        Dim name As String
        If index < names.Length Then
            name = names(index)
        Else
            name = "Agent " & index + 1
        End If
        Return name

    End Function

    'geek-style fanciness ...
    Private Sub btnNumbers_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.NamingConvention = [Lib].NamingConvention.Numbers
    End Sub
    'Private Sub btnMatrix_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMatrix.Click
    '   Me.NamingConvention = [Lib].NamingConvention.MatrixStyle
    'End Sub
    'Private Sub btnBond_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBond.Click
    '   Me.NamingConvention = [Lib].NamingConvention.BondActors
    'End Sub
    'Private Sub btnHeroes_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHeroes.Click
    '   Me.NamingConvention = [Lib].NamingConvention.GreekHeroes
    'End Sub
    'Private Sub btnJupiter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnJupiter.Click
    '   Me.NamingConvention = [Lib].NamingConvention.PlanetNames
    'End Sub

#End Region

#Region " Team Config "

    Private _mainController As mainController

    Public ReadOnly Property mainController() As mainController
        Get
            Return Me._mainController
        End Get
    End Property

    Private _camerabox As cameraBox
    Public ReadOnly Property camerabox() As cameraBox
        Get
            Return Me._camerabox
        End Get
    End Property

    Private _TeamConfig As TeamConfig
    Public ReadOnly Property TeamConfig() As TeamConfig
        Get
            Return Me._TeamConfig
        End Get
    End Property

    Private Sub ShowTeamConfigDialog()

        Dim dialog As New TeamConfigDialog(Me._TeamConfig, Me._Manifold)

        'copy operator name and team member names into dialog
        With dialog
            '.OperatorName = Me.GetName(Me.NamingConvention, 0)

            If Me.AgentCount > 0 Then

                Dim members(Me.AgentCount - 1) As String
                For i As Integer = 0 To Me.AgentCount - 1
                    members(i) = String.Format("{0}-{1}", i, Me.GetName(Me.NamingConvention, i))
                Next

                ' .TeamMembers = Strings.Join(members, ",")

            Else
                '.TeamMembers = ""

            End If

        End With

        dialog.ShowDialog(Me)
        Me._MapController.SetTeamConfig(Me._TeamConfig)

    End Sub

#End Region


#Region " Run Status "

    ''' <summary>
    ''' Will be invoked by the mainform when the user attempts to quit the application.
    ''' In case we are in a run, we enable the user to cancel his action.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function QueryCanQuitApplication() As Boolean
        Dim canQuit As Boolean = False

        Select Case Me._RunStatus
            Case UsarRunStatus.Preparing, UsarRunStatus.Ready, UsarRunStatus.Done
                canQuit = True

            Case Else
                Select Case MessageBox.Show("Are you sure you want to Quit the application?", "Quit?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                    Case DialogResult.OK
                        Me.SetRunStatus(UsarRunStatus.Done)
                        canQuit = True
                End Select

        End Select

        Return canQuit
    End Function

    Private _RunStatus As UsarRunStatus = UsarRunStatus.Ready

    Public ReadOnly Property RunStatus() As UsarRunStatus
        Get
            Return Me._RunStatus
        End Get
    End Property
    Public Sub SetRunStatus(ByVal status As UsarRunStatus)
        If Not status = Me._RunStatus Then
            Me._RunStatus = status
            Me.OnRunStatusChanged()

            'forward to agent views
            For Each control As Control In Me.pnlAgents.Controls
                If TypeOf control Is AgentController Then
                    DirectCast(control, AgentController).NotifyRunStatusChanged(status)
                End If
            Next

            Me.OnRunStatusChanged()

        End If
    End Sub



    ''' <summary>
    ''' Syncs TeamController view to facilitate current runstatus.''' 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overridable Sub OnRunStatusChanged()

        'update various UI elements
        Me.UpdateTitle()
        Me.UpdateTimer()
        Me.UpdateProceedButton()

        'show/hide panels
        Me.SuspendLayout()

        Select Case Me._RunStatus
            Case UsarRunStatus.Preparing
                ' Me._MapController.EnableMapSave(False)
                Me.pnlSelectNamingConvention.Show()
                Me.pnlOnline.Hide()
                Me.pnlButtons.Show()
                Me.pnlBack.Hide()

                'reset manifold
                Me.Manifold.AcquireWriterLock()
                Me.Manifold.Clear()
                Me.Manifold.ReleaseWriterLock()

            Case UsarRunStatus.Ready
                'preparations completed
                Me.pnlSelectNamingConvention.Hide()
                Me.pnlOnline.Show()
                Me.pnlButtons.Hide()
                Me.pnlBack.Show()


            Case UsarRunStatus.Reporting
                ' Me._MapController.EnableMapSave(True)
                'autonomous reporting?

                If (TeamConfig.CreateBackups) Then
                    Me.Backup()
                End If

            Case Else
                'agents are online
                Me.pnlOnline.Show()
                Me.pnlBack.Hide()
                Me._MapController.SetTeamConfig(Me._TeamConfig)

        End Select

        Me.ResumeLayout(True)

    End Sub

    Private Sub UpdateTitle()
        Me.lblRunStatus.Text = String.Format("Team Status: {0} - {1} ", CInt(Me._RunStatus), Me._RunStatus.ToString)
    End Sub

    Private Sub UpdateProceedButton()
        With Me.btnProceed
            Select Case Me._RunStatus
                Case UsarRunStatus.Preparing, UsarRunStatus.Ready, UsarRunStatus.Running
                    .Text = "Done / Report"
                    .Image = My.Resources.Report
                Case UsarRunStatus.Reporting
                    .Text = "Done / Stop"
                    .Image = My.Resources.Seal
                Case UsarRunStatus.Done
                    .Text = "Reset"
                    .Image = My.Resources.Reset
            End Select
        End With
    End Sub

#End Region

#Region " Offline Stage - Add / Remove / Configure Agents "

    Private _Manifold As Manifold
    Public ReadOnly Property Manifold() As Manifold
        Get
            Return Me._Manifold
        End Get
    End Property

    'Private _ManifoldImage As ManifoldImage
    'Public ReadOnly Property ManifoldImage() As ManifoldImage
    '    Get
    '        Return Me._ManifoldImage
    '    End Get
    'End Property


    Private _agentCamera As agentCamera
    Public ReadOnly Property gaentCamera() As agentCamera
        Get
            Return Me._agentCamera
        End Get
    End Property


    Private _UsarOperator As UsarOperatorAgent
    Public ReadOnly Property UsarOperator() As UsarOperatorAgent
        Get
            Return Me._UsarOperator
        End Get
    End Property

    Public ReadOnly Property AgentCount() As Integer
        Get
            Return Me.pnlAgents.Controls.Count
        End Get
    End Property

    Private Sub btnConfig_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNetwork.Click
        Me.ShowTeamConfigDialog()
    End Sub

    Public Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        With Me.pnlAgents.Controls
            Dim view As AgentController = Me.CreateAgentView(.Count)
            view.Dock = DockStyle.Top
            .Add(view)
            .SetChildIndex(view, 0)
        End With
        Me.btnReady.Enabled = Me.AgentCount > 0
    End Sub

    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        With Me.pnlAgents.Controls
            If .Count > 0 Then
                Me.DestroyAgentView(DirectCast(.Item(0), AgentController))
                .RemoveAt(0)
            End If
        End With
        Me.btnReady.Enabled = Me.AgentCount > 0
    End Sub




    Public Overridable Function CreateAgentView(ByVal index As Integer) As AgentController
        Dim name As String = Me.GetName(Me.NamingConvention, index)
        Dim view As New AgentController(Me.TeamConfig, Me._UsarOperator, name, Me._Manifold, Me._camerabox, Me._agentCamera, Me._mainController)
        ' Me._mainController.createAgent(view)


        AddHandler view.AgentStarted, AddressOf Me.AgentController_AgentStarted
        AddHandler view.AgentStopped, AddressOf Me.AgentController_AgentStopped

        Return view
    End Function

    Protected Overridable Sub DestroyAgentView(ByVal view As AgentController)
        RemoveHandler view.AgentStarted, AddressOf Me.AgentController_AgentStarted
        RemoveHandler view.AgentStopped, AddressOf Me.AgentController_AgentStopped
    End Sub

    Protected Overridable Sub AgentController_AgentStarted(ByVal sender As Object, ByVal e As EventArgs(Of Agent.Agent))
        'forward to agent views
        For Each control As Control In Me.pnlAgents.Controls
            If Not control Is sender AndAlso TypeOf control Is AgentController Then
                DirectCast(control, AgentController).NotifyAgentStarted(e.Data)
                Me._UsarOperator = DirectCast(control, AgentController).UsarOperator
            End If
        Next
    End Sub
    Protected Overridable Sub AgentController_AgentStopped(ByVal sender As Object, ByVal e As EventArgs(Of Agent.Agent))
        'forward to agent views
        For Each control As Control In Me.pnlAgents.Controls
            If Not control Is sender AndAlso TypeOf control Is AgentController Then
                DirectCast(control, AgentController).NotifyAgentStopped(e.Data)
            End If
        Next
    End Sub



    'when preparations are done, the ready button will be clicked
    Private Sub btnReady_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReady.Click
        Me.SetRunStatus(UsarRunStatus.Ready)
    End Sub


    'clicking the back button will return to the preparation stage
    Private Sub btnBack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBack.Click
        Me.SetRunStatus(UsarRunStatus.Preparing)
    End Sub

    Private Sub btnStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStart.Click
        Me.SetRunStatus(UsarRunStatus.Running)
    End Sub

#End Region

#Region " Online Stage - Running / Reporting "

    Private Sub btnProceed_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnProceed.Click

        Dim nextStatus As UsarRunStatus

        Select Case Me._RunStatus

            Case UsarRunStatus.Preparing, UsarRunStatus.Ready, UsarRunStatus.Running

                Dim message As New StringBuilder
                message.AppendLine("Are you ABSOLUTELY sure that you want to")
                message.AppendLine("- STOP ALL ROBOTS")
                message.AppendLine("- and enter reporting mode?")
                message.AppendLine()
                message.AppendLine("Please note that this action is IRREVERSIBLE.")
                message.AppendLine()

                Select Case MessageBox.Show(message.ToString, "Are you sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation Or MessageBoxIcon.Question)
                    Case DialogResult.OK
                        nextStatus = UsarRunStatus.Reporting
                    Case Else
                        'keep current status
                        nextStatus = Me._RunStatus
                End Select

                'Me.Manifold.AcquireReaderLock()
                Me.Manifold.WriteVictFile()
                'Me.Manifold.ReleaseReaderLock()

            Case UsarRunStatus.Reporting
                nextStatus = UsarRunStatus.Done

            Case UsarRunStatus.Done
                nextStatus = UsarRunStatus.Preparing

        End Select

        Me.SetRunStatus(nextStatus)

    End Sub

#End Region

#Region " Timer "

    Private totalTime As TimeSpan
    Private startTime As DateTime


    Private Sub UpdateTimer()
        Select Case Me._RunStatus
            Case UsarRunStatus.Preparing, UsarRunStatus.Ready
                Me.Timer.Enabled = False
                Me.totalTime = TimeSpan.FromMinutes(My.Settings.UsarRunTimeLength)
                Me.startTime = Now 'so the clock renders nicely

            Case UsarRunStatus.Running
                'start runtime countdown
                Me.startTime = Now
                Me.Timer.Enabled = True

            Case UsarRunStatus.Reporting
                'stop runtime countdown and start reporting time countdown
                Me.Timer.Enabled = False
                Me.totalTime = TimeSpan.FromMinutes(My.Settings.UsarReportingTimeLength)
                Me.startTime = Now
                Me.Timer.Enabled = True

            Case UsarRunStatus.Done
                'stop countdown
                Me.Timer.Enabled = False

        End Select

        Me.UpdateTimeLabel()

    End Sub


    Private _TimerStyle As TimerStyle
    Public Property TimerStyle() As TimerStyle
        Get
            Return _TimerStyle
        End Get
        Set(ByVal value As TimerStyle)
            If Not value = Me._TimerStyle Then
                _TimerStyle = value
                Me.UpdateTimeLabel()
            End If
        End Set
    End Property

    Private Sub Timer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer.Tick
        If (TeamConfig.CreateBackups And Me._RunStatus = UsarRunStatus.Running) Then
            ' Check whether to backup
            Dim elapsed As TimeSpan = DateTime.Now - startTime
            If (CInt(elapsed.TotalSeconds) Mod TeamConfig.BackupFrequency = 0) Then
                Console.WriteLine("[TeamController] Backing up map and victim files")
                'For Each control As Control In Me.pnlAgents.Controls 'Not longer needed, problem was in AcquireReaderLock
                '    If TypeOf control Is AgentController Then
                '        DirectCast(control, AgentController).Agent.Halt() 'Stop the robots, no control when backup up
                '    End If
                'Next
                Me.Backup()
            End If
        End If

        Me.UpdateTimeLabel()
    End Sub

    Private Sub UpdateTimeLabel()

        Dim nextStatus As UsarRunStatus
        Dim elapsed As TimeSpan = DateTime.Now - startTime
        Dim timeleft As TimeSpan = totalTime - elapsed
        If timeleft < TimeSpan.Zero Then
            timeleft = TimeSpan.Zero
        End If

        Dim display As TimeSpan = DirectCast(IIf(Me._TimerStyle = [Lib].TimerStyle.ShowTimeElapsed, elapsed, timeleft), TimeSpan)
        Dim redness As Integer = Min(CInt(elapsed.TotalSeconds / Me.totalTime.TotalSeconds * 255), 255)

        Me.lblTime.Text = String.Format("{0}:{1:d2}", display.Minutes, display.Seconds)
        Me.lblTime.ForeColor = System.Drawing.Color.FromArgb(255, 255 - redness, 255 - redness)

        If timeleft = TimeSpan.Zero And Constants.AUTOMATED_REPORTING Then

            Select Case Me._RunStatus
                Case UsarRunStatus.Running
                    nextStatus = UsarRunStatus.Reporting
                    Me.Backup()
                Case UsarRunStatus.Reporting
                    nextStatus = UsarRunStatus.Done
                Case UsarRunStatus.Done
                    nextStatus = UsarRunStatus.Preparing
            End Select

            Me.SetRunStatus(nextStatus)
        End If
    End Sub

#End Region


#Region " Automated Backup "

    Private Sub Backup()
        ' Find basefilename
        Dim basefilename As String = reportPath() + "\report"

        ' Backups are saved only as Tiffs, for now
        Dim formats As New List(Of ImageFormat)
        formats.Add(ImageFormat.Tiff)

        ' Save map
        Me._MapController.ManifoldView.Image.Save(formats, basefilename, Nothing)

        ' Write victim file
        'Me.Manifold.AcquireReaderLock() 'Don't, this could take infinity, in the main time the GUI hasn't control
        Me.Manifold.WriteVictFile(reportPath())
        'Me.Manifold.ReleaseReaderLock()


        ' Write victim file
        'Me.Manifold.WritePathFile(reportPath())
    End Sub

    Private Function reportPath() As String
        reportPath = My.Application.Info.DirectoryPath

        For i As Integer = 1 To 5
            reportPath = Directory.GetParent(reportPath).ToString
        Next
        reportPath += "\IUST-BACKUP"

        If Not Directory.Exists(reportPath) Then
            Directory.CreateDirectory(reportPath)
        End If

        Dim currTime As String = Format(System.DateTime.Now, "yyyy-MM-dd_HH-mm-ss")
        reportPath += "\" + currTime

        If Not Directory.Exists(reportPath) Then
            Directory.CreateDirectory(reportPath)
        End If
    End Function

#End Region


#Region " Deploy, AllTele, AllAuto buttons "

    Private Sub btnAllTele_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        With Me.pnlAgents
            For Each control As Control In .Controls
                If TypeOf control Is AgentController Then
                    ' DirectCast(control, AgentController).BehaviorDisplay.SelectedItem = "ConservativeTeleOp"
                End If
            Next
            For Each control As Control In Me.pnlAgents.Controls
                If TypeOf control Is AgentController Then
                    DirectCast(control, AgentController).NotifyAgentStatusChanged()
                End If
            Next
        End With
    End Sub

    Private Sub btnAllAuto_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        With Me.pnlAgents
            For Each control As Control In .Controls
                If TypeOf control Is AgentController Then
                    ' DirectCast(control, AgentController).BehaviorDisplay.SelectedItem = "AutonomousExploration"
                End If
            Next
            For Each control As Control In Me.pnlAgents.Controls
                If TypeOf control Is AgentController Then
                    DirectCast(control, AgentController).NotifyAgentStatusChanged()
                End If
            Next
        End With
    End Sub

    Private Sub btnDeploy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        With Me.pnlAgents
            For Each control As Control In .Controls
                If TypeOf control Is AgentController Then
                    'DirectCast(control, AgentController).BehaviorDisplay.SelectedItem = "DeploymentTest"
                End If
            Next
            For Each control As Control In Me.pnlAgents.Controls
                If TypeOf control Is AgentController Then
                    DirectCast(control, AgentController).NotifyAgentStatusChanged()
                End If
            Next
        End With
    End Sub

#End Region


    Private Sub pnlAgents_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles pnlAgents.Paint

    End Sub
End Class
