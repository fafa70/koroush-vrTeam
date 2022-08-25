Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math

Imports UvARescue.Math


Public Class ObstaclesLayer
    Inherits BufferedLayer

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)

        Me._ObstacleColor = Color.Black
        Me.ResetObstacleBrush()

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not IsNothing(Me._ObstacleBrush) Then
            Me._ObstacleBrush.Dispose()
            Me._ObstacleBrush = Nothing
        End If
        MyBase.Dispose(disposing)
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


    Public Overrides Sub RenderPatch(ByVal patch As Patch)
        MyBase.RenderPatch(patch)

        If IsNothing(patch) OrElse IsNothing(patch.Scan) OrElse IsNothing(patch.Scan.Range) Then
            'Console.WriteLine("[ObstacleLayer:RenderPatch]: empty patch, no obstacles to render")
        Return
        End If
        'create path in manifold-coords (= world coords)
        Using path As New GraphicsPath

            Dim points() As Vector2 = patch.GlobalPoints

            With patch.Scan
                Try
                    If .Range.Length > 0 Then
                        For i As Integer = 0 To .Range.Length - 1
                            If .Range(i) > .MinRange AndAlso .Range(i) < .MaxRange Then
                                path.AddRectangle(Me.ComputeScaledRect(points(i), 1))
                            End If
                        Next
                    End If
                Catch

                End Try
            End With

            Me.Parent.EnsureMinimumImageBounds(path.GetBounds(Me.Parent.TransformationMatrix))

            'plot all into the graphics buffer
            Me.Gfx.FillPath(Me._ObstacleBrush, path)

        End Using

    End Sub

End Class
