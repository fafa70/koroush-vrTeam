Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math

Imports UvARescue.Math


Public MustInherit Class BufferedLayer
    Inherits ManifoldLayer

#Region " Constructor / Destructor "

    Protected Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then

            Try

                If Not IsNothing(Me.Img) Then
                    Me.Img.Dispose()
                    Me.Img = Nothing
                End If

                If Not IsNothing(Me.Gfx) Then
                    Me.Gfx.Dispose()
                    Me.Gfx = Nothing
                End If

            Catch ex As Exception
                Console.Error.WriteLine(ex.ToString)

            End Try

        End If

        MyBase.Dispose(disposing)

    End Sub

#End Region

#Region " Maintian Image Buffer "

    Protected Gfx As Graphics
    Protected Img As Bitmap

    Private previousOffset As PointF

    ' Dim resizeMutex As New Object

    Friend Overrides Sub NotifyImageResize()
        MyBase.NotifyImageResize()

        Dim currentOffset As PointF = Me.Parent.TransformationOffset
        Dim deltaOffset As New Point(CInt(currentOffset.X - previousOffset.X), CInt(currentOffset.Y - previousOffset.Y))


        Me.previousOffset = currentOffset

        'If Not IsNothing(Me.Parent) Then
        '    Console.WriteLine(String.Format("[BufferedLayer:NotifyImageResize] Rescaling {0},{1}", deltaOffset.X, deltaOffset.Y))
        'End If

        Try
            Dim imgNew As New Bitmap(Me.Parent.ImageWidth, Me.Parent.ImageHeight, PixelFormat.Format32bppPArgb)
            Dim gfxNew As Graphics = Me.CreateGraphics(imgNew)

            If Not IsNothing(Me.Img) Then

                'copy old image into new img
                gfxNew.DrawImageUnscaled(Me.Img, CInt(deltaOffset.X * Me.Parent.TransformationScale), CInt(deltaOffset.Y * Me.Parent.TransformationScale))

                Me.Img.Dispose()
                Me.Gfx.Dispose()

            End If

            Me.Img = imgNew
            Me.Gfx = gfxNew

            Me.Gfx.Transform = Me.Parent.TransformationMatrix

        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString)

        End Try


    End Sub

    Friend Overrides Sub NotifyImageReset()
        MyBase.NotifyImageReset()

        'reset buffer
        If Not IsNothing(Me.Img) Then
            Me.Img.Dispose()
            Me.Img = Nothing
        End If
        If Not IsNothing(Me.Gfx) Then
            Me.Gfx.Dispose()
            Me.Gfx = Nothing
        End If

        Me.Img = New Bitmap(Me.Parent.ImageWidth, Me.Parent.ImageHeight, PixelFormat.Format32bppPArgb) 'This Format 3% faster according to the measurements of Bas Terwijn (March 26, 2009)
        Me.Gfx = Me.CreateGraphics(Me.Img)

        Me.Gfx.Transform = Me.Parent.TransformationMatrix

        Me.previousOffset = New Point(0, 0)

    End Sub

    Protected Function CreateGraphics(ByVal fromImage As Bitmap) As Graphics
        Dim g As Graphics = Graphics.FromImage(fromImage)

        g.PageUnit = GraphicsUnit.Pixel

        'disabled smoothing mode for jury scoring purposes. they need 'pure'
        'colors for their automated scoring tool
        'g.SmoothingMode = SmoothingMode.AntiAlias

        Return g
    End Function

#End Region

#Region " Rendering "

    Private useThumbnails As Boolean = False 'True is 7% slower in the measurements of Bas Terwijn (March 24, 2009)

    Public Overrides Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode)

        MyBase.Draw(g, renderingMode)

        If Not IsNothing(Me.Img) Then

            Dim thumb As Image = Nothing
            Try
                'only draw thumbnails in GdiPlus mode, the georeferenced maps saved on disk
                'should be of full-quality
                If Me.useThumbnails AndAlso renderingMode = ManifoldRenderingMode.GdiPlus Then
                    thumb = Me.Img.GetThumbnailImage(Img.Width, Img.Height, Nothing, Nothing)
                    g.DrawImageUnscaled(thumb, 0, 0)

                Else
                    g.DrawImageUnscaled(Me.Img, 0, 0)

                End If

            Catch ex As Exception
                Console.WriteLine("Failed to draw BufferedLayer")
                Console.WriteLine(ex)

            Finally
                If Not IsNothing(thumb) Then
                    thumb.Dispose()
                End If
            End Try
        End If
    End Sub

    Public Overrides Sub RenderAgent(ByVal agent As Agent, ByVal pose As Pose2D, ByVal point As Pose2D)
        MyBase.RenderAgent(agent, pose, point)
    End Sub

#End Region

End Class
