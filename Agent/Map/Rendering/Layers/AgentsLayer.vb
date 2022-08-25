Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.IO
Imports UvARescue.Math

Public Class AgentsLayer
    Inherits BufferedLayer

#Region " Constructor / Destructor "

    
    '    Public ReadOnly Property agentShow() As Agent
    '       Get
    '          Return Me._agent
    '     End Get
    'End Property


    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)

        Me._AgentsColor = Color.Black



        Me.ResetAgentsPen()

        Me._PoseColor = Color.Chartreuse '#7FFF00 The path is per definition cleared
        Me.ResetPoseBrush()

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then

            Try

                If Not IsNothing(Me._PoseBrush) Then
                    Me._PoseBrush.Dispose()
                    Me._PoseBrush = Nothing
                End If
                If Not IsNothing(Me._AgentsPen) Then
                    Me._AgentsPen.Dispose()
                    Me._AgentsPen = Nothing
                End If

            Catch ex As Exception
                Console.Error.WriteLine(ex.ToString)

            End Try

        End If

        MyBase.Dispose(disposing)

    End Sub

#End Region

#Region " Brushes "

    Private _PoseColor As Color
    Private _PoseBrush As Brush
    Public _PoseBrush2 As Brush = New SolidBrush(Color.Red)
    Private _agent2 As Agent


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

#End Region

#Region " Maintain Information "

    Public _AgentsColor As Color
    Private _AgentsPen As Pen
    Public Property AgentsColor() As Color
        Get
            Return Me._AgentsColor
        End Get
        Set(ByVal value As Color)
            If Not value = Me._AgentsColor Then
                Me._AgentsColor = value
                Me.ResetAgentsPen()
            End If
        End Set
    End Property
    Protected ReadOnly Property AgentsPen() As Pen
        Get
            Return Me._AgentsPen
        End Get
    End Property
    Private Sub ResetAgentsPen()
        If Not IsNothing(Me._AgentsPen) Then
            Me._AgentsPen.Dispose()
        End If
        Me._AgentsPen = New Pen(Me._AgentsColor)
    End Sub


    Private poses As New Dictionary(Of Guid, Pose2D)
    Private robotModel As New Dictionary(Of Guid, String)
    Private sizes As New Dictionary(Of Guid, Size)
    Private names As New Dictionary(Of Guid, String)
    Private goalPoses As New Dictionary(Of Guid, Point)

    Public Overrides Sub RenderAgent(ByVal agent As Agent, ByVal pose As Math.Pose2D, ByVal goal As Pose2D)
        MyBase.RenderAgent(agent, pose, goal)


        If Not Parent.Manifold._changeMotion.ContainsKey(agent.Number) Then
            Me.Parent.Manifold._changeMotion.Add(agent.Number, False)
        End If


        Me._agent2 = agent
        ' Console.WriteLine("{0}", Me._agent2.Number)
        If Not Me.poses.ContainsKey(agent.UniqueID) Then
            Me.poses.Add(agent.UniqueID, pose)
            Me.sizes.Add(agent.UniqueID, agent.Size)
            Me.names.Add(agent.UniqueID, agent.Name)
            Me.robotModel.Add(agent.UniqueID, agent.RobotModel)

            'If (agent._newTarget.X = 0 AndAlso agent._newTarget.Y = 0) Then
            'Else
            Me.goalPoses.Add(agent.UniqueID, New Point(CInt(goal.X), CInt(goal.Y)))
            'End If

        Else
            'no need to update sizes and names, only the pose is dynamic
            Me.poses(agent.UniqueID) = pose
            
            Me.goalPoses(agent.UniqueID) = New Point(CInt(goal.X), CInt(goal.Y))
            
        End If



    End Sub

    Friend Overrides Sub NotifyImageReset()
        MyBase.NotifyImageReset()

        Me.poses.Clear()
        Me.sizes.Clear()
        Me.names.Clear()

    End Sub

#End Region

#Region " Rendering "

    Public Overrides Sub RenderPatch(ByVal patch As Patch)
        MyBase.RenderPatch(patch)

        'paint patch origins
        'If patch.AgentName = "test" Then
        '    Me.Gfx.FillEllipse(Me._PoseBrush2, Me.ComputeScaledRect(patch.EstimatedOrigin.Position, 1))
        'Else
        Me.Gfx.FillEllipse(Me._PoseBrush, Me.ComputeScaledRect(patch.EstimatedOrigin.Position, 3))
        'End If
    End Sub

    Public Overrides Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode)
        MyBase.Draw(g, renderingMode)

        If Not IsNothing(Me.poses) AndAlso Me.poses.Count > 0 Then
            If Not (Parent.Manifold.currNumber = 0) Then
                Console.WriteLine("change position : {0}", Parent.Manifold._changeMotion(Parent.Manifold.currNumber))
            End If
            'construct graphics path with current agent poses
            Using path As New GraphicsPath

                Dim pose As Pose2D
                Dim size As Size
                Dim model As String

                For Each key As Guid In Me.poses.Keys
                    pose = Me.poses(key)
                    size = Me.sizes(key)
                    model = Me.robotModel(key)
                    If (model.ToLower = "p3at") Then
                        'construct ellipse centered on pose
                        path.StartFigure()
                        path.AddEllipse(New Rectangle(New Point(CInt(pose.X - size.Width / 2), CInt(pose.Y - size.Height / 2)), size))
                        path.CloseFigure()

                        'construct line indicating the rotation
                        Dim len As Single = Max(size.Height, size.Width)
                        Dim dx As Single = CType(Cos(pose.Rotation) * len, Single)
                        Dim dy As Single = CType(Sin(pose.Rotation) * len, Single)

                        path.StartFigure()
                        path.AddLine(CType(pose.X, Single), CType(pose.Y, Single), CType(pose.X + dx, Single), CType(pose.Y + dy, Single))
                        path.CloseFigure()

                        'also paint the agent's name just outside the circle's radius
                        Me.DrawReadableString(g, renderingMode, New PointF(CType(pose.X + 0.5F * Max(size.Height, size.Width), Single), CType(pose.Y + 0.5F * Max(size.Height, size.Width), Single)), Me.names(key))

                    ElseIf (model.ToLower = "comstation") Then
                        'construct ellipse centered on pose
                        path.StartFigure()
                        path.AddRectangle(New Rectangle(New Point(CInt(pose.X - size.Width / 2), CInt(pose.Y - size.Height / 2)), size))
                        path.CloseFigure()

                        'construct line indicating the rotation
                        Dim len As Single = Max(size.Height, size.Width)
                        Dim dx As Single = CType(Cos(pose.Rotation) * len, Single)
                        Dim dy As Single = CType(Sin(pose.Rotation) * len, Single)

                        path.StartFigure()
                        'path.AddLine(CType(pose.X, Single), CType(pose.Y, Single), CType(pose.X + dx, Single), CType(pose.Y + dy, Single))
                        path.CloseFigure()

                        'also paint the agent's name just outside the circle's radius
                        Me.DrawReadableString(g, renderingMode, New PointF(CType(pose.X + 0.5F * Max(size.Height, size.Width), Single), CType(pose.Y + 0.5F * Max(size.Height, size.Width), Single)), Me.names(key))

                    ElseIf (model.ToLower = "airrobot") Then
                        path.StartFigure()
                        path.AddRectangle(New Rectangle(New Point(CInt(pose.X - size.Width / 2), CInt(pose.Y - size.Height / 2)), size))
                        path.CloseFigure()

                        'construct line indicating the rotation
                        Dim len As Single = Max(size.Height, size.Width)
                        Dim dx As Single = CType(Cos(pose.Rotation) * len, Single)
                        Dim dy As Single = CType(Sin(pose.Rotation) * len, Single)

                        path.StartFigure()
                        path.AddLine(CType(pose.X, Single), CType(pose.Y, Single), CType(pose.X + dx, Single), CType(pose.Y + dy, Single))
                        path.CloseFigure()

                        'also paint the agent's name just outside the circle's radius
                        Me.DrawReadableString(g, renderingMode, New PointF(CType(pose.X + 0.5F * Max(size.Height, size.Width), Single), CType(pose.Y + 0.5F * Max(size.Height, size.Width), Single)), Me.names(key))
                    End If


                Next

                'transform from world coords to page coords
                path.Transform(Me.Parent.TransformationMatrix)

                g.DrawPath(Me._AgentsPen, path)
                Dim path2 As GraphicsPath = New GraphicsPath()

                Dim checker As Integer = 1

                If (Parent.Manifold.currNumber = 0) Then

                Else

                    For Each elem As Guid In poses.Keys
                        If (Parent.Manifold.currNumber = checker) Then
                            Console.WriteLine("change behaviour : {0}", Me.Parent.Manifold._changeMotion(Me.Parent.Manifold.currNumber))

                            path2.StartFigure()

                            path2.AddRectangle(New Rectangle(New Point(CInt(poses(elem).X - size.Width / 2), CInt(poses(elem).Y - size.Height / 2)), size))
                            path2.CloseFigure()
                            path.Transform(Me.Parent.TransformationMatrix)
                            path2.Transform(Me.Parent.TransformationMatrix)
                            g.DrawPath(Me._AgentsPen, path)
                            g.DrawPath(Pens.Red, path2)

                            If Not (Me.Parent.Manifold._targetDataBase.ContainsKey(Me.Parent.Manifold.currNumber)) Then

                            Else
                                Dim path3 As GraphicsPath = New GraphicsPath()
                                If (Me.Parent.Manifold._changeMotion(Me.Parent.Manifold.currNumber) = False) Then

                                    path3.StartFigure()

                                    path3.AddLine(New Point(CInt(Me.poses(elem).X), CInt(Me.poses(elem).Y)), New Point(CInt(Me.Parent.Manifold._targetDataBase(Parent.Manifold.currNumber).X), CInt(Me.Parent.Manifold._targetDataBase(Parent.Manifold.currNumber).Y)))
                                    'Next
                                    path3.CloseFigure()
                                    path3.Transform(Me.Parent.TransformationMatrix)
                                    g.DrawPath(Pens.Red, path3)
                                    'Me._agent2._finished = True
                                    Me._agent2.setNewTarget(Parent.Manifold.currNumber, New Point(CInt(Me.Parent.Manifold._targetDataBase(Parent.Manifold.currNumber).X), CInt(Me.Parent.Manifold._targetDataBase(Parent.Manifold.currNumber).Y)))
                                    ' Console.WriteLine("fafa!!!!!!!!{0}", goalPoses(elem))
                                ElseIf (Me.Parent.Manifold._changeMotion(Me.Parent.Manifold.currNumber) = True) Then

                                    Try

                                        path3.StartFigure()
                                        path3.AddRectangle(New System.Drawing.Rectangle(CInt(Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).X), CInt(Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).Y), CInt((Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).X - Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).X)), CInt((Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).Y - Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).Y))))
                                        path3.AddRectangle(New Drawing.Rectangle(New Point(CInt((Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).X + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).X) / 2), CInt((Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).Y + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).Y) / 2)), New Size(200, 200)))
                                        path3.AddLine(New Point(CInt(Me.poses(elem).X), CInt(Me.poses(elem).Y)), New Point(CInt((Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).X + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).X) / 2), CInt((Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).Y + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).Y) / 2)))
                                        path3.CloseFigure()
                                        path3.Transform(Me.Parent.TransformationMatrix)
                                        If (Parent.Manifold._targetDataBase.ContainsKey(Parent.Manifold.currNumber)) Then
                                            Parent.Manifold._targetDataBase(Parent.Manifold.currNumber) = New Pose2D((Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).X + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).X) / 2, (Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).Y + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).Y) / 2, Atan((Parent.Manifold.startArea(Parent.Manifold.currNumber).Y + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).Y) / (Parent.Manifold.startArea(Parent.Manifold.currNumber).X + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).X)))
                                        Else
                                            Parent.Manifold._targetDataBase.Add(Parent.Manifold.currNumber, New Pose2D((Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).X + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).X) / 2, (Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).Y + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).Y) / 2, Atan((Parent.Manifold.startArea(Parent.Manifold.currNumber).Y + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).Y) / (Parent.Manifold.startArea(Parent.Manifold.currNumber).X + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).X))))
                                        End If
                                        g.DrawPath(Pens.Red, path3)
                                        ' Me._agent2.setNewTarget(Parent.Manifold.currNumber, New Point(CInt((Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).X + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).X) / 2), CInt((Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).Y + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).Y) / 2)))
                                        'Console.WriteLine("pose : {0} {1}", Me.Parent.Manifold._targetDataBase(Parent.Manifold.currNumber).X, Me.Parent.Manifold._targetDataBase(Parent.Manifold.currNumber).Y)
                                        ' Me._agent2.setNewTarget(Me.Parent.Manifold.currNumber, New Point(CInt(((Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).X + Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).X) / 2)), CInt((Me.Parent.Manifold.endArea(Parent.Manifold.currNumber).Y + Me.Parent.Manifold.startArea(Parent.Manifold.currNumber).Y) / 2)))
                                        Me._agent2.search(Me.Parent.Manifold.currNumber, Me.Parent.Manifold.startArea(Parent.Manifold.currNumber), Me.Parent.Manifold.endArea(Parent.Manifold.currNumber))
                                    Catch ex As Exception

                                    End Try

                                End If


                            End If

                            checker = checker + 1
                        Else

                            checker = checker + 1
                        End If

                    Next

                End If


            End Using

        End If

    End Sub

#End Region

End Class
