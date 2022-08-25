Imports System.Drawing
Imports System.IO
Imports System.Math
Imports System.Windows.Forms
Imports System.Text
Imports UvARescue.Agent
Imports UvARescue.Slam
Imports UvARescue.Tools
Imports UvARescue.Math
Imports UvARescue.ImageAnalysis


Public Class cameraBox
    Inherits Windows.Forms.Form


    Private camView As New List(Of Panel)
    Private counter As Integer = 0

    Private _agentNumber As Integer



    Sub New()
        '_agentNumber = agentNumber
        'camView.BackgroundImage = image
        'Me.Controls.Add(camView)
        Me.ClientSize = New System.Drawing.Size(1400, 150)
        Me.Show()

    End Sub


    Private _Agent As Agent.Agent
    Public ReadOnly Property Agent() As Agent.Agent
        Get
            Return Me._Agent
        End Get
    End Property

    Public Sub setImage(ByVal image As Image, ByVal agentnumber As Integer)
        Dim source As Bitmap = CType(image, Bitmap)
        Dim smaller As New Bitmap(150, 150)
        Dim g As Graphics = Graphics.FromImage(smaller)
        g.DrawImage(source, 0, 0, smaller.Width + 1, smaller.Height + 1)
        camView(agentnumber - 1).BackgroundImage = smaller

        ' Console.WriteLine("number of agent : {0}", Me._agentNumber)

    End Sub

  

    Public Sub drawCam(ByVal agentnumber As Integer)

        Me._agentNumber = agentnumber
        camView.Add(New Panel With {.Size = New System.Drawing.Size(150, 150), .Left = (Me._agentNumber - 1) * 150, .BackgroundImage = Image.FromFile("cyrus-the-great-ancient.ir_.jpg")})

        For Each member As Panel In camView
            Me.Controls.Add(member)
            Me.Controls.SetChildIndex(member, 0)
        Next
        'camView(agentnumber - 1).Scale(New System.Drawing.SizeF(0.8, 0.8))
        '        With camView
        'Size = New System.Drawing.Size((_agentNumber - 1) * 320 + 320, 240)
        'End With
    End Sub

    


    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'cameraBox
        '

        Me.Name = "cameraBox"
        Me.ResumeLayout(False)

    End Sub
End Class
