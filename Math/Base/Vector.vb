Imports System.Math

<Serializable()> _
Public Class Vector



#Region " Constructors / Factory Methods "

    'copy constructor
    Public Sub New(ByVal source As Vector)
        Me.New(source.Length)
        For i As Integer = 0 To Me.Length - 1
            Me(i) = source(i)
        Next
    End Sub

    Public Sub New(ByVal length As Integer)
        Me._length = length
        ReDim Me._v(length - 1)
    End Sub

    Public Overloads Shared Function Zeros(ByVal length As Integer) As Vector
        Dim vector As New Vector(length)
        For i As Integer = 0 To length - 1
            vector(i) = 0
        Next
        Return vector
    End Function

    Public Overloads Shared Function Ones(ByVal length As Integer) As Vector
        Dim vector As New Vector(length)
        For i As Integer = 0 To length - 1
            vector(i) = 1
        Next
        Return vector
    End Function

#End Region

#Region " Data "

    Private _v As Double()
    Default Public Property Item(ByVal idx As Integer) As Double
        Get
            Return Me._v(idx)
        End Get
        Set(ByVal value As Double)
            Me._v(idx) = value
        End Set
    End Property

    Private _length As Integer
    Public ReadOnly Property Length() As Integer
        Get
            Return Me._length
        End Get
    End Property

    Public Overrides Function ToString() As String

        Dim builder As New Text.StringBuilder
        builder.AppendFormat("Vector of size {0}", Me.Length).AppendLine()
        builder.Append(" [ ")
        For i As Integer = 0 To Me.Length - 1
            builder.Append(Me(i) & " ")
        Next
        builder.Append("]").AppendLine()

        Return builder.ToString()

    End Function

#End Region

#Region " Operators "

    Public Overloads Function Negative() As Vector
        Dim result As New Vector(Me.Length)
        For i As Integer = 0 To Length - 1
            result(i) = -Me(i)
        Next
        Return result
    End Function
    Public Overloads Shared Function Negative(ByVal vector As Vector) As Vector
        Return vector.Negative
    End Function

    Public Overloads Function Add(ByVal right As Vector) As Vector
        If right.Length <> Me.Length Then _
            Throw New InvalidOperationException("Vectors must be of equal length")
        Dim result As New Vector(Me.Length)
        For i As Integer = 0 To Length - 1
            result(i) = Me(i) + right(i)
        Next
        Return result
    End Function
    Public Overloads Shared Function Add(ByVal left As Vector, ByVal right As Vector) As Vector
        Return left.Add(right)
    End Function

    Public Overloads Function Subtract(ByVal right As Vector) As Vector
        If right.Length <> Me.Length Then _
            Throw New InvalidOperationException("Vectors must be of equal length")
        Dim result As New Vector(Me.Length)
        For i As Integer = 0 To Length - 1
            result(i) = Me(i) - right(i)
        Next
        Return result
    End Function
    Public Overloads Shared Function Subtract(ByVal left As Vector, ByVal right As Vector) As Vector
        Return left.Subtract(right)
    End Function

    Public Overloads Function Multiply(ByVal factor As Double) As Vector
        Dim result As New Vector(Me.Length)
        For i As Integer = 0 To Length - 1
            result(i) = factor * Me(i)
        Next
        Return result
    End Function
    Public Overloads Shared Function Multiply(ByVal factor As Double, ByVal vector As Vector) As Vector
        Return vector.Multiply(factor)
    End Function

    Public Overloads Function InnerProduct(ByVal right As Vector) As Double
        If right.Length <> Me.Length Then _
            Throw New InvalidOperationException("Vectors must be of equal length")
        Dim result As Double = 0
        For i As Integer = 0 To Length - 1
            result += Me(i) * right(i)
        Next
        Return result
    End Function
    Public Overloads Shared Function Multiply(ByVal left As Vector, ByVal right As Vector) As Double
        Return left.InnerProduct(right)
    End Function

    Public Overloads Function OuterProduct(ByVal right As Vector) As Matrix
        If right.Length <> Me.Length Then _
            Throw New InvalidOperationException("Vectors must be of equal length")
        Dim result As Matrix = New Matrix(right.Length, Me.Length)
        For i As Integer = 0 To right.Length - 1
            For j As Integer = 0 To Me.Length - 1
                result(i, j) = Me(i) * right(j)
            Next
        Next

        Return result
    End Function

    Public Overloads Function Multiply(ByVal matrix As Matrix) As Vector
        If Me.Length <> matrix.Rows Then _
            Throw New InvalidOperationException("Dimensions must agree")
        Dim result As New Vector(matrix.Cols)
        Dim sum As Double
        For col As Integer = 0 To matrix.Cols - 1
            sum = 0
            For row As Integer = 0 To matrix.Rows - 1
                sum += Me(row) * matrix(row, col)
            Next
            result(col) = sum
        Next
        Return result
    End Function
    Public Overloads Shared Function Multiply(ByVal left As Vector, ByVal right As Matrix) As Vector
        Return left.Multiply(right)
    End Function


    Public Overloads Function Divide(ByVal divisor As Double) As Vector
        Dim result As New Vector(Me.Length)
        For i As Integer = 0 To Length - 1
            result(i) = Me(i) / divisor
        Next
        Return result
    End Function
    Public Overloads Shared Function Divide(ByVal divisor As Double, ByVal vector As Vector) As Vector
        Return vector.Divide(divisor)
    End Function


    Public Function EuclidianLength() As Double
        Return Me.InnerProduct(Me) ^ 0.5
    End Function



    Public Shared Operator -(ByVal right As Vector) As Vector
        Return right.Negative
    End Operator

    Public Shared Operator +(ByVal left As Vector, ByVal right As Vector) As Vector
        Return left.Add(right)
    End Operator
    Public Shared Operator -(ByVal left As Vector, ByVal right As Vector) As Vector
        Return left.Subtract(right)
    End Operator

    Public Overloads Shared Operator *(ByVal factor As Double, ByVal vector As Vector) As Vector
        Return vector.Multiply(factor)
    End Operator
    Public Overloads Shared Operator *(ByVal vector As Vector, ByVal factor As Double) As Vector
        Return vector.Multiply(factor)
    End Operator
    Public Overloads Shared Operator *(ByVal left As Vector, ByVal right As Vector) As Double
        Return left.InnerProduct(right)
    End Operator
    Public Overloads Shared Operator *(ByVal left As Vector, ByVal right As Matrix) As Vector
        Return left.Multiply(right)
    End Operator


    Public Overloads Shared Operator /(ByVal vector As Vector, ByVal divisor As Double) As Vector
        Return vector.Divide(divisor)
    End Operator

#End Region

#Region " Equals "

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        If TypeOf obj Is Vector Then
            Dim vector As Vector = DirectCast(obj, Vector)
            If vector.Length <> Me.Length Then Return False
            For i As Integer = 0 To Me.Length - 1
                If Abs(Me(i) - vector(i)) > 0.000000000001 Then Return False 'Precision 1E-12
            Next
            Return True
        Else
            Return MyBase.Equals(obj)
        End If
    End Function

#End Region

End Class
