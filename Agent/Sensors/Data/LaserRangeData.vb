'SEN {Type RangeScanner} {Name Scanner1} {Resolution 0.0174} {FOV 3.1415} {Range <doubles>}

<Serializable()> _
Public Class LaserRangeData
    Implements ISensorData

    Public ReadOnly MinRange As Single
    Public ReadOnly MaxRange As Single
    Public ReadOnly OffsetX As Single
    Public ReadOnly OffsetY As Single


    Public Sub New(ByVal minRange As Single, ByVal maxRange As Single, ByVal offsetx As Single, ByVal offsety As Single)

        Me.MinRange = minRange
        Me.MaxRange = maxRange
        Me.OffsetX = offsetx
        Me.OffsetY = offsety

        Me._FieldOfView = 0
        Me._Resolution = 0
        Me._Range = Nothing
        Me._MeasuredTime = 0

    End Sub
    '"lms200", LaserRangeDeviceType.SickLMS, 0.2F, 79.0F, 0.0F, 0.0F)
    Public Sub New()
        Me.MinRange = 0.2
        Me.MaxRange = 79.0
        Me.OffsetX = 0.0
        Me.OffsetY = 0.0
        Me._FieldOfView = 0
        Me._Resolution = 0

        Me._MeasuredTime = 0

    End Sub


    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
        Me._FieldOfView = Double.Parse(msg("FOV"))
        Me._Resolution = Double.Parse(msg("Resolution"))
        If msg.ContainsKey("Time") Then
            Me._MeasuredTime = Double.Parse(msg("Time"))
        Else
            'Important for InterLeagueChallenge
        End If


        'NOTE: z axis points downwards, so read distances in reverse order
        Dim dists() As String = Strings.Split(msg("Range"), ",")
        ReDim Me._Range(dists.Length - 1)
        For i As Integer = 0 To dists.Length - 1
            Me._Range(i) = Double.Parse(dists(dists.Length - i - 1))
        Next

    End Sub

    Private _MeasuredTime As Double 'timestamp in seconds, needed for Inter League Challenge
    Public ReadOnly Property MeasuredTime() As Double
        Get
            Return _MeasuredTime
        End Get
    End Property

    Private _FieldOfView As Double
    Public ReadOnly Property FieldOfView() As Double
        Get
            Return _FieldOfView
        End Get
    End Property

    Private _Resolution As Double
    Public ReadOnly Property Resolution() As Double
        Get
            Return _Resolution
        End Get
    End Property

    Private _Range() As Double
    Public ReadOnly Property Range() As Double()
        Get
            Try
                Return _Range
            Catch ' HANNE: Try to fake a laser-scan
                ReDim Me._Range(10)
                For i As Integer = 0 To 10
                    Me._Range(i) = 10.0
                Next
                Return _Range
            End Try
        End Get
    End Property

End Class
