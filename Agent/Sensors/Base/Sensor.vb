Imports System.Threading
Imports System.Globalization

''' <summary>
''' Base class for sensors.
''' 
''' A sensor is identified by its Type and Name. Some sensors only want (need)
''' to specify a type and want to receive reading sent for a sensor with any name
''' as long as the type matches, in that case use the constant MATCH_ANYNAME as 
''' the sensor's name.
''' 
''' Tha base class provides standard message handling as well as several 
''' convenient utility functions.
''' 
''' </summary>
''' <remarks></remarks>

Public MustInherit Class Sensor

    Friend Const MATCH_ANYNAME As String = "*NAME*"

    Protected Const STATUS_KEY As String = "Status"
    Protected Const STATUS_OK As String = "OK"
    Protected Const STATUS_FAIL As String = "FAILED"

#Region " Constructor "

    Protected Sub New(ByVal type As String, ByVal name As String)
        MyBase.New()
        Me._SensorType = type
        Me._SensorName = name

        If Not CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = NumberFormatInfo.InvariantInfo.NumberDecimalSeparator Then
            'This prevents BufferedLayer overflow, due to huge laser ranges if ',' and '.' are interchanged
            Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US", False)
            Console.WriteLine("[Sensor]: CurrentCulture is now {0}.", CultureInfo.CurrentCulture.Name)
        End If
    End Sub

#End Region

#Region " Properties "

    Private _SensorType As String
    Public ReadOnly Property SensorType() As String
        Get
            Return _SensorType
        End Get
    End Property

    Private _SensorName As String
    Public ReadOnly Property SensorName() As String
        Get
            Return _SensorName
        End Get
    End Property

    Private _Agent As Agent
    Public ReadOnly Property Agent() As Agent
        Get
            Return _Agent
        End Get
    End Property
    Protected Friend Sub SetAgent(ByVal agent As Agent)
        If Not agent Is Me._Agent Then
            Me._Agent = agent
            Me.OnAgentChanged()
        End If
    End Sub

    Protected Overridable Sub OnAgentChanged()

    End Sub

#End Region

#Region " Message Handling "

    ''' <summary>
    ''' The Agent will have taken care of some of the message parsing already, since 
    ''' it had to anyway in order to figure out to which sensor the message should
    ''' be forwarded. All that the agent does is extracting all the separate
    ''' curly-brached parts which are supplied here in a stringcollection.
    ''' 
    ''' It is up to the sensor to further examine the relevant parts and translate
    ''' this into a sensor reading. Since most sensors follow the same convention
    ''' within the curly braced to convey particular information this base class
    ''' will further process the strings assuming that format by default. All 
    ''' involved members are overridable so subclasses can take as much control
    ''' as desired/necessary.
    ''' </summary>
    ''' <param name="msgtype"></param>
    ''' <param name="parts"></param>
    ''' <remarks></remarks>
    Public Sub ReceiveMessage(ByVal msgtype As MessageType, ByVal parts As Specialized.StringCollection)
        Me.ProcessMessage(msgtype, Me.ToDictionary(parts))
    End Sub

    ''' <summary>
    ''' Some sensors, e.g. CameraSensor and WSS, communicate using bytes.
    ''' </summary>
    ''' <param name="bytes"></param>
    ''' <remarks></remarks>
    Public Sub ReceiveBytes(ByVal bytes() As Byte)
        Me.ProcessBytes(bytes)
    End Sub

    ''' <summary>
    ''' Assumes the default format for sensor messages. The message parts
    ''' will be broken down further into key-value pairs using the ToKeyValuePair
    ''' member and subsequently be put in a stringdictionary.
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overridable Function ToDictionary(ByVal msg As Specialized.StringCollection) As Specialized.StringDictionary
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
    Protected Overridable Function ToKeyValuePair(ByVal part As String) As KeyValuePair(Of String, String)
        'by default, every part will be processed as follows:
        '- tokenize each part by splitting on spaces
        '- key: the first token 
        '- value: concatenation of all remaining tokens

        Dim parts() As String = Strings.Split(part, " ")
        If parts.Length >= 2 Then
            Dim key As String = parts(0)
            Dim value As String = ""
            For i As Integer = 1 To parts.Length - 1
                value &= parts(i) & " "
            Next

            Return New KeyValuePair(Of String, String)(key, value.Trim)

        End If

        Return Nothing

    End Function


    Protected Overridable Sub ProcessMessage(ByVal msgtype As MessageType, ByVal msg As Specialized.StringDictionary)
        If msgtype = MessageType.Status Then 'uses STA messages instead of SEN messages
            'Console.WriteLine("Receiving a status message")
        End If
    End Sub

    Protected Overridable Sub ProcessBytes(ByVal bytes() As Byte)
    End Sub

    Public Overridable Sub ProcessGeoMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        'GEO {Type string} {Name string Location x,y,z Orientation x,y,z Mount string} {Name string Location x,y,z Orientation x,y,z Mount string}
        If msgtype = MessageType.Geometry Then

            If msg.ContainsKey("Type") Then
                If Not (Me.SensorType = msg("Type")) Then
                    Console.WriteLine(String.Format("{0} received GeometryMessage for another sensor with type {1}", Me.SensorType, msg("Type")))
                End If
            End If
        End If

    End Sub

    Public Overridable Sub ProcessConfMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        'CONF {Type string} {Name string}
        If msgtype = MessageType.Configuration Then

            If msg.ContainsKey("Type") Then
                If Not (Me.SensorType = msg("Type")) Then
                    Console.WriteLine(String.Format("{0} received ConfigurationMessage for another sensor with type {1}", Me.SensorType, msg("Type")))
                End If
            End If
        End If

    End Sub

#End Region

#Region " Some Convenience Functions "

    'in case of a response message, to check if success or failure is reported

    Protected Function IsStatusOK(ByVal msg As String) As Boolean
        Return msg.ToUpper = Sensor.STATUS_OK
    End Function
    Protected Function IsStatusOK(ByVal msg As Specialized.StringDictionary) As Boolean
        Return Me.IsStatusOK(msg(Sensor.STATUS_KEY))
    End Function

    Protected Function IsStatusFAIL(ByVal msg As String) As Boolean
        Return msg.ToUpper = Sensor.STATUS_FAIL
    End Function
    Protected Function IsStatusFAIL(ByVal msg As Specialized.StringDictionary) As Boolean
        Return Me.IsStatusFAIL(msg(Sensor.STATUS_KEY))
    End Function

#End Region
#Region "Sending Commands to UsarSim"

    ''' <summary>
    ''' To send a command to the server using the standard OPCODE convention.
    ''' </summary>
    ''' <param name="opCode"></param>
    ''' <remarks></remarks>
    Protected Sub SendSetCommand(ByVal opCode As String)
        If IsNothing(Me.Agent) Then Throw New InvalidOperationException("Sensor needs to be mounted on an Agent first")

        Dim cmd As New Text.StringBuilder
        cmd.Append("SET ")
        cmd.AppendFormat("{{Type {0}}} ", Me.SensorType)
        cmd.AppendFormat("{{Name {0}}} ", Me.SensorName)
        cmd.AppendFormat("{{Opcode {0}}}", opCode)
        cmd.Append(Environment.NewLine)

        Me.Agent.SendUsarSimCommand(cmd.ToString)
    End Sub

    ''' <summary>
    ''' To request information about the current sensor's configuration in the simulator.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SendConfigurationRequest()
        If IsNothing(Me.Agent) Then Throw New InvalidOperationException("Sensor needs to be mounted on an Agent first")

        Dim cmd As New Text.StringBuilder
        cmd.Append("GETCONF ")
        cmd.AppendFormat("{{Type {0}}}", Me.SensorType)
        If Not Me.SensorName = Sensor.MATCH_ANYNAME Then
            cmd.AppendFormat(" {{Name {0}}}", Me.SensorName) 'note the space at the beginning
        Else
            cmd.AppendFormat(" {{Name {0}}}", "")
        End If
        cmd.Append(Environment.NewLine)
        'Console.WriteLine(String.Format("    [AVINOTE] Sensor.vb::SendConfigurationRequest sends UsarSimCommand {0}", cmd.ToString))


        Me.Agent.SendUsarSimCommand(cmd.ToString)
    End Sub

    Public Sub SendGeometryRequest()
        If IsNothing(Me.Agent) Then Throw New InvalidOperationException("Sensor needs to be mounted on an Agent first")

        Dim cmd As New Text.StringBuilder
        cmd.Append("GETGEO ")
        cmd.AppendFormat("{{Type {0}}}", Me.SensorType)
        If Not Me.SensorName = Sensor.MATCH_ANYNAME Then
            cmd.AppendFormat(" {{Name {0}}}", Me.SensorName) 'note the space at the beginning
        Else
            cmd.AppendFormat(" {{Name {0}}}", "")
        End If
        cmd.Append(Environment.NewLine)
        'Console.WriteLine(String.Format("    [AVINOTE] Sensor.vb::SendGeometryRequest sends UsarSimCommand {0}", cmd.ToString))


        Me.Agent.SendUsarSimCommand(cmd.ToString)
    End Sub

#End Region

End Class
