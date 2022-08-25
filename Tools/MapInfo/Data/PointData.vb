Imports UvARescue.Math

Public Class PointData
    Implements IGraphicalObjectData

    Public Sub New(ByVal x As Double, ByVal y As Double, ByVal th As Double, ByVal sec As Double)
        Try
            _vector = New Vector2(x, y)
            _featurevector = New Vector2(th, sec)
        Catch ex As Exception
            Console.Error.WriteLine("[PointData:New] Unhandled Error.")
            Console.Error.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
        End Try
    End Sub

    Private _vector As Vector2

    Public ReadOnly Property X() As Double
        Get
            Return _vector(0)
        End Get
    End Property

    Public ReadOnly Property Y() As Double
        Get
            Return _vector(1)
        End Get
    End Property

    Private _featurevector As Vector2
    Public ReadOnly Property Th() As Double
        Get
            Return _featurevector(0)
        End Get
    End Property
    Public ReadOnly Property Sec() As Double
        Get
            Return _featurevector(1)
        End Get
    End Property

End Class
