Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math

Imports UvARescue.Math

Public Class VictimsLayer
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

        Try

            If Not IsNothing(Me.points) AndAlso Me.points.Count > 0 Then

                Using crosspath As New GraphicsPath

                    Dim length As Single = 100 'mm, length of cross-lines
                    Dim point As PointF

                    For Each key As String In Me.points.Keys
                        point = Me.points(key)

                        'construct cross centered on point
                        crosspath.StartFigure()
                        crosspath.AddLine(point.X - length, point.Y - length, point.X + length, point.Y + length)
                        crosspath.CloseFigure()
                        crosspath.StartFigure()
                        crosspath.AddLine(point.X - length, point.Y + length, point.X + length, point.Y - length)
                        crosspath.CloseFigure()

                    Next

                    'transform from woorld coords to page coords
                    crosspath.Transform(Me.Parent.TransformationMatrix)

                    g.DrawPath(Me._VictimsPen, crosspath)

                End Using

            End If

        Catch ex As Exception
            Console.WriteLine(ex)
        End Try


    End Sub

#End Region

End Class
