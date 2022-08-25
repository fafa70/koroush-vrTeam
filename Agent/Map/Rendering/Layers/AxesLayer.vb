Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math

Imports UvARescue.Math

Public Class AxesLayer
    Inherits ManifoldLayer

#Region " Constructor / Destructor "

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)
        Me._AxisColor = Color.Cyan
        Me.ResetAxisPen()
        Me.ResetAxisBrush()
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then

            Try

                If Not IsNothing(Me.Path) Then
                    Me.Path.Dispose()
                    Me.Path = Nothing
                End If
                If Not IsNothing(Me._AxisPen) Then
                    Me._AxisPen.Dispose()
                    Me._AxisPen = Nothing
                End If
                If Not IsNothing(Me._AxisBrush) Then
                    Me._AxisBrush.Dispose()
                    Me._AxisBrush = Nothing
                End If

            Catch ex As Exception
                Console.Error.WriteLine(ex.ToString)

            End Try

        End If

        MyBase.Dispose(disposing)

    End Sub

#End Region

#Region " Maintain GraphicsPath "

    Protected Path As New GraphicsPath

    Private _AxisColor As Color
    Private _AxisPen As Pen
    Private _AxisBrush As Brush
    Public Property AxisColor() As Color
        Get
            Return Me._AxisColor
        End Get
        Set(ByVal value As Color)
            If Not value = Me._AxisColor Then
                Me._AxisColor = value
                Me.ResetAxisPen()
                Me.ResetAxisBrush()
            End If
        End Set
    End Property
    Protected ReadOnly Property AxisPen() As Pen
        Get
            Return Me._AxisPen
        End Get
    End Property
    Private Sub ResetAxisPen()
        If Not IsNothing(Me._AxisPen) Then
            Me._AxisPen.Dispose()
        End If
        Me._AxisPen = New Pen(Me._AxisColor)
    End Sub
    Protected ReadOnly Property AxisBrush() As Brush
        Get
            Return Me._AxisBrush
        End Get
    End Property
    Private Sub ResetAxisBrush()
        If Not IsNothing(Me._AxisBrush) Then
            Me._AxisBrush.Dispose()
        End If
        Me._AxisBrush = New SolidBrush(Me._AxisColor)
    End Sub

    Friend Overrides Sub NotifyImageReset()
        MyBase.NotifyImageReset()

        Me.Path.Reset()

    End Sub

    Friend Overrides Sub NotifyImageResize()
        MyBase.NotifyImageResize()

        Me.Path.Reset()

        Dim rect As Rectangle = Me.Parent.ManifoldRect
        Dim meter As Integer = CInt(Me.Parent.Resolution / Me.Parent.TransformationScale)
        Dim font As Font = SystemFonts.DefaultFont

        'draw axis from left to right
        Me.Path.StartFigure()
        Me.Path.AddLine(CInt(Round(rect.Left / meter) * meter), 0, CInt(Round(rect.Right / meter) * meter), 0)
        Me.Path.CloseFigure()

        'draw axis from top to bottom
        Me.Path.StartFigure()
        Me.Path.AddLine(0, CInt(Round(rect.Top / meter) * meter), 0, CInt(Round(rect.Bottom / meter) * meter))
        Me.Path.CloseFigure()

        'indicate meters
        Dim length As Integer = CInt(meter / 4)
        For x As Integer = CInt(Round(rect.Left / meter)) To CInt(Round(rect.Right / meter))
            Me.Path.StartFigure()
            Me.Path.AddLine(x * meter, -length, x * meter, length)
            Me.Path.CloseFigure()

            If x Mod 5 = 0 Then
                Me.Path.AddString(String.Format("{0:f1}", x), font.FontFamily, CInt(FontStyle.Regular), CType(meter / 3, Single), New PointF(x * meter - meter / 10.0F, length * 1.2F), StringFormat.GenericDefault)
            End If

        Next
        For y As Integer = CInt(Round(rect.Top / meter)) To CInt(Round(rect.Bottom / meter))
            Me.Path.StartFigure()
            Me.Path.AddLine(-length, y * meter, length, y * meter)
            Me.Path.CloseFigure()

            If y Mod 5 = 0 Then
                Me.Path.AddString(String.Format("{0:f1}", y), font.FontFamily, CInt(FontStyle.Regular), CType(meter / 2, Single), New PointF(length * 1.2F, y * meter - meter / 10.0F), StringFormat.GenericDefault)
            End If

        Next

        Me.Path.Transform(Me.Parent.TransformationMatrix)

    End Sub

#End Region

#Region " Rendering "

    Public Overrides Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode)
        MyBase.Draw(g, renderingMode)
        If Not IsNothing(Me.Path) Then
            g.DrawPath(Me._AxisPen, Me.Path)
            g.FillPath(Me._AxisBrush, Me.Path)
        End If
    End Sub

#End Region

End Class
