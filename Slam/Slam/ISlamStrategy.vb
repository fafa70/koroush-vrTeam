Imports UvARescue.Agent
Imports UvARescue.Tools

Public Interface ISlamStrategy

    Sub ApplyConfig(ByVal config As Config)

    Sub ProcessSensorUpdate(ByVal sensor As Sensor)

End Interface
