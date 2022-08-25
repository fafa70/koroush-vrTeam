Imports UvARescue.Math

Public Class LineStringData
    Implements IGraphicalObjectData

    Public Sub New()
        Me._vector = Nothing
    End Sub

    Public Sub New(ByVal length As Integer)
        ReDim Me._vector(2 * length - 1)
    End Sub

    Private _vector() As Double

    'Public Property Point(ByVal index As Integer) As Vector2
    '    Get
    '        Return _vector(index)
    '    End Get
    '    Set(ByVal value As Vector2)
    '        _vector(index) = value
    '    End Set
    'End Property

    Public ReadOnly Property PointCount() As Integer
        Get
            Return CInt(_vector.Length / 2) 'even
        End Get
    End Property

    Public Property X(ByVal index As Integer) As Double
        Get
            If IsNothing(_vector) OrElse (2 * index + 0) > (_vector.Length - 1) Then
                Return 0.0
            End If
            Return _vector(2 * index + 0) 'even
        End Get
        Set(ByVal value As Double)
            If IsNothing(_vector) OrElse (2 * index + 0) > (_vector.Length - 1) Then
                ReDim _vector(2 * index + 0)
            End If
            _vector(2 * index + 0) = value
        End Set
    End Property

    Public Property Y(ByVal index As Integer) As Double
        Get
            If IsNothing(_vector) OrElse (2 * index + 1) > (_vector.Length - 1) Then
                Return 0.0
            End If
            Return _vector(2 * index + 1) 'uneven
        End Get
        Set(ByVal value As Double)
            If IsNothing(_vector) OrElse (2 * index + 1) > (_vector.Length - 1) Then
                ReDim _vector(2 * index + 1)
            End If
            _vector(2 * index + 1) = value
        End Set
    End Property
End Class
