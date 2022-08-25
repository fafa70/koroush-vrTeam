Imports UvARescue.Tools 'for TcpConnection

Imports System.Net
Imports System.Net.Sockets

Public Interface ICommDevice

    Sub StartDevice()
    Sub StopDevice()

    ReadOnly Property Port() As Integer
    ReadOnly Property Host() As String

    'Sub RequestConversation(ByVal withRobot As String)
    Sub RequestDNS(ByVal withRobot As String)

    Sub OpenConversation(ByVal client As TcpClient)
    Sub OpenConversation(ByVal client As TcpClient, ByVal toRobot As String, ByVal cam As Boolean)
    'Sub OpenConversation(ByVal connection As TcpConnection)

End Interface
