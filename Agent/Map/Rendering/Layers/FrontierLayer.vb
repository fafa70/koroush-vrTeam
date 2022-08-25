Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math

Imports UvARescue.Math

Imports AForge
Imports AForge.ImaginG
Imports AForge.Imaging.Filters


Public Class FrontierLayer
    Inherits ManifoldLayer

#Region " Constructor / Destructor "

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)
        Me._FrontierTools = New FrontierTools
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not IsNothing(Me._FrontierImage) Then
            Me._FrontierImage.Dispose()
            Me._FrontierImage = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

#End Region

#Region " Maintain Frontier Image "

    Private _Pose As Pose2D = Nothing

    Public Overrides Sub RenderAgent(ByVal agent As Agent, ByVal pose As Math.Pose2D, ByVal point As Pose2D)
        MyBase.RenderAgent(agent, pose, point)
        Me._Pose = pose
    End Sub


    Private _FrontierImage As Bitmap
    Private _FrontierTools As FrontierTools

    Public Sub RecomputeFrontiers()

        If IsNothing(Me._Pose) Then
            Exit Sub
        End If

        Dim pose As New Point( _
            CInt((Me._Pose.X + Me.Parent.TransformationOffset.X) * Me.Parent.TransformationScale), _
            CInt((Me._Pose.Y + Me.Parent.TransformationOffset.Y) * Me.Parent.TransformationScale))

        'update image
        Dim newFrontierImage As Bitmap = Me._FrontierTools.ExtractFrontierRegionsWithPathPlans(Me.Parent, pose, False)

        'dispose current copy
        If Not IsNothing(Me._FrontierImage) Then
            Me._FrontierImage.Dispose()
            Me._FrontierImage = Nothing
        End If

        Me._FrontierImage = newFrontierImage

    End Sub

    Private previousOffset As PointF

    Friend Overrides Sub NotifyImageResize()
        MyBase.NotifyImageResize()

        Dim currentOffset As PointF = Me.Parent.TransformationOffset
        Dim deltaOffset As New Point(CInt(currentOffset.X - previousOffset.X), CInt(currentOffset.Y - previousOffset.Y))
        Me.previousOffset = currentOffset

        If Not IsNothing(Me._FrontierImage) Then

            Dim imgNew As New Bitmap(Me.Parent.ImageWidth, Me.Parent.ImageHeight)

            Using gfxNew As Graphics = Graphics.FromImage(imgNew)
                'copy old image into new img
                gfxNew.Clear(Color.Black)
                gfxNew.DrawImageUnscaled(Me._FrontierImage, CInt(deltaOffset.X * Me.Parent.TransformationScale), CInt(deltaOffset.Y * Me.Parent.TransformationScale))
            End Using

            Me._FrontierImage.Dispose()
            Me._FrontierImage = imgNew

        End If

    End Sub


    Friend Overrides Sub NotifyImageReset()
        MyBase.NotifyImageReset()

        'reset buffer
        If Not IsNothing(Me._FrontierImage) Then
            Me._FrontierImage.Dispose()
            Me._FrontierImage = Nothing
        End If

    End Sub

    Public Overrides Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode)
        MyBase.Draw(g, renderingMode)

        If Not IsNothing(Me._FrontierImage) Then
            g.DrawImageUnscaled(Me._FrontierImage, 0, 0)
        End If

    End Sub

#End Region

End Class
