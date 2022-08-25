Public Interface ISensorData

    ''' <summary>
    ''' Sensor data should be able to Load itself from a fully parsed sensor message.
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <remarks></remarks>
    Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary)

End Interface
