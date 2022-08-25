Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.Windows.Forms

Imports UvARescue.Math


Public Class ObservationsLayer
    Inherits BufferedLayer

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)
        'Asked were shades or read, such as Maroon #800000, DarkRed #8B0000)
        Me._PoseColor = Color.Chartreuse '#7FFF00 The path is per definition cleared
        Me._ObstacleColor = Color.Black
        Me.ResetPoseBrush()
        Me.ResetObstacleBrush()
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not IsNothing(Me._PoseBrush) Then
            Me._PoseBrush.Dispose()
            Me._PoseBrush = Nothing
        End If
        If Not IsNothing(Me._ObstacleBrush) Then
            Me._ObstacleBrush.Dispose()
            Me._ObstacleBrush = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

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

    Private _ObstacleColor As Color
    Private _ObstacleBrush As Brush
    Public Property ObstacleColor() As Color
        Get
            Return Me._ObstacleColor
        End Get
        Set(ByVal value As Color)
            If Not value = Me._ObstacleColor Then
                Me._ObstacleColor = value
                Me.ResetObstacleBrush()
            End If
        End Set
    End Property
    Protected ReadOnly Property ObstacleBrush() As Brush
        Get
            Return Me._ObstacleBrush
        End Get
    End Property
    Private Sub ResetObstacleBrush()
        If Not IsNothing(Me._ObstacleBrush) Then
            Me._ObstacleBrush.Dispose()
        End If
        Me._ObstacleBrush = New SolidBrush(Me._ObstacleColor)
    End Sub


    Public Overrides Sub RenderPatch(ByVal patch As Patch, ByVal origin As Math.Vector2, ByVal points() As Math.Vector2)
        MyBase.RenderPatch(patch, origin, points)

        'create path in manifold-ooords (= world coords)
        Dim obstacles As New GraphicsPath
        With patch.Scan
            If .Range.Length > 0 Then
                For i As Integer = 0 To .Range.Length - 1
                    If .Range(i) > .MinRange AndAlso .Range(i) < .MaxRange Then
                        obstacles.AddRectangle(Me.ComputeScaledRect(points(i), 1))
                    End If
                Next
            End If
        End With

        Me.Parent.EnsureMinimumImageBounds(obstacles.GetBounds(Me.Parent.TransformationMatrix))

        'plot all into the graphics buffer
        Me.Gfx.FillPath(Me._ObstacleBrush, obstacles)
        Me.Gfx.FillEllipse(Me._PoseBrush, Me.ComputeScaledRect(origin, 3))

    End Sub

End Class
