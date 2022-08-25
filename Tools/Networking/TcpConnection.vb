Imports System.Net
Imports System.Net.Sockets
Imports System.Text

Public Class TcpConnection

    Protected client As New TcpClient
    Protected stream As NetworkStream = Nothing


#Region " Client - Connect / Disconnect "

    Public ReadOnly Property IsConnected() As Boolean
        Get
            Return Me.client.Connected
        End Get
    End Property

    Public Sub Connect(ByVal host As String, ByVal port As Integer)
        If Me.IsConnected Then Throw New InvalidOperationException("Agent already connected")
        Try

            Me.client.Connect(host, port)
            Me.OpenStream()

        Catch ex As Exception
            Console.WriteLine("Error connect to host '" + host + "'.")
            Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
        End Try
    End Sub

    Public Sub Disconnect()
        If Not Me.IsConnected Then Throw New InvalidOperationException("Agent not connected")
        Me.CloseStream()
        Me.client.Close()
    End Sub

    Public Function GetClient() As TcpClient
        Return Me.client
    End Function

#End Region

#Region " Stream - Open / Close "

    Public ReadOnly Property DataAvailable() As Boolean
        Get
            If Not Me.IsConnected Then Throw New InvalidOperationException("You should connect first")
            Return Me.stream.DataAvailable
        End Get
    End Property

    Protected Sub OpenStream()
        Me.stream = Me.client.GetStream()
        If Not stream.CanRead Then Throw New Exception("Stream is not readable")
        If Not stream.CanWrite Then Throw New Exception("Stream is not writable")
    End Sub

    Protected Sub CloseStream()
        Me.stream.Flush()
        Me.stream.Close()
        Me.stream.Dispose()
        Me.stream = Nothing
    End Sub

    

#End Region

#Region " Send and Receive Strings "

    ''' <summary>
    ''' a string-based (ASCII) message sending format is assumed.
    ''' </summary>
    ''' <param name="message"></param>
    ''' <remarks></remarks>
    Public Sub Send(ByVal message As String)
        If Not Me.IsConnected Then Throw New InvalidOperationException("You should connect first")

        Try

        
            'convert to bytes and send it
            Dim bytes() As Byte = Encoding.ASCII.GetBytes(message)
            Me.stream.Write(bytes, 0, bytes.Length)
            Me.stream.Flush()
            '        Console.WriteLine(String.Format("[TcpConnection] - send {0} bytes ", bytes.Length))
        Catch ex As Exception
            Console.WriteLine("[TcpConnection] Error while sending message.")
            Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
        End Try

    End Sub

    Public Function Receive(ByVal maxLength As Integer) As String
        If Not Me.IsConnected Then Throw New InvalidOperationException("You should connect first")

        'receive any inbound messages
        If stream.DataAvailable Then

            'load data into buffer, max 4K per iteration
            Dim buffer(maxLength) As Byte

            'length will tell how many bytes were actually retrieved
            Dim length As Integer = stream.Read(buffer, 0, buffer.Length)

            Console.WriteLine(String.Format("[TcpConnection] - received {0} bytes ", length))

            Return Encoding.ASCII.GetString(buffer, 0, length)

        End If

        Return String.Empty

    End Function

#End Region

End Class
