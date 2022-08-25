Imports UvARescue.Math
Imports System.Math

Public Class EncoderSensor
    Inherits SingleStateSensor(Of EncoderData)

    Public Shared ReadOnly SENSORTYPE_ENCODER As String = "Encoder"

    Private _OffsetX As Double
    Private _OffsetY As Double
    Private _OffsetTheta As Double

    Public Sub New()
        MyBase.New(New EncoderData, SENSORTYPE_ENCODER, Sensor.MATCH_ANYNAME)

        If Not IsNothing(Me.Agent) AndAlso Not IsNothing(Me.Agent.AgentConfig) AndAlso Not String.IsNullOrEmpty(Me.Agent.AgentConfig.StartLocation) AndAlso Not String.IsNullOrEmpty(Me.Agent.AgentConfig.StartRotation) Then
            _CurrentPose = New Pose2D(Me.Agent.AgentConfig.StartLocation, Me.Agent.AgentConfig.StartRotation)
        Else
            _CurrentPose = New Pose2D
        End If

        Me._X = 0
        Me._Y = 0
        Me._Theta = 0

        Me.ResetOffsets(0, 0, 0)
    End Sub

    'SEN {Type Encoder} {Name ECLeft Tick 0} {Name ECRight Tick -5}

    Protected Overrides Function ToKeyValuePair(ByVal part As String) As System.Collections.Generic.KeyValuePair(Of String, String)
        Dim parts() As String = Strings.Split(part, " ")
        If parts.Length >= 3 Then
            If parts(0).ToUpper = "NAME" Then
                Return New KeyValuePair(Of String, String)(parts(1), parts(3).Trim)
            End If
        End If

        Return MyBase.ToKeyValuePair(part)

    End Function

    Protected Overrides Sub ProcessMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessMessage(msgtype, msg)

        If msgtype = MessageType.Sensor Then
            Me.CurrentData.Load(msg)
            Me.ProcessEncoderData(Me.CurrentData)

            Me.Agent.NotifySensorUpdate(Me)
            'Me.ProcessEncoderData(Me.CurrentData) previously called by Agent
        End If

    End Sub

    Private previous_l As Integer = 0
    Private previous_r As Integer = 0
    'Private wheel_radius As Double = 0.0606 '0.0606m, from Zerg.uc
    'Private wheel_base As Double = 2 * 0.0949999 'For the Zerg, from UsarBot.ini
    Private degInTicks As Integer = 1
    Private radInTicks As Double = degInTicks * 180 / PI

    Private Function RadFromTick(ByVal tick As Integer) As Double
        Dim radians As Double = tick / radInTicks
        If (radians > PI) Then
            radians -= 2 * PI
        ElseIf (radians < -PI) Then
            radians += 2 * PI
        End If
        Return radians
    End Function

    Private _LastSynchronisation As New Dictionary(Of String, DateTime)

    Public Overridable Sub ProcessEncoderData(ByVal current_data As EncoderData)

        Dim factor As Single = 1000 'to convert from mm to m
        Dim left_name As String = "ECLeft"
        Dim right_name As String = "ECRight"

        If Not IsNothing(Me.Agent) AndAlso Not IsNothing(Me.Agent.AgentConfig) AndAlso Not String.IsNullOrEmpty(Me.Agent.AgentConfig.RobotModel) AndAlso Me.Agent.AgentConfig.RobotModel.ToLower = "p3at" Then
            left_name = "EncTestW1"
            right_name = "EncTestW2"
        End If


        If current_data.Encoder(left_name) <> previous_l AndAlso current_data.Encoder(right_name) <> previous_r Then
            'WheelRadius is not known directly (only after spawn), but will be 0.0 as default (no effect)

            Dim diff_l As Double = RadFromTick(previous_l - current_data.Encoder(left_name)) 'negatives tick-differences are forward
            Dim diff_r As Double = RadFromTick(previous_r - current_data.Encoder(right_name))
            'Console.WriteLine(String.Format("[AVINOTE] Agent\Sensor\EncoderSensor::delta ECLeft {0} deg / ECRight {1} deg", diff_l * 180 / PI, diff_r * 180 / PI))



            Dim v_l As Double = Me.Agent.WheelRadius * diff_l '2 * PI * Me.Agent.WheelRadius * (diff_l / (2 * PI)
            Dim v_r As Double = Me.Agent.WheelRadius * diff_r

            Dim omega As Double = (v_r - v_l) / Me.Agent.WheelSeparation

            ' calculate x and y, and the travelled distance (rotation omega around ICC with radius R or curvature Kappa)
            _Theta += omega 'start rotation and end rotation
            _X += Cos(_Theta) * (v_r + v_l) / 2 'the relative sum, excluding the offset
            _Y += Sin(_Theta) * (v_r + v_l) / 2

            'store those values in CurrentPose (including offset)
            _CurrentPose.X = Me.X()
            _CurrentPose.Y = Me.Y()
            _CurrentPose.Rotation = Me.Theta()


            'Console.WriteLine(String.Format("[AVINOTE] Agent\Sensor\EncoderSensor::Estimated Pose ({0},{1},{2}) (with offset ({3},{4},{5})", Me._X * factor, Me._Y * factor, Me._Theta * factor, Me.X * factor, Me.Y * factor, Me.Theta * factor))

            If Abs(omega) < 0.1 Then
                'calculate X and Y with curvature
                Dim curvature As Double = (v_r - v_l) / ((Me.Agent.WheelSeparation / 2) * (v_r + v_l))

            Else
                'calculate X and Y with radius
                Dim radius As Double = (Me.Agent.WheelSeparation / 2) * (v_r + v_l) / (v_r - v_l)
            End If


            previous_l = current_data.Encoder(left_name)
            previous_r = current_data.Encoder(right_name)
            'Console.WriteLine(String.Format("[AVINOTE] Agent\Sensor\EncoderSensor::ECLeft {0} ticks, {1} rad / ECRight {2} ticks ,{3} rad", previous_l, RadFromTick(previous_l), previous_r, RadFromTick(previous_r)))

            Dim speed_u As Single = Me.Agent.ForwardSpeed 'both rad /seconds
            Dim omega_u As Single = Me.Agent.TurningSpeed

            'So, we need to keep track of time

            Dim timestamp As DateTime = Now

            If Not Me._LastSynchronisation.ContainsKey(Me.Agent.Name) Then
                SyncLock Me._LastSynchronisation
                    Me._LastSynchronisation.Add(Me.Agent.Name, timestamp)
                End SyncLock
                Return
            End If

            Dim synchronisation_interval As Integer = 1 'second

            If timestamp - Me._LastSynchronisation(Me.Agent.Name) > TimeSpan.FromSeconds(synchronisation_interval) Then
                'compare the observation z with the control u
            End If
        End If

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

    Public Sub ResetOffsets(ByVal x As Double, ByVal y As Double, ByVal theta As Double)
        Me._OffsetX = x - Me._X
        Me._OffsetY = y - Me._Y
        Me._OffsetTheta = theta - Me._Theta
    End Sub


    Private _CurrentPose As Pose2D
    Public ReadOnly Property PoseEstimate() As Pose2D
        Get
            Return _CurrentPose
        End Get
        'The offset is already added in CurrentData functions (needs GetGeo message processing)
    End Property

End Class
