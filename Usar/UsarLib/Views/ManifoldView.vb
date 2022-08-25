Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Math
Imports System.Windows.Forms
Imports UvARescue.Agent
Imports UvARescue.Math


Public Class ManifoldView
    Inherits UserControl
    Implements IManifoldView

#Region " Constructor / Destructor "

    Public Sub New(ByVal image As ManifoldImage)
        Me.InitializeComponent()

        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.UserPaint, True)


        Me._ShowAprioriLayer = True
        Me._ShowFreeSpaceLayer = True
        Me._ShowClearSpaceLayer = True
        Me._ShowObstaclesLayer = True
        Me._ShowAgentsLayer = True

        Me.Image = image

    End Sub

    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.targetPicker = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.SuspendLayout()
        '
        'targetPicker
        '
        Me.targetPicker.Name = "targetPicker"
        Me.targetPicker.Size = New System.Drawing.Size(61, 4)
        '
        'ManifoldView
        '
        Me.Name = "ManifoldView"
        Me.Size = New System.Drawing.Size(286, 201)
        Me.ResumeLayout(False)
        'Me.Image.Manifold.AgentModel = Me._agent.RobotModel
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not IsNothing(Me.Image) Then
                Me.Image.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub
    Friend WithEvents targetPicker As System.Windows.Forms.ContextMenuStrip
    Private components As System.ComponentModel.IContainer
#End Region

#Region " Observe ManifoldImage "






    Private _Image As ManifoldImage
    Public Property Image() As ManifoldImage
        Get
            Return Me._Image
        End Get
        Set(ByVal value As ManifoldImage)
            If Not value Is Me._Image Then
                If Not IsNothing(Me._Image) Then
                    Me._Image.DetachView(Me)
                End If
                If Not IsNothing(value) Then
                    value.AttachView(Me)
                End If
                Me._Image = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Private Delegate Sub ManifoldImageUpdatedHandler()


    Public Sub NotifyManifoldImageUpdated() Implements Agent.IManifoldView.NotifyManifoldImageUpdated
        If Me.InvokeRequired Then
            Dim handler As New ManifoldImageUpdatedHandler(AddressOf Me.NotifyManifoldImageUpdated)
            Me.Invoke(handler)

        Else
            Me.Invalidate()
        End If
    End Sub

#End Region

#Region " Zooming and Panning "

    Private _AutoZoom As Boolean = True
    Private _AutoPan As Boolean = True
    Private _Zoom As Single = 1.0
    Private _Pan As New Point(0, 0)


    Public Sub ZoomIn()
        Me.ZoomTo(Me._Zoom + 0.1F)
    End Sub
    Public Sub ZoomOut()
        Me.ZoomTo(Me._Zoom - 0.1F)
    End Sub
    Public Sub ZoomTo(ByVal zoom As Single)
        Me._AutoZoom = False
        Me._Zoom = Max(zoom, 0.1F)
        Me.Invalidate()
    End Sub
    Public Sub AutoZoom()
        Me._AutoZoom = True
        Me._Zoom = 1.0
        Me._AutoPan = True
        Me._Pan = New Point(0, 0)
        Me.Invalidate()
    End Sub

    Protected Overrides Sub OnMouseWheel(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Me.ZoomTo(Me._Zoom + DirectCast(IIf(e.Delta > 0, 0.1F, -0.1F), Single))
    End Sub


    Private isPanning As Boolean = False
    Private lastPoint As Point
    Private _agent As Agent.Agent
    Protected Overrides Sub OnMouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.OnMouseDown(e)
        Me.lastPoint = e.Location

        If e.Button = Windows.Forms.MouseButtons.Left Then

            Windows.Forms.Cursor.Current = Cursors.SizeAll
            Me._AutoPan = False
            Me.isPanning = True



        ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
            Me.targetPicker = New System.Windows.Forms.ContextMenuStrip(Me.components)
            For Each agent As Agent.Agent In Me.Image.Manifold.GetTrackedAgents()
                Console.WriteLine(e.Location)
                'If (Me.Image.Manifold._changeMotion(Me.Image.Manifold.currNumber) = False) Then
                Dim robo_x As Double = e.Location.X
                Dim robo_y As Double = e.Location.Y
                Dim click_location As Vector2 = New Vector2()
                click_location.X = robo_x
                click_location.Y = robo_y
                Dim click_coord As Vector2 = New Vector2()
                click_coord = Me.Image.PixelToCoordinate(Me.ClickToImageCoord(Me.lastPoint))
                If ((Me.Image.Manifold.GetCurrentPose(agent).X) / 1000 < (click_coord.X) / 1000 + 0.5 AndAlso (Me.Image.Manifold.GetCurrentPose(agent).X) / 1000 > (click_coord.X) / 1000 - 0.5 AndAlso (Me.Image.Manifold.GetCurrentPose(agent).Y) / 1000 < (click_coord.Y) / 1000 + 0.5 AndAlso (Me.Image.Manifold.GetCurrentPose(agent).Y) / 1000 > (click_coord.Y) / 1000 - 0.5) Then
                    'Console.WriteLine("click is for agent {0} {1} {2}", agent.Number, Me.Image.Manifold.GetCurrentPose(agent).X, Me.Image.Manifold.GetCurrentPose(agent).Y)
                    'Console.WriteLine("click pose : {0} {1}", click_coord.X, click_coord.Y)
                    Me.Image.Manifold.currNumber = agent.Number
                    



                Else
                    Dim checker As Integer = 0
                    Dim goalPose As Pose2D = New Pose2D(click_coord, Atan(click_coord.Y / click_coord.X))

                    For Each agent2 As Agent.Agent In Me.Image.Manifold.GetTrackedAgents()
                        If ((Me.Image.Manifold.GetCurrentPose(agent2).X) / 1000 < (click_coord.X) / 1000 + 0.5 AndAlso (Me.Image.Manifold.GetCurrentPose(agent2).X) / 1000 > (click_coord.X) / 1000 - 0.5 AndAlso (Me.Image.Manifold.GetCurrentPose(agent2).Y) / 1000 < (click_coord.Y) / 1000 + 0.5 AndAlso (Me.Image.Manifold.GetCurrentPose(agent2).Y) / 1000 > (click_coord.Y) / 1000 - 0.5) Then
                            checker = checker + 1
                        End If
                    Next

                    If (checker = 0) Then
                        If Not (Me.Image.Manifold._targetDataBase.ContainsKey(Me.Image.Manifold.currNumber)) Then
                            'agent._newTarget = goalPose
                            Me.Image.Manifold._targetDataBase.Add(Me.Image.Manifold.currNumber, goalPose)
                        Else
                            Me.Image.Manifold._targetDataBase(Me.Image.Manifold.currNumber) = goalPose
                        End If

                        Me.Image.Manifold._finished = False
                        Me.Image.Manifold._moving = False
                        Me.Image.Manifold._Astar = New Astar(agent._CurrentPoseEstimate, goalPose)
                    End If

                    If (Me.Image.Manifold.currNumber > 0 And checker = 0) Then
                        If (Me.Image.Manifold._changeMotion(Me.Image.Manifold.currNumber) = False) Then

                        Else
                            If Not (Me.Image.Manifold.startArea.ContainsKey(Me.Image.Manifold.currNumber)) Then
                                Me.Image.Manifold.startArea.Add(Me.Image.Manifold.currNumber, Me.Image.PixelToCoordinate(Me.ClickToImageCoord(e.Location)))
                            Else
                                Me.Image.Manifold.startArea(Me.Image.Manifold.currNumber) = Me.Image.PixelToCoordinate(Me.ClickToImageCoord(e.Location))
                            End If


                        End If



                    End If

                    End If

                    'Console.WriteLine(String.Format("goal: {0}, rotation : {1}", goalPose, goalPose.Rotation * 180 / PI))
                    ' Me.targetPicker.Items.Add(New AgentMenuItem(Agent, Me, False))
                    'Me.targetPicker.Items.Add(New AgentMenuItem(Agent, Me, True))
            Next
            'Me.targetPicker.Show(Me, e.Location)
        End If


    End Sub

    Protected Overrides Sub OnMouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.OnMouseMove(e)

        If Me.isPanning Then
            Me._Pan.X += (e.X - Me.lastPoint.X)
            Me._Pan.Y += (e.Y - Me.lastPoint.Y)
            Me.lastPoint = e.Location
            Me.Invalidate()
        End If

    End Sub
    Protected Overrides Sub OnMouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button = Windows.Forms.MouseButtons.Left Then
            Me.isPanning = False
            Windows.Forms.Cursor.Current = Cursors.Default
        End If

        If e.Button = Windows.Forms.MouseButtons.Right And Me.Image.Manifold._changeMotion(Me.Image.Manifold.currNumber) = True Then
            Console.WriteLine("fafaf fafa fafa!!!!!!!!!!!1")
            If Not (Me.Image.Manifold.endArea.ContainsKey(Me.Image.Manifold.currNumber)) Then
                Me.Image.Manifold.endArea.Add(Me.Image.Manifold.currNumber, Me.Image.PixelToCoordinate(Me.ClickToImageCoord(e.Location)))
            Else
                Me.Image.Manifold.endArea(Me.Image.Manifold.currNumber) = Me.Image.PixelToCoordinate(Me.ClickToImageCoord(e.Location))

            End If
        End If

        Console.WriteLine("mouse up : {0}", Me.Image.PixelToCoordinate(Me.ClickToImageCoord(e.Location)))

    End Sub


#End Region

#Region " Layer Flags "

    Private _ShowAprioriLayer As Boolean
    Public Property ShowAprioriLayer() As Boolean
        Get
            Return Me._ShowAprioriLayer
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._ShowAprioriLayer Then
                Me._ShowAprioriLayer = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Private _ShowSafeSpaceLayer As Boolean
    Public Property ShowSafeSpaceLayer() As Boolean
        Get
            Return Me._ShowSafeSpaceLayer
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._ShowSafeSpaceLayer Then
                Me._ShowSafeSpaceLayer = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Private _ShowFreeSpaceLayer As Boolean
    Public Property ShowFreeSpaceLayer() As Boolean
        Get
            Return Me._ShowFreeSpaceLayer
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._ShowFreeSpaceLayer Then
                Me._ShowFreeSpaceLayer = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Private _ShowClearSpaceLayer As Boolean
    Public Property ShowClearSpaceLayer() As Boolean
        Get
            Return Me._ShowClearSpaceLayer
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._ShowClearSpaceLayer Then
                Me._ShowClearSpaceLayer = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Private _ShowVictimsLayer As Boolean
    Public Property ShowVictimsLayer() As Boolean
        Get
            Return Me._ShowVictimsLayer
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._ShowVictimsLayer Then
                Me._ShowVictimsLayer = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Private _ShowFrontierLayer As Boolean
    Public Property ShowFrontierLayer() As Boolean
        Get
            Return Me._ShowFrontierLayer
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._ShowFrontierLayer Then
                Me._ShowFrontierLayer = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Private _ShowObstaclesLayer As Boolean
    Public Property ShowObstaclesLayer() As Boolean
        Get
            Return Me._ShowObstaclesLayer
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._ShowObstaclesLayer Then
                Me._ShowObstaclesLayer = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Private _ShowNotesLayer As Boolean
    Public Property ShowNotesLayer() As Boolean
        Get
            Return Me._ShowNotesLayer
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._ShowNotesLayer Then
                Me._ShowNotesLayer = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Private _ShowCommsLayer As Boolean
    Public Property ShowCommsLayer() As Boolean
        Get
            Return Me._ShowCommsLayer
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._ShowCommsLayer Then
                Me._ShowCommsLayer = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Private _ShowAgentsLayer As Boolean
    Public Property ShowAgentsLayer() As Boolean
        Get
            Return Me._ShowAgentsLayer
        End Get
        Set(ByVal value As Boolean)
            If Not value = Me._ShowAgentsLayer Then
                Me._ShowAgentsLayer = value
                Me.Invalidate()
            End If
        End Set
    End Property

#End Region

#Region " Paint Routine "
    Dim offset As Point
    Dim _scale As Single
    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
        MyBase.OnPaint(e)

        'renders the image of the manifold:
        '- scaled to fit the client area
        '- while preserving the aspect-ration (so equal scaling in both dimensions)
        '- and centered vertically nad horizontally

        If Not IsNothing(Me.Image) Then

            'compute the scaling of the image in both directions
            Dim xratio As Double = Me.Image.ImageWidth / Me.ClientSize.Width
            Dim yratio As Double = Me.Image.ImageHeight / Me.ClientSize.Height

            'compute the preferred scale to exactly fit the most 
            'restraining dimension
            If xratio > yratio Then
                'width is most restraining for current image bounds
                _scale = CType(1 / xratio, Single)
            Else
                'height is most restraining for current image bounds
                _scale = CType(1 / yratio, Single)
            End If

            If Not _AutoZoom Then
                'add the zooming factor that the user set with the scrollwheel
                _scale *= Me._Zoom
            End If

            'ensure limits
            _scale = Max(_scale, 0.01F)


            'then compute the preferred offset that centers the 
            'image on screen given the current scale
            offset = New Point
            offset.X = CInt((Me.ClientSize.Width - Me.Image.ImageWidth * _scale) / 2 / _scale)
            offset.Y = CInt((Me.ClientSize.Height - Me.Image.ImageHeight * _scale) / 2 / _scale)

            If Not Me._AutoPan Then
                'add panning
                offset.X += CInt(Me._Pan.X / _scale)
                offset.Y += CInt(Me._Pan.Y / _scale)
            End If

            Dim g As Graphics = e.Graphics
            g.PageUnit = GraphicsUnit.Pixel
            g.ScaleTransform(_scale, _scale)
            g.TranslateTransform(offset.X, offset.Y)

            Me.Image.Draw(g, ManifoldRenderingMode.GdiPlus, True, Me.ShowAprioriLayer, Me.ShowFreeSpaceLayer, Me.ShowSafeSpaceLayer, Me.ShowClearSpaceLayer, Me.ShowObstaclesLayer, Me.ShowVictimsLayer, Me.ShowFrontierLayer, Me.ShowNotesLayer, Me.ShowCommsLayer, Me.ShowAgentsLayer, True)
        End If

    End Sub

    Protected Overrides Sub OnResize(ByVal e As System.EventArgs)
        MyBase.OnResize(e)
        Me.Invalidate()
    End Sub

    Protected Function ClickToImageCoord(ByVal pixel As Point) As Math.Vector2
        Return New Math.Vector2( _
            pixel.X / Me._scale - Me.offset.X, _
            pixel.Y / Me._scale - Me.offset.Y _
        )

    End Function
    'this is exact thing that i wanted it
    Public Function getLastClickLocation() As Math.Pose2D
        Return New Math.Pose2D(Me.Image.PixelToCoordinate(Me.ClickToImageCoord(Me.lastPoint)), 0)

    End Function

#End Region

    Private Sub ManifoldView_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub
End Class

Public Class AgentMenuItem
    Inherits System.Windows.Forms.ToolStripMenuItem

    Dim Agent As Agent.Agent
    Dim ManifoldView As ManifoldView
    Dim clearAgentsCurrentTargets As Boolean

    Public Sub New(ByVal agent As Agent.Agent, ByVal mv As ManifoldView, ByVal clearAgentsCurrentTargets As Boolean)
        MyBase.New()
        Me.Agent = agent
        Me.ManifoldView = mv
        Me.clearAgentsCurrentTargets = clearAgentsCurrentTargets
        Dim l As Math.Pose2D = Me.ManifoldView.getLastClickLocation()
        Me.Name = agent.Name
        'Me.Agent.SendNewTarget(Me.Agent.Name, l)


        If clearAgentsCurrentTargets Then
            Me.Text = agent.Name + " [clear]"
        Else
            Me.Text = agent.Name
        End If
    End Sub

    Protected Overloads Sub onDoubleclick(ByVal e As System.Windows.Forms.MouseEventArgs)
        Dim l As Math.Pose2D = Me.ManifoldView.getLastClickLocation()
        'Me.Agent.SendNewTarget(Me.Agent.Name, l)
        'Me.Agent.AddNewTarget(l, Me.clearAgentsCurrentTargets)

        'Console.WriteLine(l)
    End Sub
End Class
