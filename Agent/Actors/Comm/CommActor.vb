Imports UvARescue.Communication
Imports UvARescue.Math
Imports UvARescue.Tools

Imports System.Threading


Imports System.Net
Imports System.Net.Sockets

Public Class CommActor
    Inherits Actor
    Implements ICommOwner

    Public Shared ReadOnly ACTORTYPE_COMM As String = "Comm"

    'Protected Shared ReadOnly PATHLOSS_THRESHOLD As Single = -57.35 'dBm
    Protected Shared ReadOnly PATHLOSS_THRESHOLD As Single = -93 'dBm, wss cutoff 
    Protected Shared ReadOnly RESYNC_TIMEOUT As Integer = 1 'secs, time to wait to trigger next sync

#Region " Constructor "

    Public Sub New(ByVal teamConfig As TeamConfig, ByVal agentName As String, ByVal agentNumber As Integer, ByVal agentHost As String)
        MyBase.New(ACTORTYPE_COMM, ACTORTYPE_COMM)

        Me._CommDevice = New WssDevice(Me, teamConfig.WirelessServerHost, teamConfig.WirelessServerPort, teamConfig.OperatorName, teamConfig.TeamMembers, agentName, agentNumber, agentHost)

        Me._OperatorName = teamConfig.OperatorName
        Me._TeamMembers = New List(Of String)

        Dim members() As String = Strings.Split(teamConfig.TeamMembers, ",")
        For Each member As String In members
            Dim parts() As String = Strings.Split(member, "-")
            Dim name As String = member
            Me._TeamMembers.Add(name)
        Next

        ReDim Me._PathLossMatrix(Me._TeamMembers.Count - 1, Me._TeamMembers.Count - 1)
        For i As Integer = 0 To Me._PathLossMatrix.GetLength(0) - 1
            For j As Integer = 0 To Me._PathLossMatrix.GetLength(1) - 1
                If i = j Then
                    Me._PathLossMatrix(i, j) = Integer.MinValue
                End If
            Next
        Next

    End Sub

#End Region

#Region " CommDevice "

    Private _CommDevice As ICommDevice
    Public ReadOnly Property CommDevice() As ICommDevice
        Get
            Return Me._CommDevice
        End Get
    End Property

    Public Sub StartComm()
        Me.CommDevice.StartDevice()
    End Sub

    Public Sub StopComm()
        Me.CommDevice.StopDevice()
    End Sub

#End Region

#Region " Helper Vars "

    Private _AgentName As String
    Private _AgentIndex As Integer

    Private _Manifold As Manifold


    Protected Overrides Sub OnAgentChanged()
        MyBase.OnAgentChanged()
        If Not IsNothing(Me.Agent) Then

            Me._AgentName = Me.Agent.Name
            Me._AgentIndex = Me._TeamMembers.IndexOf(Me._AgentName)
            Me._Manifold = Me.Agent.Manifold

            If TypeOf Me.Agent Is CommAgent Then
                Me._CommAgent = DirectCast(Me.Agent, CommAgent)
            Else
                Me._CommAgent = Nothing
            End If

        Else
            Me._AgentName = String.Empty
            Me._AgentIndex = -1
            Me._CommAgent = Nothing
            Me._Manifold = Nothing

        End If
    End Sub

    Private _CommAgent As CommAgent = Nothing
    Protected ReadOnly Property CommAgent() As CommAgent
        Get
            Return Me._CommAgent
        End Get
    End Property

#End Region

#Region " Monitor Signal Strength to Team Members "
    'Defaults from ComServer.uc
    Protected Friend _eDo As Double = 2.0 'meter
    Protected Friend _eN As Double = 1.09 'dimensionless
    Protected Friend _ePdo As Double = -22.0 'dBm. Microwave power of sender measured at distance eDo
    Protected Friend _eCutoff As Double = -93.0 'dBm. Sensitivity of receiver
    Protected Friend _eMaxObs As Integer = 20 'number of Walls taken into account
    Protected Friend _eAttenFac As Double = 1.325 'dBm. Attenuation per wall
    'Protected Friend _eAttenFac As Double = 1.125 'dBm. Attenuation per bush

    Private _OperatorName As String
    Private _TeamMembers As List(Of String)
    Private _PathLossMatrix(,) As Double

    Public Sub NotifySignalStrengthReceived(ByVal toRobot As String, ByVal pathloss As Double) Implements Communication.ICommOwner.NotifySignalStrengthReceived
        Dim j As Integer = Me._TeamMembers.IndexOf(toRobot)


        SyncLock Me._PathLossMatrix
            Me._PathLossMatrix(Me._AgentIndex, j) = pathloss
            Me._PathLossMatrix(j, Me._AgentIndex) = pathloss
        End SyncLock

        If pathloss > PATHLOSS_THRESHOLD AndAlso Not Me.IsConnectedTo(toRobot) Then
            'to avoid each of both agents to open a connection, only the 
            'master will request the conversation


            If Me.IsMasterTo(toRobot) Then
                Me._CommDevice.RequestDNS(toRobot)
                'Me._CommDevice.RequestConversation(toRobot)
            End If

        End If

        'now that we know the other agent's name we can notify the operator
        If Not IsNothing(Me._CommAgent) Then
            Me._CommAgent.NotifySignalStrengthReceived(toRobot, pathloss)
        End If

    End Sub

    Public Function GetSignalStrengthToTeammate(ByVal toRobot As String) As Double
        Dim i As Integer = Me._TeamMembers.IndexOf(Me.Agent.Name)
        Dim j As Integer = Me._TeamMembers.IndexOf(toRobot)

        If Me.IsConnectedTo(toRobot) Then
            Return Me._PathLossMatrix(i, j)
        Else
            Return PATHLOSS_THRESHOLD
        End If
    End Function

    Public Function GetSignalStrengthToOperator(ByVal toRobot As String) As Double
        Dim i As Integer = Me._TeamMembers.IndexOf(Me._OperatorName)
        Dim j As Integer = Me._TeamMembers.IndexOf(toRobot)

        If Me.IsConnectedTo(toRobot) OrElse j = Me._AgentIndex Then
            Return Me._PathLossMatrix(i, j)
        Else
            Return PATHLOSS_THRESHOLD
        End If
    End Function

    Protected Overridable Function IsMasterTo(ByVal agentName As String) As Boolean
        Dim idx As Integer = Me._TeamMembers.IndexOf(agentName)
        Return Me._AgentIndex < idx
    End Function

    Protected Function IsSlaveTo(ByVal agentName As String) As Boolean
        Return Not Me.IsMasterTo(agentName)
    End Function

#End Region

#Region " Connection Management "

    Sub NotifyDNSReceived(ByVal toRobot As String, ByVal port As Integer) Implements ICommOwner.NotifyDNSReceived

        ' this is where "Robot A connects with a standard TCP socket to the returned port."
        'If Me.IsMasterTo(toRobot) Then

        'Handeled by WssDevice._Listener.AcceptTcpClient

        'Else

        'Connect to the specified port
        Console.WriteLine(String.Format("[CommActor] Received DNS port {0} from robot {1}", port, toRobot))


        'Look if this one is needed, Arnoud
        If Not IsConnectedTo(toRobot) Then
            Dim connection As New TcpConnection
            connection.Connect(Me._CommDevice.Host, port)

            'Look if this one is needed, Arnoud
            'Console.WriteLine("NICK: NotifyDNSReceived > opening conversation")
            Me._CommDevice.OpenConversation(connection.GetClient, toRobot, False)

            'NICK: additional conversation for camera images

            'NICK: dont know if we need this.. but sometimes the second conversation is not marked as cam conversation
            'Thread.Sleep(TimeSpan.FromSeconds(1.0))

            Dim connectionCam As New TcpConnection
            connectionCam.Connect(Me._CommDevice.Host, port)
            'Console.WriteLine("NICK: NotifyDNSReceived > opening CAM conversation")
            Me._CommDevice.OpenConversation(connectionCam.GetClient, toRobot, True)

        End If




        'End If

        'Now this has to be assigned to a conversation.
    End Sub

    'Private _ConnectedTo As New Dictionary(Of Guid, String)
    Private _ConnectedTo As New Dictionary(Of String, Integer())

    Protected Function IsConnectedTo(ByVal agentName As String) As Boolean
        Dim found As Boolean = False
        SyncLock Me._ConnectedTo
            found = Me._ConnectedTo.ContainsKey(agentName)
        End SyncLock
        'If found Then
        ' Console.WriteLine(String.Format("Connected to {0}", agentName))
        ' Else
        'Console.WriteLine(String.Format("Not connected to {0}", agentName))
        'End If
        Return found
    End Function

    'NICK
    Protected Sub StoreConnectedTo(ByVal conversation As ICommConversation, ByVal agentName As String)
        Dim conversationID As Integer = conversation.ConversationID
        Dim cam As Boolean = conversation.ConversationCam
        Dim notify As Boolean = False

        SyncLock Me._ConnectedTo
            'NICK
            If Not Me._ConnectedTo.ContainsKey(agentName) Then
                Dim InitialArray As Integer() = {conversationID, -1}
                Me._ConnectedTo.Add(agentName, InitialArray)
                'Console.WriteLine(String.Format("Adding conversation to {0}", agentName))
                notify = True
            ElseIf cam Then
                If Not Me._ConnectedTo(agentName)(1) = conversationID Then
                    Me._ConnectedTo(agentName)(1) = conversationID
                    'Console.WriteLine(String.Format("Updating conversation to {0}", agentName))
                    notify = True
                End If
            ElseIf Not cam Then
                If Not Me._ConnectedTo(agentName)(0) = conversationID Then
                    Me._ConnectedTo(agentName)(0) = conversationID
                    'Console.WriteLine(String.Format("Updating conversation to {0}", agentName))
                    notify = True
                End If
            End If
        End SyncLock
        '     Console.WriteLine(String.Format("Currently {0} conversations", Me._ConnectedTo.Count))


        'now that we know the other agent's name we can notify the operator
        If notify AndAlso Not IsNothing(Me._CommAgent) Then
            Me._CommAgent.NotifyAgentConnected(agentName)
        End If

    End Sub

    Protected Sub ClearConnectedTo(ByVal conversationID As Integer)
        Dim agentName As String = ""
        SyncLock Me._ConnectedTo
            For Each key As String In Me._ConnectedTo.Keys
                'NICK
                If Me._ConnectedTo(key)(0) = conversationID Then
                    key = agentName
                    Exit For
                End If
            Next
            'If Me._ConnectedTo.ContainsKey(conversationID) Then
            'agentName = Me._ConnectedTo(conversationID)
            'Me._ConnectedTo.Remove(conversationID)
            'End If
        End SyncLock

        'notify operator
        If Not String.IsNullOrEmpty(agentName) AndAlso Not IsNothing(Me._CommAgent) Then
            Me._CommAgent.NotifyAgentDisconnected(agentName)
        End If

    End Sub

    Protected Function GetConnectedAgentName(ByVal conversationID As Integer) As String
        SyncLock Me._ConnectedTo
            For Each key As String In Me._ConnectedTo.Keys
                'NICK: agents name not found in the regular connections.. Also check in the cam connections
                If Me._ConnectedTo(key)(0) = conversationID Or Me._ConnectedTo(key)(1) = conversationID Then
                    Return key
                End If

            Next
            'If Me._ConnectedTo.ContainsKey(conversationID) Then
            'Return Me._ConnectedTo(conversationID)
            'End If

        End SyncLock
        Return String.Empty
    End Function

    Protected Function GetConversation(ByVal agentName As String, Optional ByVal useCam As Boolean = False) As ICommConversation
        Dim conversationID As Integer = -1

        SyncLock Me._ConnectedTo
            For Each key As String In Me._ConnectedTo.Keys
                If key = agentName Then

                    'Console.WriteLine("GetConversation: {0}, {1}", Me._ConnectedTo(key)(0), Me._ConnectedTo(key)(1))

                    If useCam Then
                        conversationID = Me._ConnectedTo(key)(1)
                    Else
                        conversationID = Me._ConnectedTo(key)(0)
                    End If
                    Exit For
                End If
            Next
            'For Each key As Guid In Me._ConnectedTo.Keys
            'If Me._ConnectedTo(key) = agentName Then
            'conversationID = key
            'Exit For
            'End If
            'Next
        End SyncLock

        Dim conversation As ICommConversation = Nothing
        If Not conversationID = -1 Then
            SyncLock Me._Conversations
                If Me._Conversations.ContainsKey(conversationID) Then
                    conversation = Me._Conversations(conversationID)
                End If
            End SyncLock
        End If

        Return conversation

    End Function

    Private _Conversations As New Dictionary(Of Integer, ICommConversation)

    Public Sub NotifyConversationStarted(ByVal conversation As ICommConversation, ByVal otherName As String) Implements Communication.ICommOwner.NotifyConversationStarted

        If Not Me._Conversations.ContainsKey(conversation.ConversationID) Then
            SyncLock Me._Conversations
                Me._Conversations.Add(conversation.ConversationID, conversation)
            End SyncLock
        Else
            Console.WriteLine("[CommActor] - Warning: conversation was already known")

        End If
        If otherName = String.Empty Then
            Console.WriteLine(String.Format("[Comm] - {0} sent greetings to [unknown yet]", Me._AgentName))
            conversation.SendMessage(New GreetingMessage(Me._AgentName, Me.Agent.CurrentPoseEstimate, conversation.ConversationCam))
        ElseIf Not otherName = "WSS" Then
            Console.WriteLine(String.Format("[Comm] - {0} sent greetings to {1}", Me._AgentName, otherName))
            conversation.SendMessage(New GreetingMessage(Me._AgentName, Me.Agent.CurrentPoseEstimate, conversation.ConversationCam))
            Me.StoreConnectedTo(conversation, otherName)
        End If

        'DON'T DO AUTOMATIC SYNCS, THIS BLOWS UP COMM
        'Console.WriteLine(String.Format("[Comm] - {0} sent sync start to [unknown yet]", Me._AgentName))
        'conversation.SendMessage(New SyncStartMessage(Me.Agent.CurrentPoseEstimate))

    End Sub

    Public Sub NotifyConversationStopped(ByVal conversation As ICommConversation) Implements Communication.ICommOwner.NotifyConversationStopped
        SyncLock Me._Conversations
            Me._Conversations.Remove(conversation.ConversationID)
        End SyncLock
        Me.ClearConnectedTo(conversation.ConversationID)
    End Sub


    Public Sub NotifyMessageReceived(ByVal conversation As ICommConversation, ByVal message As Communication.Message) Implements Communication.ICommOwner.NotifyMessageReceived
        Console.WriteLine("[CommActor] Receiving message.")
        Try
            'check for broadcast messages
            If TypeOf message Is BroadcastMessage Then
                Me.ReceiveBroadcastMessage(conversation, DirectCast(message, BroadcastMessage))
            Else
                Me.ReceiveRegularMessage(conversation, message)
            End If

        Catch ex As Exception
            Console.WriteLine("Error occurred while trying to handle message.")
            Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)

        End Try
    End Sub

    Protected Overridable Sub ReceiveRegularMessage(ByVal conversation As ICommConversation, ByVal message As Message)
        Select Case message.MessageID

            Case MessageID.Text
                Me.OnTextReceived(conversation, DirectCast(message, TextMessage))
            Case MessageID.Greeting
                Me.OnGreetingReceived(conversation, DirectCast(message, GreetingMessage))
            Case MessageID.ActorCommand
                Me.OnActorCommandReceived(conversation, DirectCast(message, ActorCommandMessage))
            Case MessageID.PoseUpdate
                Me.OnPoseUpdateReceived(conversation, DirectCast(message, PoseUpdateMessage))
            Case MessageID.BehaviorChange
                Me.OnBehaviorChangeReceived(conversation, DirectCast(message, BehaviorChangeMessage))


            Case MessageID.SyncStart
                Me.OnSyncStartReceived(conversation, DirectCast(message, SyncStartMessage))
            Case MessageID.SyncMaster
                Me.OnSyncMasterReceived(conversation, DirectCast(message, SyncMasterMessage))
            Case MessageID.SyncSlave
                Me.OnSyncSlaveReceived(conversation, DirectCast(message, SyncSlaveMessage))
            Case MessageID.SyncCommit
                Me.OnSyncCommitReceived(conversation, DirectCast(message, SyncCommitMessage))
            Case MessageID.CamReq
                Me.OnCamReqReceived(conversation, DirectCast(message, CamReqMessage))
            Case MessageID.CamRepl
                'Console.WriteLine("Received cam respons from conversation with cam: {0}", conversation.ConversationCam)
                Me.OnCamReplReceived(conversation, DirectCast(message, CamReplMessage))
            Case MessageID.NewTargetLocation
                Me.OnTargetLocationReceived(conversation, DirectCast(message, NewTargetLocationMessage))
            Case MessageID.TeammateSignalStrength
                Me.OnTeammateSignalStrengthReceived(conversation, DirectCast(message, TeammateSignalStrengthMessage))
            Case Else
                Console.WriteLine("[CommActor] Warning!  Could not determine type of messageID in ReceiveRegularMessage")
        End Select
    End Sub

    Protected Overridable Sub OnGreetingReceived(ByVal conversation As ICommConversation, ByVal message As GreetingMessage)
        'store the name of the robot connected to
        Dim otherName As String = message.AgentName
        'NICK
        If conversation.incoming Then
            conversation.ConversationCam = message.UseCamConnection
        End If
        Me.StoreConnectedTo(conversation, otherName)
        Console.WriteLine(String.Format("[CommActor] - {0} received greetings from {1}, Cam conversation: {2}, incoming: {3}", Me._AgentName, otherName, conversation.ConversationCam, conversation.incoming))

        'NICK
        If Not IsNothing(Me._CommAgent) AndAlso Not conversation.ConversationCam Then
            Me._CommAgent.NotifyPoseEstimateReceived(otherName, message.AgentPose)
        End If

    End Sub

    Protected Overridable Sub OnTextReceived(ByVal conversation As ICommConversation, ByVal text As TextMessage)
        '   Console.WriteLine(String.Format("[CommActor] - {0} received text '{1}'", Me._AgentName, text.Text))

        'not used atm
    End Sub

#End Region

#Region " Broadcasting Protocol "

    Protected Overridable Sub SendBroadcastMessage(ByVal msg As BroadcastMessage)
        'NICK
        Dim useCamConversation As Boolean = msg.UseCamConnection
        'If useCamConversation Then
        '    Console.WriteLine("this is a CAM SendBroadcastMessage")
        'End If

        Dim sent As Boolean = False
        If Not msg.ToEveryone Then
            'this message is for a single recipient only

            If Not msg.IsDeliveredTo(msg.Recipient) AndAlso Me.IsConnectedTo(msg.Recipient) Then
                'we are connected to the intended recipient
                Dim conversation As ICommConversation = Me.GetConversation(msg.Recipient, useCamConversation)

                If Not IsNothing(conversation) Then
                    conversation.SendMessage(msg)
                    'Console.WriteLine(String.Format("[Comm] - {0} sent bc-message from {1} to {2}", Me._AgentName, msg.Sender, msg.Recipient))
                    sent = True
                End If
            End If
        End If

        If Not sent Then
            'either this message is to everyone, or we are not connected to 
            'the intended recipient, or the connection was lost just before
            'we tried to send it, so send to all connected team-members 

            For Each otherName As String In Me._TeamMembers
                If (Not msg.IsDeliveredTo(otherName)) AndAlso Me.IsConnectedTo(otherName) Then

                    Dim conv As ICommConversation = Me.GetConversation(otherName, useCamConversation)

                    If Not IsNothing(conv) AndAlso useCamConversation = conv.ConversationCam Then
                        'Console.WriteLine(String.Format("[Comm] - {0} sent bc-message from {1} to {2}", Me._AgentName, msg.Sender, otherName))
                        conv.SendMessage(msg)
                    End If
                End If
            Next

        End If
    End Sub


    Private _LastReceived As New Dictionary(Of MessageID, DateTime)

    Protected Overridable Sub ReceiveBroadcastMessage(ByVal conversation As ICommConversation, ByVal msg As BroadcastMessage)
        Dim useCamConversation As Boolean = msg.UseCamConnection
        'If useCamConversation Then
        '    Console.WriteLine("this is a CAM ReceiveBroadcastMessage")
        'End If

        'mark this message as delivered to me, to avoid resends
        msg.MarkDelivered(Me._AgentName)

        'check timestamp
        SyncLock Me._LastReceived
            If Not Me._LastReceived.ContainsKey(msg.MessageID) Then
                Me._LastReceived.Add(msg.MessageID, msg.Timestamp)
            Else
                Dim last As DateTime = Me._LastReceived(msg.MessageID)
                If last > msg.Timestamp Then
                    'we already received a more recent version of this message
                    Exit Sub
                Else
                    'this message is more recent than the last one
                    'store the timestamp, and proceed as usual
                    Me._LastReceived(msg.MessageID) = msg.Timestamp
                End If
            End If
        End SyncLock

        If msg.ToEveryone OrElse msg.Recipient = Me._AgentName Then
            'this message (also) is for me! forward to regular message handling
            'Console.WriteLine(String.Format("[Comm] - bc-message from {0} to {1} reached destination: {2}", msg.Sender, msg.Recipient, Me._AgentName))
            Me.ReceiveRegularMessage(conversation, msg)
        End If

        If msg.ToEveryone OrElse (Not msg.IsDeliveredTo(msg.Recipient) AndAlso Not msg.Recipient = Me._AgentName) Then
            'this message needs further proceccessing

            Dim sent As Boolean = False
            If Not msg.ToEveryone AndAlso Me.IsConnectedTo(msg.Recipient) Then
                'nice! we are connected to the intended recipient
                'NICK
                Dim conv As ICommConversation = Me.GetConversation(msg.Recipient, useCamConversation)
                If Not IsNothing(conv) Then
                    'Console.WriteLine(String.Format("[Comm] - {0} forwarded bc-message from {1} to {2}", Me._AgentName, msg.Sender, msg.Recipient))
                    conv.SendMessage(msg)
                    sent = True
                End If
            End If

            If Not sent Then

                'either this message is to everyone, or we are not connected to 
                'the intended recipient, or the connection was lost just before
                'we tried to send it, so forward to all connected team-members 

                For Each otherName As String In Me._TeamMembers
                    If Not msg.IsDeliveredTo(otherName) AndAlso Me.IsConnectedTo(otherName) Then
                        'NICK
                        Dim conv As ICommConversation = Me.GetConversation(otherName, useCamConversation)
                        If Not IsNothing(conv) Then
                            'Console.WriteLine(String.Format("[Comm] - {0} forwarded bc-message from {1} to {2}", Me._AgentName, msg.Sender, otherName))
                            conv.SendMessage(msg)
                        End If
                    End If
                Next

            End If
        End If
    End Sub

#End Region

#Region " Relay and Receive Actor Commands and Pose Updates "

    Public Sub RelayActorCommand(ByVal toRobot As String, ByVal command As String)
        Me.SendBroadcastMessage(New ActorCommandMessage(Me._AgentName, toRobot, command))
    End Sub

    Protected Overridable Sub OnActorCommandReceived(ByVal conversation As ICommConversation, ByVal message As ActorCommandMessage)

        Dim otherName As String = Me.GetConnectedAgentName(conversation.ConversationID)
        Dim robotName As String = message.Sender
        'Console.WriteLine(String.Format("[Comm] - {0} received actor command from {1} through {2}", Me._AgentName, robotName, otherName))

        If Not IsNothing(Me._CommAgent) Then
            Me.Agent.MotionTimeStamp = DateTime.Now
            Me._CommAgent.NotifyActorCommandReceived(message.Command)
        End If

    End Sub


    Public Sub RelayPoseEstimateUpdate(ByVal pose As Math.Pose2D, ByVal connectedTo() As String)
        Me.SendBroadcastMessage(New PoseUpdateMessage(Me._AgentName, pose, connectedTo))
    End Sub

    Private _PoseUpdateCounter As Integer = 7 'first request after 3 updates

    Protected Overridable Sub OnPoseUpdateReceived(ByVal conversation As ICommConversation, ByVal message As PoseUpdateMessage)

        Dim otherName As String = Me.GetConnectedAgentName(conversation.ConversationID)
        Dim robotName As String = message.Sender
        'Console.WriteLine(String.Format("[CommActor] - {0} received pose update for {1} from {2}", Me._AgentName, robotName, otherName))

        If Not IsNothing(Me._CommAgent) Then
            Me._CommAgent.NotifyPoseEstimateReceived(robotName, message.Pose)
            Me._CommAgent.NotifyConnectedToReceived(robotName, message.ConnectedTo)
        End If

        If _PoseUpdateCounter < 10 Then
            _PoseUpdateCounter = _PoseUpdateCounter + 1
        Else
            _PoseUpdateCounter = 0
            conversation.SendMessage(New SyncStartMessage(Me.Agent.CurrentPoseEstimate))
        End If


    End Sub

#End Region

#Region " Manifold Synchronization "

    Public Sub RelaySyncRequest(ByVal toRobot As String)
        Dim conversation As ICommConversation = Me.GetConversation(toRobot)
        If Not IsNothing(conversation) Then
            'Console.WriteLine(String.Format("[Comm] - {0} starting sync with {1}", Me._AgentName, toRobot))
            conversation.SendMessage(New SyncStartMessage(Me.Agent.CurrentPoseEstimate))
        End If
    End Sub



    Private _LastConfirmedUpdate As New Dictionary(Of String, DateTime)

    Protected Overridable Sub OnSyncStartReceived(ByVal conversation As ICommConversation, ByVal message As SyncStartMessage)

        Dim otherName As String = Me.GetConnectedAgentName(conversation.ConversationID)
        'Console.WriteLine(String.Format("[Comm] - {0} received sync start from {1}", Me._AgentName, otherName))

        If Not IsNothing(Me._CommAgent) Then
            Me._CommAgent.NotifyPoseEstimateReceived(otherName, message.CurrentPose)
        End If

        'first the slave will sync the master
        If Me.IsSlaveTo(otherName) Then

            'get the timestamp of the last confirmed update 
            '(retrieved by a SyncCommit message)
            Dim lastUpdate As DateTime = DateTime.MinValue
            SyncLock Me._LastConfirmedUpdate
                If Me._LastConfirmedUpdate.ContainsKey(otherName) Then
                    lastUpdate = Me._LastConfirmedUpdate(otherName)
                End If
            End SyncLock

            'collect changes since last update
            Me._Manifold.AcquireReaderLock()
            Dim response As New SyncMasterMessage
            If Not Me.LoadDataForSyncDataMessage(lastUpdate, response, otherName) Then
                response = Nothing
            End If
            Me._Manifold.ReleaseReaderLock()

            'submit update
            If Not IsNothing(response) Then
                'Console.WriteLine(String.Format("[Comm]:OnSyncStartReceived - {0} sent sync update to {1} ({2} items since {3})", Me._AgentName, otherName, response.Data.Count, lastUpdate))
                conversation.SendMessage(response)
            End If


        End If

    End Sub

    Protected Overridable Sub OnSyncMasterReceived(ByVal conversation As ICommConversation, ByVal message As SyncMasterMessage)

        Dim otherName As String = Me.GetConnectedAgentName(conversation.ConversationID)

        'get the timestamp of the last confirmed update 
        '(retrieved by a SyncCommit message)
        Dim lastUpdate As DateTime = DateTime.MinValue
        SyncLock Me._LastConfirmedUpdate
            If Me._LastConfirmedUpdate.ContainsKey(otherName) Then
                lastUpdate = Me._LastConfirmedUpdate(otherName)
            End If
        End SyncLock

        If lastUpdate = DateTime.MinValue Then
            'Console.WriteLine(String.Format("[Comm] - {0} received first sync update from {1} at {2})", Me._AgentName, otherName, DateTime.Now))
        End If

        Me._Manifold.AcquireWriterLock()

        'before applying the sync update, create the response for the slave
        Dim response As New SyncSlaveMessage
        If Not Me.LoadDataForSyncDataMessage(lastUpdate, response, otherName) Then
            response = Nothing
        End If

        'within the same lock, copy all received items into the manifold
        'Console.WriteLine(String.Format("[Comm] - {0} received sync update from {1} ({2} items)", Me._AgentName, otherName, message.Data.Count))
        Dim memento As IMemento
        For Each item As Object In message.Data
            If TypeOf item Is IMemento Then
                memento = DirectCast(item, IMemento)
                memento.Restore(Me._Manifold)
            End If
        Next

        'get the timestamp that the slave must respond with upon commit
        If Not IsNothing(response) Then
            response.CommitTimestamp = DateTime.Now
        End If

        Me._Manifold.ReleaseWriterLock()

        'commit the changes to slave, so the slave knows what to send next time
        'Console.WriteLine(String.Format("[Comm] - {0} sent sync commit to {1} ({2})", Me._AgentName, otherName, message.CommitTimestamp))
        conversation.SendMessage(New SyncCommitMessage(message.CommitTimestamp, Me.Agent.CurrentPoseEstimate))

        'and now send sync-update to slave
        If Not IsNothing(response) Then
            'Console.WriteLine(String.Format("[Comm]:OnSyncMasterReceived - {0} sent sync update to {1} ({2} items since {3})", Me._AgentName, otherName, response.Data.Count, lastUpdate))
            conversation.SendMessage(response)
        End If

    End Sub

    Protected Overridable Sub OnSyncSlaveReceived(ByVal conversation As ICommConversation, ByVal message As SyncSlaveMessage)

        Dim otherName As String = Me.GetConnectedAgentName(conversation.ConversationID)

        Me._Manifold.AcquireWriterLock()

        'copy all received items into the manifold
        'Console.WriteLine(String.Format("[Comm] - {0} received sync update from {1} ({2} items)", Me._AgentName, otherName, message.Data.Count))
        Dim memento As IMemento
        For Each item As Object In message.Data
            If TypeOf item Is IMemento Then
                memento = DirectCast(item, IMemento)
                memento.Restore(Me._Manifold)
            End If
        Next

        'this data was received from master, and the sync protocol 
        'guarantees that the master was updated already, to avoid sending
        'back the recent changes in the next sync, update the timestamp for master
        Dim timestamp As DateTime = Now
        SyncLock Me._LastConfirmedUpdate
            If Me._LastConfirmedUpdate.ContainsKey(otherName) Then
                Me._LastConfirmedUpdate(otherName) = timestamp
            Else
                Me._LastConfirmedUpdate.Add(otherName, timestamp)
            End If
        End SyncLock

        Me._Manifold.ReleaseWriterLock()

        'now commit the recent changes to master, so that master knows what to send next time
        'Console.WriteLine(String.Format("[Comm] - {0} sent sync commit to {1} ({2})", Me._AgentName, otherName, message.CommitTimestamp))
        conversation.SendMessage(New SyncCommitMessage(message.CommitTimestamp, Me.Agent.CurrentPoseEstimate))

    End Sub

    Protected Overridable Sub OnSyncCommitReceived(ByVal conversation As ICommConversation, ByVal message As SyncCommitMessage)

        Dim otherName As String = Me.GetConnectedAgentName(conversation.ConversationID)
        Dim timestamp As DateTime = message.Timestamp

        Console.WriteLine(String.Format("[Comm] - {0} received sync commit from {1} ({2})", Me._AgentName, otherName, timestamp))

        If Not IsNothing(Me._CommAgent) Then
            Me._CommAgent.NotifyPoseEstimateReceived(otherName, message.CurrentPose)
        End If

        'store timestamp 
        SyncLock Me._LastConfirmedUpdate
            If Me._LastConfirmedUpdate.ContainsKey(otherName) Then
                Me._LastConfirmedUpdate(otherName) = timestamp
            Else
                Me._LastConfirmedUpdate.Add(otherName, timestamp)
            End If
        End SyncLock

    End Sub



    '    Protected Overridable Sub OnSyncRequestReceived(ByVal conversation As ICommConversation, ByVal message As SyncRequestMessage)

    '        'construct response message with updates since timestamp
    '        Dim msg As SyncResponseMessage = Me.CreateResponseMessage(message.Timestamp)

    '        'the master will cache the response for now, and send it later when 
    '        'the slave communicates that it's ready. What it does now instead is send
    '        'the previously cached request.
    '        'the slave will immediately send the response back to master.

    '        Dim name As String = Me.GetConnectedAgentName(conversation.ConversationID)
    '        If Me.IsMasterTo(name) Then

    '            'the master keeps the response in a cache
    '            Console.WriteLine(String.Format("[Comm] - {0} buffered response for {1}", Me._AgentName, name))
    '            SyncLock Me._Responses
    '                Me._Responses(conversation.ConversationID) = msg
    '            End SyncLock

    '            'and the previously cached request is sent instead
    '            Dim request As SyncRequestMessage = Nothing
    '            SyncLock Me._Requests
    '                If Me._Requests.ContainsKey(conversation.ConversationID) Then
    '                    request = Me._Requests(conversation.ConversationID)
    '                End If
    '            End SyncLock

    '            If Not IsNothing(request) Then
    '                Console.WriteLine(String.Format("[Comm] - {0} sent buffered request to {1} ({2})", Me._AgentName, name, request.Timestamp))
    '                conversation.SendMessage(request)
    '            End If

    '        Else
    '            'the slave will simply reply to the request by sending the response
    '            Console.WriteLine(String.Format("[Comm] - {0} sent response to {1} ({2})", Me._AgentName, name, msg.Data.Count))
    '            conversation.SendMessage(msg)

    '        End If

    '    End Sub

    '    Protected Overridable Sub OnSyncResponseReceived(ByVal conversation As ICommConversation, ByVal message As SyncResponseMessage)

    '        'to keep track of the most recent update we received
    '        'default to Now, so that we will at minimum communicate
    '        'the current timestamp back to the sender.
    '        Dim timestamp As DateTime = DateTime.MinValue

    '        'copy all received items into the manifold
    '        Dim memento As IMemento
    '        For Each item As Object In message.Data
    '            If TypeOf item Is IMemento Then

    '                memento = DirectCast(item, IMemento)
    '                memento.Restore(Me._Manifold)

    '                'keep track of most recent timestamp
    '                If memento.Timestamp > timestamp Then
    '                    timestamp = memento.Timestamp
    '                End If

    '            End If
    '        Next

    '        Dim name As String = Me.GetConnectedAgentName(conversation.ConversationID)

    '        'tell that we are ready processing the response
    '        Console.WriteLine(String.Format("[Comm] - {0} sent ready to {1}", Me._AgentName, name))
    '        conversation.SendMessage(New SyncReadyMessage(timestamp))

    '        'the master will now send back his previously cached response

    '        If Me.IsMasterTo(name) Then

    '            'we still had the response for slave in cache, now we will send it
    '            Dim response As SyncResponseMessage = Nothing
    '            SyncLock Me._Responses
    '                If Me._Responses.ContainsKey(conversation.ConversationID) Then
    '                    response = Me._Responses(conversation.ConversationID)
    '                End If
    '            End SyncLock

    '            If Not IsNothing(response) Then
    '                Console.WriteLine(String.Format("[Comm] - {0} sent buffered response to {1} ({2})", Me._AgentName, name, response.Data.Count))
    '                conversation.SendMessage(response)
    '            End If

    '        End If

    '    End Sub

    '    Protected Overridable Sub OnSyncReadyReceived(ByVal conversation As ICommConversation, ByVal message As SyncReadyMessage)

    '        Dim agentName As String = Me.GetConnectedAgentName(conversation.ConversationID)
    '        Console.WriteLine(String.Format("[Comm] - {0} retrieved ready from {1}", Me._AgentName, agentName))

    '        Dim timestamp As DateTime = message.Timestamp
    '        If timestamp > DateTime.MinValue Then

    '            'store timestamp 
    '            Console.WriteLine(String.Format("[Comm] - {0} stored timestamp for {1}: {2}", Me._AgentName, agentName, timestamp))

    '            'the next time we send a request we will only request for updates more recent
    '            'then this timestamp
    '            SyncLock Me._LastConfirmedUpdate
    '                If Me._LastConfirmedUpdate.ContainsKey(agentName) Then
    '                    Me._LastConfirmedUpdate(agentName) = timestamp
    '                Else
    '                    Me._LastConfirmedUpdate.Add(agentName, timestamp)
    '                End If
    '            End SyncLock

    '        End If


    '        'the master will wait for 5 secs and then trigger another sync-cycle
    '        If Me.IsMasterTo(agentName) Then

    '            'pause for a while
    '            Console.WriteLine(String.Format("--- [Comm] - {0} starts a new sync cycle with {1} in {2} secs", Me._AgentName, agentName, RESYNC_TIMEOUT))

    '            Thread.Sleep(TimeSpan.FromSeconds(RESYNC_TIMEOUT))

    '            'trigger another sync cycle
    '            Console.WriteLine(String.Format("[Comm] - {0} sent greetings to {1}", Me._AgentName, agentName))
    '            conversation.SendMessage(New GreetingMessage(Me._AgentName, Me.Agent.CurrentPoseEstimate))

    '        End If

    '    End Sub

#End Region

#Region " Camera "

    Public Sub RelayCamReq(ByVal toRobot As String)
        'Console.WriteLine(String.Format("[Comm] - Camera request send for {0}", Me._AgentName))
        Me.SendBroadcastMessage(New CamReqMessage(Me._AgentName, toRobot))
    End Sub

    '    Protected Overridable Sub OnCamRecReceived(ByVal conversation As ICommConversation, ByVal message As CamReqMessage)
    '   Dim otherName As String = Me.GetConnectedAgentName(conversation.ConversationID)
    '      Me.Agent.OnCamRecReceived(otherName)
    ' End Sub

    Protected Overridable Sub OnCamReqReceived(ByVal conversation As ICommConversation, ByVal message As CamReqMessage)
        'Console.WriteLine(String.Format("[Comm] - Camera request received by {0}", Me._AgentName))

        If Not IsNothing(Me.Agent) Then
            Me.Agent.OnCamReq()
        End If
        'Me.RelayCamRepl(Me._OperatorName, )

        'Dim otherName As String = Me.GetConnectedAgentName(conversation.ConversationID)

        'Me.Agent.OnCamReplReceived(otherName)
        'message._camIm.Save("D:\USARSim\Results\recTest.jpg", Drawing.Imaging.ImageFormat.Jpeg)
    End Sub

    Public Sub RelayCamRepl(ByVal fromRobot As String, ByVal camIm As System.Drawing.Bitmap)
        Me.SendBroadcastMessage(New CamReplMessage(fromRobot, Me._OperatorName, camIm))
        'Console.WriteLine(String.Format("[Comm] - Camera sent of width {0}", camIm.Width))
        'Console.WriteLine("Stalling cam socket now, check if regular messages still work")
        'Threading.Thread.Sleep(TimeSpan.FromSeconds(20))
        'Console.WriteLine("Stall complete")
    End Sub

    Protected Overridable Sub OnCamReplReceived(ByVal conversation As ICommConversation, ByVal message As CamReplMessage)
        If Not IsNothing(Me.Agent) Then
            Me.Agent.OnCamRepl(message.Sender, message._camIm)
        End If

        'Console.WriteLine("To be handled by OperatorAgent only, as only operator received cam images.")
    End Sub


#End Region

#Region " Target Locations "

    Public Sub RelayTargetLocation(ByVal toRobot As String, ByVal target As Pose2D)
        'Console.WriteLine("[Target] Actually Sending the target right now")
        Me.SendBroadcastMessage(New NewTargetLocationMessage(Me._AgentName, toRobot, target))
        'Console.WriteLine("[Target] Finished Sending the target")
    End Sub


    Protected Overridable Sub OnTargetLocationReceived(ByVal conversation As ICommConversation, ByVal message As NewTargetLocationMessage)
        'Console.WriteLine(String.Format("[Comm] - Camera request received by {0}", Me._AgentName))

        If Not IsNothing(Me.Agent) Then 'should also check if this is not a operatorAgent

            'NICK
            If IsNothing(message._Target) Then
                Me.Agent.ClearAllTargets()
            Else
                Me.Agent.AddNewTarget(message._Target)
            End If
        End If
    End Sub

#End Region

#Region " Relay and Receive Behavior Change commands "
    Public Sub RelayBehaviorChangeCommand(ByVal sender As String, ByVal receiver As String, ByVal newbehavior As String)
        Me.SendBroadcastMessage(New BehaviorChangeMessage(Me._AgentName, receiver, newbehavior))
    End Sub

    Protected Overridable Sub OnBehaviorChangeReceived(ByVal conversation As ICommConversation, ByVal message As BehaviorChangeMessage)

        Dim otherName As String = Me.GetConnectedAgentName(conversation.ConversationID)
        Dim newBehavior As String = message.Behavior
        'Console.WriteLine(String.Format("[Comm] - {0} told to change behavior to {1} via {2}", Me._AgentName, newBehavior, otherName))

        If Not IsNothing(Me._CommAgent) Then
            Me._CommAgent.ChangeBehaviour(newBehavior)
        End If

    End Sub
#End Region

#Region " Relay Teammate Signal Strength Messages "
    Public Sub RelayTeammateSignalStrength(ByVal sender As String, ByVal receiver As String, ByVal t As String, ByVal s As Double)
        Me.SendBroadcastMessage(New TeammateSignalStrengthMessage(Me._AgentName, receiver, t, s))
    End Sub

    Protected Overridable Sub OnTeammateSignalStrengthReceived(ByVal conversation As ICommConversation, ByVal message As TeammateSignalStrengthMessage)

        Dim otherName As String = Me.GetConnectedAgentName(conversation.ConversationID)
        Dim newSS As Double = message.SignalStrength
        
        If Not IsNothing(Me._CommAgent) Then
            Me._CommAgent.UpdateSignalStrengthMatrix(message.Sender, message.Teammate, message.SignalStrength)
        End If

    End Sub
#End Region

#Region " Helper Functions "

    ''' <summary>
    ''' Helper function that will construct a message with all changes
    ''' since the timestamp.
    ''' </summary>
    ''' <param name="lastUpdate"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function LoadDataForSyncDataMessage(ByVal lastUpdate As DateTime, ByVal msg As SyncDataMessage, ByVal OtherName As String) As Boolean

        Try

            'patches must go first, otherwise the relations cannot restore themselves

            For Each patch As Patch In Me._Manifold.Nodes
                If patch.Timestamp > lastUpdate AndAlso Not patch.AgentName = OtherName Then
                    msg.Data.Add(patch.CreateMemento)
                End If
            Next

            For Each relation As Relation In Me._Manifold.Links
                If relation.Timestamp > lastUpdate AndAlso Not relation.ToNode.AgentName = OtherName Then
                    msg.Data.Add(relation.CreateMemento)
                End If
            Next

            msg.CommitTimestamp = DateTime.Now

            Return True

        Catch ex As Exception
            Console.WriteLine(ex)
        End Try

        Return False

    End Function

#End Region

End Class
