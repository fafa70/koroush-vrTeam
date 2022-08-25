Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math

Imports UvARescue.Math

Public Class SafeSpaceLayer
    Inherits BufferedLayer

#Region " Constructor / Destructor "

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)

        Me._SafeSpaceAlpha = CInt(255)
        Me._SafeSpaceColor = Color.Gray
        Me.ResetSafeSpaceBrush()

        Me._PoseColor = Color.Crimson '#DC143C The future path 
        Me.ResetPoseBrush()

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not IsNothing(Me._SafeSpaceBrush) Then
            Me._SafeSpaceBrush.Dispose()
            Me._SafeSpaceBrush = Nothing
        End If
        If Not IsNothing(Me._PoseBrush) Then
            Me._PoseBrush.Dispose()
            Me._PoseBrush = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub


#End Region

#Region " Brushes  "

    Private _SafeSpaceAlpha As Integer
    Private _SafeSpaceColor As Color
    Private _SafeSpaceBrush As Brush

    Private _PoseColor As Color
    Private _PoseBrush As Brush

    Public Property SafeSpaceAlpha() As Integer
        Get
            Return Me._SafeSpaceAlpha
        End Get
        Set(ByVal value As Integer)
            If Not value = Me._SafeSpaceAlpha Then
                Me._SafeSpaceAlpha = value
                Me.ResetSafeSpaceBrush()
            End If
        End Set
    End Property
    Public Property SafeSpaceColor() As Color
        Get
            Return Me._SafeSpaceColor
        End Get
        Set(ByVal value As Color)
            If Not value = Me._SafeSpaceColor Then
                Me._SafeSpaceColor = value
                Me.ResetSafeSpaceBrush()
            End If
        End Set
    End Property
    Protected ReadOnly Property SafeSpaceBrush() As Brush
        Get
            Return Me._SafeSpaceBrush
        End Get
    End Property
    Private Sub ResetSafeSpaceBrush()
        If Not IsNothing(Me._SafeSpaceBrush) Then
            Me._SafeSpaceBrush.Dispose()
        End If
        Me._SafeSpaceBrush = New SolidBrush(Color.FromArgb(Me._SafeSpaceAlpha, Me._SafeSpaceColor))
    End Sub

    Private Sub ResetPoseBrush()
        If Not IsNothing(Me._PoseBrush) Then
            Me._PoseBrush.Dispose()
        End If
        Me._PoseBrush = New SolidBrush(Me._PoseColor)
    End Sub
#End Region

#Region " Maintain Interest- and Safe-Region "

    Public Overrides Sub RenderPatch(ByVal patch As Patch)
        MyBase.RenderPatch(patch)

        'unify freespace polygon with region of interest
        Using freeSpace As New GraphicsPath

            freeSpace.StartFigure()
            freeSpace.AddPolygon(patch.GlobalPolygon)
            freeSpace.CloseFigure()

            'then construct the safe region in global coords
            Dim origin As Vector2 = patch.EstimatedOrigin.Position
            Dim safedist As Single = 3.0F * patch.Scan.Factor

            Using safeSpace As New GraphicsPath
                safeSpace.StartFigure()
                safeSpace.AddEllipse(New RectangleF(CType(origin.X - safedist, Single), CType(origin.Y - safedist, Single), 2 * safedist, 2 * safedist))
                safeSpace.CloseFigure()

                Using safeRegion As New Region(safeSpace)
                    'intersect with freespace
                    safeRegion.Intersect(freeSpace)

                    'just in case the freespacelayer was hidden
                    Me.Parent.EnsureMinimumImageBounds(safeSpace.GetBounds(Me.Parent.TransformationMatrix))

                    'plot the region into the graphics buffer
                    Me.Gfx.FillRegion(Me._SafeSpaceBrush, safeRegion)

                End Using
            End Using
        End Using

    End Sub
    'Arnoud: Commented out to check if this method results in a crash in the observers

    'Private poses As New Dictionary(Of Guid, Pose2D)

    'Public Overrides Sub RenderAgent(ByVal agent As Agent, ByVal pose As Math.Pose2D)
    ' MyBase.RenderAgent(agent, pose)

    'Dim behavior_agent As New BehaviorAgent(Agent)
    ' Dim motions As List(Of Motion)


    '   motions = behavior_agent.MotionControl.ActiveMotions()

    '' Arnoud: attempt to reach TargetPose of Motion (not succesfull yet)

    ''If motions.ContainsKey(FrontierExploration) Then
    ''pose = Motion(j).TargetPos()
    ''End If

    ''If motion_control.ActiveMotions = FrontierExploration Then
    ''pose = motion_control.TargetPos()
    ''End If

    '  If Not Me.poses.ContainsKey(agent.UniqueID) Then
    '     Me.poses.Add(agent.UniqueID, pose)
    'Else
    'no need to update sizes and names, only the pose is dynamic
    '   Me.poses(agent.UniqueID) = pose
    'End If

    'End Sub

    'Arnoud: Commented out to check if this method results in a crash in the observers
    'Friend Overrides Sub NotifyImageReset()
    ' MyBase.NotifyImageReset()

    ' Me.poses.Clear()

    ' End Sub


#End Region

End Class
