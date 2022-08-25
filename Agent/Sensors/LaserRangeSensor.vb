Public Enum LaserRangeDeviceType
    SickLMS
    Hokuyo
End Enum

Public Class LaserRangeSensor
    Inherits MultiStateSensor(Of LaserRangeData)

    Public Shared ReadOnly SENSORTYPE_RANGESCANNER As String = "RangeScanner"

    Public ReadOnly DeviceType As LaserRangeDeviceType
    'Public ReadOnly Name As String 'SensorName defined in base
    Public ReadOnly MinRange As Single
    Public ReadOnly MaxRange As Single
    Public OffsetX As Single
    Public OffsetY As Single


    Public Sub New(ByVal name As String, ByVal deviceType As LaserRangeDeviceType, ByVal minRange As Single, ByVal maxRange As Single, ByVal offsetx As Single, ByVal offsety As Single)
        'max history length = 10
        MyBase.New(SENSORTYPE_RANGESCANNER, name, 10)

        Me.DeviceType = deviceType
        Me.MinRange = minRange
        Me.MaxRange = maxRange
        Me.OffsetX = offsetx
        Me.OffsetY = offsety

    End Sub



    Protected Overrides Sub ProcessMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessMessage(msgtype, msg)

        If msgtype = MessageType.Sensor Then

            Dim data As New LaserRangeData(Me.MinRange, Me.MaxRange, Me.OffsetX, Me.OffsetY)
            data.Load(msg)
            Me.EnqueueData(data)

            Me.Agent.NotifySensorUpdate(Me)

        End If

    End Sub

    Public Overrides Sub ProcessGeoMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessGeoMessage(msgtype, msg)
        'GEO {Type string} {Name string Location x,y,z Orientation x,y,z Mount string} {Name string Location x,y,z Orientation x,y,z Mount string} 

        If Not IsNothing(Me.Agent) Then
            'This statement is not always reached, because Me.Agent is not assigned yet
            Console.WriteLine(String.Format("LaserRangeSensor: geometry values for {0} on the {1} updated with GETGEO message", Me.SensorName, Me.Agent.RobotModel))
        Else
            Console.WriteLine("LaserRangeSensor: geometry values updated with GETGEO message")
        End If

        Dim parts() As String

        If msgtype = MessageType.Geometry Then

            'For sensors with multiple instances ANYNAME is used. The names and GEOs of the all instances are returned. Handle the parts

            'Console.WriteLine(String.Format("{0} received GeometryMessage for  sensor with name {1}", Me.SensorName, msg("Name")))


            Dim key As String = "Name "
            key &= Me.SensorName

            parts = Strings.Split(msg(key), " ")
            Dim offsetZ, offsetRoll, offsetPitch, offsetYaw As Single

            If parts.Length = 6 Then
                Dim locations() As String = Strings.Split(parts(1), ",")
                Dim orientations() As String = Strings.Split(parts(3), ",")
                'To be done,  the Mount (If not HARD, the pose of the mount should be queried.

                Me.OffsetX = Single.Parse(locations(0))
                Me.OffsetY = Single.Parse(locations(1))
                offsetZ = Single.Parse(locations(2))

                offsetRoll = Single.Parse(orientations(0))
                offsetPitch = Single.Parse(orientations(1))
                offsetYaw = Single.Parse(orientations(2))
            End If

        End If
    End Sub

End Class
