<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class PhotoView
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
        Me.imgPhoto = New System.Windows.Forms.PictureBox()
        CType(Me.imgPhoto, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'imgPhoto
        '
        Me.imgPhoto.Dock = System.Windows.Forms.DockStyle.Fill
        Me.imgPhoto.Location = New System.Drawing.Point(0, 0)
        Me.imgPhoto.Name = "imgPhoto"
        Me.imgPhoto.Size = New System.Drawing.Size(150, 150)
        Me.imgPhoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.imgPhoto.TabIndex = 0
        Me.imgPhoto.TabStop = False
        '
        'PhotoView
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.imgPhoto)
        Me.Name = "PhotoView"
        CType(Me.imgPhoto, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents imgPhoto As System.Windows.Forms.PictureBox

End Class
