Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.Windows.Forms

Imports UvARescue.Math

Imports AForge
Imports AForge.Math
Imports AForge.Imaging
Imports AForge.Imaging.Filters

'(MRT)

Public Class MobilityLayer
    Inherits BufferedLayer

#Region " Constructor / Destructor "

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)

        Me._PoseColor = Color.Red ' The path colour
        Me.ResetPoseBrush()
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then

            Try

                If Not IsNothing(Me._PoseBrush) Then
                    Me._PoseBrush.Dispose()
                    Me._PoseBrush = Nothing
                End If
                If Not IsNothing(Me._MobilityImage) Then
                    Me._MobilityImage.Dispose()
                    Me._MobilityImage = Nothing
                End If

            Catch ex As Exception
                Console.Error.WriteLine(ex.ToString)

            End Try

        End If
        MyBase.Dispose(disposing)

    End Sub

#End Region

#Region " Brushes "

    ' TODO: Make PoseColor dependant of mobility (speed, etc.) (MRT)
    Private _PoseColor As Color
    Private _PoseBrush As Brush
    Public Property PoseColor() As Color
        Get
            Return Me._PoseColor
        End Get
        Set(ByVal value As Color)
            If Not value = Me._PoseColor Then
                Me._PoseColor = value
                Me.ResetPoseBrush()
            End If
        End Set
    End Property
    Protected ReadOnly Property PoseBrush() As Brush
        Get
            Return Me._PoseBrush
        End Get
    End Property
    Private Sub ResetPoseBrush()
        If Not IsNothing(Me._PoseBrush) Then
            Me._PoseBrush.Dispose()
        End If
        Me._PoseBrush = New SolidBrush(Me._PoseColor)
    End Sub

#End Region




#Region " Maintain Mobility Image "

    Private _Pose As Pose2D = Nothing

    ' Public Overrides Sub RenderAgent(ByVal agent As Agent, ByVal pose As Math.Pose2D)
    '    MyBase.RenderAgent(agent, pose)
    '    Me._Pose = pose
    'End Sub


    Private _MobilityImage As Bitmap

    Public Sub RecomputeMobility()

        If IsNothing(Me._Pose) Then
            Exit Sub
        End If

        Dim pose As New Point( _
            CInt((Me._Pose.X + Me.Parent.TransformationOffset.X) * Me.Parent.TransformationScale), _
            CInt((Me._Pose.Y + Me.Parent.TransformationOffset.Y) * Me.Parent.TransformationScale))

        'update image

        'Need for class mobilityTools?:
        'Dim newMobilityImage As Bitmap = Me._MobilityTools.ExtractMobilityRegionsWithPathPlans(Me.Parent, pose, False)


        'dispose current copy
        If Not IsNothing(Me._MobilityImage) Then
            Me._MobilityImage.Dispose()
            Me._MobilityImage = Nothing
        End If

        'Me._MobilityImage = newMobilityImage

    End Sub

    Private previousOffset As PointF

    Friend Overrides Sub NotifyImageResize()
        MyBase.NotifyImageResize()

        Console.WriteLine("[MobilityLayer:NotifyImageResize] Not necessary, same code already available in parent")
        Return


        Dim currentOffset As PointF = Me.Parent.TransformationOffset
        Dim deltaOffset As New Point(CInt(currentOffset.X - previousOffset.X), CInt(currentOffset.Y - previousOffset.Y))
        Me.previousOffset = currentOffset

        If Not IsNothing(Me._MobilityImage) Then

            Dim imgNew As New Bitmap(Me.Parent.ImageWidth, Me.Parent.ImageHeight)

            Using gfxNew As Graphics = Graphics.FromImage(imgNew)
                'copy old image into new img
                gfxNew.Clear(Color.Black)
                gfxNew.DrawImageUnscaled(Me._MobilityImage, CInt(deltaOffset.X * Me.Parent.TransformationScale), CInt(deltaOffset.Y * Me.Parent.TransformationScale))
            End Using

            Me._MobilityImage.Dispose()
            Me._MobilityImage = imgNew

        End If

    End Sub


    Friend Overrides Sub NotifyImageReset()
        MyBase.NotifyImageReset()

        'reset buffer
        If Not IsNothing(Me._MobilityImage) Then
            Me._MobilityImage.Dispose()
            Me._MobilityImage = Nothing
        End If

        Me.poses.Clear()
        Me.sizes.Clear()
        Me.names.Clear()

    End Sub

    '   Public Overrides Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode)
    '      MyBase.Draw(g, renderingMode)
    '
    '       If Not IsNothing(Me._MobilityImage) Then
    '          g.DrawImageUnscaled(Me._MobilityImage, 0, 0)
    '     End If
    '
    '   End Sub


    Private poses As New Dictionary(Of Guid, Pose2D)

    Private sizes As New Dictionary(Of Guid, Size)
    Private names As New Dictionary(Of Guid, String)
    Private speeds As New Dictionary(Of Guid, Single)

    Public Overrides Sub RenderAgent(ByVal agent As Agent, ByVal pose As Math.Pose2D, ByVal point As Pose2D)
        MyBase.RenderAgent(agent, pose, point)

        If Not Me.poses.ContainsKey(agent.UniqueID) Then
            Me.poses.Add(agent.UniqueID, pose)
            Me.sizes.Add(agent.UniqueID, agent.Size)
            Me.names.Add(agent.UniqueID, agent.Name)
            Me.speeds.Add(agent.UniqueID, agent.ForwardSpeed)
        Else
            'no need to update sizes and names, only the pose is dynamic
            Me.poses(agent.UniqueID) = pose
            Me.speeds(agent.UniqueID) = agent.ForwardSpeed
        End If

    End Sub


#End Region



#Region " Rendering "
    Private estSpeed As New Dictionary(Of Guid, Double)
    Private maxSpeed As Single

    Public Overrides Sub RenderPatch(ByVal patch As Patch)
        MyBase.RenderPatch(patch)

        'Color of brush dependant of speed (redness)
        Dim redValue As Integer


        'TODO: Make PoseColor a certain shade of red...(MRT)
        'read the command speed
        Dim pose As Pose2D
        Dim size As Size
        Dim speed As Single

        For Each key As Guid In Me.poses.Keys
            pose = Me.poses(key)
            size = Me.sizes(key)
            speed = Me.speeds(key)

            If speed > maxSpeed Then
                maxSpeed = speed
            End If
            If maxSpeed > 0 Then
                redValue = CInt(Abs(speed) / maxSpeed * 255)
                Me._PoseColor = Drawing.Color.FromArgb(255, redValue, 0, 0)
                Me.ResetPoseBrush()

                Me.Gfx.FillEllipse(Me._PoseBrush, New Rectangle(New Point(CInt(patch.EstimatedOrigin.Position.X - size.Width / 2), CInt(patch.EstimatedOrigin.Position.Y - size.Height / 2)), size))
            End If


            'paint patch origins
            'Stamp of the agent Size
            'Dim size As Size = patch.AgentSize


        Next
    End Sub

    Public Overrides Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode)
        MyBase.Draw(g, renderingMode)

        If Not IsNothing(Me.poses) AndAlso Me.poses.Count > 0 Then

            'construct graphics path with current agent poses
            '  Using path As New GraphicsPath
            '
            '           Dim pose As Pose2D
            '          Dim size As Size
            '         Dim speed As Single



            'transform from world coords to page coords
            'path.Transform(Me.Parent.TransformationMatrix)

            'wat komt hier?:
            'g.DrawPath(Me._AgentsPen, path)

            'End Using

        End If

    End Sub

#End Region

End Class
