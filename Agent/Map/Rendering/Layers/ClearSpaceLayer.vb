Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math

Imports UvARescue.Math

Public Class ClearSpaceLayer
    Inherits BufferedLayer

    Public Sub New(ByVal parent As ManifoldImage, ByVal clearFov As Single, ByVal clearRange As Single)
        MyBase.New(parent)

        Me._ClearSpaceAlpha = CInt(255) '100%
        Me._ClearSpaceColor = Color.FromArgb(0, 255, 0)
        Me.ResetClearSpaceBrush()

        Me._ClearFov = CType(clearFov / 180 * PI, Single) 'radians
        Me._ClearRange = clearRange 'mm

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not IsNothing(Me._ClearSpaceBrush) Then
            Me._ClearSpaceBrush.Dispose()
            Me._ClearSpaceBrush = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private _ClearSpaceAlpha As Integer
    Private _ClearSpaceColor As Color
    Private _ClearSpaceBrush As Brush

    Public Property ClearSpaceAlpha() As Integer
        Get
            Return Me._ClearSpaceAlpha
        End Get
        Set(ByVal value As Integer)
            If Not value = Me._ClearSpaceAlpha Then
                Me._ClearSpaceAlpha = value
                Me.ResetClearSpaceBrush()
            End If
        End Set
    End Property
    Public Property ClearSpaceColor() As Color
        Get
            Return Me._ClearSpaceColor
        End Get
        Set(ByVal value As Color)
            If Not value = Me._ClearSpaceColor Then
                Me._ClearSpaceColor = value
                Me.ResetClearSpaceBrush()
            End If
        End Set
    End Property
    Protected ReadOnly Property ClearSpaceBrush() As Brush
        Get
            Return Me._ClearSpaceBrush
        End Get
    End Property
    Private Sub ResetClearSpaceBrush()
        If Not IsNothing(Me._ClearSpaceBrush) Then
            Me._ClearSpaceBrush.Dispose()
        End If
        Me._ClearSpaceBrush = New SolidBrush(Color.FromArgb(Me._ClearSpaceAlpha, Me._ClearSpaceColor))
    End Sub


    Private _ClearFov As Single
    Public ReadOnly Property ClearFov() As Single
        Get
            Return Me._ClearFov
        End Get
    End Property

    Private _ClearRange As Single
    Public ReadOnly Property ClearRange() As Single
        Get
            Return Me._ClearRange
        End Get
    End Property

    ''' <summary>
    ''' The space that we can automatically clear is the space that
    ''' is within the view of the victsensor, as specified by the 
    ''' clearFov and clearRange.
    ''' </summary>
    ''' <param name="patch"></param>
    ''' <remarks></remarks>
    Public Overrides Sub RenderPatch(ByVal patch As Patch)
        MyBase.RenderPatch(patch)

        'construct polygon of freespace exterior
        'this is necessary to intersect the cleared space
        'with the freespace later, since also the victsensor
        'cannot look through obstacles.

        Using freeSpace As New GraphicsPath

            freeSpace.StartFigure()
            freeSpace.AddPolygon(patch.GlobalPolygon)
            freeSpace.CloseFigure()

            'now construct the clear-space triangle in patch-local coords
            Dim origin As Vector2 = patch.EstimatedOrigin.Position
            Dim lpoint As New Vector2(Cos(-Me._ClearFov / 2) * Me._ClearRange, Sin(-Me._ClearFov / 2) * Me._ClearRange)
            Dim rpoint As New Vector2(Cos(+Me._ClearFov / 2) * Me._ClearRange, Sin(+Me._ClearFov / 2) * Me._ClearRange)

            'use Pose2D classes (with dummy rotation) to project triangle to global coords
            lpoint = New Pose2D(lpoint, 0).ToGlobal(patch.EstimatedOrigin).Position
            rpoint = New Pose2D(rpoint, 0).ToGlobal(patch.EstimatedOrigin).Position

            'create triangle path (= 3 lines)
            Using clearSpace As New GraphicsPath

                clearSpace.StartFigure()
                clearSpace.AddLine(CType(origin.X, Single), CType(origin.Y, Single), CType(lpoint.X, Single), CType(lpoint.Y, Single))
                clearSpace.AddLine(CType(lpoint.X, Single), CType(lpoint.Y, Single), CType(rpoint.X, Single), CType(rpoint.Y, Single))
                clearSpace.AddLine(CType(rpoint.X, Single), CType(rpoint.Y, Single), CType(origin.X, Single), CType(origin.Y, Single))
                clearSpace.CloseFigure()

                'intersect freespace and clearspace using the Region class
                Using clearRegion As New Region(clearSpace)

                    clearRegion.Intersect(freeSpace)

                    'we need to ensure bounds
                    'most of the time the freespace layer will have done that already, but just in case
                    'the freespace layer was hidden ...
                    Me.Parent.EnsureMinimumImageBounds(clearSpace.GetBounds(Me.Parent.TransformationMatrix))

                    'plot the region into the graphics buffer
                    Me.Gfx.FillRegion(Me._ClearSpaceBrush, clearRegion)

                End Using
            End Using
        End Using

    End Sub

End Class
