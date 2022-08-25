
'SEN {Type INU} {Name string} {Orientation x,y,z}

<Serializable()> _
Public Class InuData
    Implements ISensorData

    Public Sub New()
        Me._Roll = 0
        Me._Pitch = 0
        Me._Yaw = 0
    End Sub

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
        Dim parts() As String = Strings.Split(msg("Orientation"), ",")
        Me._Roll = Double.Parse(parts(0))
        Me._Pitch = Double.Parse(parts(1))
        Me._Yaw = Double.Parse(parts(2))
    End Sub

    Private _Roll As Double
    Public ReadOnly Property Roll() As Double
        Get
            Return _Roll
        End Get
    End Property

    Private _Pitch As Double
    Public ReadOnly Property Pitch() As Double
        Get
            Return _Pitch
        End Get
    End Property

    Private _Yaw As Double
    Public ReadOnly Property Yaw() As Double
        Get
            Return _Yaw
        End Get
    End Property

End Class
