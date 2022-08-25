
'SEN {Type VictSensor} {Name VictSensorName} {PartName "[Head|Arm|Hand|Chest|Pelvis|Leg|Foot]"} {Location x,y,z} {PartName String} {Location x,y,z}

<Serializable()> _
Public Class VictimData
    Implements ISensorData

    Private _KnownPartNames As New List(Of String)

    Public Sub New()

        Me._KnownPartNames.AddRange( _
            New String() { _
                "head", "arm", "hand", "chest", "pelvis", "leg", "foot"})

        Me._Parts = New List(Of VictimPart)

    End Sub

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
        Me._Parts.Clear()
        For Each key As String In msg.Keys
            If Me._KnownPartNames.Contains(key) Then
                Me._Parts.Add(New VictimPart(key, msg(key)))
            End If
        Next
    End Sub

    Private _Parts As List(Of VictimPart)
    Public ReadOnly Property Parts() As IEnumerable(Of VictimPart)
        Get
            Return _Parts
        End Get
    End Property
    Public ReadOnly Property PartCount() As Integer
        Get
            Return Me._Parts.Count
        End Get
    End Property

    Public ReadOnly Property PartCount(ByVal partName As String) As Integer
        Get
            Dim count As Integer = 0
            For Each part As VictimPart In Me._Parts
                If part.PartName = partName Then
                    count += 1
                End If
            Next
            Return count
        End Get
    End Property




    <Serializable()> _
    Public Class VictimPart

        Friend Sub New(ByVal partName As String, ByVal value As String)

            Me._PartName = partName

            Dim parts() As String = Strings.Split(value, ",")
            Me._X = Double.Parse(parts(0))
            Me._Y = Double.Parse(parts(1))
            Me._Z = Double.Parse(parts(2))

        End Sub

        Private _PartName As String
        Public ReadOnly Property PartName() As String
            Get
                Return _PartName
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

    End Class

End Class
