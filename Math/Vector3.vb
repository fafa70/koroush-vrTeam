<Serializable()> _
Public Class Vector3
    Inherits Vector

#Region " Basics "

    'copy constructor
    Public Sub New(ByVal source As Vector)
        MyBase.New(source)
    End Sub

    Public Sub New()
        MyBase.New(3)
    End Sub

    Public Sub New(ByVal x As Double, ByVal y As Double, ByVal z As Double)
        Me.New()
        Me(0) = x
        Me(1) = y
        Me(2) = z
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

    Public Property Z() As Double
        Get
            Return Me(2)
        End Get
        Set(ByVal value As Double)
            Me(2) = value
        End Set
    End Property

#End Region

#Region " Operators "

    Public Overloads Shared Operator -(ByVal right As Vector3) As Vector3
        Return New Vector3(right.Negative)
    End Operator

    Public Overloads Shared Operator +(ByVal left As Vector3, ByVal right As Vector3) As Vector3
        Return New Vector3(left.Add(right))
    End Operator
    Public Overloads Shared Operator -(ByVal left As Vector3, ByVal right As Vector3) As Vector3
        Return New Vector3(left.Subtract(right))
    End Operator

    Public Overloads Shared Operator *(ByVal factor As Double, ByVal vector As Vector3) As Vector3
        Return New Vector3(vector.Multiply(factor))
    End Operator
    Public Overloads Shared Operator *(ByVal vector As Vector3, ByVal factor As Double) As Vector3
        Return New Vector3(vector.Multiply(factor))
    End Operator
    Public Overloads Shared Operator *(ByVal left As Vector3, ByVal right As Vector3) As Double
        Return left.InnerProduct(right)
    End Operator
    Public Overloads Shared Operator *(ByVal left As Vector3, ByVal right As Matrix3) As Vector3
        Return New Vector3(left.Multiply(right))
    End Operator

    Public Overloads Shared Operator =(ByVal left As Vector3, ByVal right As Vector3) As Boolean
        Return left.Equals(right)
    End Operator

    Public Overloads Shared Operator <>(ByVal left As Vector3, ByVal right As Vector3) As Boolean
        Return Not left.Equals(right)
    End Operator

#End Region

    Public Overloads Function OuterProduct(ByVal right As Vector3) As Matrix3
        Dim result As Matrix3 = New Matrix3
        For i As Integer = 0 To right.Length - 1
            For j As Integer = 0 To Me.Length - 1
                result(i, j) = Me(i) * right(j)
            Next
        Next

        Return result
    End Function

End Class
