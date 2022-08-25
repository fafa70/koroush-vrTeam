Imports System.Drawing

Public Class BackgroundLayer
    Inherits ManifoldLayer

    Public Sub New(ByVal parent As ManifoldImage)
        MyBase.New(parent)
        Me._BackgroundColor = Color.Blue
    End Sub

    Private _BackgroundColor As Color
    Public Property BackgroundColor() As Color
        Get
            Return Me._BackgroundColor
        End Get
        Set(ByVal value As Color)
            Me._BackgroundColor = value
        End Set
    End Property

    Public Overrides Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode)
        MyBase.Draw(g, renderingMode)
        g.Clear(Me.BackgroundColor)
    End Sub

End Class
