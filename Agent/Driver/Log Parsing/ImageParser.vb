Public Class ImageParser

    Public Shared Function ParseImageData(ByVal receiver As Agent, ByVal img As Byte()) As Boolean
        Dim success As Boolean = False
        If img.Length > 0 Then
            'send img to CameraAgent
            receiver.ReceiveBytes(CameraSensor.SENSORTYPE_CAMERA, CameraSensor.MATCH_ANYNAME, img)
            success = True
        End If
        Return success
    End Function

End Class
