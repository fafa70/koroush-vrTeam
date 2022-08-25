Imports UvARescue.Math

Public Class GPSSensor
    Inherits SingleStateSensor(Of GPSData)

    Public Shared ReadOnly SENSORTYPE_GPS As String = "GPS"

        'From GPSSensor.uc
        Private ZeroZeroLocationLatDeg As Integer = 39
    Private ZeroZeroLocationLonDeg As Integer = -77 'NIST coordinates

        Private ZeroZeroLocationLatMin As Double = 8.0273
        Private ZeroZeroLocationLonMin As Double = 12.998 'NIST coordinates

        Private ScaleLatitudeDeg As Integer = 111200 ' 111 km per degree
        Private ScaleLongitudeDeg As Integer = 71670 '  72 km per degree

        Private ScaleLatitudeMin As Double = ScaleLatitudeDeg / 60
        Private ScaleLongitudeMin As Double = ScaleLongitudeDeg / 60


    Public Sub New()
        MyBase.New(New GPSData, SENSORTYPE_GPS, Sensor.MATCH_ANYNAME)
        If Not IsNothing(Me.Agent) AndAlso Not IsNothing(Me.Agent.AgentConfig) AndAlso Not String.IsNullOrEmpty(Me.Agent.AgentConfig.StartLocation) Then

            _CurrentPose = New Vector2(Me.Agent.AgentConfig.StartLocation)
        Else
            _CurrentPose = New Vector2
        End If


    End Sub

    

    Protected Overrides Sub ProcessMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessMessage(msgtype, msg)

        If msgtype = MessageType.Sensor Then
            Me.CurrentData.Load(msg)
            Me.ProcessGPSData(Me.CurrentData)
            Me.Agent.NotifySensorUpdate(Me)
        End If

    End Sub

    'From GPSSensor.uc

    Public Overridable Sub ProcessGPSData(ByVal current_data As GPSData)
        Dim seedPose As New Pose2D(0, 0, 0)

        ' Console.WriteLine(String.Format("[Agent] - GPS received: Lateral {0:d2}:{1:f2}, Longitude {2:d2}:{3:f2}", current_data.LatDeg, current_data.LatMin, current_data.LonDeg, current_data.LonMin))

        seedPose.X = CType(current_data.LatSgn * ((current_data.LatDeg - ZeroZeroLocationLatDeg) * ScaleLatitudeDeg + (current_data.LatMin - ZeroZeroLocationLatMin) * ScaleLatitudeMin), Double)
        seedPose.Y = CType(current_data.LonSgn * ((current_data.LonDeg - ZeroZeroLocationLonDeg) * ScaleLongitudeDeg + (current_data.LonMin - ZeroZeroLocationLonMin) * ScaleLongitudeMin), Double)
        'Z should also be possible to receive from the GPS. If so, use Pose3D for seedPose

        If current_data.Fix Then
            Me.PoseEstimate.X = seedPose.X
            Me.PoseEstimate.Y = seedPose.Y
            Me.Acquired = True
        Else
            Me.Acquired = False
            'Keep the current position. The lack of progress should be signaled elsewhere (SlamSeed)
        End If

        ' Console.WriteLine(String.Format("[Agent] - GPS received: distance  from ZeroZero is ({0:f2} , {1:f2}) minutes = ({2:f2} , {3:f2}) m", (current_data.LatMin - ZeroZeroLocationLatMin), (current_data.LonMin - ZeroZeroLocationLonMin), seedPose.X, seedPose.Y))


    End Sub

#Region "Processed information"



    Private _CurrentPose As Vector2
    Public Property PoseEstimate() As Vector2
        Get
            Return _CurrentPose
        End Get
        'The offset should be added (needs GetGeo message processing)
        Set(ByVal value As Vector2)
            Me._CurrentPose = value
        End Set
    End Property

    Private _Acquired As Boolean = False
    Public Property Acquired() As Boolean
        Get
            Return _Acquired
        End Get
        Set(ByVal value As Boolean)
            _Acquired = value
        End Set
    End Property
#End Region




End Class
