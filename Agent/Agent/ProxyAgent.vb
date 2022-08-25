Imports System.Drawing
Imports UvARescue.Math


Public Class ProxyAgent
    Inherits Agent

#Region " Constructor "

    Public Sub New(ByVal manifold As Manifold, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(manifold, name, agentConfig, teamConfig)
    End Sub

#End Region

#Region " Operator "

    Private _Operator As OperatorAgent
    Public ReadOnly Property [Operator]() As OperatorAgent
        Get
            Return Me._Operator
        End Get
    End Property
    Friend Sub SetOperator(ByVal [operator] As OperatorAgent)
        Me._Operator = [operator]
    End Sub

    Private _ConnectedToAlterEgo As Boolean
    Public ReadOnly Property ConnectedToAlterEgo() As Boolean
        Get
            Return Me._ConnectedToAlterEgo
        End Get
    End Property


    Friend Overridable Sub NotifyAlterEgoConnected()
        'Debug.WriteLine(String.Format("[{0}] -- proxy connected to alter ego", Me.Name))
        Me._ConnectedToAlterEgo = True
    End Sub
    Friend Overridable Sub NotifyAlterEgoDisconnected()
        'Debug.WriteLine(String.Format("[{0}] -- proxy disconnected from alter ego", Me.Name))
        Me._ConnectedToAlterEgo = False
    End Sub


    Protected Overrides Sub OnSync()
        MyBase.OnSync()

        If Not IsNothing(Me._Operator) Then
            Debug.WriteLine(String.Format("[{0}] -- proxy requests sync", Me.Name))
            Me._Operator.RelaySyncRequest(Me.Name)
        End If

    End Sub

    
    Protected Friend Overrides Sub OnCamReq()
        'MyBase.OnCamReq()
        'NICK: commented, MyBase.OnCamReq() (Agent) calls CamRepl which triggers OnCamRepl

        If Not IsNothing(Me._Operator) Then
            'Debug.WriteLine(String.Format("[{0}] -- proxy requests camera image", Me.Name))
            Me._Operator.RelayCamReq(Me.Name)
        End If

    End Sub

    Protected Friend Overrides Sub OnCamRepl(ByVal fromRobot As String, ByVal camIm As Bitmap)
        'Console.WriteLine("Cam reply")

        'The proxy sends the image to the interface

        Debug.WriteLine(String.Format("[{0}] -- proxy receives camera image", Me.Name))

        Debug.WriteLine(String.Format("[{0}] -- should update backgroundImage", Me.Name))

        MyBase.OnCamRepl(fromRobot, camIm) '


        'The sensor will call AgentController.NotifySensorUpdate

        'Ask for another image
        'NICK
        If ShowImages Then
            Threading.Thread.Sleep(TimeSpan.FromSeconds(0.1))
            Me.CamReq()
        End If

    End Sub

    

    ''' <summary>
    ''' Proxies do not spawn in UsarSim. Their alter-ego's do. The proxy
    ''' is to interact with his alter-ego through the WSS. That is: by relaying
    ''' commands through the Operator.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Overrides Sub Spawn()
        Console.WriteLine("[ProxyAgent] Do not spawn!")
    End Sub

    ''' <summary>
    ''' Re-route UsarSim commands through the WSS.
    ''' </summary>
    ''' <param name="command"></param>
    ''' <remarks></remarks>
    Public Overrides Sub SendUsarSimCommand(ByVal command As String)
        If Not IsNothing(Me._Operator) Then
            'Debug.WriteLine(String.Format("[{0}] -- proxy relays command '{1}'", Me.Name, command))
            Me._Operator.RelayUsarSimCommand(Me.Name, command)
        End If
    End Sub

#End Region

#Region " Target Locations "
    Public Overrides Sub SendNewTarget(ByVal toRobot As String, ByVal target As Math.Pose2D)
        SyncLock Me._TargetLocations
            Me._Operator.RelayTargetLocation(toRobot, target)
            Console.WriteLine("[Target] Proxy forwarded to operator")
        End SyncLock
    End Sub
#End Region


    Public Overrides Sub ClearAllTargets()
        Me._TargetLocations.Clear()
        Me._ResetCurrentPath = True
        SendNewTarget(Me.Name, Nothing)
    End Sub

    Public Overrides Sub AddNewTarget(ByVal targetLocation As Pose2D, Optional ByVal clearAgentsCurrentTargets As Boolean = False)
        MyBase.AddNewTarget(targetLocation, clearAgentsCurrentTargets)

        'NICK
        SendNewTarget(Me.Name, targetLocation)
    End Sub


End Class
