Imports System.Windows.Forms
Imports System.Collections.Generic

Public Class AgentPatchController
    Inherits UserControl

    Friend WithEvents lblName As System.Windows.Forms.Label

    Private _SaveMapDialog As SaveMapDialog
    Friend WithEvents GraphBar As System.Windows.Forms.Panel

    Friend WithEvents LineGraph As Drawing.Graphics
    Private _Name As String
    Private _Patches As List(Of Agent.Patch)
    Private components As System.ComponentModel.IContainer
    Private _RenderRanges As List(Of SaveMapDialog.Range)
    Private _Markers As Markers

#Region " Constructor / Destructor "
    Public Sub New(ByVal savemapdialog As SaveMapDialog, ByVal name As String, ByVal patches As List(Of Agent.Patch))
        Me.InitializeComponent()

        Me._Patches = patches
        Me._Name = name
        Me._SaveMapDialog = savemapdialog
        Me._RenderRanges = New List(Of SaveMapDialog.Range)
        Me.lblName.Text = name
        Me._Markers = New Markers(Me.GraphBar)
        For i As Integer = 1 To 10
            Dim MenuItem As New ToolStripMenuItem(i.ToString())
            AddHandler MenuItem.Click, AddressOf SetMarker
            ' Me.ContextMenuStrip1.Items.Add(MenuItem)
        Next i
    End Sub

    Private Sub InitializeComponent()
        Me.lblName = New System.Windows.Forms.Label()
        Me.GraphBar = New System.Windows.Forms.Panel()
        Me.SuspendLayout()
        '
        'lblName
        '
        Me.lblName.AutoSize = True
        Me.lblName.Location = New System.Drawing.Point(3, 28)
        Me.lblName.Name = "lblName"
        Me.lblName.Size = New System.Drawing.Size(66, 13)
        Me.lblName.TabIndex = 2
        Me.lblName.Text = "Agent Name"
        '
        'GraphBar
        '
        Me.GraphBar.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GraphBar.AutoSize = True
        Me.GraphBar.BackColor = System.Drawing.Color.White
        Me.GraphBar.Location = New System.Drawing.Point(75, 3)
        Me.GraphBar.Name = "GraphBar"
        Me.GraphBar.Size = New System.Drawing.Size(522, 65)
        Me.GraphBar.TabIndex = 29
        '
        'AgentPatchController
        '
        Me.AutoSize = True
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Controls.Add(Me.GraphBar)
        Me.Controls.Add(Me.lblName)
        Me.Name = "AgentPatchController"
        Me.Size = New System.Drawing.Size(600, 103)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region " Draw spark line "
    Private covariances As List(Of Double)
    Private max_cov As Double
    Private Sub GraphBar_Paint(ByVal sender As System.Object, _
                             ByVal e As System.Windows.Forms.PaintEventArgs) _
    Handles GraphBar.Paint
        ' Compute covariances if necessary
        If covariances Is Nothing Then
            covariances = New List(Of Double)
            For Each patch As Agent.Patch In Me._Patches
                Dim cov As Double = patch.AvgCovarianceDeterminant
                covariances.Add(cov)
                If cov > max_cov Then
                    max_cov = cov
                End If
            Next
        End If

        ' Draw rectangles on the patches that have been selected
        For Each r As SaveMapDialog.Range In Me._SaveMapDialog.GetPatchesToBeRendered(Me._Name)
            Dim x, y, width, height As Integer
            x = Me.PatchNumberToCoordinate(r.first - 0.5)
            y = 1
            width = Me.PatchNumberToCoordinate(r.last + 0.5) - x
            height = Me.GraphBar.Height - y

            e.Graphics.FillRectangle(Drawing.Brushes.Aquamarine, x, y, width, height)
        Next

        If max_cov > 0.0 Then
            ' Draw the covariance line
            Dim points As List(Of Drawing.Point) = New List(Of Drawing.Point)
            For i As Integer = 0 To covariances.Count - 1
                Dim x, y As Integer
                x = CInt((i / covariances.Count) * GraphBar.Width)
                y = CInt((covariances(i) / max_cov) * GraphBar.Height)
                points.Add(New Drawing.Point(x, y))
            Next
            e.Graphics.DrawLines(Drawing.Pens.Black, points.ToArray())
        Else
            Console.WriteLine("[GraphBar_Paint] max_cov is zero")
        End If

    End Sub
#End Region

#Region " Mouse movements "
    ' DragStart is nothing means no drag is happening. Otherwise, is the local x pos of the 
    ' mouse at the time the drag started.
    Dim DragStart As Nullable(Of Integer) = Nothing
    Dim RightClickStart As Integer
    Private Sub GraphBar_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles GraphBar.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Right Then
            Me.GraphBar.Invalidate()
        Else ' Left mouse button
            Me.DragStart = e.Location.X
        End If
    End Sub

    Private Sub RemovePatch(ByVal sender As System.Object, ByVal e As System.EventArgs)
        ' RightClickStart has been set by GraphBar_MouseDown, before the context menu was shown on which this button lies.
        Me._SaveMapDialog.DelPatchesToBeRendered(Me._Name, Me.RightClickStart)
        Me.GraphBar.Invalidate()
    End Sub

    Private Sub SetMarker(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim MenuItem As ToolStripMenuItem
        Dim MarkerNo As Integer
        Try
            MenuItem = DirectCast(sender, ToolStripMenuItem)
            MarkerNo = Convert.ToInt32(MenuItem.Text)
            Me._Markers.Add(PatchNumberToCoordinate(RightClickStart), MarkerNo, RightClickStart)
        Catch ex As Exception
            Return ' The toolstripmenuitem that calls this function should always perform this cast right.
        End Try

        ' RightClickStart has been set by GraphBar_MouseDown, before the context menu was shown on which the clicked button lies.
        Dim markerLabel As Label = New Label()
        markerLabel.Text = MarkerNo.ToString()
        markerLabel.Location = New System.Drawing.Point(Me.RightClickStart, Me.Location.Y)

    End Sub

    ' Drag movement stopped: add the patches to the system.
    ' The patches are only added if the mouse moved from left to right and did not go out of the panel.
    Private Sub GraphBar_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles GraphBar.MouseUp
        If e.Button = Windows.Forms.MouseButtons.Right Then Return
        If Not Me.DragStart.HasValue Then Return

        Dim FirstPatch, LastPatch As Integer
        FirstPatch = CoordinateToPatchNumber(CInt(Me.DragStart))
        LastPatch = CoordinateToPatchNumber(e.Location.X)
        ' Swap patches if they're in the wrong order
        If LastPatch < FirstPatch Then
            Dim swap As Integer = FirstPatch
            FirstPatch = LastPatch
            LastPatch = swap
        End If

        ' Ensure invariants. Can't this be done in vb in a more civilzed wa
        If FirstPatch < 0 Then
            FirstPatch = 0
        End If
        If LastPatch < 0 Then
            LastPatch = 0
        End If
        If FirstPatch > Me._Patches.Count - 1 Then
            FirstPatch = Me._Patches.Count - 1
        End If
        If LastPatch > Me._Patches.Count - 1 Then
            LastPatch = Me._Patches.Count - 1
        End If

        Me._SaveMapDialog.AddToPatchesToBeRendered(Me._Name, FirstPatch, LastPatch)
        Me.DragStart = Nothing
        Me.GraphBar.Refresh()
    End Sub

    ' Calculate the corresponding patch number of an x coordinate on the bar
    Private Function CoordinateToPatchNumber(ByVal X As Integer) As Integer
        Return CInt(X * (Me._Patches.Count / Me.GraphBar.Width))
    End Function

    ' Calculate the corresponding x coordinate on the bar for a patch number
    ' The patch numbers can be doubles (for the routine which draws the selection
    Private Function PatchNumberToCoordinate(ByVal patch As Double) As Integer
        Dim result As Integer = CInt(patch * (Me.GraphBar.Width / Me._Patches.Count))
        If result < 0 Then
            result = 0
        End If
        If result > GraphBar.Width Then
            result = GraphBar.Width
        End If
        Return result
    End Function
#End Region

    Private Class Markers
        Private Class Marker
            Inherits System.Windows.Forms.Label
            Public Id, PatchNo As Integer

            Public Sub New(ByVal p As Control, ByVal x As Integer, ByVal id As Integer, ByVal PatchNo As Integer)
                Me.Parent = p
                Me.Text = id.ToString()
                Me.Id = id
                Me.PatchNo = PatchNo
                Me.Left = x
                Me.Width = 10
                Me.BackColor = Drawing.Color.Bisque
            End Sub
        End Class

        Private _Markers As Dictionary(Of Integer, Marker)
        Private Parent As Control

        Public Sub New(ByVal p As Control)
            Me.Parent = p
            Me._Markers = New Dictionary(Of Integer, Marker)
        End Sub

        Public Sub Add(ByVal X As Integer, ByVal Id As Integer, ByVal PatchNo As Integer)
            Dim m As Marker = New Marker(Me.Parent, X, Id, PatchNo)
            AddHandler m.Click, AddressOf Remove
            _Markers.Add(PatchNo, m)
            Parent.Invalidate()
        End Sub

        Public Sub Remove(ByVal sender As Object, ByVal e As System.EventArgs)
            Dim m As Marker = DirectCast(sender, Marker)
            RemoveHandler m.Click, AddressOf Remove
            _Markers.Remove(m.PatchNo)
            m.Dispose()
            Parent.Invalidate()
        End Sub
    End Class

End Class