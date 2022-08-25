Imports UvARescue.Tools

Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

''' <summary>
''' The central object that provides WSS capabilities to the Agent.
''' 
''' The WssDevice:
''' - holds a single TcpConnection to the WSS server to send 
''' all commands and requests
''' - holds a single WssListener on which it accepts incoming connections
''' on the listen-port that are initiated by other agents through the WSS.
''' - holds a single WssConversation for every currently active connection
''' with other agents through the WSS. Note: multiple conversations can
''' be active simultaneously.
''' </summary>
''' <remarks></remarks>
Public Class WssDevice
    Inherits RegularThread
    Implements ICommDevice

#Region " Constructor "

    Public Sub New( _
            ByVal owner As ICommOwner, _
            ByVal wssHost As String, _
            ByVal wssPort As Integer, _
            ByVal operatorName As String, _
            ByVal teamMembers As String, _
            ByVal agentName As String, _
            ByVal agentNumber As Integer, _
            ByVal agentHost As String)

        MyBase.New()
        Console.WriteLine("fafa :wss device is constructed")
        'Thread.Sleep(TimeSpan.FromSeconds(10))

        If IsNothing(owner) Then Throw New ArgumentNullException("owner")
        If String.IsNullOrEmpty(wssHost) Then Throw New ArgumentNullException("wssHost")
        If String.IsNullOrEmpty(operatorName) Then Throw New ArgumentNullException("operatorName")
        If String.IsNullOrEmpty(teamMembers) Then Throw New ArgumentNullException("teamMembers")
        If String.IsNullOrEmpty(agentName) Then Throw New ArgumentNullException("agentName")
        If Not IPAddress.TryParse(agentHost, Me._AgentHost) Then Throw New ArgumentException("host should be an IP address", "agentHost")

        If Not System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = System.Globalization.NumberFormatInfo.InvariantInfo.NumberDecimalSeparator Then
            'This prevents reading wrong dBm for European cultures
            Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo("en-US", False)
            Console.WriteLine("[WssDriver:Run]: CurrentCulture is now {0}for thread '{1}'.", System.Globalization.CultureInfo.CurrentCulture.Name, Thread.CurrentThread.Name)
        End If

        'save args in local vars
        Me._Owner = owner
        Me._WssHost = wssHost
        Me._WssPort = wssPort

        Me._OperatorName = operatorName
        Me._TeamMembers = New Dictionary(Of String, Integer)
        Dim numberOfagent As Integer = 0
        Dim members() As String = Strings.Split(teamMembers, ",")
        For Each member As String In members
            Dim nameOfagent As String = member

            'Dim parts() As String = Strings.Split(member, "-")
            'Dim number As Integer = CInt(Double.Parse(parts(0)))
            Dim name As String = nameOfagent
            'first value was 8000
            Dim port As Integer = 8000 + numberOfagent
            Me._TeamMembers.Add(name, port)
            numberOfagent = numberOfagent + 1
        Next

        Me._AgentName = agentName
        'first value was 8000 instead of 50000
        Me._ListenerPort = 8000 + agentNumber
        Me._Listener = New WssListener(Me, Me._AgentName, Me._AgentHost, Me._ListenerPort)
        Me._Conversations = New Dictionary(Of Integer, ICommConversation)

    End Sub

#End Region

#Region " Properties "

    Private _Owner As ICommOwner
    Public ReadOnly Property Owner() As ICommOwner
        Get
            Return Me._Owner
        End Get
    End Property

    Private _WssHost As String
    Public ReadOnly Property Host() As String Implements ICommDevice.Host
        Get
            Return Me._WssHost
        End Get
    End Property

    Private _WssPort As Integer
    Public ReadOnly Property Port() As Integer Implements ICommDevice.Port
        Get
            Return Me._WssPort
        End Get
    End Property

    Private _WssConnection As New TcpConnection
    Public ReadOnly Property WssConnection() As TcpConnection
        Get
            Return Me._WssConnection
        End Get
    End Property

#End Region

#Region " TcpConnection to Send Control Messages to WSS "


    Public ReadOnly Property ConnectedToWss() As Boolean
        Get
            Return Me._WssConnection.IsConnected
        End Get
    End Property

    Protected Sub ConnectToWss()
        Thread.Sleep(TimeSpan.FromSeconds(1))
        Console.WriteLine("[WssDevice]: ConnectToWSS")
        Me._WssConnection.Connect(Me._WssHost, Me._WssPort)
    End Sub

    Protected Sub DisconnectFromWss()
        Console.WriteLine("[WssDevice]: DisconnectToWSS")

        Me._WssConnection.Disconnect()
    End Sub

#End Region

#Region " Wss Commands "

    Protected Function SendWssCommand(ByVal command As String, ByVal waitForReply As Boolean, Optional ByRef reply As String = "") As Boolean

        'default to true
        Dim success As Boolean = True

        SyncLock Me._WssConnection

            Try
                If Not Command.Trim.EndsWith(Chr(13) + Chr(10)) Then
                    Command += Chr(13) + Chr(10)
                End If

                Me._WssConnection.Send(Command)

                If waitForReply Then
                    'block this thread until acknowledgement or failure message is received
                    Dim done As Boolean = False
                    While Not done

                        If Me._WssConnection.DataAvailable() Then
                            Dim msg As String = Me._WssConnection.Receive(1024)
                            If msg.TrimStart.ToUpper.StartsWith("ERROR") Then
                                reply = msg
                                success = False
                                done = True
                            Else
                                reply = msg
                                success = True
                                done = True
                            End If
                        End If

                        If Not done Then
                            Thread.Sleep(TimeSpan.FromMilliseconds(500))
                        End If

                    End While
                End If

            Catch ex As Exception
                Console.WriteLine(ex)
                success = False
            End Try

        End SyncLock

        If Not success Then
            Console.WriteLine(String.Format("[WSS] - ERROR: command: '{0}' received reply '{1}'", Command, reply))
        End If

        Return success

    End Function

    Protected Function RegisterAgent() As Boolean

        'note that the request may fail (success = false) because
        'the agent was already registered. So this function will
        'not return the response from WSS, but will return whether there
        'occurred any exceptions.

        Try
            'Dim success As Boolean = Me.SendWssCommand(String.Format("REGISTER {0} {1};", Me._AgentName, Me._AgentHost), True) 'RoboCup Atlanta 2007 WSS
            Dim reply As String = ""
            Dim success As Boolean = Me.SendWssCommand(String.Format("INIT {{Robot {0}}} {{Port {1}}}", Me._AgentName, Me._ListenerPort), True, reply)

            Dim start As Integer = reply.IndexOf("{Status ") + 8
            Dim until As Integer = reply.IndexOf("}", start)

            If start > 7 AndAlso until > start Then
                Dim status As String = reply.Substring(start, until - start)
                If status.Contains("OK") Then
                    Console.WriteLine(reply)
                End If
            End If

        Catch ex As Exception
            Console.WriteLine(ex)
            Return False
        End Try

        'return true by default, see also note above
        Return True

    End Function

    Protected Function RegisterListenerPort() As Boolean

        'note that the request may fail (success = false) because
        'the listener was already registered. So this function will
        'not return the response from WSS, but will return whether there
        'occurred any exceptions.

        Try
            Dim success As Boolean = Me.SendWssCommand(String.Format("LISTEN {0} {1};", Me._AgentName, Me._ListenerPort), True)
        Catch ex As Exception
            Console.WriteLine(ex)
            Return False
        End Try

        'return true by default, see also note above
        Return True

    End Function

    'Protected Sub RequestConversation(ByVal toRobot As String) Implements ICommDevice.RequestConversation
    '    If Not Me._TeamMembers.ContainsKey(toRobot) Then
    '        Console.WriteLine("Conversation requested to unknown robot " & toRobot)
    '    Else
    '        Dim toPort As Integer = Me._TeamMembers(toRobot)
    '        Dim success As Boolean = Me.SendWssCommand(String.Format("OPEN {0} {1} {2} {3};", Me._AgentName, Me._ListenerPort, toRobot, toPort), False)
    '    End If
    'End Sub

    Protected Sub RequestDNS(ByVal toRobot As String) Implements ICommDevice.RequestDNS
        If Not Me._TeamMembers.ContainsKey(toRobot) Then
            Console.WriteLine("DNS requested of unknown robot " & toRobot)
        Else
            Dim reply As String = ""
            If Me.SendWssCommand(String.Format("DNS {{Robot {0}}}", toRobot), True, reply) Then
                Console.WriteLine(reply)

                Dim start As Integer = reply.IndexOf("{Port ") + 6
                Dim until As Integer = reply.IndexOf("}", start)

                If start > 5 AndAlso until > start Then
                    Dim port As String = reply.Substring(start, until - start)
                    If IsNumeric(port) Then
                        Dim number As Integer = Integer.Parse(port)
                        'Console.WriteLine("[WssDevice] Can connect to port " & number)


                        Me._Owner.NotifyDNSReceived(toRobot, number)
                    End If
                End If
            Else
                Console.WriteLine(String.Format("DNS for Robot {0} FAILED!", toRobot))
            End If
        End If
    End Sub

    Protected Sub RequestSignalStrength(ByVal toRobot As String)

        If Not System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = System.Globalization.NumberFormatInfo.InvariantInfo.NumberDecimalSeparator Then
            'This prevents reading wrong dBm for European cultures
            Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo("en-US", False)
            Console.WriteLine("[WssDriver:RequestSignalStrength]: CurrentCulture is now {0} for thread '{1}'.", System.Globalization.CultureInfo.CurrentCulture.Name, Thread.CurrentThread.Name)
        End If
        Dim reply As String = ""
        If Me.SendWssCommand(String.Format("GETSS {{Robot {0}}}", toRobot), True, reply) Then
            'Console.WriteLine(reply)

            Dim start As Integer = reply.IndexOf("{Strength ") + 10
            Dim until As Integer = reply.IndexOf("}", start)

            If start > 9 AndAlso until > start Then
                Dim pathloss As String = reply.Substring(start, until - start)
                If IsNumeric(pathloss) AndAlso Not pathloss.Contains("NaN") Then
                    Dim decibel As Double = Double.Parse(pathloss)
                    Me._Owner.NotifySignalStrengthReceived(toRobot, decibel)
                End If
            End If

        End If
    End Sub

#End Region

#Region " Device Lifetime "

    Private _AgentName As String
    Private _AgentHost As IPAddress
    Private _ListenerPort As Integer

    Private _OperatorName As String
    Private _TeamMembers As Dictionary(Of String, Integer)

    Private _Listener As ICommListener


    Public Sub StartDevice() Implements ICommDevice.StartDevice

        'boot up

        '2007 competition (commented as listenerport is registered simultaneously with agent)
        'If Not Me.RegisterListenerPort() Then
        '   Throw New Exception("Could not register Listner port")
        'End If

        Me.ConnectToWss()
        While Not Me.RegisterAgent()
            'Throw New Exception("Registering Agent failed")
            MsgBox("Registering Agent failed. Perhaps start WSS?", MsgBoxStyle.OkOnly, "Registration failure")
            Me.ConnectToWss()
        End While
        Me._Listener.StartListening()

        'start the thread
        Me.Start()

    End Sub

    Public Sub StopDevice() Implements ICommDevice.StopDevice

        'first stop the thread
        Me.Stop()

        Me._Listener.StopListening()

        Me.CloseAllConversations()

    End Sub


    Protected Overrides Sub Run()

        With Thread.CurrentThread
            If String.IsNullOrEmpty(.Name) Then
                .Name = Me._AgentName & " [WssDevice]"
            End If
            .IsBackground = True
        End With

        While Me.IsRunning

            'request signal strength to all team members
            For Each name As String In Me._TeamMembers.Keys
                If Not name = Me._AgentName Then
                    Me.RequestSignalStrength(name)
                End If
            Next

            'query signal strength every other 10 seconds
            Thread.Sleep(TimeSpan.FromSeconds(10))

        End While

    End Sub


#End Region

#Region " Conversations "

    Private _Conversations As Dictionary(Of Integer, ICommConversation)

    Protected Sub OpenConversation(ByVal client As TcpClient, ByVal toRobot As String, ByVal cam As Boolean) Implements ICommDevice.OpenConversation

        'Called after a DNS-request (client-side)

        SyncLock Me._Conversations

            Try
                Dim otherIpPoint As IPEndPoint

                otherIpPoint = CType(client.Client.RemoteEndPoint, IPEndPoint)
                Console.WriteLine(String.Format("[CommActor] Connected to robot {0} on port {1}", toRobot, otherIpPoint.Port))

                Dim uniqueID As Guid = Guid.NewGuid
                Dim portID As Integer = otherIpPoint.Port

                'NICK
                Dim convID As Integer = portID
                If cam Then
                    convID += 100
                End If

                'Console.WriteLine("Opening conversation with id")
                'Console.WriteLine(convID)

                 Dim conversation As New WssConversation(Me._Owner, Me, convID, client, Me._AgentName, True)

                'NICK
                conversation.ConversationCam = cam

                Me._Conversations.Add(convID, conversation)
                conversation.StartConversation()

                Me._Owner.NotifyConversationStarted(conversation, toRobot)


            Catch ex As Exception
                Console.WriteLine(ex)

            End Try


        End SyncLock

    End Sub

    Protected Sub OpenConversation(ByVal client As TcpClient) Implements ICommDevice.OpenConversation

        ' Called when accepting a new client (server-side)
        SyncLock Me._Conversations

            Try
                Dim otherIpPoint As IPEndPoint

                otherIpPoint = CType(client.Client.RemoteEndPoint, IPEndPoint)
                Console.WriteLine(String.Format("Connected to client on port {0}", otherIpPoint.Port))

                Dim uniqueID As Guid = Guid.NewGuid
                Dim conversation As New WssConversation(Me._Owner, Me, otherIpPoint.Port, client, Me._AgentName, True)
                'NICK
                conversation.incoming = True
                Me._Conversations.Add(otherIpPoint.Port, conversation)
                conversation.StartConversation()


                Dim reply As String = ""

                'If Me.SendWssCommand(String.Format("REVERSEDNS {{Port {0}}}{1}", otherIpPoint.Port, Environment.NewLine), True, reply) Then
                If Me.SendWssCommand(String.Format("REVERSEDNS {{Port {0}}}", otherIpPoint.Port), True, reply) Then
                    Console.WriteLine(reply)
                    'expected REVERSEDNSREPLY {Port PortNumber} {Robot RobotName}
                    'when not use REVERSEDNSREPLY {Port PortNumber} {Error UnknownOrIllegalPort}

                    Dim start As Integer = reply.IndexOf("{Robot ") + 7
                    Dim until As Integer = reply.IndexOf("}", start)

                    'If no Robot in reply, the Index = -1 and start = 6

                    If start > 6 AndAlso until > start Then
                        Dim robotName As String = reply.Substring(start, until - start)

                        Console.WriteLine(String.Format("Connected to robot {0} via port {1}", robotName, otherIpPoint.Port))

                        'Arnoud store here!

                        Me._Owner.NotifyConversationStarted(conversation, robotName)
                        'We know the name, we do not know the position.

                    End If
                End If





            Catch ex As Exception
                Console.WriteLine(ex)

            End Try


        End SyncLock

    End Sub


                'Protected Sub OpenConversation(ByVal connection As TcpConnection) Implements ICommDevice.OpenConversation

                '    SyncLock Me._Conversations

                '        Try
                '            Dim uniqueID As Guid = Guid.NewGuid
                '            Dim conversation As New WssConversation(Me._Owner, Me, uniqueID, connection, Me._AgentName, True)
                '            Me._Conversations.Add(uniqueID, conversation)
                '            conversation.StartConversation()

                '        Catch ex As Exception
                '            Console.WriteLine(ex)

                '        End Try

                '    End SyncLock

                '    'WSS will automatically remove the registration of the listener 
                '    'for the agent that did NOT request the conversation. Since we 
                '    'don't keep track of who requested the conversation, we simply
                '    're-register the listener on both endpoints. For one of these
                '    'this registration will return a 'Already Registered' failure,
                '    'but that's ok.
                '    '[TODO: Does the 2008 WSS still automatically remove the registration of the listener?]
                '    'Me.RegisterListenerPort()

                'End Sub

    Protected Sub CloseAllConversations()
        SyncLock Me._Conversations
            For Each conversation As ICommConversation In Me._Conversations.Values
                conversation.StopConversation()
            Next
            Me._Conversations.Clear()
        End SyncLock
    End Sub

#End Region

End Class
