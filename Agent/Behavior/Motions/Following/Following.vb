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


Public Class Following
    Inherits Motion
    Implements IManifoldObserver

    Protected ReadOnly _Agent As Agent
    Protected ReadOnly _AgentName As String = ""
    Protected ReadOnly _Manifold As Manifold
    Protected ReadOnly _ManifoldImage As ManifoldImage
    Protected ReadOnly _FrontierTools As FrontierTools
    Protected ReadOnly _PowerLaw As Double = 1.0

#Region " Constructor "

    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)

        Me._Agent = control.Agent
        Me._PowerLaw = control.Agent.BehaviorBalance
        Me._AgentName = control.Agent.Name
        Me._Manifold = control.Agent.Manifold
        Me._ManifoldImage = control.Agent.ManifoldImage
        Me._FrontierTools = New TraversabilityTools

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

        'NICK
        Me._Agent._ResetCurrentPath = False

        'first stop the agent
        Me.Control.Agent.Halt()
        Me._IsStopped = True
        Me._LastStopped = Now

        If Not Me.SelectNewTarget(Me.Control.Agent.CurrentPoseEstimate) Then
            Exit Sub
        End If

        Me.FollowPath(Me.Control.Agent.CurrentPoseEstimate)

    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        Me.Control.Agent.Halt()
        Me._IsStopped = True
        Me._LastStopped = Now

    End Sub

#End Region

#Region " Own Pose Updates "

    Private _IsStopped As Boolean = True
    Private _LastStopped As DateTime = DateTime.MinValue

    Protected Overrides Sub ProcessPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        MyBase.ProcessPoseEstimateUpdated(pose)

        'NICK
        If Me._Agent._ResetCurrentPath Then
            Me._CurrentPath = Nothing
            Me._CurrentGoal = Nothing
            Me._Agent._ResetCurrentPath = False
        End If

        'NICK: Dirty
        'There is no plan and no current target
        If IsNothing(Me._CurrentPath) AndAlso IsNothing(Me._CurrentGoal) AndAlso Me._Agent._TargetLocations.Count > 0 Then
            Console.WriteLine("Picking new target from queue")

            If Not Me.SelectNewTarget(Me.Control.Agent.CurrentPoseEstimate) Then
                Exit Sub
            End If

            Me.FollowPath(Me.Control.Agent.CurrentPoseEstimate)
            Exit Sub
        End If


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
                If (distToRobot < 1000 And ._TeamMembers(i) <> Me.Control.Agent.Name) Then
                    .Halt()
                    Console.WriteLine("[{0}] Too close to {1}!  AvoidingTeammate...", .Name, ._TeamMembers(i))
                    If (._TeamMembers(i).Equals(Me.Control.Agent._OperatorName)) Then
                        Me.Control.ActivateMotion(MotionType.RandomWalk_Following, True)
                    Else
                        Me.Control.ActivateMotion(MotionType.AvoidTeamMate_Following, True)
                    End If
                    Exit Sub
                End If
            Next
        End With

        'If this point reached, then there is no conflict with TeamMates or ComStation.

        'check if we should select a new target
        Dim bNewTarget As Boolean = False
        Dim bNewPlan As Boolean = False

        'check if we are done with the current target
        'If IsNothing(Me.newTarget) Then
        '    bNewTarget = True
        'Else
        '    Dim tdpose As Vector2 = Me.newTarget - pose.Position
        '    Dim tdist As Double = (tdpose.X ^ 2 + tdpose.Y ^ 2) ^ 0.5
        '    bNewTarget = tdist < 500
        'End If

        'NICK
        If Not IsNothing(Me.newTarget) Then
            Dim tdpose As Vector2 = Me.newTarget - pose.Position
            Dim tdist As Double = (tdpose.X ^ 2 + tdpose.Y ^ 2) ^ 0.5
            bNewTarget = tdist < 500
        End If

        'compute displacement from where I started on this path
        If Not IsNothing(Me._CurrentInit) Then
            Dim idpose As Vector2 = pose.Position - Me.Control.Agent.CurrentPoseEstimate.Position
            Dim idist As Double = (idpose.X ^ 2 + idpose.Y ^ 2) ^ 0.5
            bNewTarget = bNewTarget OrElse idist > 4000
        End If

        'replanning after a minute
        'bNewPlan = Now - Me._LastStopped > TimeSpan.FromSeconds(60)
        'NICK
        bNewPlan = False


        ''End If

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
                Me.Control.ActivateMotion(MotionType.CorridorWalk_Following, True)
                Exit Sub
            End If

            If bNewTarget Then
                If Not Me.SelectNewTarget(pose) Then
                    Console.WriteLine("Unable to plan path to {0}", Me._CurrentGoal)
                    Exit Sub
                End If

            End If
        End If

        'execute the current path plan
        Me.FollowPath(pose)

    End Sub
#End Region

#Region "FollowPath"


    Private _CurrentPath As Vector2()
    Private _CurrentInit As Vector2
    Private _CurrentGoal As Vector2
    Private _CurrentIndex As Integer

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

        'If gdist < 250 Then
        If gdist < 350 Then
            'we have reached current goal, set new goal
            While Me._CurrentIndex < Me._CurrentPath.Length - 1

                Me._CurrentIndex += 1
                Me._CurrentGoal = Me._CurrentPath(Me._CurrentIndex)

                dgoal = Me._CurrentGoal - pose.Position
                gdist = (dgoal.X ^ 2 + dgoal.Y ^ 2) ^ 0.5

                'If gdist > 400 Then
                If gdist > 550 Then
                    'we found a new goal
                    Console.WriteLine(String.Format("[Behavior] - New Goal Position = ({0:f2} , {1:f2})", Me._CurrentGoal.X / 1000, Me._CurrentGoal.Y / 1000))
                    Exit While
                End If

            End While
        End If

        Dim dradians As Double = Atan2(dgoal.Y, dgoal.X)
        'If dgoal.Y > 0 Then
        'dradians = dradians + PI
        'End If


        'dradians = dradians - pose.Rotation
        'Dim dangle As Double = dradians / PI * 180

        'While dangle >= 180
        '    dangle -= 360
        'End While
        'While dangle < -180
        '    dangle += 360
        'End While

        Dim posRot As Double = pose.Rotation


        Dim hasToRotate As Double = (dradians - posRot)

        While hasToRotate >= PI
            hasToRotate -= 2 * PI
        End While
        While hasToRotate < -PI
            hasToRotate += 2 * PI
        End While

        hasToRotate = hasToRotate / -PI

        ' Console.WriteLine("DIST: {3}, Pose: {0}, target: {1}, toRotate: {2}", posRot, dradians, hasToRotate, gdist)

        If Abs(hasToRotate) > 0.15 Then
            With Me.Control.Agent
                If hasToRotate < 0 Then
                    .TurnRight(0.8)
                Else
                    .TurnLeft(0.8)
                End If
            End With
        Else
            With Me.Control.Agent
                .DifferentialDrive(System.Math.Min(4.0F, Convert.ToSingle(gdist * 0.005F)), Convert.ToSingle(hasToRotate))
            End With
        End If


        'With Me.Control.Agent
        '    If Abs(dangle) < 15 Then
        '        .Drive(1.5F)
        '    ElseIf dangle > 0 Then
        '        .TurnRight(0.3)
        '    ElseIf dangle < 0 Then
        '        .TurnLeft(0.3)
        '    End If
        'End With

        ' new added
        If (Me._CurrentIndex >= Me._CurrentPath.Length - 1) Then
            SelectNewTarget(Me.Control.Agent.CurrentPoseEstimate)
        End If

    End Sub
#End Region

#Region "SelectNewTarget"


    Private Function SelectNewTarget(ByVal agentPose As Pose2D) As Boolean
        Return DrawPlan(getNextTarget())
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
        ElseIf sensor.SensorType = RfidSensor.SENSORTYPE_RFID Then
            Me.ProcessRFIDData(DirectCast(sensor, RfidSensor).CurrentData)
        End If

    End Sub

    Protected Overridable Sub ProcessRFIDData(ByVal current_data As RfidData)

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
                Console.WriteLine("[{0}] Too close to {1}!  Avoiding Victim...", Me.Control.Agent.Name, part.PartName)

                Me.Control.ActivateMotion(MotionType.AvoidVictim_Following, True) ' explicit return, when victim is 2000 mm away

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

        If Abs(NormalizeAngle(current_data.Roll)) > 0.1 OrElse Abs(NormalizeAngle(current_data.Pitch)) > 0.1 Then
            With Me.Control.Agent
                .Reverse(0.5F)
            End With
            Console.WriteLine(String.Format("[FrontierExploration] - Something: tilting ({0:f2} , {1:f2}) rad -> Retreat", NormalizeAngle(current_data.Roll), NormalizeAngle(current_data.Pitch)))

            Me.Control.ActivateMotion(MotionType.CorridorWalk_Following, True)
        Else
            'Me.ProcessPoseEstimateUpdated(New Pose2D(current_data.X, current_data.Y, current_data.Yaw))
            ProcessPoseEstimateUpdated(Me.Control.Agent.CurrentPoseEstimate)
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

    Private Function DrawPlan(ByVal tempTarget As Vector2) As Boolean
        Console.WriteLine(tempTarget)
        If IsNothing(tempTarget) Then
            Return False
        End If

        Dim path As Point() = Nothing
        Dim pose As Pose2D, init As Point, target As Point

        'get info about all my current frontiers
        Dim infos() As FrontierInfo = Me._FrontierTools.ExtractFrontierInfo(Me._ManifoldImage)
        Using freearea As Bitmap = Me._FrontierTools.ExtractFreeArea(Me._ManifoldImage)
            Using walls As Bitmap = Me._FrontierTools.ExtractWalls(Me._ManifoldImage)
                Using occupancy As Bitmap = New Subtract(walls).Apply(freearea)

                    pose = Me.Control.Agent.CurrentPoseEstimate
                    init = New Point( _
                    CInt((pose.X + Me._ManifoldImage.TransformationOffset.X) * Me._ManifoldImage.TransformationScale), _
                    CInt((pose.Y + Me._ManifoldImage.TransformationOffset.Y) * Me._ManifoldImage.TransformationScale))
                    target = New Point( _
                    CInt((tempTarget.X + Me._ManifoldImage.TransformationOffset.X) * Me._ManifoldImage.TransformationScale), _
                    CInt((tempTarget.Y + Me._ManifoldImage.TransformationOffset.Y) * Me._ManifoldImage.TransformationScale))
                    Dim diagonal As Double = ((Me._ManifoldImage.ManifoldWidth ^ 2 + Me._ManifoldImage.ManifoldHeight ^ 2) ^ 0.5) / 1000
                    diagonal = Min(diagonal, 60.0) 'otherwise pathplanning takes more than 20 seconds


                    path = Me.RecomputePath(occupancy, init, target, diagonal)
                    If IsNothing(path) Then
                        Return False
                    End If

                    Console.WriteLine(String.Format("[Behavior] - New Path  ({2:f2} , {3:f2}) --> ({0:f2} , {1:f2})", tempTarget.X / 1000, tempTarget.Y / 1000, pose.X / 1000, pose.Y / 1000))
                    '//////////////////////////////////////
                    ReDim Me._CurrentPath(path.Length - 1)
                    Dim p As Integer = 0
                    For Each pt As Point In path
                        Me._CurrentPath(p) = New Vector2( _
                         pt.X / Me._ManifoldImage.TransformationScale - Me._ManifoldImage.TransformationOffset.X, _
                         pt.Y / Me._ManifoldImage.TransformationScale - Me._ManifoldImage.TransformationOffset.Y)

                        'Console.WriteLine("Path: {0},{1}", Me._CurrentPath(p)(0), Me._CurrentPath(p)(1))

                        p += 1
                    Next

                    Me._CurrentInit = pose.Position
                    Me._CurrentIndex = -1
                    Me._CurrentGoal = Nothing

                    'EMO
                    'Me._Agent.PathPlanGoalPos2 = Me._CurrentPath

                End Using
            End Using
        End Using
        Return True
    End Function

    Private Function getNextTarget() As Vector2
        ' Temporary list
        'Me._Agent.targets.enqueue(New Vector2(-9000, 35000))
        'Me._Agent.targets.enqueue(New Vector2(-9000, 45000))
        '''''''''''''''''''''''''

        Console.WriteLine("getNextTarget: {0}", Me._CurrentGoal)

        If IsNothing(Me._CurrentGoal) Then
            Try
                Me._CurrentGoal = Me._Agent._TargetLocations.Dequeue().Position()
            Catch ex As InvalidOperationException
                'the target queue was empty
                Console.WriteLine("target queue empty")

                'NICK
                Me.Control.Agent.Halt()
                Me._IsStopped = True
                Me._LastStopped = Now

                Return Nothing
            End Try
        End If

        'Console.WriteLine("curpose: {0},{1}, curgoal: {2},{3}, length: {4}", Me._Agent.CurrentPoseEstimate.Position.X, Me._Agent.CurrentPoseEstimate.Position.Y, Me._CurrentGoal.X, Me._CurrentGoal.Y, (Me._Agent.CurrentPoseEstimate.Position - Me._CurrentGoal).EuclidianLength())

        If (Me._Agent.CurrentPoseEstimate.Position - Me._CurrentGoal).EuclidianLength() < 1000 Then
            Console.WriteLine("Position {0} reached", Me._CurrentGoal)
            ' We've reached the target
            Me._CurrentGoal = Nothing

            'NICK
            Me._CurrentPath = Nothing
            Me.Control.Agent.Halt()

            'This recursive call may cycle through a number of targets if we so happen
            'to have reached multiple of them.
            Return Me.getNextTarget()
        Else
            'We've not reached the target
            Return Me._CurrentGoal
        End If
    End Function

#Region " IManifoldObserver - Keep Track of other Agents "

    'maintained by CommAgent
    'Private _OperatorName As String = ""
    'Private _TeamMembers As New List(Of String) 
    'Private _TeamPoses As New Dictionary(Of String, Pose2D)
    Private newTarget As Vector2

    ' Test if the corresponding robot is the one should be followed
    Public Sub NotifyAgentMoved(ByVal agent As Agent, ByVal pose As Math.Pose2D, ByVal commMatrix As Double(,)) Implements IManifoldObserver.NotifyAgentMoved
        If Not (Me.Control.Agent.Name.Equals(agent.Name)) And agent.RobotModel.Equals(New String(CType("AirRobot", Char()))) Then
            Dim dist As Double = (pose.Position.X ^ 2 + pose.Position.Y ^ 2) ^ 0.5
            If (dist > 0.1) Then
                newTarget = pose.Position
            End If
        End If
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
