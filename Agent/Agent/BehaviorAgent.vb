Imports UvARescue.Math

Public Class BehaviorAgent
    Inherits CommAgent

#Region " Constructor "

    Public Sub New(ByVal manifold As Manifold, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(manifold, name, agentConfig, teamConfig)

        Me._MotionControl = New MotionControl(Me)
        Me._BehaviorControl = New BehaviorControl(Me, Me._MotionControl)

        Me.ChangeBehaviour(agentConfig.BehaviorMode)

        Console.WriteLine(String.Format("[Behavior Agent] - Starts with {0}", AgentConfig.BehaviorMode))

    End Sub

    Public Sub New(ByVal agent As Agent)
        'no checks?
        Me.New(agent.Manifold, agent.Name, agent.AgentConfig, agent.TeamConfig)
    End Sub

#End Region

#Region "Properties"
    Public ReadOnly Property BehaviorBalance() As Double
        Get
            Return Me.AgentConfig.BehaviorBalance
        End Get
    End Property
#End Region

#Region " Behavior Control and Motion Control "

    Private _BehaviorControl As BehaviorControl
    Public ReadOnly Property BehaviorControl() As BehaviorControl
        Get
            Return Me._BehaviorControl
        End Get
    End Property

    Private _MotionControl As MotionControl
    Public ReadOnly Property MotionControl() As MotionControl
        Get
            Return Me._MotionControl
        End Get
    End Property



    Protected Friend Overrides Sub NotifySensorUpdate(ByVal sensor As Sensor)
        MyBase.NotifySensorUpdate(sensor)
        Me._BehaviorControl.NotifySensorUpdate(sensor)
        Me._MotionControl.NotifySensorUpdate(sensor)
    End Sub

    Public Overrides Sub NotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        MyBase.NotifyPoseEstimateUpdated(pose)
        Me._BehaviorControl.NotifyPoseEstimateUpdated(pose)
        Me._MotionControl.NotifyPoseEstimateUpdated(pose)
    End Sub

    Private _LastSynchronisation As New Dictionary(Of String, DateTime)

    Protected Friend Overrides Sub NotifyPoseEstimateReceived(ByVal agentName As String, ByVal pose As Pose2D)
        MyBase.NotifyPoseEstimateReceived(agentName, pose)

        If agentName = Me._OperatorName Then
            Return
        End If

        Dim timestamp As DateTime = Now

        If Not Me._LastSynchronisation.ContainsKey(agentName) Then
            SyncLock Me._LastSynchronisation
                Me._LastSynchronisation.Add(agentName, timestamp)
            End SyncLock
            Return
        End If

        Dim synchronisation_interval As Integer = 30 'seconds

        If timestamp - Me._LastSynchronisation(agentName) > TimeSpan.FromSeconds(synchronisation_interval) Then


            Me.CommActor.RelaySyncRequest(agentName)


            SyncLock Me._LastSynchronisation
                Me._LastSynchronisation(agentName) = timestamp
            End SyncLock

        End If

    End Sub

#End Region

#Region " Manage Currently Active Behavior "

    Protected Friend Overrides Sub NotifyAgentConnected(ByVal agentName As String)
        MyBase.NotifyAgentConnected(agentName)
        Me._BehaviorControl.NotifyAgentConnected(agentName)
        Me.CheckOperatorUplink()
    End Sub
    Protected Friend Overrides Sub NotifyAgentDisconnected(ByVal agentName As String)
        MyBase.NotifyAgentDisconnected(agentName)
        Me._BehaviorControl.NotifyAgentDisconnected(agentName)
        Me.CheckOperatorUplink()
    End Sub
#End Region


#Region " Connection To Operator "


    Dim linkMutex As New Object
    Private _LinkedToOperator As Boolean = False

    Protected Overridable Sub CheckOperatorUplink()

        SyncLock linkMutex
            Dim wasLinked As Boolean = Me._LinkedToOperator
            Dim nowLinked As Boolean = Me.IsConnectedToOperator()

            If Not wasLinked = nowLinked Then
                'something changed

                Me._LinkedToOperator = nowLinked

                If Not nowLinked Then
                    'connection with operator lost
                    Me.OnDisconnectedFromOperator()
                Else
                    'connection (re-)established
                    Me.OnConnectedToOperator()
                End If

            End If
        End SyncLock

    End Sub

    Protected Overridable Sub OnConnectedToOperator()
        Console.WriteLine(String.Format("[{0}] - Established connectivity with Operator, awaiting command", Me.Name))
        Me._BehaviorControl.NotifyOperatorConnected()
        'Me.Halt()
    End Sub
    Protected Overridable Sub OnDisconnectedFromOperator()
        Console.WriteLine(String.Format("[{0}] - Lost connectivity with Operator, initiated retreat", Me.Name))
        Me._BehaviorControl.NotifyOperatorDisconnected()
        'Me.Reverse(1.0F)
    End Sub

#End Region

#Region " Change behaviour "
    Public Overrides Sub ChangeBehaviour(ByVal newBehaviour As String)
        SyncLock Me.BehaviorControl
            Select Case newBehaviour
                Case "AutonomousExploration"
                    Me._BehaviorControl.SwitchToBehavior(BehaviorType.AutonomousExploration)
                Case "ConservativeTeleOp"
                    Me._BehaviorControl.SwitchToBehavior(BehaviorType.ConservativeTeleOp)
                    'Case "DeploymentTest"
                    '    Me._BehaviorControl.SwitchToBehavior(BehaviorType.DeploymentTest)
                    'Case "DriveCircle"
                    '   Me._BehaviorControl.SwitchToBehavior(BehaviorType.DriveCircle)
                    'Case "DriveSquare"
                    '   Me._BehaviorControl.SwitchToBehavior(BehaviorType.DriveSquare)
                    'Case "ExploreTraversibility"
                    '   Me._BehaviorControl.SwitchToBehavior(BehaviorType.ExploreTraversibility)
                    'Case "FollowCorridorBehaviorType"
                    '   Me._BehaviorControl.SwitchToBehavior(BehaviorType.FollowCorridorBehaviorType)
                Case "FollowWaypoint"
                    Me._BehaviorControl.SwitchToBehavior(BehaviorType.FollowWaypoint)
                    'Case "FollowPath"
                    '   Me._BehaviorControl.SwitchToBehavior(BehaviorType.FollowPath)
                    'Case "ObstacleAvoidanceBehaviorType"
                    '   Me._BehaviorControl.SwitchToBehavior(BehaviorType.ObstacleAvoidanceBehaviorType)
                Case "TeleOperation"
                    Me._BehaviorControl.SwitchToBehavior(BehaviorType.TeleOperation)
                Case Else
                    Console.WriteLine(String.Format("    [WARNING] Agent\Agent\BehaviorAgent.vb::ChangeBehaviour({0}) called, Case Else (TeleOperation fallback)", newBehaviour))
                    Me._BehaviorControl.SwitchToBehavior(BehaviorType.TeleOperation)
            End Select
        End SyncLock
    End Sub
#End Region

    Public Overrides Sub ClearAllTargets()
        Me._TargetLocations.Clear()
        Me._ResetCurrentPath = True
        'SendNewTarget(Me.Name, Nothing)
    End Sub



End Class
