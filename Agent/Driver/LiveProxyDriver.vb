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
Public Class LiveProxyDriver
    Inherits Driver

#Region " Constructor "

    Public Sub New(ByVal agent As Agent, ByVal config As TeamConfig, ByVal useImageServer As Boolean, ByVal createLogfile As Boolean)
        MyBase.New(agent)

        Me._Config = config

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
            Return False
        End Get
    End Property

#End Region

#Region " TcpConnection to UsarSim "


    Public ReadOnly Property ConnectedToUsarSim() As Boolean
        Get
            Return False
        End Get
    End Property

    Protected Sub ConnectToUsarSim(ByVal host As String, ByVal port As Integer)
        Console.WriteLine("[LiveProxyDriver] Does not ConnectToUsarSim")
    End Sub

    Protected Sub DisconnectFromUsarSim()
        Console.WriteLine("[LiveProxyDriver] Does not DisconnectToUsarSim")
    End Sub

#End Region

#Region " TcpConnection to ImageServer "

    Public ReadOnly Property ConnectedToImageServer() As Boolean
        Get
            Return False
        End Get
    End Property

    Protected Sub ConnectToImageServer(ByVal host As String, ByVal port As Integer)
        Console.WriteLine("[LiveProxyDriver]: Does not ConnectToImageServer")
    End Sub

    Protected Sub DisconnectFromImageServer()
        Console.WriteLine("[LiveProxyDriver]: Does not DisconnectToImageServer")
    End Sub

#End Region

#Region " Driver Lifetime "

    Public Overrides Sub Start()

        With Me._Config

            'Julian RoboCup 2008: Sleep for 3 seconds to allow robot to be spawned, before connecting
            'to new WSS.  Must be done otherwise WSS refuses connection.
            If Me._Config.WirelessServerPort = 50000 Then
                Threading.Thread.Sleep(TimeSpan.FromSeconds(3))
            End If


        End With

        If Threading.Thread.CurrentThread.Name = Nothing Then
            Threading.Thread.CurrentThread.Name = "LiveProxyDriver"
        End If


        MyBase.Start()

    End Sub


    Public Overrides Sub [Stop]()
        'first kill the main routine
        MyBase.Stop()


        Me.Agent.Finish()

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

            'Console.WriteLine(String.Format("LiveProxydriver still alive", Me.Agent.Name))
            'If Not IsNothing(Me.Agent) AndAlso Me.Agent.ShowImages() Then
            '    'To prevent a long queue of camerarequest before UsarClient is running
            '    If DirectCast(Agent, ProxyAgent).ConnectedToAlterEgo Then
            '        Me.Agent.CamReq() 'Request moved to CamResp.
            '    End If
            'End If




            'to avoid busy waiting
            'If Not bSentCmds AndAlso Not bRecvMsgs AndAlso Not bRecvImgs Then

                'Too many request makes the responses very slow.
                Threading.Thread.Sleep(TimeSpan.FromSeconds(1))

            'End If


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
