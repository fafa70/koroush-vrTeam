Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.Windows.Forms

Imports UvARescue.Math

Public Class VictimReportsLayer
    Inherits ManifoldLayer

#Region " Constructor / Destructor "

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)

        Me._VictimsColor = Color.Red
        Me.ResetVictimsPen()

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then

            Try

                If Not IsNothing(Me._VictimsPen) Then
                    Me._VictimsPen.Dispose()
                    Me._VictimsPen = Nothing
                End If

            Catch ex As Exception
                Console.Error.WriteLine(ex.ToString)

            End Try

        End If

        MyBase.Dispose(disposing)

    End Sub

#End Region

#Region " Maintain Information "

    Private _VictimsColor As Color
    Private _VictimsPen As Pen
    Public Property VictimsColor() As Color
        Get
            Return Me._VictimsColor
        End Get
        Set(ByVal value As Color)
            If Not value = Me._VictimsColor Then
                Me._VictimsColor = value
                Me.ResetVictimsPen()
            End If
        End Set
    End Property
    Protected ReadOnly Property VictimsPen() As Pen
        Get
            Return Me._VictimsPen
        End Get
    End Property
    Private Sub ResetVictimsPen()
        If Not IsNothing(Me._VictimsPen) Then
            Me._VictimsPen.Dispose()
        End If
        Me._VictimsPen = New Pen(Me._VictimsColor, 2)
    End Sub


    Private points As New Dictionary(Of String, PointF)
    Private texts As New Dictionary(Of String, String)

    Public Overrides Sub RenderVictim(ByVal victim As VictimObservation)
        MyBase.RenderVictim(victim)

        If Not Me.points.ContainsKey(victim.VictimID) Then
            victim.AcquireReaderLock()
            Me.points.Add(victim.VictimID, New PointF(CType(victim.AverageX, Single), CType(victim.AverageY, Single)))
            Me.texts.Add(victim.VictimID, Me.CreateText(victim))
            victim.ReleaseReaderLock()
        Else
            victim.AcquireReaderLock()
            Me.points(victim.VictimID) = New PointF(CType(victim.AverageX, Single), CType(victim.AverageY, Single))
            Me.texts(victim.VictimID) = Me.CreateText(victim)
            victim.ReleaseReaderLock()
        End If

    End Sub

    Protected Overridable Function CreateText(ByVal victim As VictimObservation) As String
        Dim text As String = victim.VictimID & vbNewLine
        text &= String.Format("({0:f2} , {1:f2})", victim.AverageX / 1000, victim.AverageY / 1000) & vbNewLine
        For Each part As String In victim.PartNames
            text &= Me.CreateLine(part, victim.PartCount(part))
        Next
        Return text
    End Function

    Protected Function CreateLine(ByVal partName As String, ByVal count As Integer) As String
        If count > 1 Then
            If partName.EndsWith("s") Then
                partName &= "es"
            ElseIf partName.EndsWith("y") Then
                partName = partName.Substring(0, partName.Length - 1) & "ies"
            Else
                partName &= "s"
            End If
        End If
        Return String.Format("- {1} {0}" & vbNewLine, partName, count)
    End Function

    Friend Overrides Sub NotifyImageReset()
        MyBase.NotifyImageReset()

        Me.points.Clear()
        Me.texts.Clear()

    End Sub

#End Region

#Region " Rendering "

    Public Overrides Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode)
        MyBase.Draw(g, renderingMode)

        If Not IsNothing(Me.points) AndAlso Me.points.Count > 0 Then

            'Dim textpath As New GraphicsPath
            'Dim rectpath As New GraphicsPath

            Dim length As Single = 100 'mm, length of cross-lines

            Dim point As PointF
            Dim text As String

            length += 50

            For Each key As String In Me.points.Keys
                point = Me.points(key)
                text = Me.texts(key)

                Try
                    Me.DrawReadableString(g, renderingMode, New PointF(point.X - length, point.Y + length), text, True)
                Catch ex As Exception
                    Console.WriteLine(ex)
                End Try
                'paint the text 

                'textpath.StartFigure()
                'textpath.AddString(text, font.FontFamily, CInt(font.Style), 150, New PointF(point.X + length, point.Y + length), StringFormat.GenericDefault)
                'textpath.CloseFigure()

                ''add nice background 
                'Dim rectf As New RectangleF(New PointF(point.X + length, point.Y + length), g.MeasureString(text, font))
                'rectf.Inflate(5, 5)

                'rectpath.StartFigure()
                'rectpath.AddRectangle(rectf)
                'rectpath.CloseFigure()

            Next

            'transform from woorld coords to page coords
            'rectpath.Transform(Me.Parent.TransformationMatrix)
            'textpath.Transform(Me.Parent.TransformationMatrix)

            'g.DrawPath(Pens.Black, rectpath)
            'g.FillPath(Brushes.LightYellow, rectpath)
            'g.DrawPath(Pens.Black, textpath)


        End If

    End Sub

#End Region

End Class
