Imports System.IO

Imports UvARescue.Math

Public Class OperatorAgent
    Inherits CommAgent

#Region " Constructor "

    Public Sub New(ByVal manifold As Manifold, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(manifold, name, agentConfig, teamConfig)

        Me.CreateCommActor(teamConfig) 'The operator is the only Agent which start Communicating when Spawned from the UI

        ReDim Me._SignalStrengthMatrix(Me._TeamMembers.Count - 1, Me._TeamMembers.Count - 1)
        For i As Integer = 0 To Me._SignalStrengthMatrix.GetLength(0) - 1
            For j As Integer = 0 To Me._SignalStrengthMatrix.GetLength(1) - 1
                Me._SignalStrengthMatrix(i, j) = -100
            Next
        Next

        Me.ManifoldImage.AddBaseStationPoseToCommLayer(Me.UniqueID, Me.StartPose)
    End Sub

#End Region

#Region " Proxy Registration "

    Private _Proxies As New Dictionary(Of String, ProxyAgent)

    Public Sub RegisterProxy(ByVal proxy As ProxyAgent)
        If Not Me._Proxies.ContainsKey(proxy.Name) Then

            'register proxy
            Me._Proxies.Add(proxy.Name, proxy)
            proxy.SetOperator(Me)

            'it could be that the operator already connected to the alter ego
            If Me.IsConnectedTo(proxy.Name) Then
                proxy.NotifyAlterEgoConnected()
            End If

        End If
    End Sub

    Public Sub UnRegisterProxy(ByVal proxy As ProxyAgent)
        If Me._Proxies.ContainsKey(proxy.Name) Then
            proxy.SetOperator(Nothing)
            Me._Proxies.Remove(proxy.Name)
        End If
    End Sub

#End Region

#Region " Incoming Notifications from CommActor forwarded to Proxies "

    Protected Friend Overrides Sub NotifyAgentConnected(ByVal agentName As String)
        MyBase.NotifyAgentConnected(agentName)

        If Me._Proxies.ContainsKey(agentName) Then
            Me._Proxies(agentName).NotifyAlterEgoConnected()
        End If

    End Sub

    Protected Friend Overrides Sub NotifyAgentDisconnected(ByVal agentName As String)
        MyBase.NotifyAgentDisconnected(agentName)

        If Me._Proxies.ContainsKey(agentName) Then
            Me._Proxies(agentName).NotifyAlterEgoDisconnected()
        End If

    End Sub



    Protected Friend Overrides Sub NotifySignalStrengthReceived(ByVal agentName As String, ByVal pathloss As Double)
        MyBase.NotifySignalStrengthReceived(agentName, pathloss)

        ' RIKNOTE: what purpose do the proxy agents serve?
        If Me._Proxies.ContainsKey(agentName) Then
            Me._Proxies(agentName).NotifySignalStrengthUpdated(pathloss)
        End If

        'Arnoud: trying to flash the signal strengths of all agents in the UsarOperator window.
        'If agentName = "Operator" Then
        '  Me.NotifySignalStrengthUpdated(pathloss)
        'End If

    End Sub



    Private _LastSynchronisation As New Dictionary(Of String, DateTime)

    Protected Friend Overrides Sub NotifyPoseEstimateReceived(ByVal agentName As String, ByVal pose As Pose2D)
        MyBase.NotifyPoseEstimateReceived(agentName, pose)

        If Me._Proxies.ContainsKey(agentName) Then
            Dim proxy As ProxyAgent = Me._Proxies(agentName)

            Me.Manifold.AcquireWriterLock()
            Me.Manifold.LocalizeAgent(proxy, Nothing, pose, Me._SignalStrengthMatrix)
            Me.Manifold.ReleaseWriterLock()
        End If

        Dim timestamp As DateTime = Now

        If Not Me._LastSynchronisation.ContainsKey(agentName) Then
            SyncLock Me._LastSynchronisation
                Me._LastSynchronisation.Add(agentName, timestamp)
            End SyncLock
            Return
        End If

        Dim synchronisation_interval As Integer = 10 'seconds

        If timestamp - Me._LastSynchronisation(agentName) > TimeSpan.FromSeconds(synchronisation_interval) Then

            Me.CommActor.RelaySyncRequest(agentName)


            SyncLock Me._LastSynchronisation
                Me._LastSynchronisation(agentName) = timestamp
            End SyncLock

        End If

    End Sub

#End Region

#Region " Incoming Requests from Proxies forwarded to CommActor "

    Friend Overridable Sub RelayUsarSimCommand(ByVal agentName As String, ByVal command As String)
        Me.CommActor.RelayActorCommand(agentName, command)
    End Sub

    Friend Overridable Sub RelaySyncRequest(ByVal agentName As String)
        Me.CommActor.RelaySyncRequest(agentName)
    End Sub

    Friend Overridable Sub RelayCamReq(ByVal agentName As String)
        Me.CommActor.RelayCamReq(agentName)
    End Sub

    'Arnoud: German Open 2008: CamReply from proxy?
    Friend Overridable Sub RelayCamRepl(ByVal agentName As String, ByVal camIm As System.Drawing.Bitmap)
        Me.CommActor.RelayCamRepl(agentName, camIm)
    End Sub

    Protected Friend Overrides Sub OnCamRepl(ByVal fromRobot As String, ByVal camIm As System.Drawing.Bitmap)
        'Console.WriteLine(String.Format("[Operator] - Camera image received from {0}", fromRobot))
        'The real robot operator the image to the proxy
        If Me._Proxies.ContainsKey(fromRobot) Then
            Me._Proxies(fromRobot).OnCamRepl(fromRobot, camIm)
        End If
        'camIm.Save(Me.reportPath + "\" + fromRobot + "latestImage.jpg", Drawing.Imaging.ImageFormat.Jpeg)
    End Sub

    Private Function reportPath() As String
        reportPath = My.Application.Info.DirectoryPath
        'Console.WriteLine(reportPath)
        For i As Integer = 1 To 5
            reportPath = Directory.GetParent(reportPath).ToString
        Next
        reportPath += "\AOJRF_REPORTS"
        'Console.WriteLine(reportPath)

        If Not Directory.Exists(reportPath) Then
            Directory.CreateDirectory(reportPath)
        End If

        Dim currTime As String = System.DateTime.Now.Day.ToString() + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Year.ToString() + "_" + System.DateTime.Now.Hour.ToString() + "h" + System.DateTime.Now.Minute.ToString()
        reportPath += "\" + currTime
        If Not Directory.Exists(reportPath) Then
            Directory.CreateDirectory(reportPath)
        End If
    End Function

    Friend Overridable Sub RelayTargetLocation(ByVal agentName As String, ByVal location As Pose2D)
        Console.WriteLine("[Target] Call via the OperatorAgent should invoke the Relay in its CommActor")
        Me.CommActor.RelayTargetLocation(agentName, location)
    End Sub

#End Region

#Region " Teammate Signal Strength "

    Protected Friend Overrides Sub UpdateSignalStrengthMatrix(ByVal fromRobot As String, ByVal toRobot As String, ByVal strength As Double)
        Dim i As Integer = Me._TeamMembers.IndexOf(fromRobot)
        Dim j As Integer = Me._TeamMembers.IndexOf(toRobot)

        Me._SignalStrengthMatrix(i, j) = strength
        Me._SignalStrengthMatrix(j, i) = strength

        'Uncomment out the below to output status of all connections
        'Console.WriteLine("[JDHNOTE] ________________________________________________")
        'Console.WriteLine("[JDHNOTE] OperatorAgent.vb: UpdateSignalStrengthMatrix ")
        'For m As Integer = 0 To Me._TeamMembers.Count - 1
        ' For n As Integer = 0 To Me._TeamMembers.Count - 1
        ' Console.Write(Me._SignalStrengthMatrix(m, n))
        ' Console.Write(" ")
        ' Next
        ' Console.WriteLine()
        ' Next
        ' Console.WriteLine("[JDHNOTE] ________________________________________________")
    End Sub
#End Region

End Class
