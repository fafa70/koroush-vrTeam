Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math

Imports UvARescue.Math

Public Class FreeSpaceLayer
    Inherits BufferedLayer

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)
        'Me._FreeSpaceAlpha = CInt(255 * 0.2) '20%
        Me._FreeSpaceAlpha = CInt(255) '100%, used for competiion of 2007 to compy with rules
        Me._FreeSpaceColor = Color.White
        Me.ResetFreeSpacePenAndBrush()
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not IsNothing(Me._FreeSpaceBrush) Then
            Me._FreeSpaceBrush.Dispose()
            Me._FreeSpaceBrush = Nothing
        End If
        If Not IsNothing(Me._FreeSpacePen) Then
            Me._FreeSpacePen.Dispose()
            Me._FreeSpacePen = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private _FreeSpaceAlpha As Integer
    Private _FreeSpaceColor As Color
    Private _FreeSpacePen As Pen
    Private _FreeSpaceBrush As Brush
    Public Property FreeSpaceAlpha() As Integer
        Get
            Return Me._FreeSpaceAlpha
        End Get
        Set(ByVal value As Integer)
            If Not value = Me._FreeSpaceAlpha Then
                Me._FreeSpaceAlpha = value
                Me.ResetFreeSpacePenAndBrush()
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
                Me.ResetFreeSpacePenAndBrush()
            End If
        End Set
    End Property
    Protected ReadOnly Property FreeSpacePen() As Pen
        Get
            Return Me._FreeSpacePen
        End Get
    End Property
    Private Sub ResetFreeSpacePenAndBrush()

        If Not IsNothing(Me._FreeSpacePen) Then
            Me._FreeSpacePen.Dispose()
        End If
        Me._FreeSpacePen = New Pen(Color.FromArgb(Me._FreeSpaceAlpha, Me._FreeSpaceColor))

        If Not IsNothing(Me._FreeSpaceBrush) Then
            Me._FreeSpaceBrush.Dispose()
        End If
        Me._FreeSpaceBrush = New SolidBrush(Color.FromArgb(Me._FreeSpaceAlpha, Me._FreeSpaceColor))

    End Sub

    Public Overrides Sub RenderPatch(ByVal patch As Patch)
        MyBase.RenderPatch(patch)

        'create path in manifold-ooords (= world coords)
        Using freeSpace As New GraphicsPath

            Dim origin As Vector2 = patch.EstimatedOrigin.Position
            Dim points() As Vector2 = patch.GlobalPoints
            Dim filter() As Boolean = patch.Filter

            'OLD STYLE (SLOW!) - DRAW EACH INDIVIDUAL SCAN BEAM
            'For Each point As Vector2 In points
            '    freeSpace.AddLine(CType(origin.X, Single), CType(origin.Y, Single), CType(point.X, Single), CType(point.Y, Single))
            'Next

            'set to true for cleaner maps
            Dim hideOutOfRangeBeams As Boolean = False

            Dim polygon(points.Length) As PointF 'the origin will be appended too
            For i As Integer = 0 To points.Length - 1
                If hideOutOfRangeBeams AndAlso filter(i) Then
                    polygon(i) = New PointF(CType(origin.X, Single), CType(origin.Y, Single))
                Else
                    polygon(i) = New PointF(CType(points(i).X, Single), CType(points(i).Y, Single))
                End If
            Next

            'add origin
            polygon(polygon.Length - 1) = New PointF(CType(origin.X, Single), CType(origin.Y, Single))

            freeSpace.StartFigure()
            freeSpace.AddPolygon(polygon)
            freeSpace.CloseFigure()

            Me.Parent.EnsureMinimumImageBounds(freeSpace.GetBounds(Me.Parent.TransformationMatrix))

            'plot the path into the graphics buffer
            Me.Gfx.FillPath(Me._FreeSpaceBrush, freeSpace)

        End Using
    End Sub

End Class
