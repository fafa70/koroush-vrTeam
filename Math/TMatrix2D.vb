Imports System.Math

<Serializable()> _
Public Class TMatrix2D
    Inherits Matrix3

#Region " Constructors "

    Public Sub New()
        Me.New(0, 0, 0)
    End Sub
    Public Sub New(ByVal rotation As Double)
        Me.New(0, 0, rotation)
    End Sub

    Public Sub New(ByVal translation As Vector2)
        Me.New(translation.X, translation.Y, 0)
    End Sub
    Public Sub New(ByVal translation As Vector2, ByVal rotation As Double)
        Me.New(translation.X, translation.Y, rotation)
    End Sub

    Public Sub New(ByVal dx As Double, ByVal dy As Double)
        Me.New(dx, dy, 0)
    End Sub
    Public Sub New(ByVal dx As Double, ByVal dy As Double, ByVal rotation As Double)
        Me.LoadIdentity()
        Me.DeltaX = dx
        Me.DeltaY = dy
        Me.Rotation = rotation
    End Sub

#End Region

#Region " Data Wrappers "

    Public Property DeltaX() As Double
        Get
            Return Me(0, 2)
        End Get
        Set(ByVal value As Double)
            Me(0, 2) = value
        End Set
    End Property
    Public Property DeltaY() As Double
        Get
            Return Me(1, 2)
        End Get
        Set(ByVal value As Double)
            Me(1, 2) = value
        End Set
    End Property

    Private _Rotation As Double
    Public Property Rotation() As Double
        Get
            Return _Rotation
        End Get
        Set(ByVal value As Double)
            If Not value = Me._Rotation Then
                Me._Rotation = value

                Dim s As Double = CType(Sin(value), Double)
                Dim c As Double = CType(Cos(value), Double)
                Me(0, 0) = c
                Me(0, 1) = -s
                Me(1, 0) = s
                Me(1, 1) = c

            End If
        End Set
    End Property

    Public Property Translation() As Vector2
        Get
            Return New Vector2(Me.DeltaX, Me.DeltaY)
        End Get
        Set(ByVal value As Vector2)
            Me.DeltaX = value.X
            Me.DeltaY = value.Y
        End Set
    End Property

#End Region

#Region " Special Operators "

    Public Overloads Sub Translate(ByVal translation As Vector2)
        Me.Translate(translation.X, translation.Y)
    End Sub
    Public Overloads Sub Translate(ByVal x As Double, ByVal y As Double)
        Me.DeltaX += x
        Me.DeltaY += y
    End Sub

    Public Overloads Sub Rotate(ByVal angle As Double)
        Me.Rotation += angle
    End Sub

#End Region

#Region " Operators "

    Public Overloads Function Multiply(ByVal vector As Vector2) As Vector2
        Dim result As New Vector2()
        result.X = vector.X * Me(0, 0) + vector.Y * Me(0, 1) + Me(0, 2)
        result.Y = vector.X * Me(1, 0) + vector.Y * Me(1, 1) + Me(1, 2)
        Return result
    End Function

    Public Overloads Shared Operator -(ByVal right As TMatrix2D) As TMatrix2D
        Return New TMatrix2D(-right.DeltaX, -right.DeltaY, -right.Rotation)
    End Operator

    Public Overloads Shared Operator *(ByVal matrix As TMatrix2D, ByVal vector As Vector3) As Vector3
        Return New Vector3(matrix.Multiply(vector))
    End Operator
    Public Overloads Shared Operator *(ByVal matrix As TMatrix2D, ByVal vector As Vector2) As Vector2
        Return matrix.Multiply(vector)
    End Operator
    Public Overloads Shared Operator *(ByVal matrix As TMatrix2D, ByVal pose As Pose2D) As Pose2D
        Dim vector As New Vector3(pose.X, pose.Y, 1)
        vector = matrix * vector
        Return New Pose2D(vector.X, vector.Y, pose.Rotation + matrix.Rotation)
    End Operator

#End Region

End Class

