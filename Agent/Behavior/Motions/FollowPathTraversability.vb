' Motion that lets the robot follow a path while making use of the traversability index of the path

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

Public Class FollowPathTraversability
    Inherits Motion

    Private _LastStopped As DateTime = DateTime.MinValue
    Private _IsStopped As Boolean = True
    Private _TraversabilityTools As TraversabilityTools = New TraversabilityTools
    Protected ReadOnly _Manifold As Manifold
    Protected ReadOnly _ManifoldImage As ManifoldImage

    Public Sub New(ByVal control As MotionControl)
        MyBase.New(control)
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Motions\FollowPathTraversability.vb::New() called")
        Me._Manifold = control.Agent.Manifold
        Me._ManifoldImage = control.Agent.ManifoldImage
    End Sub

#Region " Activation / DeActivation "
    Private target As Vector2 = Me.Control.Agent.PathPlanGoalPos

    Private followingPath As Boolean = True
    Protected Overrides Sub OnActivated()
        MyBase.OnActivated()
        followingPath = True
        Console.WriteLine("[RIKNOTE] Activated FollowPathTraversability Motion!")
        'first stop the agent
        Me.Control.Agent.Halt()
        Me._IsStopped = True
        Me._LastStopped = Now
        Dim init As Vector2
        Dim pose As Pose2D = Me.Control.Agent.CurrentPoseEstimate
        'Dim curlocation As Point = New Point(Me.Control.Agent.CurrentPoseEstimate.X, Me.Control.Agent.CurrentPoseEstimate.Y)
        Dim target As Vector2 = Me.Control.Agent.PathPlanGoalPos

        target = New Vector2( _
                      CInt((1000 * target.X + Me._ManifoldImage.TransformationOffset.X) * Me._ManifoldImage.TransformationScale), _
                      CInt((1000 * target.Y + Me._ManifoldImage.TransformationOffset.Y) * Me._ManifoldImage.TransformationScale))
        Dim path As List(Of Point)

        ' On activation, plan the path and then follow it
        Using freearea As Bitmap = Me._TraversabilityTools.ExtractFreeArea(Me._ManifoldImage)
            Using walls As Bitmap = Me._TraversabilityTools.ExtractWalls(Me._ManifoldImage)
                Using occupancy As Bitmap = New Subtract(walls).Apply(freearea)
                    init = New Point( _
                      CInt((pose.X + Me._ManifoldImage.TransformationOffset.X) * Me._ManifoldImage.TransformationScale), _
                      CInt((pose.Y + Me._ManifoldImage.TransformationOffset.Y) * Me._ManifoldImage.TransformationScale))
                    path = Me._TraversabilityTools.ComputePathPlanTraversability(occupancy, occupancy, init, target)
                End Using
            End Using
        End Using

        If Not IsNothing(path) And Not path.Count = 0 Then
            Console.WriteLine("[Pathplanner] Found path! Path length: {0}", path.Count)
            Me._CurrentPath = ListToVector(path)
            DriveAlongPath(pose)
        Else
            Console.WriteLine("[Pathplanner] No path could be found!")
        End If


    End Sub

    Protected Overrides Sub OnDeActivated()
        MyBase.OnDeActivated()
        followingPath = False
        'first stop the agent
        Me.Control.Agent.Halt()
        Me._IsStopped = True
        Me._LastStopped = Now
    End Sub

    Protected Overrides Sub ProcessPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        MyBase.ProcessPoseEstimateUpdated(pose)

        'execute the current path plan
        If followingPath Then
            Me.DriveAlongPath(pose)
        End If

    End Sub

    Function ListToVector(ByVal l As List(Of Point)) As Vector2()
        Dim i As Integer
        Dim r(l.Count) As Vector2
        For i = 0 To l.Count - 1
            r(i) = New Vector2((l(i).X / Me._ManifoldImage.TransformationScale) - Me._ManifoldImage.TransformationOffset.X, (l(i).Y / Me._ManifoldImage.TransformationScale) - Me._ManifoldImage.TransformationOffset.Y)
        Next
        Return r
    End Function


#End Region

    Public Sub DriveAlongPath(ByVal pose As Pose2D)
        'Console.WriteLine("[RIKNOTE] Driving along path!")

        Dim init As Vector2 = New Vector2(pose.Position) ' CInt((pose.X + Me._ManifoldImage.TransformationOffset.X) * Me._ManifoldImage.TransformationScale), CInt((pose.Y + Me._ManifoldImage.TransformationOffset.Y) * Me._ManifoldImage.TransformationScale))
        ' Execute driving commands
        If IsNothing(Me._CurrentPath) Then
            'we have no path to follow
            Exit Sub
        ElseIf IsNothing(Me._CurrentGoal) Then
            'init plan
            Me._CurrentIndex = 0
            Me._CurrentGoal = Me._CurrentPath(Me._CurrentIndex)
        End If

        'compute displacement to currently planned (intermediate) goal 
        Dim dgoal As Vector2 = Me._CurrentGoal - init

        Dim ggoal As Vector2 = New Vector2(target.X * 1000 - init.X, target.Y * 1000 - init.Y)
        Dim goaldist As Double = (ggoal.X ^ 2 + ggoal.Y ^ 2) ^ 0.5
        Console.WriteLine("Distance to goal: {0}", goaldist)
        If (goaldist < 250) Then
            Me.OnDeActivated()
            Exit Sub
        End If

        Dim gdist As Double = (dgoal.X ^ 2 + dgoal.Y ^ 2) ^ 0.5

        If gdist < 250 Then
            'we have reached current goal, set new goal
            While Me._CurrentIndex < Me._CurrentPath.Length - 1

                Me._CurrentIndex += 1
                Me._CurrentGoal = Me._CurrentPath(Me._CurrentIndex)
                If Not IsNothing(Me._CurrentGoal) Then
                    dgoal = Me._CurrentGoal - init
                    gdist = (dgoal.X ^ 2 + dgoal.Y ^ 2) ^ 0.5

                    If gdist > 400 Then
                        'we found a new goal
                        Console.WriteLine(String.Format("[Behavior] - New Goal Position = ({0:f2} , {1:f2})", Me._CurrentGoal.X / 1000, Me._CurrentGoal.Y / 1000))
                        Exit While
                    End If
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
            If Abs(dangle) < 10 Then
                'Console.WriteLine("[Pathplanner] Driving forward!")
                .Drive(1.5F)
            ElseIf dangle > 0 Then
                'Console.WriteLine("[Pathplanner] Turning right!")
                .TurnRight(0.2)
            ElseIf dangle < 0 Then
                'Console.WriteLine("[Pathplanner] Turning left!")
                .TurnLeft(0.2)
            End If
        End With

    End Sub

    Private _CurrentPath As Vector2()
    'Private _CurrentInit As Vector2
    Private _CurrentGoal As Vector2
    Private _CurrentIndex As Integer


End Class
