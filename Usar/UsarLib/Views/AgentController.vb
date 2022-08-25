Imports System.Drawing
Imports System.IO
Imports System.Math
Imports System.Windows.Forms
Imports System.Text
Imports UvARescue.Agent
Imports UvARescue.Slam
Imports UvARescue.Tools
Imports UvARescue.Math
Imports UvARescue.ImageAnalysis
Imports UvARescue.Agent.BehaviorControl

Public Class AgentController
    Inherits UserControl
    Implements IAgentObserver

#Region " Constructor / Destructor "

    Public Sub New(ByVal teamConfig As TeamConfig, ByVal usarOperator As UsarOperatorAgent, ByVal agentName As String, ByVal manifold As Manifold, ByVal camerabox As cameraBox, ByVal cameraAgent As agentCamera, ByVal maincon As mainController)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.UserPaint, True)

        ' Me._Manifold._changeMotion.Add(Me.Agent.Number, False)


        Me.Name = "AgentController"
        Me._agentCamera = cameraAgent
        Me._TeamConfig = teamConfig
        Me._AgentName = agentName
        Me._AgentConfig = New UsarAgentConfig(agentName)
        Me._maincontroller = maincon

        'try to load last used config file
        Dim cfgfile As String = "newmap"
        If File.Exists(cfgfile) Then
            Try
                Me._AgentConfig.Load(cfgfile)
                Me._TeamConfig.Load(cfgfile)
            Catch ex As Exception
            End Try
        End If

        Me._AgentCommActor = New CommAgent(manifold, agentName, AgentConfig, teamConfig)
        'Me._myMainAlgorithm = New Astar(manifold, agentName, AgentConfig, teamConfig, Me.Agent._rotation)
        Me._Manifold = manifold
        Me._UsarOperator = usarOperator
        Me._RunStatus = UsarRunStatus.Preparing
        Me._cambox = camerabox



        Me.LoadBehaviors()
        If Not TypeOf Me._Agent Is UsarOperatorAgent AndAlso Not Me._AgentConfig.BehaviorMode Is Nothing Then

            Me.ShowCurrentBehavior(Me._AgentConfig.BehaviorMode)
        End If


        Me.btnAction.Hide()
        Me.ResetDrivePanel()
        Me.ResetLabels()
        Me.ResetSpeedLabel()
        Me.Focus()
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

#Region " Team Configuration "


    Private _maincontroller As mainController
    Public ReadOnly Property maincontroller() As mainController
        Get
            Return Me._maincontroller
        End Get
    End Property

    Private _TeamConfig As TeamConfig
    Public ReadOnly Property TeamConfig() As TeamConfig
        Get
            Return Me._TeamConfig
        End Get
    End Property

#End Region

#Region " Agent Configuration "


    Private _agentCamera As agentCamera
    Public ReadOnly Property gaentCamera() As agentCamera
        Get
            Return Me._agentCamera
        End Get
    End Property







    Private _AgentName As String
    Public Property AgentName() As String
        Get
            Return Me._AgentName
        End Get
        Set(ByVal value As String)
            If Not value = Me._AgentName Then
                Me._AgentName = value
                Me._AgentConfig.AgentName = value
                Me.ResetLabels()
            End If
        End Set
    End Property


    Private _AgentCommActor As CommAgent
    Public ReadOnly Property AgentCommActor() As CommAgent
        Get
            Return Me._AgentCommActor
        End Get
    End Property



    Private _AgentConfig As UsarAgentConfig
    Public ReadOnly Property AgentConfig() As UsarAgentConfig
        Get
            Return Me._AgentConfig
        End Get
    End Property

    Private Sub ShowAgentConfigView()
        Dim dialog As New AgentConfigDialog(Me._AgentConfig)
        Dim result As DialogResult = dialog.ShowDialog(Me)
        Select Case result
            Case DialogResult.OK
                Me.ResetLabels()
        End Select
    End Sub

#End Region

#Region " Monitor Current RunStatus "

    Private _RunStatus As UsarRunStatus
    Protected ReadOnly Property RunStatus() As UsarRunStatus
        Get
            Return Me._RunStatus
        End Get
    End Property

    Public Sub NotifyRunStatusChanged(ByVal status As UsarRunStatus)
        Me.OnRunStatusChanged(status)
    End Sub

    Protected Overridable Sub OnRunStatusChanged(ByVal status As UsarRunStatus)
        Me._RunStatus = status

        Me.ResetActionButton()
        Me.ResetLabels()

        Select Case status
            Case UsarRunStatus.Reporting, UsarRunStatus.Done
                Me.StopAgent()
        End Select

    End Sub

    Private Sub ResetActionButton()
        With Me.btnAction
            Select Case Me._RunStatus
                Case UsarRunStatus.Preparing, UsarRunStatus.Ready
                    .Hide()
                    .Enabled = (Me._RunStatus = UsarRunStatus.Preparing)
                    .Image = My.Resources.Gear

                Case UsarRunStatus.Running
                    Me.pnlCamImage.Show()
                    .Show()
                    .Enabled = True
                    If IsNothing(Me._Agent) Then
                        .Image = My.Resources.Lightning
                    Else
                        If Me._Agent.IsPaused Then
                            .Image = My.Resources.Play
                        Else
                            .Image = My.Resources.Pause

                            If TypeOf Me._Agent Is OperatorAgent Then
                                Me.btnDrive.Visible = False
                                Me.btnHeadLight.Visible = False
                                Me.BtnFlip.Visible = False
                            Else
                                Me.btnDrive.Visible = True
                                Me.btnHeadLight.Visible = True
                                Me.BtnFlip.Visible = True
                            End If
                        End If
                    End If


                Case UsarRunStatus.Reporting, UsarRunStatus.Done

                    .Enabled = False
                    .Image = My.Resources.Play

                    Me.btnDrive.Visible = False
                    Me.btnHeadLight.Visible = False
                    Me.BtnFlip.Visible = False
                    Me.pnlCamImage.Hide()
                    Me.SuspendLayout()
                    Me.pnlDrive.Hide()
                    Me.pnlImgControl.Hide()
                    Me.pnlControl.Show()
                    Me.ResumeLayout(True)
            End Select
        End With
    End Sub

    Private Sub btnAction_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAction.Click
        Select Case Me._RunStatus
            Case UsarRunStatus.Preparing
                Me.ShowAgentConfigView()

            Case UsarRunStatus.Ready
                'no associated action

            Case UsarRunStatus.Running
                If IsNothing(Me._Agent) Then
                    Me.StartAgent()
                Else
                    If Me.Agent.IsPaused Then
                        Me.Agent.Resume()
                    Else
                        Me.Agent.Pause()
                    End If
                End If

            Case UsarRunStatus.Reporting, UsarRunStatus.Done

        End Select
    End Sub

#End Region

#Region " Monitoring Agent Status "

    Public Sub NotifyAgentStarted(ByVal agent As Agent.Agent)
        If TypeOf agent Is UsarOperatorAgent Then
            If Not agent Is Me._UsarOperator Then

                'new operator agent was spawned
                Dim [operator] As UsarOperatorAgent = DirectCast(agent, UsarOperatorAgent)

                If Not IsNothing(Me._Agent) AndAlso TypeOf Me._Agent Is UsarProxyAgent Then
                    'register proxy (if necessary)
                    Dim proxy As UsarProxyAgent = DirectCast(Me._Agent, UsarProxyAgent)
                    If Not IsNothing(Me._UsarOperator) Then
                        'unregister from current operator
                        Me._UsarOperator.UnRegisterProxy(proxy)
                    End If
                    If Not IsNothing([operator]) Then
                        'register with new operator
                        [operator].RegisterProxy(proxy)
                    End If

                End If

                'update local reference
                Me._UsarOperator = [operator]

            End If
        End If
    End Sub

    Public Sub NotifyAgentStopped(ByVal agent As Agent.Agent)
        If TypeOf agent Is UsarOperatorAgent Then
            If agent Is Me._UsarOperator Then

                'operator agent was stopped
                Dim [operator] As UsarOperatorAgent = DirectCast(agent, UsarOperatorAgent)

                If Not IsNothing(Me._Agent) AndAlso TypeOf Me._Agent Is UsarProxyAgent Then
                    'unregister proxy 
                    [operator].UnRegisterProxy(DirectCast(Me._Agent, UsarProxyAgent))
                End If

                'update local reference
                Me._UsarOperator = [operator]

            End If
        End If
    End Sub

    Public _Manifold As Manifold
    Public ReadOnly Property Manifold() As Manifold
        Get
            Return Me._Manifold
        End Get
    End Property

    Private _UsarOperator As UsarOperatorAgent
    Public ReadOnly Property UsarOperator() As UsarOperatorAgent
        Get
            Return Me._UsarOperator
        End Get
    End Property

    Private _Driver As IDriver = Nothing
    Public ReadOnly Property Driver() As IDriver
        Get
            Return Me._Driver
        End Get
    End Property

    Private _Agent As Agent.Agent
    Public ReadOnly Property Agent() As Agent.Agent
        Get
            Return Me._Agent
        End Get
    End Property


    Public Event AgentStarted As EventHandler(Of EventArgs(Of Agent.Agent))
    Public Event AgentStopped As EventHandler(Of EventArgs(Of Agent.Agent))


    Public Sub StartAgent()
        If Not IsNothing(Me._Agent) Then Throw New InvalidOperationException("Agent already initialized")

        With Me._AgentConfig
            Me._cambox.drawCam(.AgentNumber)
            Me._maincontroller.createAgent(Me, .AgentNumber)
            Me._agentCamera.initPanel(.AgentNumber)
            Me._AgentCommActor.CreateCommActor(Me._TeamConfig)
            Me._Agent = .CreateAgent(Me._Manifold, Me._TeamConfig, AgentStartMode.FromCommanderGUI)
            Me._Agent.AddObserver(Me)

            If TypeOf Me._Agent Is UsarOperatorAgent Then
                Me._UsarOperator = DirectCast(Me._Agent, UsarOperatorAgent)
            ElseIf TypeOf Me._Agent Is UsarProxyAgent Then
                If Not IsNothing(Me._UsarOperator) Then
                    Me._UsarOperator.RegisterProxy(DirectCast(Me._Agent, UsarProxyAgent))
                End If
            End If

            Me.ShowCurrentBehavior(.BehaviorMode)

            Me._Driver = .CreateDriver(Agent, Me._TeamConfig)
            Me._AgentCommActor.startComm()

        End With

        If Not IsNothing(Me._Driver) Then
            Me._Driver.Start()
        End If

        Me.ResetDrivePanel()

        If TypeOf Me._Agent Is UsarFollowAgent Then
            ' btnStartTracker.Visible = True
        End If

        RaiseEvent AgentStarted(Me, New EventArgs(Of Agent.Agent)(Me._Agent))

    End Sub

    Private Sub StopAgent()
        If Not IsNothing(Me._Driver) AndAlso Me._Driver.IsRunning Then
            Me._Driver.Stop()
        End If
        Me._Driver = Nothing

        If Not IsNothing(Me._Agent) Then
            Me._Agent.RemoveObserver(Me)

            If TypeOf Me._Agent Is UsarProxyAgent Then
                If Not IsNothing(Me._UsarOperator) Then
                    Me._UsarOperator.UnRegisterProxy(DirectCast(Me._Agent, UsarProxyAgent))
                End If
            End If

            RaiseEvent AgentStopped(Me, New EventArgs(Of Agent.Agent)(Me._Agent))

        End If
        Me._Agent = Nothing

        Me.ResetDrivePanel()


    End Sub


    Private _Pathloss As String = ""

    Private Sub ResetLabels()

        Me.lblTitle.Text = String.Format("#{2}: {0} - [{1}]", Me._AgentName, Me._AgentConfig.RobotModel, Me._AgentConfig.AgentNumber)

        Select Case Me._RunStatus
            Case UsarRunStatus.Preparing
                'abuse various labels to display current config
                With Me._AgentConfig
                    Me.lblPose.Text = String.Format("{0} m", .StartLocation)
                    ' Me.lblInfo.Text = String.Format("{0} rad", .StartRotation)
                    'Me.lblInfo.ForeColor = Color.White
                    If .LogPlayback Then
                        Try
                            '     Me.lblPower.Text = Path.GetFileName(.LogFile)
                        Catch ex As Exception
                            '    Me.lblPower.Text = "No logfile selected"
                        End Try
                    Else
                        Dim opts As String = ""
                        If .UseImageServer Then opts += " +ImgServer"
                        ' Me.lblPower.Text = "Live" & opts
                    End If
                    'Me.lblLight.Text = String.Format("{0} +{1} +{2}", .MappingMode, .ScanMatcher, .SeedMode)
                    Me.ShowCurrentBehavior(.BehaviorMode)
                End With

            Case UsarRunStatus.Ready
                'configuration done
                'Me.lblInfo.Text = "Configured"
                'Me.lblInfo.ForeColor = Color.Blue
                Me.ShowCurrentBehavior(Me._AgentConfig.BehaviorMode)

            Case UsarRunStatus.Reporting, UsarRunStatus.Done
                'Me.lblInfo.Text = "Stopped"
                'Me.lblInfo.ForeColor = Color.Red

            Case Else

                Dim info As String
                Dim fclr As Color
                If IsNothing(Me._Agent) Then
                    info = "Ready to Spawn"
                    fclr = Color.Yellow
                Else
                    If Not Me._Agent.IsRunning Then
                        info = "Stopped"
                        fclr = Color.Red
                    Else
                        If Me._Agent.IsPaused Then
                            info = "Paused"
                            fclr = Color.Yellow
                        Else
                            info = "Running"
                            fclr = Color.LawnGreen
                        End If
                    End If
                End If

                'Me.lblInfo.Text = info & Me._Pathloss
                'Me.lblInfo.ForeColor = fclr

        End Select
    End Sub

    Private CamImageMutex As New Object

    Private _cambox As cameraBox
    Public ReadOnly Property cambox() As cameraBox
        Get
            Return Me._cambox
        End Get
    End Property


    Private Delegate Sub SensorUpdateHandler(ByVal sensor As Sensor)
    Public Sub NotifySensorUpdate(ByVal sensor As Sensor) Implements IAgentObserver.NotifySensorUpdate
        Try
            'make sure we are on the UI thread
            If Me.InvokeRequired Then
                If sensor.SensorType = CameraSensor.SENSORTYPE_CAMERA Then
                    Dim handler As New SensorUpdateHandler(AddressOf Me.NotifySensorUpdate)
                    Me.BeginInvoke(handler, sensor)
                    
                End If

            Else

                SyncLock Me.CamImageMutex
                    If Me.Agent.ShowImages Then
                        If sensor.SensorType = CameraSensor.SENSORTYPE_CAMERA Then

                            Dim image As Image = DirectCast(sensor, CameraSensor).LatestCamIm
                            If Not image Is Nothing Then
                                ' Me.Agent.AdjustCamImage(image)
                                'Me.pnlCamImage.BackgroundImage = image

                                _cambox.setImage(image, Me.Agent.Number)
                                Me._agentCamera.showCurrCamera(image, Me.Agent.Number)
                                Me._maincontroller.showCurrController(Me._Manifold.currNumber)
                                If (Me._Manifold.currNumber = 0) Then
                                    Console.WriteLine("the number is empty")

                                Else
                                    Me._agentCamera.setCurrCamera(Me._Manifold.currNumber)

                                End If
                                

                            End If
                            'Me.pnlCamImage.Invalidate()

                        End If
                    End If
                End SyncLock

            End If
        Catch ex As Exception
            Debug.WriteLine(ex.ToString)

        End Try

    End Sub

    Private Delegate Sub PoseUpdateHandler(ByVal pose As Math.Pose2D)
    Public Sub NotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D) Implements IAgentObserver.NotifyPoseEstimateUpdated

        'make sure we are on the UI thread
        If Me.InvokeRequired Then
            Dim handler As New PoseUpdateHandler(AddressOf Me.NotifyPoseEstimateUpdated)
            Me.BeginInvoke(handler, pose)

        Else
            Dim estimate As String = pose.ToString(1)
            If Not IsNothing(Me._Agent) Then
                Dim truth As String = String.Format("{0:f1} , {1:f1}, {2:f1}", Me._Agent.GroundTruthPose.X, Me._Agent.GroundTruthPose.Y, Me._Agent.GroundTruthPose.Z)
                Me.lblPose.Text = String.Format("{0} ({1}) m", estimate, truth)

                ' Me.lblPower.Text = String.Format("{0} secs", Me._Agent.Status.Battery)
                'Me.lblLight.Text = DirectCast(IIf(Me._Agent.Status.LightToggle, "On", "Off"), String)
            End If

        End If
    End Sub

    Private Delegate Sub AgentAlertHandler(ByVal alert As String)
    Public Sub NotifyAlertReceived(ByVal alert As String) Implements IAgentObserver.NotifyAlertReceived

        'make sure we are on the UI thread
        If Me.InvokeRequired Then
            Dim handler As New AgentAlertHandler(AddressOf Me.NotifyAlertReceived)
            Me.BeginInvoke(handler, alert)

        Else
            ' Me.lblAlert.Text = "Alert: " + alert
            ' Me.lblAlert.Visible = True

        End If
    End Sub

    Private Delegate Sub AgentStatusChangedHandler()
    Public Sub NotifyAgentStatusChanged() Implements IAgentObserver.NotifyAgentStarted, IAgentObserver.NotifyAgentSpawned, IAgentObserver.NotifyAgentPaused, IAgentObserver.NotifyAgentResumed, IAgentObserver.NotifyAgentStopped

        'make sure we are on the UI thread
        If Me.InvokeRequired Then
            Dim handler As New AgentStatusChangedHandler(AddressOf Me.NotifyAgentStatusChanged)
            Me.BeginInvoke(handler)

        Else

            Me.ResetActionButton()
            Me.ResetLabels()

        End If
    End Sub

    Private Delegate Sub StrengthReceivedHandler(ByVal pathloss As Double)
    Public Sub NotifySignalStrengthReceived(ByVal pathloss As Double) Implements IAgentObserver.NotifySignalStrengthReceived

        'make sure we are on the UI thread
        If Me.InvokeRequired Then
            Dim handler As New StrengthReceivedHandler(AddressOf Me.NotifySignalStrengthReceived)
            Me.BeginInvoke(handler, pathloss)

        Else

            Me._Pathloss = String.Format(" {0} dBm", pathloss)
            Me.ResetLabels()

        End If

    End Sub

#End Region

#Region " Changing Behavior "
    Private Sub LoadBehaviors()
        Dim behaviors As Array = System.Enum.GetNames(GetType(BehaviorType))
        Dim behavior As String
        For Each behavior In behaviors
            ' Me.BehaviorDisplay.Items.Add(behavior)
        Next
    End Sub

    Private Sub ShowCurrentBehavior(ByVal currBehavior As String)
        If TypeOf Me._Agent Is UsarOperatorAgent Then
            'Me.BehaviorDisplay.Visible = False
        Else 'TypeOf Me._Agent Is UsarProxyAgent Or TypeOf Me._Agent Is UsarAgent Then
            'Me.BehaviorDisplay.Visible = True
        End If
        'Me.BehaviorDisplay.SelectedItem = currBehavior
    End Sub

    Private Sub BehaviorDisplay_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim newBehavior As String
        'If Me.BehaviorDisplay.SelectedItem Is Nothing Then
        'newBehavior = "TeleOperation"
        'Else
        'newBehavior = Me.BehaviorDisplay.SelectedItem.ToString
        'End If
        If Not (newBehavior = Me._AgentConfig.BehaviorMode) Then
            Me._AgentConfig.BehaviorMode = newBehavior
            If Not IsNothing(Me._Agent) Then
                If Not IsNothing(Me._UsarOperator) AndAlso Not IsNothing(Me._UsarOperator.CommActor) AndAlso TypeOf Me._Agent Is UsarProxyAgent Then
                    Me._UsarOperator.CommActor.RelayBehaviorChangeCommand(Me._UsarOperator.Name, Me._Agent.Name, newBehavior)
                Else
                    Me._Agent.ChangeBehaviour(newBehavior)
                End If
            End If
            Me.ShowCurrentBehavior(newBehavior)
        End If
    End Sub
#End Region

#Region " Driving "

    Private Sub ResetDrivePanel()
        Dim enable As Boolean = True
        enable = enable AndAlso Not IsNothing(Me._Agent)
        enable = enable AndAlso Me._Agent.IsRunning

        If enable Then
            Me.btnDrive.Enabled = True
        Else
            Me.btnDrive.Enabled = False
        End If

    End Sub

    Private Sub btnDrive_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDrive.Click
        Me.SuspendLayout()
        Me.pnlControl.Hide()
        If Me.Agent.RobotModel.Equals(New String(CType("AirRobot", Char()))) Then
            Me.pnlFly.Show()
        ElseIf Me.Agent.RobotModel.Equals(New String(CType("kenaf", Char()))) Then
            Me.pnlKenaf.Show()

        ElseIf Me.Agent.RobotModel.Equals(New String(CType("Nao", Char()))) Then
            Me.pnlKenaf.Show()

        Else
            Me.pnlDrive.Show()
        End If
        Me.pnlImgControl.Show()

        Me.Focus()
        Me.ResumeLayout(True)

        'NICK
        Me.Agent.ShowImages = True
        CamOn()
        Me.Agent.CamReq()
    End Sub

    Private Sub btnCancelDrive_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancelDrive.Click
        'NICK
        Me.Agent.ShowImages = False
        CamOff()

        Me.SuspendLayout()
        Me.pnlDrive.Hide()
        Me.pnlImgControl.Hide()

        Me.pnlControl.Show()
        Me.ResumeLayout(True)
    End Sub

    Private Sub btnHeadLight_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHeadLight.Click
        Me.Agent.ToggleHeadLight()
    End Sub

    Private _Speed As Single = 0.6F

    Private Sub ResetSpeedLabel()
        Me.lblSpeed.Text = String.Format("{0:f2} rad/s", Me._Speed)
    End Sub

    'Private Sub btnSpeedUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSpeedUp.Click
    '   Me._Speed += 0.25F
    'During setup the Agent is not yet spawn
    '  If Not IsNothing(Me.Agent) AndAlso Me._Speed > Me.Agent.MaxSpeed() Then
    '     Me._Speed = Me.Agent.MaxSpeed()
    'End If
    'Me.ResetSpeedLabel()
    'End Sub
    'Private Sub btnSpeedDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSpeedDown.Click

    '   If Me._Speed > 0.25F Then
    '      Me._Speed -= 0.25F
    ' Else
    '    Me._Speed = Me._Speed * 0.8F 'Slowly towards 0.0
    'End If
    'Me.ResetSpeedLabel()
    'End Sub

    Private counter As Integer = 0
    Private Sub btnForward_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnForward.Click
        counter = counter + 1
        If Not Me.Agent Is Nothing Then
            Me.Agent.Drive(CSng(counter * (0.5) + Me._Speed))
        End If
    End Sub
    Private Sub btnReverse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReverse.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.Reverse(1.5)
            counter1 = 0
            counter = 0
            counter2 = 0
        End If
    End Sub
    Private Sub btnStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStop.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.Halt()
            Me.Focus()
            counter1 = 0
            counter = 0
            counter2 = 0
            'Me.Agent.CamReq()
        End If
    End Sub
    Private counter1 As Integer = 0
    Private Sub btnTurnLeft_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTurnLeft.Click
        counter1 = counter1 + 1
        If Not Me.Agent Is Nothing Then
            '0.5
            Me.Agent.TurnLeft(CSng(counter1 * 0.2))
            'Me.Agent.StrafeLeft(Me._Speed)
        End If
    End Sub
    Private counter2 As Integer = 0
    Private Sub btnTurnRight_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTurnRight.Click
        counter2 = counter2 + 1
        If Not Me.Agent Is Nothing Then
            '0.5
            Me.Agent.TurnRight(CSng(counter2 * 0.2))
        End If
    End Sub

    Private Sub btnFrontLeft_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFrontLeft.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.DifferentialDrive(0.6, +0.2F)
            'Me.Agent.TurnLeft(0.05)
            'Me.Agent.StrafeLeft(Me._Speed)
        End If
    End Sub
    Private Sub btnFrontRight_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFrontRight.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.DifferentialDrive(0.6, -0.2F)
            'Me.Agent.TurnRight(0.05)
        End If
    End Sub

    Private Sub btnFlip_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnFlip.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.Flip()
        End If
    End Sub

    Private Sub btnSync_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSync.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.Sync()
        End If
    End Sub

    'Private Sub alertAcknowledged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BehaviorDisplay.SelectedIndexChanged, btnForward.Click, btnReverse.Click, btnTurnLeft.Click, btnTurnRight.Click, btnFrontRight.Click, btnFrontLeft.Click
    '   Me.lblAlert.Visible = False
    '  If Not Me.Agent Is Nothing Then
    '     Me.Agent.MotionTimeStamp = DateTime.Now
    'End If
    'End Sub

#End Region

#Region " Flying "
    Private Sub btnCancelFly_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancelFly.Click
        Me.SuspendLayout()
        Me.pnlFly.Hide()
        Me.pnlControl.Show()
        Me.ResumeLayout(True)
    End Sub

    Private Sub btnFlyHalt_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyHalt.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.FlyHalt()
            Me.Focus()
        End If
    End Sub

    Private Sub btnFlyUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyUp.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.FlyUp(Me._Speed)
        End If
    End Sub

    Private Sub btnDownUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyDown.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.FlyDown(Me._Speed)
        End If
    End Sub

    Private Sub btnFlyForward_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyForward.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.Forward(Me._Speed)
        End If
    End Sub

    Private Sub btnFlyBackward_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyBackward.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.Backward(Me._Speed)
        End If
    End Sub

    Private Sub btnFlyStrafeLeft_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyStrafeLeft.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.StrafeLeft(Me._Speed)
        End If
    End Sub
    Private Sub btnFlyStrafeRight_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyStrafeRight.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.StrafeRight(Me._Speed)
        End If
    End Sub

    Private Sub btnFlyTurnLeft_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyTurnLeft.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.FlyTurnLeft(Me._Speed / 10)
        End If
    End Sub
    Private Sub btnFlyTurnRight_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyTurnRight.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.FlyTurnRight(Me._Speed / 10)
        End If
    End Sub

    Private Sub btnFlyTarget_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyTarget.Click
        'Julian RoboCup 2009 Temp commented out.  Want this button to take pictures.  Only for this competition.
        Me.btnCamReq_Click(sender, e)

        'If Not Me.Agent Is Nothing Then
        ' Me.addTarget()
        'End If
    End Sub


#End Region


#Region " Nao "
    Private Sub btnCancelKenaf_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancelFly.Click, btnCancelKenaf.Click
        Me.SuspendLayout()
        Me.pnlKenaf.Hide()
        Me.pnlControl.Show()
        Me.ResumeLayout(True)
    End Sub

    Private Sub btnKenafHalt_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyHalt.Click, btnKenafHalt.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.Halt()
            counter3 = 0
            counter4 = 0
            Me.Focus()
        End If
    End Sub
    Private counter3 As Integer = 0
    Private Sub btnKenafFrontUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyUp.Click, HeadUp.Click
        If Not Me.Agent Is Nothing Then
            ' Console.WriteLine("headup")
            Me.Agent.HeadYaw(CSng((0.2) * counter3 + counter3))
            counter = counter + 1
        Else
        End If


    End Sub

    Private counter4 As Integer = 0
    Private Sub btnKenafFrontDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyDown.Click, HeadDown.Click
        If Not Me.Agent Is Nothing Then

            Me.Agent.HeadYaw(CSng((counter4) * (-0.2) - 0.2))
        Else
            Console.WriteLine("headdown")
        End If
    End Sub

    Private Sub btnKenafRearUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyUp.Click, HeadLeft.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.HeadPitch(0.2)

        End If
    End Sub

    Private Sub btnKenafRearDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyDown.Click, HeadRight.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.RearDown()
        End If
    End Sub

    Private Sub btnKenafForward_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyForward.Click, btnKenafForward.Click
        If Not Me.Agent Is Nothing Then
            ' Kenaf is slow, so multiply by two
            Me.Agent.Drive(2 * Me._Speed)
        End If
    End Sub

    Private Sub btnKenafBackward_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyBackward.Click, btnKenafBackward.Click
        If Not Me.Agent Is Nothing Then
            ' Kenaf is slow, so multiply by two
            Me.Agent.Reverse(2 * Me._Speed)
        End If
    End Sub

    Private Sub btnKenafLeft_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyStrafeLeft.Click, btnKenafLeft.Click
        If Not Me.Agent Is Nothing Then
            ' Kenaf is slow, so multiply by two
            Me.Agent.TurnLeft(2 * Me._Speed)
        End If
    End Sub
    Private Sub btnKenafRight_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFlyStrafeRight.Click, btnKenafRight.Click
        If Not Me.Agent Is Nothing Then
            ' Kenaf is slow, so multiply by two
            Me.Agent.TurnRight(2 * Me._Speed)
        End If
    End Sub

    Private Sub btnCamReqKenaf_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCamReqKenaf.Click
        ' Julian Robocup 09 temp fix.  Want this button to take picture.  
        'Can also be used to toggle cameras when there are more than one on a robot, see code at bottom.
        Me.btnCamReq_Click(sender, e)

        'If Not Me.Agent Is Nothing Then
        ' Me.Agent.ToggleMultiView()
        ' End If
    End Sub



#End Region

#Region " Camera Control Functionality "

    'NICK
    Private _ShowCameraImages As Boolean = False

    Private Sub btnCamToggle_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If (Me.Agent.ShowImages) Then
            Me._ShowCameraImages = False
            'Me.btnCamToggle.Image = My.Resources.CameraOff
            CamOff()
        Else
            Me._ShowCameraImages = True
            'Me.btnCamToggle.Image = My.Resources.CameraOn
            CamOn()
        End If
        Me.Agent.ToggleShowImages(Me.Agent.ShowImages)
    End Sub

    Private Sub CamOff()
        Me.Agent.ShowImages = False
        Me._ShowCameraImages = False
        'Me.btnCamToggle.Image = My.Resources.CameraOff
        Me.pnlCamImage.BackgroundImage = Nothing
    End Sub

    Private Sub CamOn()
        Me._ShowCameraImages = True
        Me.Agent.ShowImages = True
        'Me.btnCamToggle.Image = My.Resources.CameraOn
    End Sub

    Private _camImgIndex As Integer = 1
    Private Sub btnCamReq_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCamReq.Click
        If Not Me.Agent Is Nothing Then
            Me.Agent.CamReq()

            Me._ShowCameraImages = True
            'Me.btnCamToggle.Image = My.Resources.CameraOn
            CamOn()
        End If
        Me.Agent.ReleaseRFIDTagFly()
        If (Me.Agent.ShowImages) Then
            ' Me.Agent.CamRepl()

        End If


        'conv.sendtext(Me.Agent.PoseEstimate)

        If Not IsNothing(Me.pnlCamImage.BackgroundImage) Then
            Dim camIm As System.Drawing.Image = Me.pnlCamImage.BackgroundImage
            Dim filename As String
            Dim pathName As String = Path.Combine(My.Computer.FileSystem.SpecialDirectories.MyDocuments, "USARSim\Results\View\")
            Dim fileInfo As New FileInfo(pathName)
            If Not fileInfo.Directory.Exists Then
                fileInfo.Directory.Create()
            End If
            If Not IsNothing(Me.Agent) Then 'Me.Agent is nothing if the button is clicked at the end of the run.
                Dim currPose As Pose2D = Me.Agent.CurrentPoseEstimate
                filename = Path.Combine(pathName, Me._camImgIndex.ToString + "_" + Me.Agent.Name + "_" + CInt(currPose.X / 100).ToString + "_" + CInt(currPose.Y / 100).ToString + "_" + CInt(currPose.Rotation * 180 / System.Math.PI).ToString + ".jpg")
            Else
                filename = Path.Combine(pathName, Me._camImgIndex.ToString + ".jpg")
            End If

            camIm.Save(filename, Drawing.Imaging.ImageFormat.Jpeg)
            Me._camImgIndex += 1
        End If
    End Sub

    Private Sub btnCamUp_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        If Not Me.Agent Is Nothing Then
            Me.Agent.CamUp()
        End If
    End Sub

    Private Sub btnCamDown_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        If Not Me.Agent Is Nothing Then
            Me.Agent.CamDown()
        End If
    End Sub

    Private Sub btnFlyCamUp_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles btnFlyCamUp.MouseDown
        If Not Me.Agent Is Nothing Then
            Me.Agent.FlyCamUp()
        End If
    End Sub

    Private Sub btnFlyCamDown_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles btnFlyCamDown.MouseDown
        If Not Me.Agent Is Nothing Then
            Me.Agent.FlyCamDown()
        End If
    End Sub

    '    Private Sub btnCamStopMovement(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles btnCamUp.MouseUp, btnCamDown.MouseUp
    '        If Not Me.Agent Is Nothing Then
    '            Me.Agent.CamStop()
    '        End If
    '    End Sub
#End Region

#Region "Tracker drawing functions"
    Private Delegate Sub trackUpdateHandler(ByVal displayImage As Image)
    Public Sub NotifyTrackerImage(ByVal displayImage As Image) Implements IAgentObserver.NotifyTrackerImage
        If Me.InvokeRequired Then
            Dim handler As New trackUpdateHandler(AddressOf Me.NotifyTrackerImage)
            Me.BeginInvoke(handler, displayImage)
        Else
        End If

    End Sub
    Private Sub btnStartTracker_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Agent.toggleTracker()
        Return

        'previous code
        'Activate tracker boolean in the usarFollowAgent
        'Get USARFollowAgent object, and call function
        If TypeOf Me._Agent Is UsarSkinDetAgent Then
            DirectCast(Me._Agent, UsarSkinDetAgent).activateTracker()
        End If
    End Sub

#End Region

#Region " Keyboard input for flying the AirRobot "

    Protected Overrides Function ProcessCmdKey(ByRef msg As System.Windows.Forms.Message, ByVal keyData As System.Windows.Forms.Keys) As Boolean
        If Not Me.Agent Is Nothing Then

            'Fixed keyboard controls when conservative behavior causes the agent to halt
            Me.Agent.MotionTimeStamp = DateTime.Now

            Select Case keyData

                Case Keys.Left
                    Me.Agent.TurnLeft(0.5)
                    Return True

                Case Keys.Right
                    Me.Agent.TurnRight(0.5)
                    Return True

                Case Keys.Up
                    Me.Agent.Drive(Me._Speed)
                    Return True

                Case Keys.Down
                    Me.Agent.Reverse(1.5)
                    Return True

                Case Keys.Escape
                    Me.Agent.Halt()
                    Me.Focus()

                Case Keys.S
                    Me._Speed += 0.25F
                    'During setup the Agent is not yet spawn
                    If Me._Speed > Me.Agent.MaxSpeed() Then
                        Me._Speed = Me.Agent.MaxSpeed()
                    End If
                    Me.ResetSpeedLabel()
                    Return True

                Case Keys.OemMinus
                    If Me._Speed > 0.25F Then
                        Me._Speed -= 0.25F
                    Else
                        Me._Speed = Me._Speed * 0.8F 'Slowly towards 0.0
                    End If
                    Me.ResetSpeedLabel()
                    Return True

                Case Else
                    Return MyBase.ProcessCmdKey(msg, keyData)

            End Select

        End If

    End Function 'ProcessCmdKey 


#End Region


#Region " Image Adjustment Scrollbar "

    Private Sub ImgBar_Brightness_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles ImgBar_Brightness.Scroll
        Me.Agent.CamBrightness = ImgBar_Brightness.Value
    End Sub
    Private Sub ImgBar_Contrast_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles ImgBar_Contrast.Scroll
        Me.Agent.CamContrast = ImgBar_Contrast.Value
    End Sub
#End Region

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)


    End Sub

    Private Sub pnlCamImage_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles pnlCamImage.Paint

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)



    End Sub

    Private Sub lblInfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub pnlStatus_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles pnlStatus.Paint

    End Sub

    Private Sub lblAlert_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub





    Private Sub lblTitle_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblTitle.Click

    End Sub

    Private Sub AgentController_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub lblPower_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub Button1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs)
        'Me._agentCamera.setCurrCamera(Integer.Parse(TextBox1.Text()))
    End Sub

    Private Sub TextBox1_TextChanged_1(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub TextBox1_TextChanged_2(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub Button1_Click_2(ByVal sender As System.Object, ByVal e As System.EventArgs)








    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged

        'If (Me._Manifold._changeMotion.ContainsKey(Me.Agent.Number)) Then
        If (Me._Manifold._changeMotion(Me.Agent.Number) = False) Then
            Me._Manifold._changeMotion(Me.Agent.Number) = True
        Else
            Me._Manifold._changeMotion(Me.Agent.Number) = False

            'End If

            'Else
            '   Me._Manifold._changeMotion.Add(Me.Agent.Number, True)


        End If




    End Sub

    Private Sub TextBox1_TextChanged_3(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub TrackBar1_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar1.Scroll
        Dim temp As Double = 0.6
        If (temp + (Me.TrackBar1.Value / 10) < Me.Agent.MaxSpeed) Then
            Me._Speed = CSng(temp + (Me.TrackBar1.Value / 10))
        Else
            Me._Speed = Me.Agent.MaxSpeed
        End If
        Me.ResetSpeedLabel()
    End Sub

    Private Sub lblPose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblPose.Click

    End Sub
End Class
