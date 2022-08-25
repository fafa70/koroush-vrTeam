Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.IO

Imports UvARescue.Math

Public Class CommsLayer
    Inherits ManifoldLayer

#Region " Constructor / Destructor "


    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)
        Me._CommColor = Color.Black
        Me.ResetCommPen()
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            Try
                If Not IsNothing(Me._CommPen) Then
                    Me._CommPen.Dispose()
                    Me._CommPen = Nothing
                End If
            Catch ex As Exception
                Console.Error.WriteLine(ex.ToString)
            End Try
        End If
        MyBase.Dispose(disposing)
    End Sub

#End Region

#Region " Maintain Information "

    'Private _ColorStrong As Color = Color.Green
    'Private _ColorMedium As Color = Color.Orange
    'Private _ColorBad As Color = Color.OrangeRed
    'Private _ColorVeryBad As Color = Color.Red

    Private _CommColor As Color

    Private _CommPen As Pen
    Protected ReadOnly Property CommPen() As Pen
        Get
            Return Me._CommPen
        End Get
    End Property
    Private Sub ResetCommPen()
        If Not IsNothing(Me._CommPen) Then
            Me._CommPen.Dispose()
        End If
        Me._CommPen = New Pen(Me._CommColor, 2)
    End Sub


    Public Sub AddBaseStationPose(ByVal uniqueID As Guid, ByVal pose As Pose2D)
        Me.poses.Add(uniqueID, pose)
        Me.numbers.Add(uniqueID, 0)
    End Sub


    Friend Overrides Sub NotifyImageReset()
        MyBase.NotifyImageReset()
        Me.poses.Clear()
    End Sub

#End Region

#Region " Rendering "

    Private poses As New Dictionary(Of Guid, Pose2D)
    Private numbers As New Dictionary(Of Guid, Integer)
    Private CommMatrix As Double(,)


    Public Sub RenderCommLinks(ByVal agent As Agent, ByVal pose As Pose2D, ByVal newCommMatrix As Double(,))
        
        If Not Me.poses.ContainsKey(agent.UniqueID) Then
            Me.poses.Add(agent.UniqueID, pose)
            Me.numbers.Add(agent.UniqueID, agent.Number)
        Else
            'no need to update number, only the pose is dynamic
            Me.poses(agent.UniqueID) = pose
        End If

        Me.CommMatrix = newCommMatrix
    End Sub

    Public Overrides Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode)
        MyBase.Draw(g, renderingMode)

        Dim pose1 As Pose2D
        Dim pose2 As Pose2D

        Using path As New GraphicsPath

            For Each agent1 As Guid In Me.poses.Keys
                For Each agent2 As Guid In Me.poses.Keys
                    If (agent1 <> agent2) Then
                        pose1 = Me.poses(agent1)
                        pose2 = Me.poses(agent2)
                        'If (CommMatrix(Me.numbers(agent1), Me.numbers(agent2)) > -91) Then
                        'Me._CommColor = Color.Red
                        'ElseIf (CommMatrix(Me.numbers(agent1), Me.numbers(agent2)) > -85) Then
                        'Me._CommColor = Color.OrangeRed
                        'ElseIf (CommMatrix(Me.numbers(agent1), Me.numbers(agent2)) > -75) Then
                        'Me._CommColor = Color.Orange
                        'ElseIf (CommMatrix(Me.numbers(agent1), Me.numbers(agent2)) > -60) Then
                        'Me._CommColor = Color.Green
                        'End If
                        Me.ResetCommPen()
                        path.StartFigure()
                        path.AddLine(CType(pose1.X, Single), CType(pose1.Y, Single), CType(pose2.X, Single), CType(pose2.Y, Single))
                        path.CloseFigure()
                    End If
                Next
            Next
            path.Transform(Me.Parent.TransformationMatrix)

            g.DrawPath(Me._CommPen, path)

        End Using

    End Sub



#End Region

End Class
