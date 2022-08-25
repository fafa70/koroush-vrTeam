''' <summary>
''' A single-state sensor only hold the last (i.e. current) sensor reading.
''' </summary>
''' <typeparam name="TData"></typeparam>
''' <remarks></remarks>
Public Class SingleStateSensor(Of TData As ISensorData)
    Inherits Sensor

    Protected Sub New(ByVal data As TData, ByVal type As String, ByVal name As String)
        MyBase.New(type, name)
        Me._data = data
    End Sub

    Private _data As TData
    Public ReadOnly Property CurrentData() As TData 'Friend goes well for Agent, but not for VictimSensor (used by ManifoldSlam and SkinDetector)
        Get
            Return _data
        End Get
    End Property

    ''' <summary>
    ''' Updates the current data with the data from the msg. 
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub ProcessMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessMessage(msgtype, msg)
        Me.CurrentData.Load(msg)
    End Sub

    Public Overrides Sub ProcessGeoMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessGeoMessage(msgtype, msg)
        'GEO {Type string} {Name string Location x,y,z Orientation x,y,z Mount string} {Name string Location x,y,z Orientation x,y,z Mount string}

    End Sub

End Class
