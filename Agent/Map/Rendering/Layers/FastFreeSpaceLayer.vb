Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.Windows.Forms

Imports UvARescue.Math

Public Class FastFreeSpaceLayer
    Inherits BufferedLayer

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)

        Me._FreeSpaceAlpha = CInt(255) '100%
        Me._FreeSpaceColor = Color.FromArgb(255, 255, 255)
        Me.ResetFreeSpaceBrush()

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not IsNothing(Me._FreeSpaceBrush) Then
            Me._FreeSpaceBrush.Dispose()
            Me._FreeSpaceBrush = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private _FreeSpaceAlpha As Integer
    Private _FreeSpaceColor As Color
    Private _FreeSpaceBrush As Brush
    Public Property FreeSpaceAlpha() As Integer
        Get
            Return Me._FreeSpaceAlpha
        End Get
        Set(ByVal value As Integer)
            If Not value = Me._FreeSpaceAlpha Then
                Me._FreeSpaceAlpha = value
                Me.ResetFreeSpaceBrush()
            End If
        End Set
    End Property
    Public Property FreeSpaceColor() As Color
        Get
            Return Me._FreeSpaceColor
        End Get
        Set(ByVal value As Color)
            If Not value = Me._FreeSpaceColor Then
                Me._FreeSpaceColor = value
                Me.ResetFreeSpaceBrush()
            End If
        End Set
    End Property
    Protected ReadOnly Property FreeSpaceBrush() As Brush
        Get
            Return Me._FreeSpaceBrush
        End Get
    End Property
    Private Sub ResetFreeSpaceBrush()
        If Not IsNothing(Me._FreeSpaceBrush) Then
            Me._FreeSpaceBrush.Dispose()
        End If
        Me._FreeSpaceBrush = New SolidBrush(Color.FromArgb(Me._FreeSpaceAlpha, Me._FreeSpaceColor))
    End Sub


    Public Overrides Sub RenderPatch(ByVal patch As Patch, ByVal origin As Math.Vector2, ByVal points() As Math.Vector2)
        MyBase.RenderPatch(patch, origin, points)

        'construct the polygon from all the points.
        Dim polygon(points.Length) As PointF
        For i As Integer = 0 To points.Length - 1
            polygon(i) = New PointF(CType(points(i).X, Single), CType(points(i).Y, Single))
        Next
        polygon(polygon.Length - 1) = New PointF(CType(origin.X, Single), CType(origin.Y, Single))

        'construct path
        Dim freeSpace As New GraphicsPath
        freeSpace.StartFigure()
        freeSpace.AddPolygon(polygon)
        freeSpace.CloseFigure()

        'now construct the scan circle in gobal coords
        Dim length As Single = CType(patch.Scan.MaxRange * patch.Scan.Factor, Single)
        Dim scanRect As New RectangleF(CType(origin.X - length, Single), CType(origin.Y - length, Single), 2 * length, 2 * length)
        Dim scanSpace As New GraphicsPath
        scanSpace.StartFigure()
        scanSpace.AddEllipse(scanRect)
        scanSpace.CloseFigure()

        'intersect freespace and clearspace using the Region class
        Using region As New Region(scanSpace)

            region.Intersect(freeSpace)

            'we need to ensure bounds
            'most of the time the freespace layer will have done that already, but just in case
            'the freespace layer was hidden ...
            Me.Parent.EnsureMinimumImageBounds(freeSpace.GetBounds(Me.Parent.TransformationMatrix))

            'plot the region into the graphics buffer
            Me.Gfx.FillRegion(Me._FreeSpaceBrush, region)

        End Using

    End Sub

End Class
