Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math

Imports UvARescue.Tools

Imports AForge
Imports AForge.Imaging
Imports AForge.Imaging.Filters




Public Class GeoTiffLayer
    Inherits ManifoldLayer

#Region " Constructor / Destructor "

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)
        Me._FileName = String.Empty
        Me._FilterRed = False
        Me._FilterGreen = True
        Me._FilterBlue = True
        Me._FilterImage = Nothing
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then

            Try

                If Not IsNothing(Me._GeoTiff) Then
                    Me._GeoTiff.Dispose()
                    Me._GeoTiff = Nothing
                End If

            Catch ex As Exception
                Console.Error.WriteLine(ex.ToString)

            End Try

        End If

        MyBase.Dispose(disposing)

    End Sub

#End Region

#Region " Maintain Bitmap "

    Private _FileName As String
    Private _GeoTiff As GeoTiff

    Public Property FileName() As String
        Get
            Return Me._FileName
        End Get
        Set(ByVal value As String)
            Me._FileName = value
            Me.ResetGeoTiff()
        End Set
    End Property
    Protected ReadOnly Property GeoTiff() As GeoTiff
        Get
            Return Me._GeoTiff
        End Get
    End Property
    Private Sub ResetGeoTiff()
        If Not IsNothing(Me._GeoTiff) Then
            Me._GeoTiff.Dispose()
        End If
        Me._GeoTiff = Tools.GeoTiff.Load(Me._FileName)
        'Me.ApplyFilter()

        'check bounds
        If Not IsNothing(Me._GeoTiff) Then
            With Me._GeoTiff

                'compute bounds using a path object
                Using path As New GraphicsPath

                    Dim right As Single = .OffsetY * 1000
                    Dim left As Single = right - (.Image.Height * Abs(.PixelHeight) * 1000)
                    Dim top As Single = .OffsetX * 1000
                    Dim bottom As Single = top + (.Image.Width * Abs(.PixelWidth) * 1000)

                    'draw a line from top-left to bottom-right
                    path.StartFigure()
                    path.AddLine(left, top, right, bottom)
                    path.CloseFigure()

                    Dim rect As RectangleF = path.GetBounds(Me.Parent.TransformationMatrix)

                    Me.Parent.EnsureMinimumImageBounds(rect)

                End Using

            End With
        End If

    End Sub

#End Region

#Region " Filtering "

    Private _FilterRed As Boolean
    Public Property FilterRed() As Boolean
        Get
            Return Me._FilterRed
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._FilterRed Then
                Me._FilterRed = value
                ' Me.ApplyFilter()
            End If
        End Set
    End Property

    Private _FilterGreen As Boolean
    Public Property FilterGreen() As Boolean
        Get
            Return Me._FilterGreen
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._FilterGreen Then
                Me._FilterGreen = value
                'Me.ApplyFilter()
            End If
        End Set
    End Property

    Private _FilterBlue As Boolean
    Public Property FilterBlue() As Boolean
        Get
            Return Me._FilterBlue
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._FilterBlue Then
                Me._FilterBlue = value
                'Me.ApplyFilter()
            End If
        End Set
    End Property



    Private _FilterImage As Drawing.Image
    Protected ReadOnly Property FilterImage() As Drawing.Image
        Get
            Return Me._FilterImage
        End Get
    End Property

    Protected Overridable Sub ApplyFilter()

        'dispose current copy
        If Not IsNothing(Me._FilterImage) Then
            Me._FilterImage.Dispose()
            Me._FilterImage = Nothing
        End If

        'construct new copy
        If Not IsNothing(Me._GeoTiff) AndAlso Not IsNothing(Me._GeoTiff.Image) Then

            If Me._FilterRed OrElse Me._FilterGreen OrElse Me._FilterBlue Then

                Dim r As New IntRange(0, CInt(IIf(Me._FilterRed, 0, 255)))
                Dim g As New IntRange(0, CInt(IIf(Me._FilterGreen, 0, 255)))
                Dim b As New IntRange(0, CInt(IIf(Me._FilterBlue, 0, 255)))

                Dim filter As New ChannelFiltering(r, g, b)
                Dim bitmap As New Bitmap(Me._GeoTiff.Image)
                Me._FilterImage = filter.Apply(bitmap)

            Else

                'no filter
                Me._FilterImage = New Bitmap(Me._GeoTiff.Image)

            End If

        End If

    End Sub

#End Region

#Region " Rendering "

    Public Overrides Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode)
        MyBase.Draw(g, renderingMode)
        If Not IsNothing(Me._GeoTiff) AndAlso Not IsNothing(Me._FilterImage) Then

            'keep a copy of the currently applicable transformation
            Dim backup As Matrix = g.Transform

            With Me._GeoTiff

                'before doing anything else, move the canvas to the origin of the manifold
                g.TranslateTransform(Me.Parent.TransformationOffset.X * Me.Parent.TransformationScale, Me.Parent.TransformationOffset.Y * Me.Parent.TransformationScale)


                'GeoTiff = 90 degrees rotated wrt Gdi+, so rotate the canvas
                g.TranslateTransform(.Image.Width * Abs(.PixelWidth) * Me.Parent.TransformationScale, 0.0F)
                g.RotateTransform(90)


                'compute the offsets, note that after rotation, the y-axis of GeoTiff is still 
                'flipped wrt the y-axis of Gdi+
                Dim xOffset As Single = .OffsetX * Me.Parent.Resolution
                Dim yOffset As Single = -.OffsetY * Me.Parent.Resolution
                g.TranslateTransform(xOffset, yOffset)


                'apply the correct scaling 
                Dim xScaling As Single = CType(Abs(.PixelWidth) * Me.Parent.Resolution, Single)
                Dim yScaling As Single = CType(Abs(.PixelHeight) * Me.Parent.Resolution, Single)
                g.ScaleTransform(xScaling, yScaling)


                'the canvas is set up, draw the geotiff image
                g.DrawImageUnscaled(Me._FilterImage, 0, 0)


            End With

            'restore previous transformation matrix
            g.Transform = backup

        End If
    End Sub

#End Region

End Class
