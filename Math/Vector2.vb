<Serializable()> _
Public Class Vector2
    Inherits Vector

#Region " Basics "

    'copy constructor
    Public Sub New(ByVal source As Vector)
        MyBase.New(source)
    End Sub

    Public Sub New()
        MyBase.New(2)
    End Sub

    Public Sub New(ByVal x As Double, ByVal y As Double)
        Me.New()
        Me(0) = x
        Me(1) = y
    End Sub

    Public Sub New(ByVal startLocation As String)
        Me.New()

        Dim parts() As String = Strings.Split(startLocation, ",")

        If parts.Length > 2 Then
            Me(0) = Single.Parse(parts(0))
            Me(1) = Single.Parse(parts(1))
        End If
    End Sub


    Public Property X() As Double
        Get
            Return Me(0)
        End Get
        Set(ByVal value As Double)
            Me(0) = value
        End Set
    End Property

    Public Property Y() As Double
        Get
            Return Me(1)
        End Get
        Set(ByVal value As Double)
            Me(1) = value
        End Set
    End Property

#End Region

#Region " Operators "

    Public Overloads Shared Operator -(ByVal right As Vector2) As Vector2
        Return New Vector2(right.Negative)
    End Operator

    Public Overloads Shared Operator +(ByVal left As Vector2, ByVal right As Vector2) As Vector2
        Return New Vector2(left.Add(right))
    End Operator
    Public Overloads Shared Operator -(ByVal left As Vector2, ByVal right As Vector2) As Vector2
        Return New Vector2(left.Subtract(right))
    End Operator

    Public Overloads Shared Operator *(ByVal factor As Double, ByVal vector As Vector2) As Vector2
        Return New Vector2(vector.Multiply(factor))
    End Operator
    Public Overloads Shared Operator *(ByVal vector As Vector2, ByVal factor As Double) As Vector2
        Return New Vector2(vector.Multiply(factor))
    End Operator
    Public Overloads Shared Operator *(ByVal left As Vector2, ByVal right As Vector2) As Double
        Return left.InnerProduct(right)
    End Operator
    Public Overloads Shared Operator *(ByVal left As Vector2, ByVal right As Matrix2) As Vector2
        Return New Vector2(left.Multiply(right))
    End Operator

    Public Overloads Shared Operator /(ByVal vector As Vector2, ByVal divisor As Double) As Vector2
        Return New Vector2(vector.Divide(divisor))
    End Operator

#End Region

    Public Overloads Function OuterProduct(ByVal right As Vector2) As Matrix2
        Dim result As Matrix2 = New Matrix2
        For i As Integer = 0 To right.Length - 1
            For j As Integer = 0 To Me.Length - 1
                result(i, j) = Me(i) * right(j)
            Next
        Next

        Return result
    End Function

End Class
