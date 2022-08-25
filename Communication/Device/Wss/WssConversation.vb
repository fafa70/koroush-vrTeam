Imports UvARescue.Tools

Imports System.IO
Imports System.Math
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading

Imports ICSharpCode.SharpZipLib.BZip2
Imports System.Runtime.Serialization.Formatters.Binary


Public Class WssConversation
    Inherits RegularThread
    Implements ICommConversation

#Region " Constructor "

    Public Sub New(ByVal owner As ICommOwner, _
        ByVal device As WssDevice, _
        ByVal conversationID As Integer, _
        ByVal client As TcpClient, _
        ByVal agentName As String, _
        ByVal useCompression As Boolean)

        If IsNothing(owner) Then Throw New ArgumentNullException("owner")
        If IsNothing(device) Then Throw New ArgumentNullException("device")
        If IsNothing(client) Then Throw New ArgumentNullException("client")

        Me._Owner = owner
        Me._Device = device
        Me._Connection = device.WssConnection()
        Me._Client = client
        Me._Stream = client.GetStream

        Me._ConversationID = conversationID
        Me._ConversationCam = False
        Me._incoming = False
        Me._AgentName = agentName
        Me._UseCompression = useCompression

        If Not Me._Stream.CanRead Then Throw New Exception("Stream is not readable")
        If Not Me._Stream.CanWrite Then Throw New Exception("Stream is not writable")

    End Sub

    'Public Sub New(ByVal owner As ICommOwner, _
    '    ByVal device As WssDevice, _
    '    ByVal conversationID As Guid, _
    '    ByVal connection As TcpConnection, _
    '    ByVal agentName As String, _
    '    ByVal useCompression As Boolean)

    '    If IsNothing(owner) Then Throw New ArgumentNullException("owner")
    '    If IsNothing(device) Then Throw New ArgumentNullException("device")
    '    If IsNothing(connection) Then Throw New ArgumentNullException("connection")

    '    Me._Owner = owner
    '    Me._Device = device
    '    Me._Client = connection.client
    '    Me._Connection = connection
    '    Me._Stream = connection.client.GetStream

    '    Me._ConversationID = conversationID
    '    Me._AgentName = agentName
    '    Me._UseCompression = useCompression

    '    If Not Me._Stream.CanRead Then Throw New Exception("Stream is not readable")
    '    If Not Me._Stream.CanWrite Then Throw New Exception("Stream is not writable")

    'End Sub
#End Region

#Region " TcpConnection to via WSS to other robot"


    Public ReadOnly Property ConnectedToWss() As Boolean
        Get
            Return Me._Connection.IsConnected
        End Get
    End Property

    Protected Sub ConnectToWss(ByVal host As String, ByVal port As Integer)
        Me._Connection.Connect(host, port)
    End Sub

    Protected Sub DisconnectFromWss()
        Me._Connection.Disconnect()
    End Sub

#End Region

#Region " Conversation Lifetime "

    Private _ConversationID As Integer
    Public ReadOnly Property ConversationID() As Integer Implements ICommConversation.ConversationID
        Get
            Return Me._ConversationID
        End Get
    End Property

    'NICK
    Private _ConversationCam As Boolean
    Public Property ConversationCam() As Boolean Implements ICommConversation.ConversationCam
        Get
            Return Me._ConversationCam
        End Get
        Set(ByVal value As Boolean)
            Me._ConversationCam = value
        End Set
    End Property

  Private _incoming As Boolean
    Public Property incoming() As Boolean Implements ICommConversation.incoming
        Get
            Return Me._incoming
        End Get
        Set(ByVal value As Boolean)
            Me._incoming = value
        End Set
    End Property

    Protected Mutex As New Object

    Private _Owner As ICommOwner
    Private _Device As WssDevice
    Private _Client As TcpClient
    Private _Connection As TcpConnection
    Private _AgentName As String
    Private _Stream As NetworkStream

    Protected ReadOnly Property IsConnected() As Boolean
        Get
            SyncLock Me.Mutex
                If Not IsNothing(Me._Client) Then
                    Return Me._Client.Connected
                End If
            End SyncLock
            Return False
        End Get
    End Property

    Protected ReadOnly Property IsDataAvailable() As Boolean
        Get
            SyncLock Me.Mutex
                If Not IsNothing(Me._Stream) Then
                    Return Me._Stream.DataAvailable
                End If
            End SyncLock
            Return False
        End Get
    End Property

    Public Sub StartConversation() Implements ICommConversation.StartConversation
        Me.Start()
        Me._Owner.NotifyConversationStarted(Me, "WSS")
    End Sub

    Public Sub StopConversation() Implements ICommConversation.StopConversation

        Try
            'may be invoked by external thread, use locking
            SyncLock Me.Mutex
                If Not IsNothing(Me._Client) AndAlso Me._Client.Connected Then

                    Me._Stream.Flush()
                    Me._Stream.Close()
                    Me._Stream.Dispose()

                    Me._Client.Close()

                End If
            End SyncLock

            Me._Owner.NotifyConversationStopped(Me)

        Catch ex As Exception

        End Try

        'kill the thread
        Me.Stop()

    End Sub

    Protected Overrides Sub Run()

        Dim lastAlive As DateTime = Now

        If String.IsNullOrEmpty(Thread.CurrentThread.Name) Then
            Thread.CurrentThread.Name = Me._AgentName & " [WssConversation]"
        End If

        Try
            While Me.IsRunning AndAlso Me.IsConnected
                If Not Me.Receive(4096) Then

                    'receive returns false only when no new data was available
                    'in the tcp-buffer

                    'send a ping to make sure we get an exception if the server 
                    'closed the underlying connection. I don't know of another way
                    'to check if a connection still works. Somehow the Connected
                    'property on the client keeps saying that the connection is alive
                    'while actually it isn't. The ping works nicely though.
                    If Now - lastAlive > TimeSpan.FromSeconds(30) Then
                        'only once per 30 secs, to avoid blowing up the WSS
                        Me.SendText("Ping")
                        Console.WriteLine("[WssConversation]: Send Ping")
                        lastAlive = Now
                    End If

                    'just wait and then try again
                    'note: do no wait too long in order to stay close to real-time
                    Thread.Sleep(TimeSpan.FromMilliseconds(500))

                End If
            End While

        Catch ex As Exception
            Console.WriteLine(ex)

        Finally
            Me.StopConversation()

        End Try

    End Sub

#End Region

#Region " Core Data Sending and Retrieval Routines "

    ''' <summary>
    ''' This routine actually streams the bytes of the tcp-wire.
    ''' It does so by wrapping the bytes in a SEND command as dictated
    ''' by the WSS specs.
    ''' </summary>
    ''' <param name="protocol"></param>
    ''' <param name="data"></param>
    ''' <remarks></remarks>
    Protected Sub Send(ByVal protocol As Protocol, ByVal data() As Byte)

        'prepare byte array
        Dim infolen As Integer = 5 '1 for ID + 4 for length encoding
        Dim datalen As Integer = data.Length

        'Dim cmdtxt As String = String.Format("SEND {0} ", datalen + infolen)
        'Dim cmdlen As Integer = cmdtxt.Length + 1 '1 extra for ';' at the end
        Dim cmdlen As Integer = 0

        'now we now how long our complete message is going to be, init byte-array
        Dim bytes(cmdlen + infolen + datalen - 1) As Byte


        'paste the command at position 0
        'Encoding.ASCII.GetBytes(cmdtxt, 0, cmdtxt.Length, bytes, 0)


        'paste message ID at the next position (1 byte)
        'Dim offset As Integer = cmdtxt.Length
        Dim offset As Integer = 0
        bytes(offset) = protocol 'the first byte is reserved for the protocol


        'encode message length into subsequent 4 bytes
        Dim value As Integer = datalen 'the value to encode
        For i As Integer = infolen - 2 To 0 Step -1
            If i > 0 Then
                Dim m As Integer = CInt(Byte.MaxValue ^ i)
                Dim v As Integer = CInt(Floor(value / m))
                bytes(offset + i + 1) = CByte(v)
                value -= CInt(v * m)
            Else
                bytes(offset + i + 1) = CByte(value)
            End If
        Next

        'now paste the actual data into the byte array
        Array.Copy(data, 0, bytes, offset + infolen, data.Length)

        'paste semi-colon
        'Encoding.ASCII.GetBytes(";", 0, 1, bytes, bytes.Length - 1)

        'byte array has been constructed, now wire it
        SyncLock Me.Mutex
            With Me._Stream
                .Write(bytes, 0, bytes.Length)
                .Flush() 'force immediate transmission
            End With
        End SyncLock

    End Sub

    'helper var to cache residu of previous read attempts
    Private msgbuffer() As Byte = Nothing

    ''' <summary>
    ''' Will receive maxLength bytes at a time. Any messages
    ''' contained in the tcp-buffer will be reconstructed using the helper
    ''' function for the specific byte-encoding protocol.
    ''' </summary>
    ''' <param name="maxLength"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function Receive(ByVal maxLength As Integer) As Boolean

        'the return value of this function, to inform if we received any new data
        Dim receivedNewData As Boolean = False

        'receive any new available bytes
        If Me.IsDataAvailable Then

            'load data into a temporary buffer
            Dim buffer(maxLength - 1) As Byte

            'nread will tell how many bytes were actually retrieved
            Dim nread As Integer = Me.Read(buffer)

            Dim offset As Integer
            If IsNothing(Me.msgbuffer) Then
                'no data available from previous read, start new array
                offset = 0
                ReDim msgbuffer(nread - 1)
            Else
                'expand previously retrieved data and append buffer into it
                offset = msgbuffer.Length
                'Console.WriteLine(String.Format("[WssConversation] expand buffer with {0}", offset))
                ReDim Preserve msgbuffer(msgbuffer.Length + nread - 1)
            End If

            'copy buffer into msgbuffer
            For i As Integer = 0 To nread - 1
                msgbuffer(offset + i) = buffer(i)
            Next

            receivedNewData = True

            'Console.WriteLine("[WssConversation] Receive NewData.")


        End If

        '      If Not IsNothing(Me.msgbuffer) Then
        '           Console.WriteLine(String.Format("[WssConversation] bufferLength {0}", Me.msgbuffer.Length))
        'End If

        'at this point the buffer may contain a complete or a partial message
        'the first 5 bytes store the type (1 byte) and length (4 bytes)
        'so we need at least that to proceed
        If Not IsNothing(Me.msgbuffer) AndAlso Me.msgbuffer.Length > 5 Then

            'get the protocol
            Dim msgprotocol As Protocol = CType(msgbuffer(0), Protocol)



            'read the msg length from bytes 2 to 5
            Dim msglength As Integer = 0
            For i As Integer = 0 To 3
                msglength += CInt(Byte.MaxValue ^ i * msgbuffer(i + 1))
            Next

            'Select Case msgprotocol
            '    Case Protocol.Text
            '        Console.WriteLine(String.Format("[WssConversation] Text msglength {0}", msglength))
            '    Case Protocol.Binary
            '        Console.WriteLine(String.Format("[WssConversation] Binary msglength {0}", msglength))
            '    Case Protocol.Compressed
            '        Console.WriteLine(String.Format("[WssConversation] Compressed msglength {0}", msglength))
            '    Case Else
            '        Console.WriteLine(String.Format("[WssConversation] unknown protocol {0}", msgprotocol))
            'End Select

            'check if we have this msg in full within the current msgbuffer
            If msgbuffer.Length >= msglength + 5 Then

                'ok we have it, so extract the relevant part
                Dim curmsg() As Byte = Nothing
                Dim nxtmsg() As Byte = Nothing

                'the current message 
                ReDim curmsg(msglength - 1)
                For i As Integer = 0 To curmsg.Length - 1
                    'skip first 5 bytes
                    curmsg(i) = msgbuffer(i + 5)
                Next

                'any subsequent messages
                ReDim nxtmsg(msgbuffer.Length - curmsg.Length - 5 - 1)
                For i As Integer = 0 To nxtmsg.Length - 1
                    nxtmsg(i) = msgbuffer(curmsg.Length + 5 + i)
                Next

                'next time we will proceed with the remainder
                Me.msgbuffer = nxtmsg

                'forward current message to specific handler
                Select Case msgprotocol
                    Case Protocol.Text
                        Me.ReceiveText(curmsg)
                    Case Protocol.Binary
                        Me.ReceiveMessage(curmsg, False)
                    Case Protocol.Compressed
                        Me.ReceiveMessage(curmsg, True)
                End Select

            End If

        End If

        Return receivedNewData

    End Function

    Private Function Read(ByRef buffer() As Byte) As Integer
        SyncLock Me.Mutex
            Return Me._Stream.Read(buffer, 0, buffer.Length)
        End SyncLock
    End Function

#End Region

#Region " Text Messaging "

    ''' <summary>
    ''' Plain text sending, ASCII encoded
    ''' </summary>
    ''' <param name="text"></param>
    ''' <remarks></remarks>
    Protected Sub SendText(ByVal text As String) Implements ICommConversation.SendText
        If String.IsNullOrEmpty(text) Then Exit Sub
        Me.Send(Protocol.Text, Encoding.ASCII.GetBytes(text))
    End Sub

    ''' <summary>
    ''' Decode as ASCII text.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <remarks></remarks>
    Protected Sub ReceiveText(ByVal data() As Byte)
        Console.WriteLine("[WssConversation] Receiving text.")
        If Not IsNothing(data) Then
            Me._Owner.NotifyMessageReceived(Me, New TextMessage(Encoding.ASCII.GetString(data)))
        End If
    End Sub

#End Region

#Region " Binary Messaging "

    Private _UseCompression As Boolean
    Public ReadOnly Property UseCompression() As Boolean
        Get
            Return Me._UseCompression
        End Get
    End Property

    Protected Sub SendMessage(ByVal message As Message) Implements ICommConversation.SendMessage
        If IsNothing(message) Then Exit Sub

        Console.WriteLine("[WssConversation] Sending message.")

        Try

            'assume a binary message
            Dim protocol As Protocol = Communication.Protocol.Binary
            Dim data() As Byte = Nothing
            Using stream As New MemoryStream
                Dim formatter As New BinaryFormatter

                If Me._UseCompression Then

                    'use Bzip2 compression
                    protocol = Communication.Protocol.Compressed
                    Using bzip As New BZip2OutputStream(stream)
                        If bzip.CanWrite Then
                        formatter.Serialize(bzip, message)
                        End If
                    End Using

                Else

                    formatter.Serialize(stream, message)

                End If

                data = stream.ToArray

            End Using

            If Not IsNothing(data) Then
                Me.Send(protocol, data)
            End If

        Catch ex As Exception
            Console.WriteLine("Exception occurred while trying to send message")
            Console.WriteLine(ex)

        End Try

    End Sub

    Protected Sub ReceiveMessage(ByVal data() As Byte, ByVal useCompression As Boolean)
        If IsNothing(data) Then Exit Sub

        'Console.WriteLine("[WssConversation] Receiving message.")

        Using stream As New MemoryStream(data)

            Dim formatter As New BinaryFormatter
            Dim result As Object

            If useCompression Then

                'bzip2 compression was used
                Using bzip As New BZip2InputStream(stream)
                    result = formatter.Deserialize(bzip)
                End Using

            Else

                result = formatter.Deserialize(stream)

            End If

            If TypeOf result Is Message Then
                Me._Owner.NotifyMessageReceived(Me, DirectCast(result, Message))
            End If

        End Using

    End Sub

#End Region

End Class
