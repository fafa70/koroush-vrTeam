Public Class InuSensor
    Inherits SingleStateSensor(Of InuData)

    Public Shared ReadOnly SENSORTYPE_INU As String = "INU"

    Public Sub New()
        MyBase.New(New InuData, SENSORTYPE_INU, Sensor.MATCH_ANYNAME)
    End Sub

End Class
