Imports UvARescue.Tools

Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

''' <summary>
''' Listens to a dedicated listen-port and accepts incoming 
''' connections from the WSS.
''' </summary>
''' <remarks></remarks>
Public Class WssListener
    Inherits RegularThread
    Implements ICommListener

    Public Sub New(ByVal device As ICommDevice, ByVal agentName As String, ByVal address As IPAddress, ByVal port As Integer)
        If IsNothing(device) Then Throw New ArgumentNullException("device")

        Me._Device = device
        Me._AgentName = agentName
        Me._Listener = New TcpListener(address, port)

    End Sub

    Private _Device As ICommDevice
    Private _AgentName As String
    Private _Listener As TcpListener


    Public Sub StartListening() Implements ICommListener.StartListening
        Me._Listener.Start()
        Me.Start()
    End Sub

    Public Sub StopListening() Implements ICommListener.StopListening
        Me._Listener.Stop()
        Me.Stop()
    End Sub

    Protected Overrides Sub Run()

        With Thread.CurrentThread
            If String.IsNullOrEmpty(.Name) Then
                .Name = Me._AgentName & " [WssListener]"
            End If
            .IsBackground = True
        End With

        While Me.IsRunning

            Try

                'accept incoming tcp connection from WSS
                Dim client As TcpClient = Me._Listener.AcceptTcpClient

                Console.WriteLine(String.Format("Accepted to client from WSS"))

                'forward client to device 

                'Look if needed
                Me._Device.OpenConversation(client)

                

            Catch ex As Exception
                Console.WriteLine(String.Format("[WSSListener] connection to WSS closed (exception handled)"))
                Console.WriteLine(ex)

            End Try

        End While

    End Sub

End Class
