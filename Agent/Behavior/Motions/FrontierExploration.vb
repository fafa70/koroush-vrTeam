Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Imports System.Threading

Imports UvARescue.Math
Imports UvARescue.Communication



Imports AForge.Imaging
Imports AForge.Imaging.Filters


Public Class FrontierExploration
    Inherits Motion
    Implements IManifoldObserver

    Protected ReadOnly _AgentName As String = ""
    Protected ReadOnly _Manifold As Manifold

    Protected ReadOnly _ManifoldImage As ManifoldImage
    Protected ReadOnly _FrontierTools As FrontierTools
    Protected ReadOnly _PowerLaw As Double = 1.0



#Region " Constructor "

    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)

        Me._PowerLaw = control.Agent.BehaviorBalance
        Me._AgentName = control.Agent.Name
        Me._Manifold = control.Agent.Manifold
        Me._ManifoldImage = control.Agent.ManifoldImage
        Me._FrontierTools = New FrontierTools

        With control.Agent.TeamConfig

            'Me._OperatorName = .OperatorName

            'Dim members() As String = Strings.Split(.TeamMembers, ",")
            'For Each member As String In members
            'Dim parts() As String = Strings.Split(member, "-")
            'Dim name As String = parts(1)
            'Me._TeamMembers.Add(name)
            'Next

        End With

        Me._Manifold.AddObserver(Me)

    End Sub

#End Region

#Region " Activation / DeActivation "

    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()

        'first stop the agent
        Me.Control.Agent.Halt()
        Me._IsStopped = True
        Me._LastStopped = Now

        If Not Me.SelectNewTarget(Me.Control.Agent.CurrentPoseEstimate) Then
            'no target selected, nothing sensible to do, so just turn
            'Me.Control.Agent.TurnRight(0.2)
            'no target available, change motion
            Me.Control.ActivateMotion(MotionType.CorridorWalk, True)
            Exit Sub
        End If

        'execute the current path plan
        Me.FollowPath(Me.Control.Agent.CurrentPoseEstimate)

    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()

        'first stop the agent
        Me.Control.Agent.Halt()
        Me._IsStopped = True
        Me._LastStopped = Now

    End Sub

#End Region

#Region " Own Pose Updates "

    Private _IsStopped As Boolean = True

    Private _TargetInfo As FrontierInfo
    Private _TargetPos As Vector2

    Public Property TargetPos() As Vector2
        Get
            Return Me._TargetPos
        End Get
        Set(ByVal value As Vector2)
            Me._TargetPos.X = value.X
            Me._TargetPos.Y = value.Y
        End Set
    End Property

    Private _LastStopped As DateTime = DateTime.MinValue

    Protected Overrides Sub ProcessPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        MyBase.ProcessPoseEstimateUpdated(pose)

        'Check whether robot is too close to a TeamMate
        Dim distToRobot As Double = Double.MaxValue
        Dim teammatePose As Pose2D

        With Me.Control.Agent
            'In loop for all agents having lower number 
            '(Robots with higher numbers give way to those with lower numbers.
            ' This order chosen so that robots do not run into the ComStation,
            ' which will have the lowest number)            
            For i As Integer = 0 To (.Number - 1)
                'Calculate distance between the two robots
                If ._TeamPoses.ContainsKey(._TeamMembers(i)) Then
                    teammatePose = ._TeamPoses.Item(._TeamMembers(i))
                    distToRobot = ((pose.X - teammatePose.X) ^ 2 + (pose.Y - teammatePose.Y) ^ 2) ^ 0.5
                End If

                'If too close
                If (distToRobot < 1000) Then
                    .Halt()
                    Console.WriteLine("[{0}] Too close to {1}!  Avoiding...", .Name, ._TeamMembers(i))
                    If (._TeamMembers(i) = .Name) Then
                        Me.Control.ActivateMotion(MotionType.CorridorWalk, True)
                    Else
                        Me.Control.ActivateMotion(MotionType.AvoidTeamMate, True)
                    End If
                    Exit Sub
                End If
            Next
        End With

        'If this point reached, then there is no conflict with TeamMates or ComStation.

        'check if we should select a new target
        Dim bNewTarget As Boolean = False
        Dim bNewPlan As Boolean = False

        If IsNothing(Me._TargetInfo) Then
            'we don't have a target yet
            bNewTarget = True
            bNewPlan = False
            Me._LastStopped = Now
        Else
            'check if we are done with the current target
            Dim tdpose As Vector2 = Me._TargetPos - pose.Position
            Dim tdist As Double = (tdpose.X ^ 2 + tdpose.Y ^ 2) ^ 0.5
            bNewTarget = tdist < 500

            'compute displacement from where I started on this path
            Dim idpose As Vector2 = pose.Position - Me._CurrentInit
            Dim idist As Double = (idpose.X ^ 2 + idpose.Y ^ 2) ^ 0.5
            bNewTarget = bNewTarget OrElse idist > 4000

            'replanning after a minute
            bNewPlan = Now - Me._LastStopped > TimeSpan.FromSeconds(60)

        End If

        If bNewTarget OrElse bNewPlan Then

            If Not Me._IsStopped Then
                'first stop the agent
                Me.Control.Agent.Halt()
                Me._IsStopped = True
                Me._LastStopped = Now

                Exit Sub
            End If

            Me._IsStopped = False

            If bNewPlan Then 'no progress, maybe stuck
                Console.WriteLine(String.Format("[FrontierExploration] - too long ({0:f2} s) no progress", Now - Me._LastStopped))

                Me.Control.ActivateMotion(MotionType.CorridorWalk, True)

                Exit Sub
            End If

            If bNewTarget Then


                If Not Me.SelectNewTarget(pose) Then
                    'no target selected, nothing sensible to do, so just turn
                    'Me.Control.Agent.TurnRight(0.2)
                    'no target available, change motion
                    'fafa actives randomWalk because of make base faster
                    'Me.Control.ActivateMotion(MotionType.CorridorWalk, True)
                    Me.Control.ActivateMotion(MotionType.RandomWalk, True)
                    Exit Sub
                End If

            End If
        End If

        'execute the current path plan
        Me.FollowPath(pose)

    End Sub
#End Region

#Region "FollowPath"


    ' Array containing the current path to follow
    Private _CurrentPath As Vector2()
    Private _CurrentInit As Vector2
    Private _CurrentGoal As Vector2
    Private _CurrentIndex As Integer

    ' Travels to the next entry in _CurrentPath
    Private Sub FollowPath(ByVal pose As Pose2D)

        If IsNothing(Me._CurrentPath) Then
            'we have no path to follow
            Exit Sub
        ElseIf IsNothing(Me._CurrentGoal) Then
            'init plan
            Me._CurrentIndex = 0
            Me._CurrentGoal = Me._CurrentPath(Me._CurrentIndex)
        End If

        'compute displacement to currently planned (intermediate) goal 
        Dim dgoal As Vector2 = Me._CurrentGoal - pose.Position
        Dim gdist As Double = (dgoal.X ^ 2 + dgoal.Y ^ 2) ^ 0.5

        If gdist < 250 Then
            'we have reached current goal, set new goal
            While Me._CurrentIndex < Me._CurrentPath.Length - 1

                Me._CurrentIndex += 1
                Me._CurrentGoal = Me._CurrentPath(Me._CurrentIndex)

                dgoal = Me._CurrentGoal - pose.Position
                gdist = (dgoal.X ^ 2 + dgoal.Y ^ 2) ^ 0.5

                If gdist > 400 Then
                    'we found a new goal
                    Console.WriteLine(String.Format("[Behavior] - New Goal Position = ({0:f2} , {1:f2})", Me._CurrentGoal.X / 1000, Me._CurrentGoal.Y / 1000))
                    Exit While
                End If

            End While
        End If

        Dim dradians As Double = Atan2(dgoal.Y, dgoal.X) - pose.Rotation
        Dim dangle As Double = dradians / PI * 180

        While dangle >= 180
            dangle -= 360
        End While
        While dangle < -180
            dangle += 360
        End While

        With Me.Control.Agent
            If Abs(dangle) < 15 Then
                .Drive(1.5F)
            ElseIf dangle > 0 Then
                .TurnRight(0.3)
            ElseIf dangle < 0 Then
                .TurnLeft(0.3)
            End If
        End With

    End Sub
#End Region

#Region "SelectNewTarget"


    Protected Sub pathPlanTo(ByVal pose As Pose2D)
        Dim distance As Double = Double.MaxValue

        Dim distance2 As Double = Double.MaxValue
        Dim goalPose As Pose2D = New Pose2D()

        goalPose.X = pose.X
        goalPose.Y = pose.Y
        goalPose.Rotation = Atan2(goalPose.Y, goalPose.X)

        Dim dangle As Double = goalPose.Rotation * 180 / PI
        Dim dangle2 As Double = Me.Control.Agent.GroundTruthPose.Yaw * 180 / PI
        'fuzzy part of path planning is here


        ' Dim mainDistance As AForge.Fuzzy.LinguisticVariable = New AForge.Fuzzy.LinguisticVariable("distance", 0, 200)
        'Dim state1 As AForge.Fuzzy.TrapezoidalFunction = New AForge.Fuzzy.TrapezoidalFunction(0, 5, 10, 15)
        'Dim soclose As AForge.Fuzzy.FuzzySet = New AForge.Fuzzy.FuzzySet("soclose", state1)
        'Dim state2 As AForge.Fuzzy.TrapezoidalFunction = New AForge.Fuzzy.TrapezoidalFunction(12, 20, 30, 40)
        'Dim close As AForge.Fuzzy.FuzzySet = New AForge.Fuzzy.FuzzySet("close", state2)
        'Dim state3 As AForge.Fuzzy.TrapezoidalFunction = New AForge.Fuzzy.TrapezoidalFunction(35, 50, 70, 90)
        'Dim far As AForge.Fuzzy.FuzzySet = New AForge.Fuzzy.FuzzySet("far", state3)
        'Dim state4 As AForge.Fuzzy.TrapezoidalFunction = New AForge.Fuzzy.TrapezoidalFunction(80, 100, TrapezoidalFunction.EdgeType.Right)
        'Dim sofar As AForge.Fuzzy.FuzzySet = New AForge.Fuzzy.FuzzySet("sofar", state4)
        'mainDistance.AddLabel(soclose)
        'mainDistance.AddLabel(close)
        'mainDistance.AddLabel(far)
        'mainDistance.AddLabel(sofar)
        'For i As Integer = 0 To 15
        'soclose.GetMembership(i)
        'Next

        'For i As Integer = 12 To 40
        'close.GetMembership(i)
        'Next

        'For i As Integer = 35 To 90
        'far.GetMembership(i)
        'Next

        'For i As Integer = 80 To 200
        'sofar.GetMembership(i)
        'Next

        'Dim speed As LinguisticVariable = New LinguisticVariable("speed", 0, 2)
        'Dim soslow As SingletonFunction = New SingletonFunction(0.5)
        'Dim speedState1 As FuzzySet = New FuzzySet("soslow", soslow)
        'Dim slow As SingletonFunction = New SingletonFunction(1.0)
        'Dim speedState2 As FuzzySet = New FuzzySet("slow", slow)
        'Dim fast As SingletonFunction = New SingletonFunction(1.5)
        'Dim speedState3 As FuzzySet = New FuzzySet("fast", fast)
        'Dim sofast As SingletonFunction = New SingletonFunction(2.0)
        'Dim speedState4 As FuzzySet = New FuzzySet("sofast", sofast)
        'speed.AddLabel(speedState1)
        'speed.AddLabel(speedState2)
        'speed.AddLabel(speedState3)
        'speed.AddLabel(speedState4)
        'Dim FuzzyDatabase As Database = New Database()
        'FuzzyDatabase.AddVariable(mainDistance)
        'FuzzyDatabase.AddVariable(speed)

        'Dim _interface As InferenceSystem = New InferenceSystem(FuzzyDatabase, New CentroidDefuzzifier(1000))
        '_interface.NewRule("rule1", "IF distance IS soclose THEN speed IS soslow")
        '_interface.NewRule("rule2", "IF distance IS close THEN speed IS slow")
        '_interface.NewRule("rule3", "IF distance IS far THEN speed IS fast")
        '_interface.NewRule("rule4", "IF distance IS sofar THEN speed IS sofast")

        '_interface.SetInput("distance", CSng(distance2))
        'Dim fuzzyoutput As FuzzyOutput = _interface.ExecuteInference("speed")

        'Dim oc As FuzzyOutput.OutputConstraint = fuzzyoutput.OutputList
        'Dim counters As Integer = 0
        'For Each oc As FuzzyOutput.OutputConstraint In fuzzyoutput.OutputList
        'Console.WriteLine(oc.Label + " - " + oc.FiringStrength.ToString())
        'counters = counters + 1
        'Next
        ' Dim output As Single = oc.FiringStrength()
        Dim currPose As Pose2D = New Pose2D(Me.Control.Agent.GroundTruthPose.X, Me.Control.Agent.GroundTruthPose.Y, Me.Control.Agent.GroundTruthPose.Yaw)
        Console.WriteLine(String.Format("goal angle : {0},robot angle : {1}", dangle, dangle2))
        Console.WriteLine()
        'Console.WriteLine(String.Format("counters {0}", counters))
        While (dangle2 - dangle > 8)

            Me.Control.Agent.TurnLeft(0.2F)

            dangle2 = dangle2 - 0.015


        End While
        Me.Control.Agent.Halt()
        While (dangle2 - dangle < -8)
            Me.Control.Agent.TurnRight(0.2F)
            dangle2 = dangle2 + 0.015
        End While
        Me.Control.Agent.Halt()
        distance = ((goalPose.X - Me.Control.Agent.GroundTruthPose.X) ^ 2 + (goalPose.Y - Me.Control.Agent.GroundTruthPose.Y) ^ 2) ^ 0.5
        distance2 = ((goalPose.X - Me.Control.Agent.GroundTruthPose.X) ^ 2 + (goalPose.Y - Me.Control.Agent.GroundTruthPose.Y) ^ 2) ^ 0.5


        If (distance > distance2 * Sin(8)) Then


            Me.Control.Agent.DifferentialDrive(1.0F, 0)
            While (distance > distance2 * Sin(8))
                For i As Integer = 120 To 150
                    ' If (Me.Control.Agent.laserRangeSensor.Range(i) > 2.3 Or Me.Control.Agent.laserRangeSensor.Range(i) < 1) Then
                    'Me.Control.Agent.DifferentialDrive(0.1, 0.3)
                    'End If
                Next



                'For i As Integer = 120 To 150
                ' If ( > 2.3 OrElse laser.Range(i) < 1) Then
                'Me.Agent.DifferentialDrive(0.1, 0.3)
                'Me.AlertObstacle = True
                'Dim alert As String = "HOK - OBS - " + laser.Range(i).ToString
                'Me.Agent.NotifyAlertReceived(alert)
                'End If
                'Next
                distance = ((goalPose.X - Me.Control.Agent.GroundTruthPose.X) ^ 2 + (goalPose.Y - Me.Control.Agent.GroundTruthPose.Y) ^ 2) ^ 0.5
                'this part is for multithreading

                System.Windows.Forms.Application.DoEvents()

            End While
        End If




        Me.Control.Agent.Halt()
        Console.WriteLine(String.Format("last robot pose : {0} {1}", Me.Control.Agent.GroundTruthPose.X, Me.Control.Agent.GroundTruthPose.Y))




    End Sub



    Private Function SelectNewTarget(ByVal agentPose As Pose2D) As Boolean


        'reset current target
        Me._TargetInfo = Nothing
        Me._TargetPos = Nothing
        Me._CurrentInit = Nothing
        Me._CurrentPath = Nothing

        'get info about all my current frontiers
        Dim infos() As FrontierInfo = Me._FrontierTools.ExtractFrontierInfo(Me._ManifoldImage)

        'Dim bestpath1 As Point()
        'Dim bestpath2 As Point()
        'Dim bestpath3 As Point()
        Dim my_init As Point
        Dim diagonal As Double

        'typically: eCutoff_dist = 40.000m
        'eCutoff_dist = _eDo * Exp(Log(10, Exp(1)) * (_ePdo - _eCutoff) / (10 * _eN))
        'typically: 5 obstacles -> eMaxObs_dist = 30m
        'typically: 2 obstacles -> eMaxObs_dist = 2400m
        'eMaxObs_dist = _eDo * Exp(Log(10, Exp(1)) * (_ePdo - (_eCutoff + _eMaxObs * Me._eAttenFac)) / (10 * _eN))

        diagonal = ((Me._ManifoldImage.ManifoldWidth ^ 2 + Me._ManifoldImage.ManifoldHeight ^ 2) ^ 0.5) / 1000
        diagonal = Min(diagonal, 60.0) 'otherwise pathplanning takes more than 20 seconds

        my_init = New Point( _
                             CInt((agentPose.X + Me._ManifoldImage.TransformationOffset.X) * Me._ManifoldImage.TransformationScale), _
                             CInt((agentPose.Y + Me._ManifoldImage.TransformationOffset.Y) * Me._ManifoldImage.TransformationScale))
        Console.WriteLine(String.Format("{0}-[Behavior] - {1:f2} PowerLaw", Me._AgentName, _PowerLaw))

        Console.WriteLine(String.Format("{0}-[FrontierExploration] - step 1 ({1:f2} s)", Me._AgentName, Now - Me._LastStopped))

        'acquire occupancy grid
        Using freearea As Bitmap = Me._FrontierTools.ExtractFreeArea(Me._ManifoldImage)
            Using walls As Bitmap = Me._FrontierTools.ExtractWalls(Me._ManifoldImage)
                Using occupancy As Bitmap = New Subtract(walls).Apply(freearea)
                    'Using mobility As Bitmap = Me._FrontierTools.ExtractMobilityArea(Me._ManifoldImage)

                    'Arnoud GO2008 day3: use mobility as mask (or add) over occupancy 

                    'compute utilities for every robot-frontier pair
                    Dim utils(Me.Control.Agent._TeamMembers.Count - 1, infos.Length - 1) As Double
                    Dim pathplanned(Me.Control.Agent._TeamMembers.Count - 1, infos.Length - 1) As Boolean

                    Dim bestmemberutil(Me.Control.Agent._TeamMembers.Count - 1) As Double
                    Dim goodmemberutil(Me.Control.Agent._TeamMembers.Count - 1) As Double 'second best
                    Dim bestmemberfrontier(Me.Control.Agent._TeamMembers.Count - 1) As Integer
                    Dim bestmember As Integer
                    Dim bestfrontier As Integer
                    Dim bestutil As Double = Double.MinValue
                    Dim max_area As Double
                    Dim candidates As Integer
                    Dim operator_present As Boolean = False


                    Dim info As FrontierInfo
                    Dim infoPose As Pose2D, comm_success(infos.Length - 1) As Double
                    Dim name As String, pose As Pose2D, init As Point
                    Dim dist_robot As Double, dist_operator As Double, pathloss As Double
                    Dim info_gain As Double, max_dist As Double, util As Double
                    Dim path As Point() = Nothing


                    For i As Integer = 0 To Me.Control.Agent._TeamMembers.Count - 1

                        'initialize util to nothing
                        bestmemberutil(i) = Double.MinValue

                        If Me.Control.Agent._TeamMembers(i) = Me.Control.Agent._OperatorName Then
                            operator_present = True
                        End If

                    Next i

                    max_area = Double.MinValue
                    candidates = 0

                    For j As Integer = 0 To infos.Length - 1
                        info = infos(j)

                        If IsNothing(info) Then
                            Continue For
                        End If

                        ' choose the best after checking 1/e 
                        If info.Area > max_area Then

                            '[JULNOTE] Isn't infos.length the number of frontiers?  why multiply by 1/e?
                            If j < (infos.Length - 1) * 0.369 OrElse infos.Length = 1 Then
                                max_area = info.Area
                                candidates = 1
                            Else
                                candidates = candidates + 1
                            End If
                        End If

                    Next j


                    If candidates = 0 Then
                        Console.WriteLine(String.Format("{0}-[Behavior] - {1} frontiers", Me._AgentName, candidates))
                    Else
                        Console.WriteLine(String.Format("{0}-[Behavior] - {1} frontiers of size {2:f2} m^2 or larger", Me._AgentName, candidates, max_area))
                    End If

                    Console.WriteLine(String.Format("{0}-[FrontierExploration] - step 2 ({1:f2} s)", Me._AgentName, Now - Me._LastStopped))


                    'max_area = 1 'fixed threshold, do not consider Areas smaller 1 m^2
                    'candidates = 0

                    'For j As Integer = 0 To infos.Length - 1
                    'info = infos(j)

                    ' choose the best after checking 1/e 
                    ' If info.Area > max_area Then
                    'candidates = candidates + 1
                    'End If

                    'Next j

                    'If max_area > Double.MinValue Then
                    'Console.WriteLine(String.Format("[Behavior] - {0} frontiers of size {1:f2} m^2", candidates, max_area))
                    'End If

                    Dim signalCutOff As Double = -93 'dBm

                    If Not IsNothing(Me.Control.Agent.CommActor) Then
                        signalCutOff = Me.Control.Agent.CommActor._eCutoff
                    End If

                    For j As Integer = infos.Length - 1 To 0 Step -1

                        info = infos(j)
                        If IsNothing(info) OrElse IsNothing(info.Center) Then
                            Continue For
                        End If

                        infoPose = New Pose2D((info.Center.X / Me._ManifoldImage.TransformationScale) - Me._ManifoldImage.TransformationOffset.X, (info.Center.Y / Me._ManifoldImage.TransformationScale) - Me._ManifoldImage.TransformationOffset.Y, 0.0)

                        If Me.Control.Agent._TeamPoses.ContainsKey(Me.Control.Agent._OperatorName) Then

                            pose = Me.Control.Agent._TeamPoses(Me.Control.Agent._OperatorName)

                            dist_operator = ((pose.X - infoPose.X) ^ 2 + (info.Center.Y - infoPose.Y) ^ 2) ^ 0.5 / 1000
                            ' RIKNOTE: THIS MOTION (AUTONOMOUS EXPLORATION BEHAVIOR) SHOULD NOT USE SIGNAL STRENGTH
                            pathloss = Me.Control.Agent.EstimateSignalStrengthAtDistance(dist_operator)
                            Console.WriteLine(String.Format("{0}-[FrontierExploration] - At distance {1:f2} m to CommStation the pathloss is estimated on {2:f2} dBm", Me._AgentName, dist_operator, pathloss))

                        Else
                            dist_operator = diagonal / 2 'nothing known, so assume that the CommStation is in the center of the known world
                            pathloss = Me.Control.Agent.EstimateSignalStrengthAtDistance(diagonal)
                            Console.WriteLine(String.Format("{0}-[FrontierExploration] - At distance {1:f2} m to CommStation the pathloss is roughly estimated on {2:f2} dBm", Me._AgentName, dist_operator, pathloss))
                        End If

                        comm_success(j) = 2 * System.Math.Pow(10, signalCutOff - pathloss)
                        comm_success(j) = Max(comm_success(j), 0.0)
                        comm_success(j) = 1 - Min(comm_success(j), 1.0)

                        Console.WriteLine(String.Format("{0}-[FrontierExploration] - Communication success is estimated on {1} %", Me._AgentName, CInt(comm_success(j) * 100)))

                        For i As Integer = 0 To Me.Control.Agent._TeamMembers.Count - 1

                            'initialize util to nothing
                            utils(i, j) = Double.MinValue
                            pathplanned(i, j) = False

                            name = Me.Control.Agent._TeamMembers(i)
                            If name = Me.Control.Agent._OperatorName AndAlso Not name = Me._AgentName Then
                                'operator cannot explore, expect when I am the operator myself (UsarOperator has no behavior)
                                Continue For

                            ElseIf Not Me.Control.Agent._TeamPoses.ContainsKey(name) Then
                                'we cannot plan paths for robot whose pose we don't know
                                If name = Me._AgentName Then
                                    Console.WriteLine(String.Format("{0}-[FrontierExploration] - Cannot SelectNewTarget if current position is unknown", Me._AgentName))
                                    Return False
                                End If
                                Continue For

                            ElseIf info.Area < max_area Then
                                'don't waste time on too small frontiers
                                Continue For

                            End If


                            pose = Me.Control.Agent._TeamPoses(name)
                            init = New Point( _
                             CInt((pose.X + Me._ManifoldImage.TransformationOffset.X) * Me._ManifoldImage.TransformationScale), _
                             CInt((pose.Y + Me._ManifoldImage.TransformationOffset.Y) * Me._ManifoldImage.TransformationScale))


                            'compute utility
                            dist_robot = ((info.Center.X - init.X) ^ 2 + (info.Center.Y - init.Y) ^ 2) ^ 0.5 / 10
                            'radius = Sqrt(info.Area / PI)
                            info_gain = info.Area * comm_success(j)
                            util = info_gain / dist_robot ^ _PowerLaw

                            If util > bestmemberutil(i) Then

                                goodmemberutil(i) = bestmemberutil(i)
                                bestmemberutil(i) = util
                                bestmemberfrontier(i) = j
                                If util > bestutil Then
                                    bestutil = util
                                    bestmember = i
                                    bestfrontier = j
                                End If

                            End If

                            utils(i, j) = util

                        Next i
                    Next j

                    If bestutil = Double.MinValue Then
                        'No Frontier found
                        Console.WriteLine("{0}-[FrontierExploration] - No frontier found ({1} present)", Me._AgentName, infos.Length)

                        Return False
                    End If

                    Console.WriteLine(String.Format("{0}-[FrontierExploration] - step 3 ({1:f2} s)", Me._AgentName, Now - Me._LastStopped))


                    Dim assignments As New Dictionary(Of String, FrontierInfo)
                    Dim replan As Boolean = True
                    Dim member As Integer = 0


                    Console.WriteLine(String.Format("{0}-[Behavior] - Manifold has a diagonal of {1:f2} m", Me._AgentName, diagonal))

                    While (member < Me.Control.Agent._TeamPoses.Count) AndAlso (Not assignments.ContainsKey(Me._AgentName)) AndAlso (bestutil > 0.0) AndAlso (Now - Me._LastStopped < TimeSpan.FromSeconds(20)) 'Do not think too long

                        info = infos(bestmemberfrontier(bestmember))

                        pose = Me.Control.Agent._TeamPoses(Me.Control.Agent._TeamMembers(bestmember))
                        init = New Point( _
                         CInt((pose.X + Me._ManifoldImage.TransformationOffset.X) * Me._ManifoldImage.TransformationScale), _
                         CInt((pose.Y + Me._ManifoldImage.TransformationOffset.Y) * Me._ManifoldImage.TransformationScale))

                        dist_robot = ((info.Center.X - init.X) ^ 2 + (info.Center.Y - init.Y) ^ 2) ^ 0.5 / 10

                        Console.WriteLine(String.Format("{0}-[FrontierExploration] - Plan Path for {1} to Frontier of size {2:f2} m^2 at distance {3:f2} m and communication probability {3:f0}", Me._AgentName, Me.Control.Agent._TeamMembers(bestmember), infos(bestmemberfrontier(bestmember)).Area, dist_robot, comm_success(bestmemberfrontier(bestmember))))


                        'plan path to this frontier
                        If goodmemberutil(bestmember) > 0.0 And _PowerLaw > 0.0 Then

                            max_dist = (info.Area * comm_success(bestmemberfrontier(bestmember))) / goodmemberutil(bestmember) ^ (1 / _PowerLaw)
                            If max_dist > diagonal Then
                                max_dist = diagonal
                            End If
                        Else
                            max_dist = diagonal
                        End If

                        Console.WriteLine(String.Format("{0}-[FrontierExploration] - Stop pathplanning at distance {2:f2} m", Me._AgentName, Me.Control.Agent._TeamMembers(bestmember), max_dist))

                        path = Me.RecomputePath(occupancy, init, New Point(CInt(info.Center.X), CInt(info.Center.Y)), max_dist)

                        'give the threads of the other robots time to do their control
                        'Thread.Sleep(250) 'Specify TimeSpan zero to indicate that this thread should be suspended to allow other waiting threads to execute

                        If Not IsNothing(path) Then

                            pathplanned(bestmember, bestmemberfrontier(bestmember)) = True

                            util = (info.Area * comm_success(bestmemberfrontier(bestmember))) / (path.Length / 10) ^ _PowerLaw
                            dist_robot = path.Length / 10
                            If Me.Control.Agent._TeamMembers(bestmember) = Me._AgentName Then
                                'replan = False
                            End If
                        Else
                            'Try again, after doing pp for goodmember
                            pathplanned(bestmember, bestmemberfrontier(bestmember)) = False

                            If goodmemberutil(bestmember) > 1.1 * Double.MinValue Then
                                util = goodmemberutil(bestmember) * 0.9
                            Else
                                util = Double.MinValue
                            End If
                            dist_robot = max_dist * 1.1
                        End If

                        utils(bestmember, bestmemberfrontier(bestmember)) = util

                        For j As Integer = 0 To infos.Length - 1

                            If assignments.ContainsValue(infos(j)) Then
                                'frontier was already assigned
                                Continue For
                            End If

                            If utils(bestmember, j) > utils(bestmember, bestmemberfrontier(bestmember)) Then

                                bestmemberutil(bestmember) = utils(bestmember, j)
                                bestmemberfrontier(bestmember) = j

                            End If
                        Next j

                        If pathplanned(bestmember, bestmemberfrontier(bestmember)) Then

                            member = member + 1 'This agent is ready

                            If utils(bestmember, bestmemberfrontier(bestmember)) > Double.MinValue Then
                                Console.WriteLine(String.Format("{0}-[FrontierExploration] - Best Frontier for {1} of size {2:f2} m^2 at distance {3:f2} m and communication success {4:f0}%-> Utility {5:f2}", Me._AgentName, Me.Control.Agent._TeamMembers(bestmember), infos(bestmemberfrontier(bestmember)).Area, dist_robot, 100 * comm_success(bestmemberfrontier(bestmember)), util))

                                assignments.Add(Me.Control.Agent._TeamMembers(bestmember), infos(bestmemberfrontier(bestmember)))
                                If Me.Control.Agent._TeamMembers(bestmember) = Me._AgentName Then
                                    replan = False
                                End If
                            End If

                        End If

                        'Decide which agent is next

                        bestutil = Double.MinValue

                        For i As Integer = 0 To Me.Control.Agent._TeamMembers.Count - 1

                            If assignments.ContainsKey(Me.Control.Agent._TeamMembers(i)) Then
                                Continue For
                            End If

                            'recompute bestmember
                            bestmemberutil(i) = Double.MinValue

                            For j As Integer = 0 To infos.Length - 1

                                If assignments.ContainsValue(infos(j)) Then
                                    'frontier was already assigned
                                    Continue For
                                End If

                                If utils(i, j) > bestmemberutil(i) Then

                                    goodmemberutil(i) = bestmemberutil(i)
                                    bestmemberutil(i) = utils(i, j)
                                    bestmemberfrontier(i) = j

                                End If
                            Next j

                            If utils(i, bestmemberfrontier(i)) > bestutil Then

                                bestutil = utils(i, bestmemberfrontier(i))
                                bestmember = i
                                bestfrontier = bestmemberfrontier(i)

                            End If

                        Next i
                        Console.WriteLine(String.Format("{0}-[FrontierExploration] - step x ({1:f2} s)", Me._AgentName, Now - Me._LastStopped))


                    End While


                    'get my assignment
                    If Not assignments.ContainsKey(Me._AgentName) Then
                        'Time was up
                        Dim myNumber As Integer = 0

                        For i As Integer = 0 To Me.Control.Agent._TeamMembers.Count - 1
                            If Me.Control.Agent._TeamMembers(i) = Me._AgentName Then
                                myNumber = i
                            End If
                        Next

                        'find best frontier for this agent
                        bestmemberutil(myNumber) = Double.MinValue

                        For j As Integer = 0 To infos.Length - 1

                            If assignments.ContainsValue(infos(j)) Then
                                'frontier was already assigned
                                Continue For
                            End If

                            If utils(myNumber, j) > bestmemberutil(myNumber) Then

                                goodmemberutil(myNumber) = bestmemberutil(myNumber)
                                bestmemberutil(myNumber) = utils(myNumber, j)
                                bestmemberfrontier(myNumber) = j

                            End If
                        Next j

                        If bestmemberutil(myNumber) > 0.0 Then
                            Console.WriteLine(String.Format("{0}-[FrontierExploration] - Quickly choose Frontier of size {1:f2} m^2, communication success {2:f0}%-> Utility {3:f2}", Me.Control.Agent._TeamMembers(myNumber), infos(bestmemberfrontier(myNumber)).Area, 100 * comm_success(bestmemberfrontier(myNumber)), bestmemberutil(myNumber)))

                            assignments.Add(Me.Control.Agent._TeamMembers(myNumber), infos(bestmemberfrontier(myNumber)))
                            replan = True
                        Else
                            'infos seems not threadsave, infos.length is here in some cases not longer known.
                            Try
                                Console.WriteLine(String.Format("{0}-[FrontierExploration] There were not enough frontiers to also keep this agent busy ({1} present)"), Me._AgentName, infos.Length)
                            Catch e As FormatException
                            End Try
                            Return False
                        End If
                    End If


                    'plan path to my assigned frontier
                    pose = agentPose
                    info = assignments(Me._AgentName)

                    init = New Point( _
                       CInt((pose.X + Me._ManifoldImage.TransformationOffset.X) * Me._ManifoldImage.TransformationScale), _
                       CInt((pose.Y + Me._ManifoldImage.TransformationOffset.Y) * Me._ManifoldImage.TransformationScale))

                    If replan Then
                        path = Me.RecomputePath(occupancy, init, New Point(CInt(info.Center.X), CInt(info.Center.Y)), diagonal)
                    End If


                    If IsNothing(path) Then
                        Console.WriteLine(String.Format("{0}-[FrontierExploration] No path to the destination {1},{2} could be found"), Me._AgentName, info.Center.X, info.Center.Y)
                        Return False
                    End If

                    Me._TargetInfo = info
                    Me._CurrentInit = pose.Position

                    'get global coord of the new target
                    With Me._ManifoldImage
                        Me._TargetPos = New Vector2( _
                         Me._TargetInfo.Center.X / .TransformationScale - .TransformationOffset.X, _
                         Me._TargetInfo.Center.Y / .TransformationScale - .TransformationOffset.Y)
                    End With

                    Console.WriteLine(String.Format("{0}-[Behavior] - New Frontier Selected ({1:f2} , {2:f2})", Me._AgentName, Me._TargetPos.X / 1000, Me._TargetPos.Y / 1000))
                    Me._LastStopped = Now 'Give it some time to try this new frontier


                    ReDim Me._CurrentPath(path.Length - 1)
                    Dim p As Integer = 0
                    For Each pt As Point In path
                        Me._CurrentPath(p) = New Vector2( _
                         pt.X / Me._ManifoldImage.TransformationScale - Me._ManifoldImage.TransformationOffset.X, _
                         pt.Y / Me._ManifoldImage.TransformationScale - Me._ManifoldImage.TransformationOffset.Y)
                        p += 1
                    Next

                    Me._CurrentIndex = -1
                    Me._CurrentGoal = Nothing
                    'End Using
                End Using
            End Using
        End Using


        Return True

    End Function

    Private Function RecomputePath(ByVal occupancy As Bitmap, ByVal init As Point, ByVal goal As Point, ByVal max_dist As Double) As Point()

        'get shortest path on occupancy grid
        Dim path As Point() = Me._FrontierTools.ComputePathPlan(occupancy, init, goal, True, max_dist)
        If IsNothing(path) Then
            Return Nothing
        End If

        'use the plan that was acquired on the freespace to create a small window 
        'so we don't waste too much time on the safety image extraction
        Dim window As New Rectangle(init.X, init.Y, 10, 10)
        Dim rect As Rectangle
        For Each pt As Point In path
            rect = New Rectangle(pt, New Size(1, 1))
            If Not window.Contains(rect) Then
                window = Rectangle.Union(window, rect)
            End If
        Next

        window.Inflate(10, 10)
        window.Intersect(Me._ManifoldImage.ImageRect)

        'now use window to compute a safe path on the gaussian blurred image
        Using gauss As Bitmap = Me._FrontierTools.ExtractGaussianImage(occupancy, window)
            path = Me._FrontierTools.ComputePathPlan(gauss, init, goal, False, max_dist)
        End Using

        Return path
    End Function

    'OBSOLETE
    Private Function SelectNewTargetUsingSafetyMap(ByVal pose As Pose2D) As Boolean

        Dim infos() As FrontierInfo = Me._FrontierTools.ExtractFrontierInfo(Me._ManifoldImage)

        Dim init As New Point( _
            CInt((pose.X + Me._ManifoldImage.TransformationOffset.X) * Me._ManifoldImage.TransformationScale), _
            CInt((pose.Y + Me._ManifoldImage.TransformationOffset.Y) * Me._ManifoldImage.TransformationScale))

        Dim maxinfo As FrontierInfo = Nothing
        Dim maxutil As Double = Double.MinValue
        Dim maxpath As Point() = Nothing

        Using freearea As Bitmap = Me._FrontierTools.ExtractFreeArea(Me._ManifoldImage)
            Using walls As Bitmap = Me._FrontierTools.ExtractWalls(Me._ManifoldImage)
                Using occupancy As Bitmap = New Subtract(walls).Apply(freearea)

                    Dim path As Point(), dist As Double, util As Double
                    For Each info As FrontierInfo In infos

                        If info.Area < 1 Then
                            'do not process frontiers smaller than 1 m^2
                            Continue For
                        End If

                        If info.Area > 100 Then
                            'Too much space for FrontierExploration
                            Return False
                        End If

                        'compute path in reverse direction so that the routine will return quicker in case of disjoint frontiers
                        path = Me._FrontierTools.ComputePathPlan(occupancy, New Point(CInt(info.Center.X), CInt(info.Center.Y)), init, True, 80.0)
                        If Not IsNothing(path) Then

                            'select target with largest utility
                            dist = path.Length / 10 'in meters
                            util = info.Area / dist ^ 2

                            If util > maxutil Then
                                maxinfo = info
                                maxutil = util
                                maxpath = path
                            End If

                        End If

                    Next

                End Using
            End Using
        End Using


        If IsNothing(maxinfo) Then
            'no target selected
            Me._TargetInfo = Nothing
            Me._TargetPos = Nothing
            Me._CurrentPath = Nothing
            Return False

        Else
            'use the plan that was acquired on the freespace to create a small window 
            'so we don't waste too much time on the safety image extraction
            Dim window As New Rectangle(init.X, init.Y, 10, 10)
            Dim rect As Rectangle
            For Each p As Point In maxpath
                rect = New Rectangle(p, New Size(1, 1))
                If Not window.Contains(rect) Then
                    window = Rectangle.Union(window, rect)
                End If
            Next

            window.Inflate(10, 10)
            window.Intersect(Me._ManifoldImage.ImageRect)

            Using occupancy As Bitmap = Me._FrontierTools.ExtractSafetyImage(Me._ManifoldImage, window)
                'Using occupancy As Bitmap = Me._FrontierTools.ExtractGaussianImage(Me._ManifoldImage)

                'get robot pose in image coords
                Dim goal As New Point(CInt(maxinfo.Center.X), CInt(maxinfo.Center.Y))

                'get path plan in image coords
                Dim path() As Point = Me._FrontierTools.ComputePathPlan(occupancy, init, goal, False, 80.0)
                If IsNothing(path) Then
                    'no path 
                    Return False
                End If


                Me._TargetInfo = maxinfo

                'get global coord of the new target
                With Me._ManifoldImage
                    Me._TargetPos = New Vector2( _
                        Me._TargetInfo.Center.X / .TransformationScale - .TransformationOffset.X, _
                        Me._TargetInfo.Center.Y / .TransformationScale - .TransformationOffset.Y)
                End With

                Console.WriteLine(String.Format("[Behavior] - New Frontier Selected ({0:f2} , {1:f2})", Me._TargetPos.X / 1000, Me._TargetPos.Y / 1000))


                ReDim Me._CurrentPath(path.Length - 1)
                Dim i As Integer = 0
                For Each p As Point In path
                    Me._CurrentPath(i) = New Vector2( _
                        p.X / Me._ManifoldImage.TransformationScale - Me._ManifoldImage.TransformationOffset.X, _
                        p.Y / Me._ManifoldImage.TransformationScale - Me._ManifoldImage.TransformationOffset.Y)
                    i += 1
                Next

                Me._CurrentIndex = -1
                Me._CurrentGoal = Nothing

            End Using

        End If

        Return True

    End Function

#End Region

#Region " Sensor Updates "

    Protected Overrides Sub ProcessSensorUpdate(ByVal sensor As Sensor)
        MyBase.ProcessSensorUpdate(sensor)

        If sensor.SensorType = LaserRangeSensor.SENSORTYPE_RANGESCANNER Then
            Me.ProcessLaserRangeData(sensor.SensorName, DirectCast(sensor, LaserRangeSensor).PeekData)
        ElseIf sensor.SensorType = SonarSensor.SENSORTYPE_SONAR Then
            Me.ProcessSonarData(DirectCast(sensor, SonarSensor).CurrentData)
        ElseIf sensor.SensorType = InsSensor.SENSORTYPE_INS Then
            Me.ProcessInsData(DirectCast(sensor, InsSensor).CurrentData)
        ElseIf sensor.SensorType = GroundTruthSensor.SENSORTYPE_GROUNDTRUTH Then
            Me.ProcessGroundTruthData(DirectCast(sensor, GroundTruthSensor).CurrentData)
        ElseIf sensor.SensorType = VictimSensor.SENSORTYPE_VICTIM Then
            Me.ProcessVictimData(DirectCast(sensor, VictimSensor).CurrentData)
        End If

    End Sub

    Protected Overridable Sub ProcessVictimData(ByVal current_data As VictimData)

        'Check whether robot is too close to a Victim
        Dim distToPart As Double = Double.MaxValue
        Dim pose As Pose2D = Me.Control.Agent.CurrentPoseEstimate

        Dim factor As Single = 1000 'to convert from m to mm

        'With Me.Control.Agent

        For Each part As VictimData.VictimPart In current_data.Parts

            If part.PartName = "chest" Or part.PartName = "head" Or part.PartName = "leg" Or part.PartName = "hand" Then

                'note that the part is observed in local coords wrt the current pose of the robot
                'transform to global coordinate wrt pose using a Pose2D object with dummy rotation
                Dim pglobal As Vector2 = New Pose2D(CType(part.X * factor, Single), CType(part.Y * factor, Single), 0).ToGlobal(pose).Position

                distToPart = ((pose.X - pglobal.X) ^ 2 + (pose.Y - pglobal.Y) ^ 2) ^ 0.5

            End If

            If (distToPart < 1000) Then
                Me.Control.Agent.Halt()
                Console.WriteLine("[{0}] Too close to {1}!  Avoiding...", Me.Control.Agent.Name, part.PartName)

                Me.Control.ActivateMotion(MotionType.AvoidVictim, True) ' explicit return, when victim is 2000 mm away

                Continue For 'one part too close is enough

            End If


        Next
        'End With

    End Sub

    Protected Overridable Sub ProcessLaserRangeData(ByVal sensorName As String, ByVal laser As LaserRangeData)
    End Sub

    Protected Overridable Sub ProcessSonarData(ByVal sonar As SonarData)
    End Sub

    Protected Overridable Sub ProcessInsData(ByVal ins As InsData)
    End Sub

    Protected Overridable Sub ProcessGroundTruthData(ByVal current_data As GroundTruthData)

        'Check whether robot is tilting excessively
        If Abs(NormalizeAngle(current_data.Roll)) > 0.1 OrElse Abs(NormalizeAngle(current_data.Pitch)) > 0.3 Then
            With Me.Control.Agent
                .Reverse(0.5F)
            End With
            Console.WriteLine(String.Format("[FrontierExploration] - Something: tilting ({0:f2} , {1:f2}) rad -> Retreat", NormalizeAngle(current_data.Roll), NormalizeAngle(current_data.Pitch)))

            Me.Control.ActivateMotion(MotionType.CorridorWalk, True)
        End If

    End Sub

    Private Function NormalizeAngle(ByVal radians As Double) As Double
        While radians > PI
            radians -= 2 * PI
        End While
        While radians <= -PI
            radians += 2 * PI
        End While
        Return radians
    End Function

#End Region

#Region " IManifoldObserver - Keep Track of other Agents "

    'maintained by CommAgent
    'Private _OperatorName As String = ""
    'Private _TeamMembers As New List(Of String) 
    'Private _TeamPoses As New Dictionary(Of String, Pose2D)

    Public Sub NotifyAgentMoved(ByVal agent As Agent, ByVal pose As Math.Pose2D, ByVal commMatrix As Double(,)) Implements IManifoldObserver.NotifyAgentMoved
        'triggered by LocalizeAgent(proxy,Nothing,pose), when received by Comm
    End Sub

    Public Sub NotifyCleared() Implements IManifoldObserver.NotifyCleared
    End Sub

    Public Sub NotifyPatchInserted(ByVal patch As Patch) Implements IManifoldObserver.NotifyPatchInserted
    End Sub
    Public Sub NotifyRelationInserted(ByVal relation As Relation) Implements IManifoldObserver.NotifyRelationInserted
    End Sub
    Public Sub NotifyVictimUpdated(ByVal victim As VictimObservation) Implements IManifoldObserver.NotifyVictimUpdated
    End Sub

    'Public Sub NotifyOmnicamUpdated(ByVal patch As Patch) Implements IManifoldObserver.NotifyOmnicamUpdated
    'End Sub

#End Region

End Class
