<Serializable()> _
Public Class Matrix2
    Inherits Matrix

#Region " Basics "

    'copy constructor
    Public Sub New(ByVal source As Matrix)
        MyBase.New(source)
    End Sub

    Public Sub New()
        MyBase.New(2)
    End Sub

    Public Sub New(ByVal v00 As Double, ByVal v01 As Double, ByVal v10 As Double, ByVal v11 As Double)
        MyBase.New(2)
        Me(0, 0) = v00
        Me(0, 1) = v01
        Me(1, 0) = v10
        Me(1, 1) = v11
    End Sub

    Public Overloads Shared Function Identity() As Matrix2
        Dim matrix As New Matrix2
        matrix.LoadIdentity()
        Return matrix
    End Function

    Public Overloads Shared Function Zeros() As Matrix2
        Dim matrix As New Matrix2
        matrix.LoadZeros()
        Return matrix
    End Function

    

#End Region


#Region "Functions"
    Public Overloads Function Transpose() As Matrix2
        Return New Matrix2(MyBase.Transpose)
    End Function

    Public Overrides Function Determinant() As Double
        Return Me(0, 0) * Me(1, 1) - Me(0, 1) * Me(1, 0)
    End Function

    Public Overloads Shared Function Determinant(ByVal matrix As Matrix2) As Double
        Return matrix.Determinant
    End Function

    Public Overloads Function Invert() As Matrix2
        Dim answer As New Matrix2
        Dim det As Double = Me.Determinant
        answer(0, 0) = 1 / det * Me(1, 1)
        answer(0, 1) = 1 / det * -Me(0, 1)
        answer(1, 0) = 1 / det * -Me(1, 0)
        answer(1, 1) = 1 / det * Me(0, 0)
        Return answer
    End Function

#End Region

#Region " Operators "

    Public Overloads Shared Operator -(ByVal right As Matrix2) As Matrix2
        Return New Matrix2(right.Negative)
    End Operator

    Public Overloads Shared Operator +(ByVal left As Matrix2, ByVal right As Matrix2) As Matrix2
        Return New Matrix2(left.Add(right))
    End Operator
    Public Overloads Shared Operator -(ByVal left As Matrix2, ByVal right As Matrix2) As Matrix2
        Return New Matrix2(left.Subtract(right))
    End Operator

    Public Overloads Shared Operator *(ByVal factor As Double, ByVal matrix As Matrix2) As Matrix2
        Return New Matrix2(matrix.Multiply(factor))
    End Operator
    Public Overloads Shared Operator *(ByVal matrix As Matrix2, ByVal factor As Double) As Matrix2
        Return New Matrix2(matrix.Multiply(factor))
    End Operator
    Public Overloads Shared Operator *(ByVal left As Matrix2, ByVal right As Matrix2) As Matrix2
        Return New Matrix2(left.Multiply(right))
    End Operator
    Public Overloads Shared Operator *(ByVal matrix As Matrix2, ByVal vector As Vector2) As Vector2
        Return New Vector2(matrix.Multiply(vector))
    End Operator

    Public Overloads Shared Operator /(ByVal factor As Double, ByVal matrix As Matrix2) As Matrix2
        Return New Matrix2(matrix.Multiply(1 / factor))
    End Operator
    Public Overloads Shared Operator /(ByVal matrix As Matrix2, ByVal factor As Double) As Matrix2
        Return New Matrix2(matrix.Multiply(1 / factor))
    End Operator
    Public Overloads Shared Operator /(ByVal left As Matrix2, ByVal right As Matrix2) As Matrix2
        Return New Matrix2(left.Multiply(right.Invert))
    End Operator
    Public Overloads Shared Operator /(ByVal matrix As Matrix2, ByVal vector As Vector2) As Vector2
        Return New Vector2((matrix.Transpose * matrix).Invert * matrix.Transpose * vector) 'Matlab v5 manual 4-23
    End Operator
    Public Overloads Shared Operator =(ByVal left As Matrix2, ByVal right As Matrix2) As Boolean
        Return left.Equals(right)
    End Operator

    Public Overloads Shared Operator <>(ByVal left As Matrix2, ByVal right As Matrix2) As Boolean
        Return Not left.Equals(right)
    End Operator

#End Region

#Region "Unit Testing of Matrix Operation"

    Public Overloads Shared Sub ClassTest()

        Dim J As New Matrix2(0, -1, 1, 0)
        Dim Jt As Matrix2 = J.Transpose
        Dim left_side, right_side As New Matrix2

        left_side = Jt * J
        right_side = Matrix2.Identity()


        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 5.3.8
            Console.Error.WriteLine("[Matrix2] The product of orthogonal matrix and its transpose should be I")
        End If

        Dim A As New Matrix2(1, 2, 3, 4)
        Dim B As New Matrix2(5, 6, 7, 8)

        left_side = (A * B).Transpose()
        right_side = B.Transpose() * A.Transpose()

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 5.3.9
            Console.Error.WriteLine("[Matrix2] The transpose should have the property (AB)^T = B^T A^T")
        End If

        left_side = A.Transpose().Invert()
        right_side = A.Invert().Transpose()

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 5.3.9
            Console.Error.WriteLine("[Matrix2] If A is invertable, the property (A^T)^-1 = (A^-1)^T should hold")
        End If

        Dim T As New Matrix2(1, 2, 0, 3)

        If Not Determinant(T) = 3 Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 6.1.6
            Console.Error.WriteLine("[Matrix2] The Determinant of a triangular matrix should be product of its diagonal")
        End If

        If Not Trace(T) = 4 Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, P. 234
            Console.Error.WriteLine("[Matrix2] The trace of a triangular matrix should be sum of its diagonal")
        End If

        left_side = A + B - A
        right_side = B

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Def 1.3.5
            Console.Error.WriteLine("[Matrix2] The fact 'A + B - A = B' should hold ")
        End If

        left_side = A + Matrix2.Zeros()
        right_side = A

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Def 1.3.5
            Console.Error.WriteLine("[Matrix2] The fact 'A + 0 = A' should hold ")
        End If

        left_side = B * Matrix2.Identity()
        right_side = B

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 2.4.6
            Console.Error.WriteLine("[Matrix2] The fact 'B * I = B' should hold ")
        End If

        Dim v As Vector2 = New Vector2(1, 2)
        Dim div As Vector2 = B / v
        Dim left_side_vector, right_side_vector As Vector2

        left_side_vector = B * div
        right_side_vector = v

        If Not Vector2.Equals(left_side_vector, right_side_vector) Then
            'Matlab v5 manual 4-13
            Console.Error.WriteLine("[Matrix2] The fact B * (B/v) = v should hold ")
        End If




    End Sub

#End Region

End Class


