Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Slam

''' <summary>
''' Base class used for USAR Agents.
''' </summary>
''' <remarks></remarks>
Public Class UsarAgent
    'Inherits CommAgent
    Inherits BehaviorAgent

    Public Sub New(ByVal manifold As Manifold, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(manifold, name, agentConfig, teamConfig)

        ''NOTE: these are mounted by Agent already
        'Me.Mount(New InsSensor) '2007 format
        'Me.Mount(New OdometrySensor)

        'Moved agentConfig dependent mounting of LaserScanner and Camera to Agent, 
        'Also ProxyAgent needs a camera


        'RoboCup08 Temp out Me.Mount(New RfidSensor)
        'ProbRob08 Temp out Me.Mount(New SonarSensor) 'moved to Agent:New
        'RoboCup08 Temp out Me.Mount(New TouchSensor)
        Me.Mount(New VictimSensor) ' 2007 format

        'for backwards compatibility with logfiles from 2006, we also mount the now 
        'obsolete sensors.
        Me.Mount(New InuSensor) '2006 format, became InsSensor in 2007
        'RoboCup08 Temp out Me.Mount(New VictimRfidSensor) '2006 format, becamse VictimSensor in 2007

        Me._Dimensions = New Vector3(0.0, 0.0, 0.0)

        Me._COG = New Vector3(0.0, 0.0, 0.0)

        Me._WheelRadius = 0.0
        Me._WheelSeparation = 0.0
        Me._WheelBase = 0.0

        Me._MaxSpeed = 8.0

    End Sub

    Protected Overrides Sub OnAgentSpawned()
        MyBase.OnAgentSpawned()


        Me.SendGeometryRequest() 'First Body
        Me.SendConfigurationRequest()


    End Sub

    Private counter As Integer = 0
    Public Overrides Sub NotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        MyBase.NotifyPoseEstimateUpdated(pose)

        'keep track of the number of localization updates
        Me.counter += 1
        'Console.WriteLine(String.Format("[{0}]- {1:d3}: current pose = {2}", Me.Name, counter, pose.ToString))

    End Sub

    'Protected Overrides Sub ReceiveMessage(ByVal msgtype As MessageType, ByVal type As String, ByVal name As String, ByVal parts As Specialized.StringCollection)

    '    'forward the message to relevant sensors
    '    Select Case msgtype

    '        Case MessageType.Geometry
    '            Me.ReceiveGeometryMessage(msgtype, parts)

    '        Case Else
    '            MyBase.ReceiveMessage(msgtype, type, name, parts)

    '    End Select
    'End Sub


    Protected Overridable Sub SendGeometryRequest()
        Dim cmd As New Text.StringBuilder
        cmd.Append("GETGEO ")
        cmd.AppendFormat("{{Type {0}}}", "Robot") 'Not Me.AgentConfig.RobotModel, checked in KRobot.uc '
        cmd.AppendFormat(" {{Name {0}}}", Me.Name) 'note the space at the beginning
        cmd.Append(Environment.NewLine)

        Console.WriteLine(String.Format("    [AVINOTE] UsarAgent.vb::SendGeometryRequest sends UsarSimCommand {0}", cmd.ToString))

        Me.SendUsarSimCommand(cmd.ToString)
    End Sub

    Protected Overridable Sub SendConfigurationRequest()
        Dim cmd As New Text.StringBuilder
        cmd.Append("GETCONF ")
        cmd.AppendFormat("{{Type {0}}}", "Robot") 'Not Me.AgentConfig.RobotModel, checked in KRobot.uc '
        cmd.AppendFormat(" {{Name {0}}}", Me.Name) 'note the space at the beginning
        cmd.Append(Environment.NewLine)

        'Console.WriteLine(String.Format("    [AVINOTE] UsarAgent.vb::SendConfigurationRequest sends UsarSimCommand {0}", cmd.ToString))

        Me.SendUsarSimCommand(cmd.ToString)
    End Sub

    
#Region "Properties"

    Private _Dimensions As Vector3
    Public ReadOnly Property Dimensions() As Vector3
        Get
            Return _Dimensions
        End Get
    End Property

    Private _COG As Vector3
    Public ReadOnly Property COG() As Vector3
        Get
            Return _COG
        End Get
    End Property

    Private _MaxSpeed As Single
    Public Overrides ReadOnly Property MaxSpeed() As Single
        Get
            Return _MaxSpeed
        End Get
    End Property

    Private _WheelRadius As Single
    Public Overrides ReadOnly Property WheelRadius() As Single
        Get
            Return _WheelRadius
        End Get
    End Property

    Private _WheelSeparation As Single
    Public Overrides ReadOnly Property WheelSeparation() As Single
        Get
            Return _WheelSeparation
        End Get
    End Property

    Private _WheelBase As Single
    Public Overrides ReadOnly Property WheelBase() As Single
        Get
            Return _WheelBase
        End Get
    End Property

#End Region

#Region " Message Handling "

    Protected Overrides Sub ReceiveConfMessage(ByVal msgtype As MessageType, ByVal parts As Specialized.StringCollection)
        MyBase.ReceiveConfMessage(msgtype, parts)
        Me.ProcessConfMessage(msgtype, Me.ToDictionary(parts))
    End Sub

    Protected Overrides Sub ProcessConfMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessConfMessage(msgtype, msg)

        If msg.ContainsKey("Type") Then
            If msg("Type") = "GroundVehicle" OrElse msg("Type") = "LeggedRobot" OrElse msg("Type") = "NauticVehicle" OrElse msg("Type") = "AerialVehicle" Then

                If msg.ContainsKey("Name") Then
                    If Not (msg("Name").EndsWith(Me.RobotModel)) Then
                        Console.WriteLine(String.Format("{0} received ConfigurationMessage for another robot with type {1}", Me.Name, msg("Name")))
                    End If
                End If

                If msg.ContainsKey("SteeringType") Then
                    'parse when needed
                    'String: “AckermanSteered” or “SkidSteered” or “OmniDrive for "GroundVehicle"
                    'String: “TwoLegs” or “FourLegs for "LeggedRobot"
                    'String: “VariableDepth” for "NauticVehicle"
                    'String: “RotaryWing” for "AerialVehicle"
                End If

                If msg.ContainsKey("Mass") Then
                    Console.WriteLine(String.Format("{0} has mass {1} kg", Me.Name, msg("Mass")))
                    'parse when needed
                    'float: mass in kilograms
                End If

            End If

            If msg("Type") = "GroundVehicle" Then

                If msg.ContainsKey("MaxSpeed") Then
                    Me._MaxSpeed = Single.Parse(msg("MaxSpeed")) 'maximum spin speed of the robot’s wheels, in radians per second
                    Console.WriteLine(String.Format("{0} has a MaxSpeed {1} rad/s", Me.Name, msg("MaxSpeed")))
                End If

                If msg.ContainsKey("MaxTorque") Then
                    'parse when needed
                    'float: maximum torque of the robot, in unreal units
                End If

                If msg.ContainsKey("MaxFrontSteer") Then
                    'parse when needed for AckermanSteered robots
                    'float: maximum steering angle for robot’s front wheels, in radians
                End If

                If msg.ContainsKey("MaxRearSteer") Then
                    'parse when needed for AckermanSteered robots
                    'float: maximum steering angle for robot’s rear wheels, in radians
                End If

            End If

            'Else it is a Sensor Message, handled by MyBase
        End If

    End Sub

    ''' <summary>
    ''' The Agent will have taken care of some of the message parsing already, since 
    ''' it had to anyway in order to figure out to which body the message should
    ''' be forwarded. All that the agent does is extracting all the separate
    ''' curly-brached parts which are supplied here in a stringcollection.
    ''' 
    ''' It is up to the body to further examine the relevant parts and translate
    ''' this into properties relevant for that body
    ''' </summary>
    ''' <param name="msgtype"></param>
    ''' <param name="parts"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub ReceiveGeoMessage(ByVal msgtype As MessageType, ByVal parts As Specialized.StringCollection)
        MyBase.ReceiveGeoMessage(msgtype, parts)
        Me.ProcessGeoMessage(msgtype, Me.ToDictionary(parts))
    End Sub

    Protected Overrides Sub ProcessGeoMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessGeoMessage(msgtype, msg)

        Dim parts() As String
        If msg.ContainsKey("Type") Then
            If msg("Type") = "GroundVehicle" OrElse msg("Type") = "LeggedRobot" OrElse msg("Type") = "NauticVehicle" OrElse msg("Type") = "AerialVehicle" Then

                If msg.ContainsKey("Name") Then
                    If Not (msg("Name").EndsWith(Me.RobotModel)) Then
                        Console.WriteLine(String.Format("{0} received GeometryMessage for another robot with type {1}", Me.Name, msg("Name")))
                    End If
                End If

                If msg.ContainsKey("Dimensions") Then
                    parts = Strings.Split(msg("Dimensions"), ",")
                    Me._Dimensions.X = Double.Parse(parts(0))
                    Me._Dimensions.Y = Double.Parse(parts(1))
                    Me._Dimensions.Z = Double.Parse(parts(2))
                End If

                If msg.ContainsKey("COG") Then
                    parts = Strings.Split(msg("COG"), ",")
                    Me._COG.X = Double.Parse(parts(0))
                    Me._COG.Y = Double.Parse(parts(1))
                    Me._COG.Z = Double.Parse(parts(2))
                End If

                If msg.ContainsKey("WheelRadius") Then
                    Me._WheelRadius = Single.Parse(msg("WheelRadius"))
                End If

                If msg.ContainsKey("WheelSeparation") Then
                    Me._WheelSeparation = Single.Parse(msg("WheelSeparation"))
                End If

                If msg.ContainsKey("WheelBase") Then
                    Me._WheelBase = Single.Parse(msg("WheelBase"))
                End If

            End If

            'Else it is a Sensor Message, handled by MyBase
        End If

    End Sub


    'Protected Sub ProcessGeoMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)

    '    'Handle the Response for a "GETGEO {Type Robot}"-message

    '    'GEO {Type GroundVehicle} {Name string} {Dimensions x,y,z} {COG x,y,z} {WheelRadius float} {WheelSeparation float} {WheelBase float}

    '    'GEO {Type LeggedRobot} {Name string} {Dimensions x,y,z} {COG x,y,z}

    '    'GEO {Type NauticVehicle} {Name string} {Dimensions x,y,z} {COG x,y,z}

    '    'GEO {Type AerialVehicle} {Name string} {Dimensions x,y,z} {COG x,y,z}

    '    'best strategy seems to define an actuator-base with specialisation body and wheels. The body and wheels should be mounted, and the messages handled by the corresponding data classes.

    '    If msgtype = MessageType.Geometry Then
    '        Me.CurrentData.Load(msg)
    '        Me.Agent.NotifySensorUpdate(Me)
    '    End If

    'End Sub

    ''' <summary>
    ''' Assumes the default format for body messages. The message parts
    ''' will be broken down further into key-value pairs using the ToKeyValuePair
    ''' member and subsequently be put in a stringdictionary.
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overrides Function ToDictionary(ByVal msg As Specialized.StringCollection) As Specialized.StringDictionary

        Dim dict As New Specialized.StringDictionary

        Dim pair As KeyValuePair(Of String, String)
        For Each part As String In msg
            pair = Me.ToKeyValuePair(part)
            If Not IsNothing(pair) AndAlso Not String.IsNullOrEmpty(pair.Key) AndAlso Not dict.ContainsKey(pair.Key) Then
                dict.Add(pair.Key, pair.Value)
            End If
        Next

        Return dict
    End Function

    ''' <summary>
    ''' Breaks down a single message part into a key and its corresponding value.
    ''' </summary>
    ''' <param name="part"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overrides Function ToKeyValuePair(ByVal part As String) As KeyValuePair(Of String, String)
        'by default, every part will be processed as follows:
        '- tokenize each part by splitting on spaces
        '- key: the first token 
        '- value: concatenation of all remaining tokens

        Dim parts() As String = Strings.Split(part, " ")
        Dim value As String = ""

        If parts.Length >= 2 Then
            Dim key As String = parts(0)
            If key = "Name" AndAlso parts(parts.Length - 1) = "HARD" Then

                key &= " " & parts(1) ' Use part(1) to make an unique Key for GEO messages

                For i As Integer = 2 To parts.Length - 1 'start at part(2)
                    value &= parts(i) & " "
                Next

            Else

                For i As Integer = 1 To parts.Length - 1
                    value &= parts(i) & " "
                Next
            End If


            Return New KeyValuePair(Of String, String)(key, value.Trim)

        End If

        Return Nothing

    End Function





#End Region



End Class
