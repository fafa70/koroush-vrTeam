
'STA {Type GroundVehicle} {Time 495.55} {FrontSteer 0.0000} {RearSteer 0.0000} {LightToggle False} {LightIntensity 0} {Battery 3600} {View -1}

<Serializable()> _
Public Class StatusData
    Implements ISensorData

    Public Sub New()
    End Sub

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
        If msg.ContainsKey("Time") Then
            Me._Time = Double.Parse(msg("Time"))
        End If
        If msg.ContainsKey("LightToggle") Then
            Me._LightToggle = Boolean.Parse(msg("LightToggle")) 'No LightToggle in UDK
        End If
        If msg.ContainsKey("LightIntensity") Then
            Me._LightIntensity = Integer.Parse(msg("LightIntensity"))
        End If
        If msg.ContainsKey("Battery") Then
            Me._Battery = Integer.Parse(msg("Battery"))
        End If
        If msg.ContainsKey("View") Then
            Me._View = Integer.Parse(msg("View")) 'No View in UT3
        End If

    End Sub

    Private _Time As Double
    Public ReadOnly Property Time() As Double
        Get
            Return Me._Time
        End Get
    End Property

    Private _LightToggle As Boolean
    Public ReadOnly Property LightToggle() As Boolean
        Get
            Return Me._LightToggle
        End Get
    End Property

    Private _LightIntensity As Integer
    Public ReadOnly Property LightIntensity() As Integer
        Get
            Return Me._LightIntensity
        End Get
    End Property

    Private _Battery As Integer
    Public ReadOnly Property Battery() As Integer
        Get
            Return Me._Battery
        End Get
    End Property

    Private _View As Integer = -1
    Public ReadOnly Property View() As Integer
        Get
            Return Me._View
        End Get
    End Property

End Class
