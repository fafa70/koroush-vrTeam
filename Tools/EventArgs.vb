Public Class EventArgs(Of T)
    Inherits EventArgs

    Public Sub New(ByVal data As T)
        Me._data = data
    End Sub

    Private _data As T

    Public ReadOnly Property Data() As T
        Get
            Return Me._data
        End Get
    End Property

End Class
