
'SEN {Type VictSensor} {Name VictSensorName} {PartName "[Head|Arm|Hand|Chest|Pelvis|Leg|Foot]"} {Location x,y,z} {PartName String} {Location x,y,z} 

Public Class VictimSensor
    Inherits SingleStateSensor(Of VictimData)

    Public Shared ReadOnly SENSORTYPE_VICTIM As String = "VictSensor"

    Public Sub New()
        MyBase.New(New VictimData, SENSORTYPE_VICTIM, Sensor.MATCH_ANYNAME)
    End Sub

    Private _PreviousKey As String

    ''' <summary>
    ''' Specific handling is required since the information for 1 victim 
    ''' is spread over multiple parts and also a single reading can involve
    ''' an arbitrary number of victim parts.
    ''' </summary>
    ''' <param name="part"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overrides Function ToKeyValuePair(ByVal part As String) As System.Collections.Generic.KeyValuePair(Of String, String)
        Dim parts() As String = Strings.Split(part, " ")
        If parts.Length >= 2 Then
            Dim key As String
            Dim value As String

            If parts(0).ToUpper = "PARTNAME" Then
                'just remember the key
                Me._PreviousKey = parts(1)
                Return Nothing

            ElseIf parts(0).ToUpper = "LOCATION" Then
                'get previously cached key
                key = Me._PreviousKey
                value = parts(1).Trim
                Return New KeyValuePair(Of String, String)(key, value)

            End If

        End If

        Return MyBase.ToKeyValuePair(part)

    End Function

    Protected Overrides Sub ProcessMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessMessage(msgtype, msg)

        If msgtype = MessageType.Sensor Then
            Me.CurrentData.Load(msg)
            Me.Agent.NotifySensorUpdate(Me)
        End If

    End Sub

End Class
