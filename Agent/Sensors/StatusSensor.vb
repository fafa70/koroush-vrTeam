Public Class StatusSensor
    Inherits SingleStateSensor(Of StatusData)

    Public Shared ReadOnly SENSORTYPE_STATUS As String = "STATUS"

    Public Sub New()
        MyBase.New(New StatusData, SENSORTYPE_STATUS, Sensor.MATCH_ANYNAME)
    End Sub

    Protected Overrides Sub ProcessMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessMessage(msgtype, msg)

        If msgtype = MessageType.Status Then 'uses STA messages instead of SEN messages
            Me.CurrentData.Load(msg)
            Me.Agent.NotifySensorUpdate(Me)
        End If

    End Sub

End Class
