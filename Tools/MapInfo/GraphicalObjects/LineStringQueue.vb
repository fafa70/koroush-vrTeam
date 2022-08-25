'Imports UvARescue.Math

Public Class LineStringQueue
    Inherits DataQueue(Of LineStringData)

    Public Shared ReadOnly GRAPHICALOBJECTTYPE_POINT As String = "LineString"

    Public Sub New(ByVal name As String)
        MyBase.New(GRAPHICALOBJECTTYPE_POINT, name, 1000) 'for the moment

    End Sub

End Class
