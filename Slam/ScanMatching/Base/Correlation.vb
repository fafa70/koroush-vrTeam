Imports UvARescue.Math

Public Structure Correlation(Of T As Vector2)

    Public Sub New(ByVal point1 As T, ByVal point2 As T)
        Me.Point1 = point1
        Me.Point2 = point2
    End Sub

    Public ReadOnly Point1 As T
    Public ReadOnly Point2 As T

End Structure

