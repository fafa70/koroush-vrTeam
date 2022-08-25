<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Me.stbStatus = New System.Windows.Forms.StatusStrip()
        Me.pnlController = New System.Windows.Forms.Panel()
        Me.SuspendLayout()
        '
        'stbStatus
        '
        Me.stbStatus.Location = New System.Drawing.Point(0, 321)
        Me.stbStatus.Name = "stbStatus"
        Me.stbStatus.Size = New System.Drawing.Size(784, 22)
        Me.stbStatus.TabIndex = 1
        Me.stbStatus.Text = "StatusStrip1"
        '
        'pnlController
        '
        Me.pnlController.Dock = System.Windows.Forms.DockStyle.Right
        Me.pnlController.ForeColor = System.Drawing.SystemColors.MenuHighlight
        Me.pnlController.Location = New System.Drawing.Point(0, 0)
        Me.pnlController.Name = "pnlController"
        Me.pnlController.Size = New System.Drawing.Size(784, 321)
        Me.pnlController.TabIndex = 10
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(784, 343)
        Me.Controls.Add(Me.pnlController)
        Me.Controls.Add(Me.stbStatus)
        Me.Name = "MainForm"
        Me.Text = "IUST base ,farbod seidali"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents stbStatus As System.Windows.Forms.StatusStrip
    Friend WithEvents pnlController As System.Windows.Forms.Panel

End Class
