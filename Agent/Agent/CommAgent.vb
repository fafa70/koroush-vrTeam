Imports System.Math

Imports UvARescue.Communication
Imports UvARescue.Math
Imports UvARescue.Tools


''' <summary>
''' Sub-class of Agent that adds a CommActor.
''' 
''' This functionality was factored into a separate class since
''' the WSS functionality may be a bit heavy on the CPU. 
''' </summary>
''' <remarks></remarks>
Public Class CommAgent
    Inherits Agent

#Region " Constructor "

    Public Sub New(ByVal manifold As Manifold, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(manifold, name, agentConfig, teamConfig)

        Me._OperatorName = teamConfig.OperatorName

        'get the ordered list of team members, the indexes in the list will denote
        'the Number of each robot
        Me._TeamMembers = New List(Of String)
        Dim members() As String = Strings.Split(teamConfig.TeamMembers, ",")

        For Each member As String In members
            'Dim parts() As String = Strings.Split(member, "-")
            Dim memberName As String = member
            Me._TeamMembers.Add(memberName)

        Next

        Console.WriteLine("[commAgent]: whole number: {0}", Me._TeamMembers.Count)

        'initialize connectivity matrix
        ReDim Me._Connections(Me._TeamMembers.Count - 1, Me._TeamMembers.Count - 1)
        For i As Integer = 0 To Me._Connections.GetLength(0) - 1
            For j As Integer = 0 To Me._Connections.GetLength(1) - 1
                If i = j Then
                    Me._Connections(i, j) = False
                End If
            Next
        Next

        'setup comm actor
        'JDH-- temporary changed (1272-1366), to guarantee that CommActor is always created (for testing)
        'Moved Operator special case to OperatorAgent
        If agentConfig.SpawnFromCommander = False Then
            Me.CreateCommActor(teamConfig)
        End If

    End Sub

#End Region

#Region " CommActor "

    Private _CommActor As CommActor
    '    Protected ReadOnly Property CommActor() As CommActor
    ReadOnly Property CommActor() As CommActor
        Get
            Return Me._CommActor
        End Get
    End Property

    Public Overridable Sub CreateCommActor(ByVal teamConfig As TeamConfig)
        Me._CommActor = New CommActor(teamConfig, Me.Name, Me.Number, teamConfig.LocalHost)
        If Not IsNothing(Me._CommActor) Then
            Me.Mount(Me._CommActor)
        End If

    End Sub

    ''' <summary>
    ''' We can only start communicating when the robot is spawned in the simulator
    ''' </summary>
    ''' <remarks></remarks>
    Protected Friend Overrides Sub OnAgentSpawned()
        MyBase.OnAgentSpawned()
        If Not IsNothing(Me._CommActor) Then
            Me._CommActor.StartComm()
        End If
    End Sub

    Public Sub startComm()
        MyBase.OnAgentSpawned()
        If Not IsNothing(Me._CommActor) Then
            Me._CommActor.StartComm()
        End If
    End Sub


    ''' <summary>
    ''' Stop communicating when robots stop.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Friend Overrides Sub OnAgentStopped()
        If Not IsNothing(Me._CommActor) Then
            Me._CommActor.StopComm()
        End If
        MyBase.OnAgentStopped()
    End Sub

#End Region


#Region " Maintain List of Connected Team Members "

    Protected Friend _TeamMembers As List(Of String)
    Protected Friend _OperatorName As String = String.Empty

    Public Function IsOperator(ByVal name As String) As Boolean
        Return Not String.IsNullOrEmpty(name) AndAlso name = Me._OperatorName
    End Function


    'to store the team members I am directly connected to
    Private _ConnectedMembers As New List(Of String)

    'to store the full connectivity of the team
    Private _Connections(,) As Boolean

    ''' <summary>
    ''' Only checks direct connections
    ''' </summary>
    ''' <param name="member"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function IsDirectlyConnectedTo(ByVal member As String) As Boolean
        SyncLock Me._ConnectedMembers
            Return Me._ConnectedMembers.Contains(member)
        End SyncLock
    End Function

    ''' <summary>
    ''' Uses the connection matrix, so also checks indirect connections
    ''' </summary>
    ''' <param name="member"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function IsConnectedTo(ByVal member As String) As Boolean
        If Me.IsDirectlyConnectedTo(member) Then
            'that was quick!
            Return True

        Else
            'ok, check indirect connections

            Dim iFrom As Integer
            Dim iMember As Integer
            Dim iCount As Integer

            'copy relevant values from teammembers into local vars
            'so we can release the mutex and allow access by other threads
            SyncLock Me._TeamMembers
                iFrom = Me._TeamMembers.IndexOf(Me.Name)
                iMember = Me._TeamMembers.IndexOf(member)
                iCount = Me._TeamMembers.Count
            End SyncLock

            If iFrom < 0 OrElse iCount < 1 Then
                'TeamMembers not initiated yet
                If member = Me.Name Then
                    Return True
                Else
                    Return False
                End If
            End If

            'treat the matrix as a graph that is searched breadth-first
            Dim iFrontier As New List(Of Integer)

            SyncLock Me._Connections

                'initialize the search frontier with the team members I am directly connected to 
                For idx As Integer = 0 To iCount - 1
                    If Me._Connections(iFrom, idx) Then
                        'there is a connection between team members iFrom and idx
                        iFrontier.Add(idx)
                    End If
                Next

                'to avoid duplicate loops (i.e. infinite loops)
                Dim iHistory As New List(Of Integer)

                While Not iFrontier.Count = 0

                    'update history
                    iHistory.AddRange(iFrontier)

                    'construct next frontier
                    Dim iNextFrontier As New List(Of Integer)

                    'process current frontier
                    For Each iFromTemp As Integer In iFrontier

                        'check every team member
                        For iToTemp As Integer = 0 To iCount - 1
                            If Not iHistory.Contains(iToTemp) AndAlso Me._Connections(iFromTemp, iToTemp) Then
                                'we found a yet unprocessed connection

                                If iToTemp = iMember Then
                                    'found the needle!
                                    Return True
                                End If

                                'put it in the next frontier (if not there already)
                                If Not iNextFrontier.Contains(iToTemp) Then
                                    iNextFrontier.Add(iToTemp)
                                End If

                            End If
                        Next
                    Next

                    'done with current frontier, prepare for next iteration
                    iFrontier = iNextFrontier
                    iNextFrontier.Clear()

                End While

            End SyncLock

            'apparently we did not find it
            Return False

        End If

    End Function

    Public Function IsConnectedToOperator() As Boolean
        Return Me.IsConnectedTo(Me._OperatorName)
    End Function


    Public Function IsDirectlyConnectedToOperator() As Boolean
        Return Me.IsDirectlyConnectedTo(Me._OperatorName)
    End Function

    Protected Overridable Function GetDirectlyConnectedMembers() As String()
        SyncLock Me._ConnectedMembers
            Return Me._ConnectedMembers.ToArray
        End SyncLock
    End Function


    Protected Friend Overridable Sub NotifyAgentConnected(ByVal agentName As String)

        If Me.Name = agentName Then
            Console.WriteLine("[{0}] - Adding myself as directly connected '{1}'", Me.Name, agentName)
        End If

        'update list of team members I am directly connected to
        SyncLock Me._ConnectedMembers
            If Not Me._ConnectedMembers.Contains(agentName) Then
                Me._ConnectedMembers.Add(agentName)
            End If
        End SyncLock

        'get indexes for use in the connection matrix
        Dim iFrom As Integer, iTo As Integer
        SyncLock Me._TeamMembers
            iFrom = Me._TeamMembers.IndexOf(Me.Name)
            iTo = Me._TeamMembers.IndexOf(agentName)
        End SyncLock

        'set the boolean flag in connection matrix
        SyncLock Me._Connections
            'the matrix is symmetic
            Me._Connections(iFrom, iTo) = True
            Me._Connections(iTo, iFrom) = True
        End SyncLock

        'The other Pose estimates are implicit in the Sync messages
        Me._CommActor.RelayPoseEstimateUpdate(Me.CurrentPoseEstimate, Me.GetDirectlyConnectedMembers)

    End Sub

    Protected Friend Overridable Sub NotifyAgentDisconnected(ByVal agentName As String)

        'update list of team members I am directly connected to
        SyncLock Me._ConnectedMembers
            If Me._ConnectedMembers.Contains(agentName) Then
                Me._ConnectedMembers.Remove(agentName)
            End If
        End SyncLock

        'get indexes for use in the connection matrix
        Dim iFrom As Integer, iTo As Integer
        SyncLock Me._TeamMembers
            iFrom = Me._TeamMembers.IndexOf(Me.Name)
            iTo = Me._TeamMembers.IndexOf(agentName)
        End SyncLock

        'set the boolean flag in connection matrix
        SyncLock Me._Connections
            'the matrix is symmetic
            Me._Connections(iFrom, iTo) = False
            Me._Connections(iTo, iFrom) = False
        End SyncLock

    End Sub


    Protected Friend Overridable Sub NotifyConnectedToReceived(ByVal agentName As String, ByVal connectedTo() As String)

        Dim iFrom As Integer
        Dim iTo(connectedTo.Length - 1) As Integer

        SyncLock Me._TeamMembers

            'the index of the agent that sent this info
            iFrom = Me._TeamMembers.IndexOf(agentName)

            'the indexes of the team members he is directly connected to
            Dim idx As Integer = 0
            For Each name As String In connectedTo
                iTo(idx) = Me._TeamMembers.IndexOf(name)
                idx += 1
            Next

        End SyncLock

        'update connection matrix
        SyncLock Me._Connections

            'first reset everything to false
            For idx As Integer = 0 To Me._Connections.GetLength(1) - 1
                'the matrix is symmetric
                Me._Connections(iFrom, idx) = False
                Me._Connections(idx, iFrom) = False
            Next

            'then set all flags for those connected
            For Each idx As Integer In iTo
                'the matrix is symmetric
                Me._Connections(iFrom, idx) = True
                Me._Connections(idx, iFrom) = True
            Next

        End SyncLock

        'Sending many bc to myself
        'Me._CommActor.RelayPoseEstimateUpdate(Me.CurrentPoseEstimate, connectedTo)


    End Sub

#End Region

#Region " Incoming Notifications from Comm "

    Protected Friend Overridable Sub NotifyActorCommandReceived(ByVal command As String)
        ' RIKNOTE: for manual control inputs?
        Console.WriteLine("[{0}] - Executing '{1}'", Me.Name, command)
        Me.SendUsarSimCommand(command)
    End Sub

    Private _PathLossTable As New QuadTree(Of Vector2)


    Protected Friend Overridable Sub NotifySignalStrengthReceived(ByVal agentName As String, ByVal pathloss As Double)
        'Arnoud: The WssDevice does a RequestSignalStrength at regular intervals
        'Store the combination of PathLoss and Distance in a quadTree
        If agentName = Me._OperatorName AndAlso Me._TeamPoses.ContainsKey(agentName) Then
            Dim operatorPose As Pose2D
            Dim operatorDistance As Double

            operatorPose = Me._TeamPoses(agentName)
            Console.WriteLine(String.Format("[{0}] - CommStation is ({1:f2},{2:f2}) m and Robot on ({3:f2},{4:f2}) m", Me.Name, operatorPose.X / 1000, operatorPose.Y / 1000, Me.CurrentPoseEstimate.X / 1000, Me.CurrentPoseEstimate.Y / 1000))

            operatorDistance = ((Me.CurrentPoseEstimate.X - operatorPose.X) ^ 2 + (Me.CurrentPoseEstimate.Y - operatorPose.Y) ^ 2) ^ 0.5 / 1000
            'Console.WriteLine(String.Format("[{0}] - Estimated distance to CommStation is {1:f2} m for {2:f2} dBm", Me.Name, operatorDistance, pathloss))

            Dim combination As New Vector2(operatorDistance, pathloss)
            _PathLossTable.Insert(combination)
            combination = Nothing
        End If

        'Julian RoboCup2011
        'Also send to operator signal strengths between robots, to display in comm link layer in UsarCommander
        'This could be resource intensive (not sure), if not needed feel free to comment line below out
        If Not IsOperator(Me.Name) Then
            'Console.WriteLine(String.Format("Sending teammate signal strength message to operator (me to {0})", agentName))
            Me.CommActor.RelayTeammateSignalStrength(Me.Name, Me._OperatorName, agentName, pathloss)
        End If
    End Sub
    

    ' RIKNOTE: only FrontierExploration makes use of this, NOT AutonomousExploration...
    Protected Friend Function EstimateSignalStrengthAtDistance(ByVal distance As Double) As Double
        'Retrieve the combination of PathLoss and Distance in a quadTree

        Dim neighbour As Vector2
        Dim operatorPose, myPose As Pose2D

        If IsNothing(Me.CommActor) Then 'Spawned directly robot with operator-name
            Return -55 'dBm 'high change there will be communication
        End If

        If Me._TeamPoses.ContainsKey(Me._OperatorName) Then
            operatorPose = Me._TeamPoses(Me._OperatorName)
        Else
            Return Me.CommActor._eCutoff + 0.6 '50% change there will be communication
        End If

        If Me._TeamPoses.ContainsKey(Me.Name) Then
            myPose = Me._TeamPoses(Me.Name)
        Else
            Return Me.CommActor._eCutoff + 0.6 '50% change there will be communication
        End If

        

        Dim myDistance As Double = ((myPose.X - operatorPose.X) ^ 2 + (myPose.Y - operatorPose.Y) ^ 2) ^ 0.5 / 1000
        Dim attenuation As Double = 10 * Me.CommActor._eN * Log((myDistance) / Me.CommActor._eDo, Exp(1)) / Log(10, Exp(1)) - 10 * Me.CommActor._eN * Log((distance) / Me.CommActor._eDo, Exp(1)) / Log(10, Exp(1))

        ' distance part of formula in UsarComServer.uc: 
        ' Pd=ePdo-(10*eN*(Loge(d/eDo)/Loge(10)))-(ctr*eAttenFac);
        Console.WriteLine(String.Format("[EstimateSignalStrengthAtDistance] - CommStation is at distance {0:f2} m, I an currently on distance {1:f2} m and expects an attenuation of {2:f2} dBm", distance, myDistance, attenuation))

        Dim combination As New Vector2(distance, Me.CommActor.GetSignalStrengthToOperator(Me.Name) + attenuation)


        'Search in the band between No Obstacles and Many Obstacles
        'Start with from the current PathLoss
        neighbour = _PathLossTable.FindNearestNeighbour(combination, 16) '8m and 8 dBm

        If Not IsNothing(neighbour) Then
            Return neighbour.Y
        ElseIf Not IsNothing(Me.CommActor) Then
            Return Me.CommActor._eCutoff + 0.6 '50% change there will be communication
        Else
            Return -92.8 'dBm 'small change there will be communication
        End If
    End Function
    Protected Friend _TeamPoses As New Dictionary(Of String, Pose2D)

    Protected Friend Overridable Sub NotifyPoseEstimateReceived(ByVal agentName As String, ByVal pose As Pose2D)
        'Console.WriteLine("[{0}] - Receiving PoseEstimate from '{1}'", Me.Name, agentName)
        SyncLock Me._TeamPoses
            If Me._TeamPoses.ContainsKey(agentName) Then
                Me._TeamPoses(agentName) = pose
            Else
                Me._TeamPoses.Add(agentName, pose)
            End If
        End SyncLock
    End Sub

#End Region

#Region " Relay Interesting Information through Comm "
    Private _Counter As Integer = 5




    Public Overrides Sub NotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        MyBase.NotifyPoseEstimateUpdated(pose)

        SyncLock Me._TeamPoses
            If Me._TeamPoses.ContainsKey(Me.Name) Then
                Me._TeamPoses(Me.Name) = pose
            Else
                Me._TeamPoses.Add(Me.Name, pose)
            End If
        End SyncLock


        If Not IsNothing(Me._CommActor) Then
            Dim connectedTo() As String = Me.GetDirectlyConnectedMembers
            If _Counter = 5 Then
                Me._CommActor.RelayPoseEstimateUpdate(pose, connectedTo)
            End If
            _Counter = _Counter - 1
            If _Counter = 0 Then
                _Counter = 5 'Relay 1:5 PoseUpdates 
            End If
        End If

    End Sub

#End Region

#Region " Camera "

    Protected Friend Overrides Sub OnCamRepl(ByVal fromRobot As String, ByVal camIm As System.Drawing.Bitmap)
        'MyBase.OnCamRepl(fromRobot, camIm)
        'The real robot sends the image to the operator

        If Not Me.IsOperator(Me.Name) AndAlso Not IsNothing(Me._CommActor) Then
            'Console.WriteLine(String.Format("[{0}] -- Client replies with camera image", Me.Name))
            Me._CommActor.RelayCamRepl(fromRobot, camIm)
        End If

    End Sub
#End Region

#Region " Target Locations "
    Public Overrides Sub SendNewTarget(ByVal toRobot As String, ByVal target As Pose2D)
        SyncLock Me._TargetLocations
            If Not Me.IsOperator(Me.Name) AndAlso Not IsNothing(Me._CommActor) Then
                Console.WriteLine("fafa")
                Me._CommActor.RelayTargetLocation(toRobot, target)
            Else
                Me.AddNewTarget(target)
            End If
        End SyncLock
    End Sub
#End Region


#Region " Change behavior "
    Overrides Sub ChangeBehaviour(ByVal newBehaviour As String)
    End Sub
#End Region

#Region " Teammate Signal Strength "
    Protected Friend Overridable Sub UpdateSignalStrengthMatrix(ByVal fromRobot As String, ByVal toRobot As String, ByVal strength As Double)


    End Sub
#End Region

End Class
