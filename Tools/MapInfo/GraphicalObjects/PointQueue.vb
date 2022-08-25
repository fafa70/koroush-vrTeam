'Imports UvARescue.Math

Public Class PointQueue
    Inherits DataQueue(Of PointData)

    Public Shared ReadOnly GRAPHICALOBJECTTYPE_POINT As String = "Point"

    Public Sub New(ByVal name As String)
        MyBase.New(GRAPHICALOBJECTTYPE_POINT, name, 1400) 'for the moment

    End Sub

End Class
