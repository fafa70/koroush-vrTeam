Imports UvARescue.Math

<Serializable()> _
Public Class FrontierInfo

    Private _Area As Double
    Private _Center As Vector2
    Private _Frontier As Vector2

    Public Sub New(ByVal area As Double, ByVal x As Double, ByVal y As Double)
        Me._Area = area
        Me._Center = New Vector2(x, y)
        Me._Frontier = New Vector2
    End Sub

    Public ReadOnly Property Area() As Double
        Get
            Return Me._Area
        End Get
    End Property

    Public ReadOnly Property Center() As Vector2
        Get
            Return Me._Center
        End Get
    End Property

    Public Property Frontier() As Vector2
        Get
            Return Me._Frontier
        End Get
        Set(ByVal value As Vector2)
            Me._Frontier.X = value.X
            Me._Frontier.Y = value.Y
        End Set
    End Property

End Class
