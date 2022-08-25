Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.IO
Imports Microsoft.VisualBasic.FileIO

Imports UvARescue.Math

Public Class NotesLayer
    Inherits ManifoldLayer

#Region " Constructor / Destructor "


    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)

        Me._VictimsColor = Color.Red
        Me.ResetVictimsPen()

        Me._GoalColor = Color.Red
        Me.ResetGoalPen()

        Me.ReadInPoints()

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

#Region " Read in Goal points "


    Private goals As List(Of Point)
    Private elevations As List(Of Double)

    ' This function written for the Teleoperation test of RoboCup 2009.
    ' Text file with goal points must sit in the root directory of the code
    ' (i.e. if your visual studio project is in C:\projects\thisproject, the text file must sit in C:\projects.
    Public Sub ReadInPoints()
        goals = New List(Of Point)
        elevations = New List(Of Double)
        Dim inputPath As String = My.Application.Info.DirectoryPath
        For i As Integer = 1 To 4
            inputPath = Directory.GetParent(inputPath).ToString
        Next
        inputPath += "\goals.txt"

        If Not File.Exists(inputPath) Then
            Console.WriteLine("[NotesLayer -> ReadInPoints] File could not be found!")
            Exit Sub
        End If

        Dim reader As StreamReader = File.OpenText(inputPath)
        Dim line As String = reader.ReadLine.Trim
        Dim linecounter As Integer = 0

        Try

            While Not line Is Nothing
                If line.StartsWith(My.Settings.CommentChar) Then
                    Try
                        line = reader.ReadLine.Trim
                    Catch
                        reader.Close()
                        Exit While
                    End Try

                    Continue While
                End If

                line = line.Replace(vbTab, " ").Trim
                Dim parts() As String = Strings.Split(line, " ")

                Dim name As String = parts(0)
                Dim tuple() As String = Strings.Split(parts(1), ",")
                Dim x As Single = Single.Parse(tuple(0))
                Dim y As Single = Single.Parse(tuple(1))
                Dim z As Single = Single.Parse(tuple(2))

                goals.Add(New Point(CInt(1000 * x), CInt(1000 * y))) 'Add name?
                elevations.Add(1000 * z)

                Try
                    line = reader.ReadLine.Trim
                Catch
                    reader.Close()
                    Exit While
                End Try

            End While

        Catch
            Console.WriteLine("[NotesLayer.vb] Error reading from file {0}", inputPath)

        Finally
            reader.Close()
        End Try


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

    Private _GoalColor As Color
    Private _GoalPen As Pen
    Public Property GoalColor() As Color
        Get
            Return Me._GoalColor
        End Get
        Set(ByVal value As Color)
            If Not value = Me._GoalColor Then
                Me._GoalColor = value
                Me.ResetGoalPen()
            End If
        End Set
    End Property
    Protected ReadOnly Property GoalPen() As Pen
        Get
            Return Me._GoalPen
        End Get
    End Property
    Private Sub ResetGoalPen()
        If Not IsNothing(Me._GoalPen) Then
            Me._GoalPen.Dispose()
        End If
        Me._GoalPen = New Pen(Me._GoalColor, 2)
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

            For Each key As String In Me.points.Keys
                point = Me.points(key)
                text = Me.texts(key)

                Try
                    Me.DrawReadableString(g, renderingMode, New PointF(point.X - length, point.Y + length), text, True)
                Catch ex As Exception
                    Console.WriteLine(ex)
                End Try

            Next

        End If

        ' The code below only written for the Teleoperation test of RoboCup2009.
        Using crosspath As New GraphicsPath
            Dim length As Single = 100 'mm, length of cross-lines

            For i As Integer = 0 To goals.Count - 1
                Dim goal As Point = goals.Item(i)
                Dim elevation As Double = elevations.Item(i)

                Try
                    'construct cross centered on point
                    crosspath.StartFigure()
                    crosspath.AddLine(CInt(goal.X - length), CInt(goal.Y - length), CInt(goal.X + length), CInt(goal.Y + length))
                    crosspath.CloseFigure()
                    crosspath.StartFigure()
                    crosspath.AddLine(CInt(goal.X - length), CInt(goal.Y + length), CInt(goal.X + length), CInt(goal.Y - length))
                    crosspath.CloseFigure()
                    'Me.DrawReadableString(g, renderingMode, New PointF(goal.X - length, goal.Y + length), String.Format("{0:0.00} {1:0.00} {2:0.00}", goal.X / 1000, goal.Y / 1000, elevation / 1000), True)

                Catch ex As Exception
                    Console.WriteLine(ex)
                End Try
            Next

            'transform from woorld coords to page coords
            crosspath.Transform(Me.Parent.TransformationMatrix)

            g.DrawPath(Me._GoalPen, crosspath)
        End Using
    End Sub

#End Region

End Class
