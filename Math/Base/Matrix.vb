<Serializable()> _
Public Class Matrix

#Region " Constructors / Factory Methods "

    'copy constructor
    Public Sub New(ByVal source As Matrix)
        Me.New(source.Rows, source.Cols)
        For row As Integer = 0 To Me.Rows - 1
            For col As Integer = 0 To Me.Cols - 1
                Me(row, col) = source(row, col)
            Next
        Next
    End Sub

    Public Sub New(ByVal rows As Integer, ByVal cols As Integer)
        Me._rows = rows
        Me._cols = cols
        ReDim Me._m(rows - 1, cols - 1)
    End Sub

    Public Sub New(ByVal size As Integer)
        Me.New(size, size)
    End Sub

    Public Sub LoadIdentity()
        For row As Integer = 0 To Me.Rows - 1
            For col As Integer = 0 To Me.Cols - 1
                If row = col Then
                    Me(row, col) = 1
                Else
                    Me(row, col) = 0
                End If
            Next
        Next
    End Sub

    Public Shared Function Identity(ByVal size As Integer) As Matrix
        Dim matrix As New Matrix(size, size)
        matrix.LoadIdentity()
        Return matrix
    End Function

    Public Sub LoadZeros()
        For row As Integer = 0 To Me.Rows - 1
            For col As Integer = 0 To Me.Cols - 1
                Me(row, col) = 0
            Next
        Next
    End Sub

    Public Overloads Shared Function Zeros(ByVal size As Integer) As Matrix
        Return Zeros(size, size)
    End Function
    Public Overloads Shared Function Zeros(ByVal rows As Integer, ByVal cols As Integer) As Matrix
        Dim matrix As New Matrix(rows, cols)
        matrix.LoadZeros()
        Return matrix
    End Function

    Public Overloads Shared Function Ones(ByVal size As Integer) As Matrix
        Return Ones(size, size)
    End Function
    Public Overloads Shared Function Ones(ByVal rows As Integer, ByVal cols As Integer) As Matrix
        Dim matrix As New Matrix(rows, cols)
        For row As Integer = 0 To rows - 1
            For col As Integer = 0 To cols - 1
                matrix(row, col) = 1
            Next
        Next
        Return matrix
    End Function

#End Region

#Region " Data "

    Private _m(,) As Double
    Default Public Overloads Property Item(ByVal row As Integer, ByVal col As Integer) As Double
        Get
            Return Me._m(row, col)
        End Get
        Set(ByVal value As Double)
            Me._m(row, col) = value
        End Set
    End Property

    Private _rows As Integer
    Public ReadOnly Property Rows() As Integer
        Get
            Return Me._rows
        End Get
    End Property

    Private _cols As Integer
    Public ReadOnly Property Cols() As Integer
        Get
            Return Me._cols
        End Get
    End Property

    Public Overrides Function ToString() As String

        Dim builder As New Text.StringBuilder
        builder.AppendFormat("Matrix of size {0}x{1}", Me.Rows, Me.Cols).AppendLine()
        builder.Append(" [ ")
        For i As Integer = 0 To Me.Rows - 1
            For j As Integer = 0 To Me.Cols - 1
                builder.Append(Me(i, j) & " ")
            Next
            If i < Me.Rows - 1 Then
                builder.AppendLine()
            Else
                builder.Append("]").AppendLine()
            End If
        Next

        Return builder.ToString()

    End Function

#End Region

#Region "Protected Help functions for Operators "

    Protected Friend Overloads Function Negative() As Matrix
        Dim result As New Matrix(Me.Rows, Me.Cols)
        For row As Integer = 0 To Me.Rows - 1
            For col As Integer = 0 To Me.Cols - 1
                result(row, col) = -Me(row, col)
            Next
        Next
        Return result
    End Function
    Protected Friend Overloads Shared Function Negative(ByVal matrix As Matrix) As Matrix
        Return matrix.Negative
    End Function

    Protected Friend Overloads Function Add(ByVal right As Matrix) As Matrix
        If right.Rows <> Me.Rows OrElse right.Cols <> Me.Cols Then _
            Throw New InvalidOperationException("Matrixes must be of equal dimensions")
        Dim result As New Matrix(Me.Rows, Me.Cols)
        For row As Integer = 0 To Me.Rows - 1
            For col As Integer = 0 To Me.Cols - 1
                result(row, col) = Me(row, col) + right(row, col)
            Next
        Next
        Return result
    End Function
    Protected Friend Overloads Shared Function Add(ByVal left As Matrix, ByVal right As Matrix) As Matrix
        Return left.Add(right)
    End Function

    Protected Friend Overloads Function Subtract(ByVal right As Matrix) As Matrix
        If right.Rows <> Me.Rows OrElse right.Cols <> Me.Cols Then _
            Throw New InvalidOperationException("Matrixes must be of equal dimensions")
        Dim result As New Matrix(Me.Rows, Me.Cols)
        For row As Integer = 0 To Me.Rows - 1
            For col As Integer = 0 To Me.Cols - 1
                result(row, col) = Me(row, col) - right(row, col)
            Next
        Next
        Return result
    End Function
    Protected Friend Overloads Function Subtract(ByVal left As Matrix, ByVal right As Matrix) As Matrix
        Return left.Subtract(right)
    End Function

    Protected Friend Overloads Function Multiply(ByVal factor As Double) As Matrix
        Dim result As New Matrix(Me.Rows, Me.Cols)
        For row As Integer = 0 To Me.Rows - 1
            For col As Integer = 0 To Me.Cols - 1
                result(row, col) = factor * Me(row, col)
            Next
        Next
        Return result
    End Function
    Protected Friend Overloads Shared Function Multiply(ByVal factor As Double, ByVal matrix As Matrix) As Matrix
        Return matrix.Multiply(factor)
    End Function

    Protected Friend Overloads Function Multiply(ByVal right As Matrix) As Matrix
        If Me.Cols <> right.Rows Then _
            Throw New InvalidOperationException("Inner Matrix dimensions must agree")
        Dim result As New Matrix(Me.Rows, right.Cols)
        Dim sum As Double
        For row As Integer = 0 To Me.Rows - 1
            For col As Integer = 0 To right.Cols - 1
                sum = 0
                For idx As Integer = 0 To Me.Cols - 1
                    sum += Me(row, idx) * right(idx, col)
                Next
                result(row, col) = sum
            Next
        Next
        Return result
    End Function
    Protected Friend Overloads Shared Function Multiply(ByVal left As Matrix, ByVal right As Matrix) As Matrix
        Return left.Multiply(right)
    End Function

    Protected Friend Overloads Function Multiply(ByVal vector As Vector) As Vector
        If Me.Cols <> vector.Length Then _
            Throw New InvalidOperationException("Dimensions must agree")
        Dim result As New Vector(Me.Rows)
        Dim sum As Double
        For row As Integer = 0 To Me.Rows - 1
            sum = 0
            For col As Integer = 0 To Me.Cols - 1
                sum += Me(row, col) * vector(col)
            Next
            result(row) = sum
        Next
        Return result
    End Function
    Protected Friend Overloads Shared Function Multiply(ByVal left As Matrix, ByVal right As Vector) As Vector
        Return left.Multiply(right)
    End Function
#End Region

#Region "Public accessable functions "
    Public Overridable Overloads Function Determinant() As Double
        If Me.Rows = 1 AndAlso Me.Cols = 1 Then
            Return Me(0, 0)
        End If
        'Could use a Laplace expansion, see 'Bretscher's "Linear Algebra with Application" 3rd edition, Def 6.1.4
        'Should define the Minor operation first!
        Throw New NotSupportedException("Determinant computation not supported on Matrix")
    End Function
    Public Overloads Shared Function Determinant(ByVal matrix As Matrix) As Double
        Return matrix.Determinant
    End Function

    Public Overridable Function Invert() As Matrix
        If Me.Rows = 1 AndAlso Me.Cols = 1 Then
            Dim answer As New Matrix(1)
            Dim det As Double = Me.Determinant 'what if det = 0
            answer(0, 0) = 1 / det
            Return (answer)
        End If
        Throw New NotSupportedException("Invert computation not supported on Matrix")

    End Function


    Public Overloads Function Trace() As Double
        If Me.Rows <> Me.Cols Then _
            Throw New InvalidOperationException("Cannot compute Trace for non-square matrixes")
        Dim result As Double = 0
        For idx As Integer = 0 To Me.Rows - 1
            result += Me(idx, idx)
        Next
        Return result
    End Function
    Public Overloads Shared Function Trace(ByVal matrix As Matrix) As Double
        Return matrix.Trace
    End Function


    Public Overloads Function Transpose() As Matrix
        Dim result As New Matrix(Me.Cols, Me.Rows)
        For row As Integer = 0 To Me.Rows - 1
            For col As Integer = 0 To Me.Cols - 1
                result(col, row) = Me(row, col)
            Next
        Next
        Return result
    End Function
    Public Overloads Shared Function Transpose(ByVal matrix As Matrix) As Matrix
        Return matrix.Transpose()
    End Function

#End Region

#Region "Operators "

    Public Overloads Shared Operator -(ByVal right As Matrix) As Matrix
        Return right.Negative
    End Operator

    Public Overloads Shared Operator +(ByVal left As Matrix, ByVal right As Matrix) As Matrix
        Return left.Add(right)
    End Operator
    Public Overloads Shared Operator -(ByVal left As Matrix, ByVal right As Matrix) As Matrix
        Return left.Subtract(right)
    End Operator

    Public Overloads Shared Operator *(ByVal factor As Double, ByVal matrix As Matrix) As Matrix
        Return matrix.Multiply(factor)
    End Operator
    Public Overloads Shared Operator *(ByVal matrix As Matrix, ByVal factor As Double) As Matrix
        Return matrix.Multiply(factor)
    End Operator
    Public Overloads Shared Operator *(ByVal matrix As Matrix, ByVal vector As Vector) As Vector
        Return matrix.Multiply(vector)
    End Operator
    Public Overloads Shared Operator *(ByVal left As Matrix, ByVal right As Matrix) As Matrix
        Return left.Multiply(right)
    End Operator

    Public Overloads Shared Operator =(ByVal left As Matrix, ByVal right As Matrix) As Boolean
        Return left.Equals(right)
    End Operator

    Public Overloads Shared Operator <>(ByVal left As Matrix, ByVal right As Matrix) As Boolean
        Return Not left.Equals(right)
    End Operator

#End Region

#Region " Equals "

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        If TypeOf obj Is Matrix Then
            Dim matrix As Matrix = DirectCast(obj, Matrix)
            If matrix.Cols <> Me.Cols Then Return False
            If matrix.Rows <> Me.Rows Then Return False
            For i As Integer = 0 To Me.Rows - 1
                For j As Integer = 0 To Me.Cols - 1
                    If Me(i, j) <> matrix(i, j) Then Return False
                Next
            Next
            Return True
        Else
            Return MyBase.Equals(obj)
        End If
    End Function

#End Region

#Region "Unit Testing of Matrix Operation"

    Public Overloads Shared Sub ClassTest()

        Dim J As New Matrix(1)
        J(0, 0) = 1.0
        Dim Jt As Matrix = J.Transpose
        Dim left_side, right_side As New Matrix(1)

        left_side = Jt * J
        right_side = Matrix.Identity(1)

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 5.3.8
            Console.Error.WriteLine("[Matrix] The product of orthogonal matrix and its transpose should be I")
        End If

        Dim A As New Matrix(1)
        A(0, 0) = 1
        Dim B As New Matrix(1)
        B(0, 0) = 2.2

        left_side = (A * B).Transpose()
        right_side = B.Transpose() * A.Transpose()

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 5.3.9
            Console.Error.WriteLine("[Matrix] The transpose should have the property (AB)^T = B^T A^T")
        End If

        left_side = A.Transpose().Invert()
        right_side = A.Invert().Transpose()

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 5.3.9
            Console.Error.WriteLine("[Matrix] If A is invertable, the property (A^T)^-1 = (A^-1)^T should hold")
        End If

        Dim T As New Matrix(1)
        T(0, 0) = 1.1

        If Not Determinant(T) = 1.1 Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 6.1.6
            Console.Error.WriteLine("[Matrix] The Determinant of a triangular matrix should be product of its diagonal")
        End If

        If Not Trace(T) = 1.1 Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, P. 234
            Console.Error.WriteLine("[Matrix] The trace of a triangular matrix should be sum of its diagonal")
        End If

        left_side = A + B - A
        right_side = B

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Def 1.3.5
            Console.Error.WriteLine("[Matrix] The fact 'A + B - A = B' should hold ")
        End If

        left_side = A + Matrix.Zeros(1)
        right_side = A

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Def 1.3.5
            Console.Error.WriteLine("[Matrix] The fact 'A + 0 = A' should hold ")
        End If

        left_side = B * Matrix.Identity(1)
        right_side = B

        If Not (left_side = right_side) Then
            'Bretscher's "Linear Algebra with Application" 3rd edition, Fact 2.4.6
            Console.Error.WriteLine("[Matrix] The fact 'B * I = B' should hold ")
        End If
    End Sub

#End Region


End Class
