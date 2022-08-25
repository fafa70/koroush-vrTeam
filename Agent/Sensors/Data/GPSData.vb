
'SEN {Type GPS} {Name GPS1} {Latitude 47,40.33,N} {Longitude 122,18.5,W} {Fix 1} {Satellites 8}

<Serializable()> _
Public Class GPSData
    Implements ISensorData

    Private _Satellites As Integer

    Public Sub New()
        'Default: NIST coordinates
        Me._LatDeg = 39
        Me._LatMin = 8.0273
        Me._LatSgn = +1 ' "N"  (positive Latitude)
        Me._LonDeg = 77
        Me._LonMin = 12.998
        Me._LonSgn = -1 ' "W"  (negative Longitude)
        Me._Fix = False
        Me._Satellites = 0
    End Sub

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
        Dim parts() As String

        Dim fixInteger As Integer = -1

        If msg.ContainsKey("Fix") Then
            fixInteger = Integer.Parse(msg("Fix"))
        End If

        If fixInteger = 1 Then
            Me._Fix = True 'a value of 1 means a position is acquired


            Dim hemisphere As String

            parts = Strings.Split(msg("Latitude"), ",")
            Me._LatDeg = Integer.Parse(parts(0))
            Me._LatMin = Double.Parse(parts(1))
            hemisphere = Char.Parse(parts(2))
            If hemisphere.ToUpper.StartsWith("N") Then
                Me._LatSgn = +1
            Else
                Me._LatSgn = -1
            End If

            parts = Strings.Split(msg("Longitude"), ",")
            Me._LonDeg = Integer.Parse(parts(0))
            Me._LonMin = Double.Parse(parts(1))
            hemisphere = Char.Parse(parts(2))
            If hemisphere.ToUpper.StartsWith("E") Then
                Me._LonSgn = +1
            Else
                Me._LonSgn = -1
            End If

            'Are there also satellites after Lat,Lon messages

            Me._Satellites = Integer.Parse(msg("Satellites"))
            If Me._Satellites < 4 Then
                Me._Fix = False 'do not trust a value from less 4 satellites
            End If

        Else
            Me._Fix = False 'a value of 0 means no position is acquired. Other values are errors, which also should lead to false
            'SEN {Type GPS} {Name string} {Fix 0} {Satellites numSats}


        End If



    End Sub

    'Arnoud: also possible Latitude() returns object (min,sec,cardinality)
    Private _LatDeg As Integer
    Public ReadOnly Property LatDeg() As Integer
        Get
            Return _LatDeg
        End Get
    End Property

    Private _LatMin As Double
    Public ReadOnly Property LatMin() As Double
        Get
            Return _LatMin
        End Get
    End Property

    Private _LatSgn As Integer
    Public ReadOnly Property LatSgn() As Integer
        Get
            Return _LatSgn
        End Get
    End Property

    Private _LonDeg As Integer
    Public ReadOnly Property LonDeg() As Integer
        Get
            Return _LonDeg
        End Get
    End Property

    Private _LonMin As Double
    Public ReadOnly Property LonMin() As Double
        Get
            Return _LonMin
        End Get
    End Property

    Private _LonSgn As Integer
    Public ReadOnly Property LonSgn() As Integer
        Get
            Return _LonSgn
        End Get
    End Property

    Private _Fix As Boolean
    Public ReadOnly Property Fix() As Boolean
        Get
            Return _Fix
        End Get
    End Property

End Class
