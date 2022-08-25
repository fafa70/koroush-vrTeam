
'SEN {Type RFID} {Name string} {ID int} {Location x,y,z}

<Serializable()> _
Public Class RfidData
    Implements ISensorData

    Public Sub New()
        Me._Tags = New List(Of RfidTag)
    End Sub

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
        Me._Tags.Clear()
        For Each key As String In msg.Keys
            If IsNumeric(key) Then
                Me._Tags.Add(New RfidTag(CInt(key), msg(key)))
            End If
        Next
    End Sub

    Private _Tags As List(Of RfidTag)
    Public ReadOnly Property Tags() As IEnumerable(Of RfidTag)
        Get
            Return _Tags
        End Get
    End Property
    Public ReadOnly Property TagCount() As Integer
        Get
            Return Me._Tags.Count
        End Get
    End Property





    Public Class RfidTag

        Friend Sub New(ByVal key As Integer, ByVal value As String)
            Me._ID = key

            Dim parts() As String = Strings.Split(value, ",")
            Me._X = Double.Parse(parts(0))
            Me._Y = Double.Parse(parts(1))
            Me._Z = Double.Parse(parts(2))
        End Sub

        Private _ID As Integer
        Public ReadOnly Property ID() As Integer
            Get
                Return _ID
            End Get
        End Property

        Private _X As Double
        Public ReadOnly Property X() As Double
            Get
                Return _X
            End Get
        End Property

        Private _Y As Double
        Public ReadOnly Property Y() As Double
            Get
                Return _Y
            End Get
        End Property

        Private _Z As Double
        Public ReadOnly Property Z() As Double
            Get
                Return _Z
            End Get
        End Property

        Public Function IsSingleShot() As Boolean
            Return Me.ID >= 10000
        End Function

    End Class

End Class
