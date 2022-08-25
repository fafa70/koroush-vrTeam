Imports UvARescue.Math

Public Class TachometerSensor
    Inherits SingleStateSensor(Of TachometerData)

    Public Shared ReadOnly SENSORTYPE_TACHOMETER As String = "Tachometer"

    Public Sub New(ByVal numberOfWheels As Integer)
        MyBase.New(New TachometerData(numberOfWheels), SENSORTYPE_TACHOMETER, Sensor.MATCH_ANYNAME)
    End Sub

    Protected Overrides Sub ProcessMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessMessage(msgtype, msg)

        If msgtype = MessageType.Sensor Then
            Me.CurrentData.Load(msg)

            Me.Agent.NotifySensorUpdate(Me)
        End If

    End Sub

    

End Class
