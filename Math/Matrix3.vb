<Serializable()> _
Public Class Matrix3
    Inherits Matrix

#Region " Basics "

    'copy constructor
    Public Sub New(ByVal source As Matrix)
        MyBase.New(source)
    End Sub

    Public Sub New()
        MyBase.New(3)
    End Sub

    Public Sub New(ByVal v00 As Double, ByVal v01 As Double, ByVal v02 As Double, ByVal v10 As Double, ByVal v11 As Double, ByVal v12 As Double, ByVal v20 As Double, ByVal v21 As Double, ByVal v22 As Double)
        MyBase.New(3)
        Me(0, 0) = v00
        Me(0, 1) = v01
        Me(0, 2) = v02
        Me(1, 0) = v10
        Me(1, 1) = v11
        Me(1, 2) = v12
        Me(2, 0) = v20
        Me(2, 1) = v21
        Me(2, 2) = v22
    End Sub

    Public Overloads Shared Function Identity() As Matrix3
        Dim matrix As New Matrix3
        matrix.LoadIdentity()
        Return matrix
    End Function

    Public Overloads Shared Function Zeros() As Matrix3
        Dim matrix As New Matrix3
        matrix.LoadZeros()
        Return matrix
    End Function
#End Region

#Region "Functions"

    Public Overloads Function Transpose() As Matrix3
        Return New Matrix3(MyBase.Transpose)
    End Function

    Public Overrides Function Determinant() As Double
        Return Me(0, 0) * (Me(2, 2) * Me(1, 1) - Me(2, 1) * Me(1, 2)) - Me(1, 0) * (Me(2, 2) * Me(0, 1) - Me(2, 1) * Me(0, 2)) + Me(2, 0) * (Me(1, 2) * Me(0, 1) - Me(1, 1) * Me(0, 2))
    End Function

    Public Overloads Shared Function Determinant(ByVal matrix As Matrix3) As Double
        Return matrix.Determinant
    End Function

    Public Overloads Function Invert() As Matrix3
        Dim answer As New Matrix3
        Dim det As Double = Me.Determinant
        answer(0, 0) = (1.0 / det) * (Me(2, 2) * Me(1, 1) - Me(2, 1) * Me(1, 2))
        answer(0, 1) = (1.0 / det) * (Me(2, 1) * Me(0, 2) - Me(2, 2) * Me(0, 1))
        answer(0, 2) = (1.0 / det) * (Me(1, 2) * Me(0, 1) - Me(1, 1) * Me(0, 2))

        answer(1, 0) = (1.0 / det) * (Me(2, 0) * Me(1, 2) - Me(2, 2) * Me(1, 0))
        answer(1, 1) = (1.0 / det) * (Me(2, 2) * Me(0, 0) - Me(2, 0) * Me(0, 2))
        answer(1, 2) = (1.0 / det) * (Me(1, 0) * Me(0, 2) - Me(1, 2) * Me(0, 0))

        answer(2, 0) = (1.0 / det) * (Me(2, 1) * Me(1, 0) - Me(2, 0) * Me(1, 1))
        answer(2, 1) = (1.0 / det) * (Me(2, 0) * Me(0, 1) - Me(2, 1) * Me(0, 0))
        answer(2, 2) = (1.0 / det) * (Me(1, 1) * Me(0, 0) - Me(1, 0) * Me(0, 1))
        Return answer
    End Function
#End Region


#Region " Operators "

    Public Overloads Shared Operator -(ByVal right As Matrix3) As Matrix3
        Return New Matrix3(Right.Negative)
    End Operator

    Public Overloads Shared Operator +(ByVal left As Matrix3, ByVal right As Matrix3) As Matrix3
        Return New Matrix3(left.Add(right))
    End Operator
    Public Overloads Shared Operator -(ByVal left As Matrix3, ByVal right As Matrix3) As Matrix3
        Return New Matrix3(left.Subtract(right))
    End Operator

    Public Overloads Shared Operator *(ByVal factor As Double, ByVal matrix As Matrix3) As Matrix3
        Return New Matrix3(matrix.Multiply(factor))
    End Operator
    Public Overloads Shared Operator *(ByVal matrix As Matrix3, ByVal factor As Double) As Matrix3
        Return New Matrix3(matrix.Multiply(factor))
    End Operator
    Public Overloads Shared Operator *(ByVal left As Matrix3, ByVal right As Matrix3) As Matrix3
        Return New Matrix3(left.Multiply(right))
    End Operator
    Public Overloads Shared Operator *(ByVal matrix As Matrix3, ByVal vector As Vector3) As Vector3
        Return New Vector3(matrix.Multiply(vector))
    End Operator

    Public Overloads Shared Operator /(ByVal factor As Double, ByVal matrix As Matrix3) As Matrix3
        Return New Matrix3(matrix.Multiply(1 / factor))
    End Operator
    Public Overloads Shared Operator /(ByVal matrix As Matrix3, ByVal factor As Double) As Matrix3
        Return New Matrix3(matrix.Multiply(1 / factor))
    End Operator
    Public Overloads Shared Operator /(ByVal left As Matrix3, ByVal right As Matrix3) As Matrix3
        Return New Matrix3(left.Multiply(right.Invert))
    End Operator
    Public Overloads Shared Operator /(ByVal vector As Vector3, ByVal matrix As Matrix3) As Vector3
        Return New Vector3((matrix * matrix.Transpose).Invert * matrix * vector) 'Matlab v5 manual 4-23, with (B/A)'=(A'\B')
    End Operator

    Public Overloads Shared Operator /(ByVal matrix As Matrix3, ByVal vector As Vector3) As Vector3
        Return New Vector3(matrix.Multiply(1 / vector.InnerProduct(vector) * vector)) 'matrix times pseudoinverse vector pinv(u) = inv(u' * u) * u'
    End Operator

    Public Overloads Shared Operator =(ByVal left As Matrix3, ByVal right As Matrix3) As Boolean
        Return left.Equals(right)
    End Operator

    Public Overloads Shared Operator <>(ByVal left As Matrix3, ByVal right As Matrix3) As Boolean
        Return Not left.Equals(right)
    End Operator

#End Region

    Public Overloads Shared Sub ClassTest()

        Dim J As New Matrix3(0, -1, 0, 1, 0, 0, 0, 0, -1)
        Dim Jt As Matrix3 = J.Transpose
        Dim left_side, right_side As New Matrix3

        left_side = Jt * J
        right_side = Matrix3.Identity()

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 5.3.8
            Console.Error.WriteLine("[Matrix3] The product of orthogonal matrix and its transpose should be I")
        End If

        Dim A As New Matrix3(1, 2, 3, 4, 5, 6, 7, 8, 9)
        Dim B As New Matrix3(1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8, 9.9)

        left_side = (A * B).Transpose()
        right_side = B.Transpose() * A.Transpose()

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 5.3.9
            Console.Error.WriteLine("[Matrix3] The transpose should have the property (AB)^T = B^T A^T")
        End If

        left_side = A.Transpose().Invert()
        right_side = A.Invert().Transpose()

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 5.3.9
            Console.Error.WriteLine("[Matrix3] If A is invertable, the property (A^T)^-1 = (A^-1)^T should hold")
        End If

        Dim T As New Matrix3(1, 2, 3, 0, 4, 5, 0, 0, 6)

        If Not Determinant(T) = 24 Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 6.1.6
            Console.Error.WriteLine("[Matrix3] The Determinant of a triangular matrix should be product of its diagonal")
        End If

        If Not Trace(T) = 11 Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, P. 234
            Console.Error.WriteLine("[Matrix] The trace of a triangular matrix should be sum of its diagonal")
        End If

        left_side = Matrix3.Identity() + B - Matrix3.Identity()
        right_side = B

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Def 1.3.5
            Console.Error.WriteLine("[Matrix3] The fact 'I + B - I = B' should hold ")
        End If

        left_side = A + Matrix3.Zeros()
        right_side = A

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Def 1.3.5
            Console.Error.WriteLine("[Matrix3] The fact 'A + 0 = A' should hold ")
        End If

        left_side = B * Matrix3.Identity()
        right_side = B

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 2.4.6
            Console.Error.WriteLine("[Matrix3] The fact 'B * I = B' should hold ")
        End If

        A(0, 0) = 1 'Matlab v5 manual p. 4-4 pascal(3)
        A(0, 1) = 1
        A(0, 2) = 1

        A(1, 0) = 1
        A(1, 1) = 2
        A(1, 2) = 3

        A(2, 0) = 1
        A(2, 1) = 3
        A(2, 2) = 6

        B(0, 0) = 8 'Matlab v5 manual p. 4-4 magic(3)
        B(0, 1) = 1
        B(0, 2) = 6

        B(1, 0) = 3
        B(1, 1) = 5
        B(1, 2) = 7

        B(2, 0) = 4
        B(2, 1) = 9
        B(2, 2) = 2

        Dim X As Matrix3 = New Matrix3

        X(0, 0) = 15 'Matlab v5 manual p. 4-9
        X(0, 1) = 15
        X(0, 2) = 15

        X(1, 0) = 26
        X(1, 1) = 38
        X(1, 2) = 26

        X(2, 0) = 41
        X(2, 1) = 70
        X(2, 2) = 39

        left_side = A * B
        right_side = X

        If Not (left_side = right_side) Then
            ''Matlab v5 manual p. 4-9
            Console.Error.WriteLine("[Matrix3] The fact 'A * B = X' should hold ")
        End If

        Dim u As Vector3 = New Vector3(3, 1, 4) ' Matlab v5 manual p. 4-5 column vector
        Dim left_side_vector As Vector3 = New Vector3(8, 17, 30) ' Matlab v5 manual p. 4-9
        Dim right_side_vector As Vector3 = A * u

        If Not (left_side_vector = right_side_vector) Then
            'Matlab v5 manual 4-9
            Console.Error.WriteLine("[Matrix3] The fact A * u = x should hold ")
        End If

        Dim v As Vector3 = New Vector3(2, 0, -1) ' Matlab v5 manual p. 4-5 column vector
        left_side_vector = New Vector3(12, -7, 10) ' Matlab v5 manual p. 4-9
        right_side_vector = v * B

        If Not (left_side_vector = right_side_vector) Then
            'Matlab v5 manual 4-9
            Console.Error.WriteLine("[Matrix3] The fact v * B = y should hold ")
        End If

        X = B / A
        If Not (X * A = B) Then
            'Matlab v5 manual 4-13
            Console.Error.WriteLine("[Matrix3] The fact X * A = B should hold ")
        End If

        Dim y As Vector3 = New Vector3(5, -4, 1)
        'y = B / v
        If Not (y = v / A) Then
            'Matlab v5 manual 4-13
            Console.Error.WriteLine("[Matrix3] The fact y = v / A should hold ")
        End If

        X(0, 0) = 4 'Matlab y * v
        X(0, 1) = 0
        X(0, 2) = -2

        X(1, 0) = -0.4
        X(1, 1) = 0
        X(1, 2) = 0.2

        X(2, 0) = 2.4
        X(2, 1) = 0
        X(2, 2) = -1.2

        y = New Vector3(2, -0.2, 1.2)


        If Not (y.OuterProduct(v) = X) Then
            'Matlab v5 manual 4-13
            Console.Error.WriteLine("[Matrix3] The fact y * v = X should hold ")
        End If

        y = New Vector3(0.307692307692308, 0.653846153846154, 1.153846153846154)

        If Not (A / u = y) Then
            'Matlab v5 manual 4-13
            Console.Error.WriteLine("[Matrix3] The fact y = A / u' should hold ")
        End If

    End Sub

End Class


