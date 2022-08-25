Imports UvARescue.Agent
Imports UvARescue.Math

<Serializable()> _
Public Class ScanObservation

    Public Sub New(ByVal factor As Single, ByVal rangeData As LaserRangeData)
        Me._Factor = factor
        Me._RangeData = rangeData
    End Sub

    Private _Factor As Single
    Public ReadOnly Property Factor() As Single
        Get
            Return _Factor
        End Get
    End Property


    Private _RangeData As LaserRangeData

    Public ReadOnly Property FieldOfView() As Double
        Get
            Return Me._RangeData.FieldOfView
        End Get
    End Property

    Public ReadOnly Property MeasuredTime() As Double
        Get
            Return Me._RangeData.MeasuredTime
        End Get
    End Property

    Public ReadOnly Property Resolution() As Double
        Get
            Return Me._RangeData.Resolution
        End Get
    End Property

    Public ReadOnly Property MinRange() As Double
        Get
            Return _RangeData.MinRange
        End Get
    End Property

    Public ReadOnly Property MaxRange() As Double
        Get
            Return _RangeData.MaxRange
        End Get
    End Property

    Public ReadOnly Property OffsetX() As Single
        Get
            Return Me._RangeData.OffsetX
        End Get
    End Property

    Public ReadOnly Property OffsetY() As Single
        Get
            Return Me._RangeData.OffsetY
        End Get
    End Property

    Public ReadOnly Property Range() As Double()
        Get
            Return Me._RangeData.Range
        End Get
    End Property

    Public ReadOnly Property Length() As Integer
        Get
            Try
                Return Me.Range.Length
            Catch
                Return 5
            End Try
        End Get
    End Property

End Class
