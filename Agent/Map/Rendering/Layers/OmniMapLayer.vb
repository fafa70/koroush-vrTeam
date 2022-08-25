Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.IO

Imports UvARescue.Math

Public Class OmniMapLayer
    Inherits ManifoldLayer

#Region " Constructor / Destructor "

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        MyBase.Dispose(disposing)
    End Sub

#End Region

#Region " Constants "

    'Initial color and transparency of the layer
    Private InitialColor As Color = Color.FromArgb(0, 127, 127, 127)
    Private Transparency As Byte = 255

    'Scale of the camera image to match the layer's size
    Private Const CameraScale As Double = (3 / 20)

    Private Const Gamma As Byte = 40
    Private Const Sigma As Single = 15

#End Region

#Region " Maintain Informat"

    'Bitmap on which the map will be drawn
    Private Map As Bitmap

    'Used for resizing the Map on the fly
    Private previousOffset As PointF

    'When the Manifold images resizes, the Map also has to be resized
    Friend Overrides Sub NotifyImageResize()
        MyBase.NotifyImageResize()

        Dim currentOffset As PointF = Me.Parent.TransformationOffset
        Dim deltaOffset As New Point(CInt(currentOffset.X - previousOffset.X), CInt(currentOffset.Y - previousOffset.Y))
        Me.previousOffset = currentOffset

        If Not IsNothing(Me.Map) Then

            Dim imgNew As New Bitmap(Me.Parent.ImageWidth, Me.Parent.ImageHeight)

            Using gfxNew As Graphics = Graphics.FromImage(imgNew)
                'copy old image into new img
                gfxNew.Clear(InitialColor)
                gfxNew.DrawImageUnscaled(Me.Map, CInt(deltaOffset.X * Me.Parent.TransformationScale), CInt(deltaOffset.Y * Me.Parent.TransformationScale))
            End Using

            Me.Map.Dispose()
            Me.Map = imgNew

        End If
    End Sub

    'Reset the Map
    Friend Overrides Sub NotifyImageReset()
        MyBase.NotifyImageReset()

        If Not IsNothing(Map) Then
            Map.Dispose()
            Map = Nothing
        End If
    End Sub

#End Region

#Region " Rendering "

    'Render an omnicam image update
    Public Overrides Sub RenderOmnicam(ByVal patch As Patch)
        MyBase.RenderPatch(patch)

        'Dim difference As TimeSpan = (patch.Timestamp - patch.Omnicam.TimeStamp)
        'Console.Out.WriteLine("SYNCHRONIZATION DIFF: {0}", difference.TotalMilliseconds)

        ''If the Map is Nothing, create a new one and initialize it, using the InitialColor
        'If IsNothing(Map) Then
        '    Map = New Bitmap(Me.Parent.ImageWidth, Me.Parent.ImageHeight)
        '    Using g As Graphics = Graphics.FromImage(Map)
        '        g.Clear(InitialColor)
        '    End Using
        'End If

        'If Not IsNothing(patch.Omnicam) Then
        '    patch.Omnicam.AcquireReaderLock()

        '    'Get the birdview image
        '    Dim birdview As Bitmap

        '    Try
        '        birdview = CType(patch.Omnicam.BirdView.Clone(), Bitmap)
        '    Catch ex As Exception
        '        birdview = Nothing
        '    End Try

        '    If Not IsNothing(birdview) Then

        '        'Transform it to the layer size and rotation
        '        Using transformed As Bitmap = TransformBitmap(birdview, patch.EstimatedOrigin.Rotation, CameraScale)
        '            'Get the size of the image
        '            Dim imageSize As Size = New Size(transformed.Width, transformed.Height)

        '            'Calculate the location of the image on the map
        '            Dim mapPoint As Point = New Point( _
        '                CInt((patch.EstimatedOrigin.X + Me.Parent.TransformationOffset.X) * Me.Parent.TransformationScale), _
        '                CInt((patch.EstimatedOrigin.Y + Me.Parent.TransformationOffset.Y) * Me.Parent.TransformationScale))
        '            mapPoint.Offset(-CInt(imageSize.Width / 2), -CInt(imageSize.Height / 2))

        '            'The starting point for the camera image
        '            Dim cameraPoint As Point = New Point(0, 0)

        '            'If the camera images falls outside the map, crop the values.
        '            If mapPoint.X < 0 Then
        '                cameraPoint.X = cameraPoint.X - mapPoint.X
        '                imageSize.Width = imageSize.Width + mapPoint.X
        '                mapPoint.X = 0
        '            End If
        '            If mapPoint.Y < 0 Then
        '                cameraPoint.Y = cameraPoint.Y - mapPoint.Y
        '                imageSize.Height = imageSize.Height + mapPoint.Y
        '                mapPoint.Y = 0
        '            End If
        '            If mapPoint.X + imageSize.Width > Map.Width Then
        '                imageSize.Width = Map.Width - mapPoint.X
        '            End If
        '            If mapPoint.Y + imageSize.Height > Map.Height Then
        '                imageSize.Height = Map.Height - mapPoint.Y
        '            End If

        '            'Merge the camera image with the map, using the gamma algorithm
        '            MergeCameraImage(patch, transformed, cameraPoint, mapPoint, imageSize)
        '        End Using
        '        patch.Omnicam.ReleaseReaderLock()
        '    End If
        'End If
    End Sub

    'Draws the Map on the layer
    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics, ByVal renderingMode As ManifoldRenderingMode)
        MyBase.Draw(g, renderingMode)

        If Not IsNothing(Map) Then
            g.DrawImageUnscaled(Map, 0, 0)
        End If
    End Sub

#End Region

#Region " Image Utilities "

    'Transforms a camera image, according to a rotation and a scale
    Function TransformBitmap(ByVal origImage As Bitmap, ByVal rotation As Double, ByVal scale As Double) As Bitmap

        'Calculate the new size, given the scale and increase the size to fit any rotation.
        Dim newSize As Size = New Size(CInt(Round(origImage.Width * scale)), CInt(Round(origImage.Height * scale)))
        Dim maxLength As Integer = Max(newSize.Width, newSize.Height)
        Dim increaseSize As Single = CSng(Sqrt(2 * (maxLength / 2) ^ 2) - (maxLength / 2))

        Dim newImage As New Bitmap(newSize.Width + CInt(increaseSize * 2), newSize.Height + CInt(increaseSize * 2), origImage.PixelFormat)
        Dim centerPoint As New PointF(CSng(newImage.Width / 2), CSng(newImage.Height / 2))

        Using g As Graphics = Graphics.FromImage(newImage)
            'Translate to center
            g.TranslateTransform(centerPoint.X, centerPoint.Y)
            'Rotate
            g.RotateTransform(-CSng((rotation / (2 * PI)) * 360))
            'Translate back
            g.TranslateTransform(-centerPoint.X, -centerPoint.Y)
            'Draw the image
            g.DrawImage(origImage, CInt(increaseSize), CInt(increaseSize), newSize.Width, newSize.Height)
        End Using

        newImage.RotateFlip(RotateFlipType.Rotate270FlipX)
        Return newImage
    End Function

    'Merges a camera image with the current Map using a gamma factor
    Public Sub MergeCameraImage(ByVal patch As Patch, ByVal cameraImage As Bitmap, _
        ByVal cameraPoint As Point, ByVal mapPoint As Point, ByVal size As Size)

        Dim center As Point = New Point(CInt(Round(size.Width / 2)), CInt(Round(size.Height / 2)))

        'Put the data from both images into the memory
        Dim camData As BitmapData = cameraImage.LockBits(New Rectangle(cameraPoint, size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        Dim mapData As BitmapData = Map.LockBits(New Rectangle(mapPoint, size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)

        For y As Integer = 0 To size.Height - 1
            For x As Integer = 0 To size.Width - 1
                'Get the alpha of the camera data
                Dim A As Byte = Marshal.ReadByte(camData.Scan0, (camData.Stride * y) + (4 * x) + 3)

                'If the alpha is zero, ignore the value (we do not care about the transparent parts)
                If A <> 0 Then

                    'Since the laser range scanners can only see 180 degrees, skip the rest of the bird-view
                    Dim deg As Integer = CInt((((Atan2(y - center.Y, x - center.X) - _
                        patch.EstimatedOrigin.Rotation) / (2 * PI)) * 360) Mod 360)
                    If deg < 0 Then
                        deg = deg + 360
                    End If

                    If (deg > 90 AndAlso deg < 270) Then
                        Continue For
                    End If

                    If deg <= 90 Then
                        deg = deg + 90
                    Else
                        deg = deg - 270
                    End If

                    Dim obstacle As Double = 1

                    With patch.Scan
                        If .Range(deg) > .MinRange AndAlso .Range(deg) < .MaxRange Then
                            Dim obstaclePoint As Vector2 = patch.GlobalPoints(deg)
                            Dim radius1 As Double = Sqrt((x - center.X) ^ 2 + (y - center.Y) ^ 2)

                            Dim mapObstaclePoint As Point = New Point( _
                                CInt((obstaclePoint.X + Me.Parent.TransformationOffset.X) * Me.Parent.TransformationScale - (mapPoint.X + center.X)), _
                                CInt((obstaclePoint.Y + Me.Parent.TransformationOffset.Y) * Me.Parent.TransformationScale - (mapPoint.Y + center.Y)))

                            Dim radius2 As Double = Sqrt(mapObstaclePoint.X ^ 2 + mapObstaclePoint.Y ^ 2)

                            If (radius1 >= radius2) Then
                                obstacle = 0
                            End If
                        End If
                    End With

                    Dim cTransparency As Byte = Marshal.ReadByte(mapData.Scan0, (mapData.Stride * y) + (4 * x) + 3)

                    'Retrieve all rgb values
                    Dim R1 As Byte
                    Dim G1 As Byte
                    Dim B1 As Byte

                    If cTransparency = 0 Then
                        R1 = InitialColor.R
                        G1 = InitialColor.G
                        B1 = InitialColor.B
                    Else
                        R1 = Marshal.ReadByte(mapData.Scan0, (mapData.Stride * y) + (4 * x) + 2)
                        G1 = Marshal.ReadByte(mapData.Scan0, (mapData.Stride * y) + (4 * x) + 1)
                        B1 = Marshal.ReadByte(mapData.Scan0, (mapData.Stride * y) + (4 * x) + 0)
                    End If

                    Dim R2 As Byte = Marshal.ReadByte(camData.Scan0, (camData.Stride * y) + (4 * x) + 2)
                    Dim G2 As Byte = Marshal.ReadByte(camData.Scan0, (camData.Stride * y) + (4 * x) + 1)
                    Dim B2 As Byte = Marshal.ReadByte(camData.Scan0, (camData.Stride * y) + (4 * x) + 0)

                    'Calculate the Gaussian value, for x and y
                    Dim gaussian As Double = GetGaussian(New Point(x, y), center)

                    'Calculate the final Gamma factor
                    Dim factor As Byte = CByte(Gamma * gaussian * obstacle)

                    If factor > 0 Then
                        'Calculate the new rgb values
                        Dim R3 As Byte = GetNewColorValue(R1, R2, factor)
                        Dim G3 As Byte = GetNewColorValue(G1, G2, factor)
                        Dim B3 As Byte = GetNewColorValue(B1, B2, factor)

                        'Store the new values
                        Marshal.WriteByte(mapData.Scan0, (mapData.Stride * y) + (4 * x) + 3, Transparency)
                        Marshal.WriteByte(mapData.Scan0, (mapData.Stride * y) + (4 * x) + 2, R3)
                        Marshal.WriteByte(mapData.Scan0, (mapData.Stride * y) + (4 * x) + 1, G3)
                        Marshal.WriteByte(mapData.Scan0, (mapData.Stride * y) + (4 * x) + 0, B3)
                    End If
                End If
            Next
        Next

        'Unlock the images
        cameraImage.UnlockBits(camData)
        Map.UnlockBits(mapData)
    End Sub

    'Gaussian Filter
    Public Function GetGaussian(ByVal location As Point, ByVal center As Point) As Double
        Return E ^ (-(((location.X - center.X) ^ 2 + (location.Y - center.Y) ^ 2) / (2 * Sigma ^ 2)))
    End Function

    'Calculate new values for r, g or b, using a Gamma factor
    Public Function GetNewColorValue(ByVal map As Integer, ByVal view As Integer, ByVal factor As Integer) As Byte
        If map - view > 0 Then
            Return CByte(Max(view, map - factor))
        Else
            Return CByte(Min(view, map + factor))
        End If
    End Function

#End Region

End Class
