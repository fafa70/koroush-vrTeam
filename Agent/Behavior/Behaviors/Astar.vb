Imports UvARescue.Math
Imports System.Collections.Specialized
Imports System.IO


Public Class Astar

    Public Sub New(ByVal startPoint As Pose2D, ByVal endPoint As Pose2D)
        Dim startNode As New node(New Drawing.Point(CInt(startPoint.X), CInt(startPoint.Y)), True, True)

        'myGrid.Add(startNode)

        Me.endArea = endPoint
        Me.startArea = startPoint
        For i As Integer = 0 To 12
            Me.HeuristicDistance.Add(1000000000.0)
            Me.realDistance.Add(10000)
        Next

        For i As Integer = 0 To 3
            Me.myGrid.Add(False)
        Next


    End Sub

    Public Sub Save(ByVal filename As String)
        Try
            filename = Path.GetFullPath(filename)
            Using writer As New StreamWriter(filename, False)
                For Each section As String In Me.evidence
                    writer.WriteLine(section)
                Next

                writer.Flush()
                writer.Close()

            End Using

            'saved successfully
            ' Me._FileName = filename
        Catch ex As Exception
            Console.Out.WriteLine("Could not save to '" + filename + "'.")
        End Try
    End Sub

    Public Sub addHeuristicValue(ByVal index As Integer, ByVal distance As Double)

        Me.HeuristicDistance(index) = distance


    End Sub


    Public Sub addRealDistance(ByVal index As Integer, ByVal distance As Double)
        Me.realDistance(index) = distance

    End Sub



    Public Function findMin(ByVal mainList As List(Of Double)) As Integer
        Dim listIndex As Integer = 0

        For i As Integer = 0 To 8
            If (mainList(listIndex) < mainList(i)) Then
                'nothing
            Else
                listIndex = i
            End If
        Next

        Return listIndex

    End Function




    Public Sub addMemory(ByVal index As Integer, ByVal checked As Boolean)

        Me.myGrid(index) = checked

    End Sub




    Function showMemory(ByVal index As Integer) As Boolean
        Return myGrid(index)

    End Function




    Private wholeDistance As Single
    Public myGrid As New List(Of Boolean)
    Private node_x As Integer = 1
    Private node_y As Integer = 1

    Public evidence As New List(Of String)
    Public HeuristicDistance As New List(Of Double)
    Public realDistance As New List(Of Double)
    Public heading As Integer

    Public endArea As Pose2D
    Public startArea As Pose2D


End Class
