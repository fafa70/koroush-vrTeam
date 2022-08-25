Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math

Imports UvARescue.Math


Public MustInherit Class ManifoldLayer
    Implements IDisposable

#Region " Constructor / Destructor "

    Protected Sub New(ByVal parent As ManifoldImage)
        If IsNothing(parent) Then Throw New ArgumentNullException("parent")
        Me._Parent = parent
    End Sub

    Private disposedValue As Boolean = False        ' To detect redundant calls
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If disposing Then

            'SyncLock Me.Mutex
            '    Try

            '    Catch ex As Exception
            '        Console.Error.WriteLine(ex.ToString)

            '    End Try

            'End SyncLock

        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

#Region " Base Layer Functionality "

    Private _Parent As ManifoldImage
    Public ReadOnly Property Parent() As ManifoldImage
        Get
            Return Me._Parent
        End Get
    End Property

    Public Overridable Sub RenderAgent(ByVal agent As Agent, ByVal pose As Pose2D, ByVal point As Pose2D)
    End Sub

    Public Overridable Sub RenderPatch(ByVal patch As Patch)
    End Sub

    Public Overridable Sub RenderRelation(ByVal relation As Relation)
    End Sub

    Public Overridable Sub RenderVictim(ByVal victim As VictimObservation)
    End Sub

    Public Overridable Sub RenderOmnicam(ByVal patch As Patch)
    End Sub

    Friend Overridable Sub NotifyImageResize()
    End Sub

    Friend Overridable Sub NotifyImageReset()
    End Sub







    Protected Overridable Function ComputeScaledRect(ByVal center As Vector2, ByVal size As Single) As RectangleF
        Return Me.ComputeScaledRect(New PointF(CType(center.X, Single), CType(center.Y, Single)), size, size)
    End Function
    Protected Overridable Function ComputeScaledRect(ByVal center As Vector2, ByVal width As Single, ByVal height As Single) As RectangleF
        Return Me.ComputeScaledRect(New PointF(CType(center.X, Single), CType(center.Y, Single)), width, height)
    End Function
    Protected Overridable Function ComputeScaledRect(ByVal center As PointF, ByVal size As Single) As RectangleF
        Return Me.ComputeScaledRect(center, size, size)
    End Function
    Protected Overridable Function ComputeScaledRect(ByVal center As Vector2, ByVal size As Size) As RectangleF
        Return Me.ComputeScaledRect(center, CType(size.Width, Single), CType(size.Height, Single))
    End Function
    Protected Overridable Function ComputeScaledRect(ByVal center As PointF, ByVal width As Single, ByVal height As Single) As RectangleF
        Dim wscaled As Single = width / Me.Parent.TransformationScale
        Dim hscaled As Single = height / Me.Parent.TransformationScale
        Return New RectangleF(CType(center.X - 0.5 * wscaled, Single), CType(center.Y - 0.5 * hscaled, Single), wscaled, hscaled)
    End Function

#End Region

#Region " Drawing "

    Public Overridable Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode)
    End Sub

    Protected Sub DrawReadableString(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode, ByVal point As PointF, ByVal text As String, Optional ByVal boxed As Boolean = False)

        Dim angle As Single = Me.Parent.GetRotationOffset(renderingMode)
        Dim font As Font = New Font(SystemFonts.DefaultFont.FontFamily.Name, 4)


        'Console.WriteLine(String.Format("ReadableString font name: {0}", font.OriginalFontName))

        Dim points() As PointF = New PointF() {point}
        Me.Parent.TransformationMatrix.TransformPoints(points)
        point = points(0)

        g.TranslateTransform(point.X, point.Y)
        g.RotateTransform(-angle)

        If boxed Then

            'add nice box 
            Dim rectf As New RectangleF(New PointF(0.0F, 0.0F), g.MeasureString(text, font))
            rectf.Inflate(1, 1) 'some spacing

            g.FillRectangle(Brushes.LightYellow, rectf)
            g.DrawRectangle(Pens.Black, rectf.X, rectf.Y, rectf.Width, rectf.Height)

        End If

        g.DrawString(text, font, Brushes.Black, 0.0F, 0.0F)

        g.RotateTransform(angle)
        g.TranslateTransform(-point.X, -point.Y)

    End Sub

#End Region

End Class
