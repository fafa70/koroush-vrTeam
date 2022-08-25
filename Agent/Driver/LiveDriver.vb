Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Globalization

Imports UvARescue.Tools


''' <summary>
''' Used for agents that obtain live sensor data from the Usarsim server.
''' </summary>
''' <remarks></remarks>
Public Class LiveDriver
    Inherits Driver

#Region " Constructor "

    Public Sub New(ByVal agent As Agent, ByVal config As TeamConfig, ByVal useImageServer As Boolean, ByVal createLogfile As Boolean) 'should also create useConnectionToUsarSim
        MyBase.New(agent)

        Me._Config = config
        Me._UseImageServer = useImageServer

        If createLogfile Then
            Me.OpenLog(String.Format(My.Settings.LogFileNameFormat, agent.Name))
        End If

    End Sub

    Private _Config As TeamConfig
    Protected ReadOnly Property Config() As TeamConfig
        Get
            Return Me._Config
        End Get
    End Property

    Private _UseImageServer As Boolean
    Protected ReadOnly Property UseImageServer() As Boolean
        Get
            Return Me._UseImageServer
        End Get
    End Property

#End Region

#Region " TcpConnection to UsarSim "

    Private _UsarSimConnection As New TcpMessagingConnection

    Public ReadOnly Property ConnectedToUsarSim() As Boolean
        Get
            Return Me._UsarSimConnection.IsConnected
        End Get
    End Property

    Protected Sub ConnectToUsarSim(ByVal host As String, ByVal port As Integer)
        If Not IsNothing(Me.Agent) AndAlso Not IsNothing(Me.Agent.Name) Then
            Console.WriteLine(String.Format("[LiveDriver]: {0} does ConnectToUsarSim", Me.Agent.Name))
        Else
            Console.WriteLine("[LiveDriver]: ConnectToUsarSim")
        End If
        Me._UsarSimConnection.Connect(host, port)
    End Sub

    Protected Sub DisconnectFromUsarSim()
        If Not IsNothing(Me.Agent) AndAlso Not IsNothing(Me.Agent.Name) Then
            Console.WriteLine(String.Format("[LiveDriver]: {0} does DisconnectFromUsarSim", Me.Agent.Name))
        Else
            Console.WriteLine("[LiveDriver]: DisconnectFromUsarSim")
        End If
        Me._UsarSimConnection.Disconnect()
    End Sub

#End Region

#Region " TcpConnection to ImageServer "

    Private _ImageServerConnection As New TcpCameraConnection

    Public ReadOnly Property ConnectedToImageServer() As Boolean
        Get
            Return Me._ImageServerConnection.IsConnected
        End Get
    End Property

    Protected Sub ConnectToImageServer(ByVal host As String, ByVal port As Integer)
        If Not IsNothing(Me.Agent) AndAlso Not IsNothing(Me.Agent.Name) Then
            Console.WriteLine(String.Format("[LiveDriver]: {0} does ConnectToImageServer", Me.Agent.Name))
        Else
            Console.WriteLine("[LiveDriver]: ConnectToImageServer")
        End If

        Me._ImageServerConnection.Connect(host, port)
    End Sub

    Protected Sub DisconnectFromImageServer()
        If Not IsNothing(Me.Agent) AndAlso Not IsNothing(Me.Agent.Name) Then
            Console.WriteLine(String.Format("[LiveDriver]: {0} does DisconnectToImageServer", Me.Agent.Name))
        Else
            Console.WriteLine("[LiveDriver]: DisconnectToImageServer")
        End If
        Me._ImageServerConnection.Disconnect()
    End Sub

#End Region

#Region " Driver Lifetime "

    Public Overrides Sub Start()

        With Me._Config

            Me.ConnectToUsarSim(.UsarSimHost, .UsarSimPort)

            If Me._UseImageServer Then
                Me.ConnectToImageServer(.ImageServerHost, .ImageServerPort)
            End If

            'Julian RoboCup 2008: Sleep for 3 seconds to allow robot to be spawned, before connecting
            'to new WSS.  Must be done otherwise WSS refuses connection.
            If Me._Config.WirelessServerPort = 50000 Then
                Threading.Thread.Sleep(TimeSpan.FromSeconds(3))
            End If


        End With

        If Threading.Thread.CurrentThread.Name = Nothing Then
            Threading.Thread.CurrentThread.Name = "LiveDriver"
        End If


        If Me.ConnectedToUsarSim = True Then
            Threading.Thread.CurrentThread.Priority = ThreadPriority.AboveNormal
            MyBase.Start()
        Else
            Console.WriteLine("[LiveDriver:Start] No connection to UsarSim")
            Me.Agent.Finish()
        End If

    End Sub


    Public Overrides Sub [Stop]()
        'first kill the main routine
        MyBase.Stop()

        'then disconnect 
        If Me.ConnectedToImageServer Then
            Me.DisconnectFromImageServer()
        End If
        If Me.ConnectedToUsarSim Then
            Me.DisconnectFromUsarSim()
        End If

        Me.Agent.Finish()

        Me.CloseLog()

    End Sub


    Protected Overrides Sub Run()

        System.Threading.Thread.CurrentThread.Name = Me.Agent.Name

        'See http://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo.currentculture(VS.71).aspx

        If Not CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = NumberFormatInfo.InvariantInfo.NumberDecimalSeparator Then
            'This prevents BufferedLayer overflow, due to huge laser ranges if ',' and '.' are interchanged
            Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US", False)
            Console.WriteLine("[LifeDriver:Run]: CurrentCulture is now {0}.", CultureInfo.CurrentCulture.Name)
        End If


        'first, spawn the robot on the server
        Me.Agent.Spawn()

        Dim bSentCmds As Boolean
        Dim bRecvMsgs As Boolean
        Dim bRecvImgs As Boolean

        While Me.IsRunning

            'reset flags 
            bSentCmds = False
            bRecvMsgs = False
            bRecvImgs = False

            'check for pause
            Me.CheckForPause()

            If Me.Agent.UsarSimCommands.Count > 0 Then
                'send out any pending usarsim commands

                bSentCmds = True 'set the flag


                While Me.Agent.UsarSimCommands.Count > 0
                    Me._UsarSimConnection.Send(Me.Agent.UsarSimCommands.Dequeue)
                End While

            End If


            'receive any inbound messages from usarsim
            If Me._UsarSimConnection.DataAvailable Then

                bRecvMsgs = True 'set the flag

                Dim msgs As Specialized.StringCollection = Me._UsarSimConnection.ReceiveMessages(4096)
                For Each msg As String In msgs
                    'Console.WriteLine(String.Format("[msg time] = {0}", msg))
                    If Not LineParsers.ParseUsarSimLine(Me.Agent, msg) Then
                        Me.Error("Could not parse message.", msg) 'GetGeo and GetConf not supported in UT3 (yet)
                        Me.Log(msg)
                    Else
                        Me.Log(msg)
                    End If
                Next

            End If

            'receive any new camera image from imgserver
            If Me.ConnectedToImageServer Then
                If Me._ImageServerConnection.DataAvailable Then

                    bRecvImgs = True 'set the flag

                    ' get image data
                    Dim bytes As Byte() = Me._ImageServerConnection.ReceiveImageData(640 * 480 * 3 + 1)
                    If Not IsNothing(bytes) AndAlso bytes.Length > 0 Then
                        If Not ImageParser.ParseImageData(Me.Agent, bytes) Then
                            Me.Error("Could not parse image data.", bytes.ToString)
                        End If
                        Me._ImageServerConnection.SendAcknowledgement()
                    End If

                End If
            End If

            'to avoid busy waiting
            If Not bSentCmds AndAlso Not bRecvMsgs AndAlso Not bRecvImgs Then

                'nothing happened this round, the agent may have crashed or 
                'perhaps it's the ComStation that does not have any sensors
                'avoid CPU overload while doing nothing: sleep 1 second

                Threading.Thread.Sleep(TimeSpan.FromSeconds(1))
            Else
                'If Not IsNothing(Me.Agent) AndAlso Not IsNothing(Me.Agent.Name) Then
                '    If bSentCmds Then
                '        Console.WriteLine(String.Format("[LiveDriver]: {0} busy with sending commands", Me.Agent.Name))
                '    End If
                '    If bRecvMsgs Then
                '        Console.WriteLine(String.Format("[LiveDriver]: {0} busy with receiving messages", Me.Agent.Name))
                '    End If
                '    If bRecvImgs Then
                '        Console.WriteLine(String.Format("[LiveDriver]: {0} busy with receiving images", Me.Agent.Name))
                '    End If
                'End If
            End If



        End While

    End Sub

#End Region

#Region " Logging "

    Private logger As LogFileWriter

    Protected Sub OpenLog(ByVal logfilename As String)
        If Not IsNothing(Me.logger) Then Throw New InvalidOperationException("Log already opened")

        Dim fileInfo As New FileInfo(logfilename)
        If Not fileInfo.Directory.Exists Then
            fileInfo.Directory.Create()
        End If
        Me.LogFile = logfilename

        Me.logger = New LogFileWriter(logfilename)

    End Sub
    Protected Sub CloseLog()
        If Not IsNothing(Me.logger) Then
            Me.logger.Flush()
            Me.logger.Close()
            Me.logger.Dispose()
        End If
    End Sub

    Protected Overridable Sub [Error](ByVal msg As String, ByVal line As String)
        'comment the line before logging
        Me.Log(String.Format("{0} [ERROR] - {1}", My.Settings.CommentChar, line))
        Console.Error.WriteLine(msg & vbNewLine & vbTab & line)
    End Sub

    Protected Overridable Sub Log(ByVal line As String)
        If Not IsNothing(Me.logger) Then
            Me.logger.WriteLine(line)
        End If
    End Sub

#End Region

End Class
