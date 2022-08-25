'SEN {Type Encoder} {Name ECLeft Tick 0} {Name ECRight Tick -5}

<Serializable()> _
Public Class EncoderData
    Implements ISensorData

    Public Sub New()

        Me._encoder = New Dictionary(Of String, Integer)

    End Sub

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load

        For Each key As String In msg.Keys
            If key.ToUpper.StartsWith("EC") OrElse key.ToUpper.StartsWith("ENC") Then 'UT3 USARBot.P3AT
                Me._encoder(key.ToLower) = Integer.Parse(msg(key))
            End If
        Next
        'Console.WriteLine(String.Format("[EncoderData:Load] ecleft {0} ecright {1}", Me._encoder("ecleft"), Me._encoder("ecright")))
    End Sub

    Private _encoder As Dictionary(Of String, Integer)
    Public ReadOnly Property Encoder(ByVal encoderName As String) As Integer
        Get
            If Me._encoder.Count = 0 Then
                Return 0
            Else
                Return Me._encoder(encoderName.ToLower)
            End If

        End Get
    End Property

End Class

