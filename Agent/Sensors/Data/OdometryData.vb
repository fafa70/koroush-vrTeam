'SEN {Type Odometry} {Name string} {Pose x,y,theta}

<Serializable()> _
Public Class OdometryData
	Implements ISensorData

    Private _OffsetX As Double
    Private _OffsetY As Double
    Private _OffsetTheta As Double

    Public Sub New()

        Me._X = 0
        Me._Y = 0
        Me._Theta = 0

        Me.ResetOffsets(0, 0, 0)

    End Sub

    Private Function NormalizeAngle(ByVal radians As Double) As Double
        While radians > System.Math.PI
            radians -= 2 * System.Math.PI
        End While
        While radians <= -System.Math.PI
            radians += 2 * System.Math.PI
        End While
        Return radians
    End Function

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
        Dim parts() As String = Strings.Split(msg("Pose"), ",")
        If Not parts(0) = "-1.#IND" OrElse Not parts(1) = "-1.#IND" OrElse Not parts(2) = "-1.#IND" Then
            Me._X = Double.Parse(parts(0))
            Me._Y = Double.Parse(parts(1))

                Me._Theta = Me.NormalizeAngle(Double.Parse(parts(2)))


            If msg.ContainsKey("Time") Then
                Me._MeasuredTime = Double.Parse(msg("Time"))

            End If
        Else
            Console.WriteLine("Odometry received INF numbers")
        End If
    End Sub

    Public Sub ResetOffsets(ByVal x As Double, ByVal y As Double, ByVal theta As Double)
        Me._OffsetX = x - Me._X
        Me._OffsetY = y - Me._Y
        Me._OffsetTheta = theta - Me._Theta
    End Sub

    Private _X As Double
    Public ReadOnly Property X() As Double
        Get
            Return _OffsetX + _X
        End Get
    End Property

    Private _Y As Double
    Public ReadOnly Property Y() As Double
        Get
            Return _OffsetY + _Y
        End Get
    End Property

    Private _Theta As Double
    Public ReadOnly Property Theta() As Double
        Get
            Return _OffsetTheta + _Theta
        End Get
    End Property
    Public ReadOnly Property OffsetX() As Double
        Get
            Return _OffsetX
        End Get
    End Property

    Public ReadOnly Property OffsetY() As Double
        Get
            Return _OffsetY
        End Get
    End Property

    Private _MeasuredTime As Double 'timestamp in seconds
    Public ReadOnly Property MeasuredTime() As Double
        Get
            Return _MeasuredTime
        End Get
    End Property



End Class
