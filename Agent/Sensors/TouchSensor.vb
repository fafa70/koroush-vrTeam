Public Class TouchSensor
	Inherits SingleStateSensor(Of TouchData)

    Public Shared ReadOnly SENSORTYPE_TOUCH As String = "Touch"

    Public Sub New()
        MyBase.New(New TouchData, SENSORTYPE_TOUCH, Sensor.MATCH_ANYNAME)
    End Sub

End Class
