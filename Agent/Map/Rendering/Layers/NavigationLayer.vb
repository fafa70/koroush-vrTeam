Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math

Imports UvARescue.Math


Public Class NavigationLayer
    Inherits BufferedLayer

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        MyBase.Dispose(disposing)
    End Sub


    Public Overrides Sub RenderPatch(ByVal patch As Patch)
        MyBase.RenderPatch(patch)

        'create paths in manifold-ooords (= world coords)
        Using obstpath As New GraphicsPath

            Dim origin As Vector2 = patch.EstimatedOrigin.Position
            Dim points() As Vector2 = patch.GlobalPoints
            Dim filter() As Boolean = patch.Filter

            Dim polygon(points.Length) As PointF 'the origin will be appended too
            With patch.Scan
                If .Range.Length > 0 Then
                    For i As Integer = 0 To .Range.Length - 1
                        If filter(i) Then
                            polygon(i) = New PointF(CType(origin.X, Single), CType(origin.Y, Single))
                        Else
                            obstpath.AddRectangle(Me.ComputeScaledRect(points(i), 1))
                            polygon(i) = New PointF(CType(points(i).X, Single), CType(points(i).Y, Single))
                        End If
                    Next
                End If
            End With

            'add origin
            polygon(polygon.Length - 1) = New PointF(CType(origin.X, Single), CType(origin.Y, Single))



            Using freepath As New GraphicsPath

                freepath.StartFigure()
                freepath.AddPolygon(polygon)
                freepath.CloseFigure()


                Dim rect As RectangleF = obstpath.GetBounds(Me.Parent.TransformationMatrix)
                Using obst As New Bitmap(CInt(rect.Width), CInt(rect.Height))
                    Using free As New Bitmap(CInt(rect.Width), CInt(rect.Height))

                        Using gfx As Graphics = Graphics.FromImage(obst)
                            gfx.Transform = Me.Parent.TransformationMatrix
                            gfx.TranslateTransform(-rect.X, -rect.Y)
                            gfx.Clear(Color.Blue)
                            gfx.FillPath(Brushes.Black, obstpath)
                        End Using

                        Using gfx As Graphics = Graphics.FromImage(free)
                            gfx.Transform = Me.Parent.TransformationMatrix
                            gfx.TranslateTransform(-rect.X, -rect.Y)
                            gfx.Clear(Color.Blue)
                            gfx.FillPath(Brushes.White, freepath)
                        End Using

                    End Using
                End Using
            End Using
        End Using

    End Sub

End Class
