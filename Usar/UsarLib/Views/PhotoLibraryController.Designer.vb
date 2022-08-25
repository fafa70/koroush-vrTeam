<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class PhotoLibraryController
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.pnlPhotos = New System.Windows.Forms.FlowLayoutPanel()
        Me.SuspendLayout()
        '
        'pnlPhotos
        '
        Me.pnlPhotos.AutoScroll = True
        Me.pnlPhotos.BackColor = System.Drawing.Color.DarkRed
        Me.pnlPhotos.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlPhotos.ForeColor = System.Drawing.SystemColors.ActiveCaption
        Me.pnlPhotos.Location = New System.Drawing.Point(0, 0)
        Me.pnlPhotos.Name = "pnlPhotos"
        Me.pnlPhotos.Size = New System.Drawing.Size(400, 158)
        Me.pnlPhotos.TabIndex = 0
        Me.pnlPhotos.WrapContents = False
        '
        'PhotoLibraryController
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Black
        Me.Controls.Add(Me.pnlPhotos)
        Me.Name = "PhotoLibraryController"
        Me.Size = New System.Drawing.Size(400, 158)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents pnlPhotos As System.Windows.Forms.FlowLayoutPanel

End Class
