
'SEN {Type VictRFID} {Name VictRFID} {ID None} {Status None} {Location 3.68,1.96,-0.34} {ID Becky_stpax} {Status None} {Location 2.52,-1.31,-0.46}

'this sensor was used in the RoboCup Rescue 2006 competitions
'in 2007 a new way of working with victims was introduced and this
'sensor became obsolete.

'See also the VictimSensor, which was used in the 2007 competitions

Public Class VictimRfidSensor
    Inherits SingleStateSensor(Of VictimRfidData)

    Public Shared ReadOnly SENSORTYPE_VICTIMRFID As String = "VictRFID"

    Public Sub New()
        MyBase.New(New VictimRfidData, SENSORTYPE_VICTIMRFID, Sensor.MATCH_ANYNAME)
    End Sub

    Private noneCount As Integer

    Protected Overrides Function ToDictionary(ByVal msg As System.Collections.Specialized.StringCollection) As System.Collections.Specialized.StringDictionary
        Me.noneCount = 0
        Return MyBase.ToDictionary(msg)
    End Function

    Private _PreviousKey As String
    Private _PreviousStatus As String

    ''' <summary>
    ''' Specific handling is required since the information for 1 victim 
    ''' is spread over multiple parts and also a single reading can involve
    ''' an arbitrary number of victims.
    ''' </summary>
    ''' <param name="part"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overrides Function ToKeyValuePair(ByVal part As String) As System.Collections.Generic.KeyValuePair(Of String, String)
        Dim parts() As String = Strings.Split(part, " ")
        If parts.Length >= 2 Then
            Dim key As String
            Dim value As String

            If parts(0).ToUpper = "ID" Then
                'just remember the key
                Me._PreviousKey = parts(1).Trim
                Return Nothing

            ElseIf parts(0).ToUpper = "STATUS" Then
                'just remember the key
                value = ""
                For i As Integer = 1 To parts.Length - 1
                    value &= parts(i) & " "
                Next
                Me._PreviousStatus = value.Trim
                Return Nothing

            ElseIf parts(0).ToUpper = "LOCATION" Then
                'get previously cached key and status
                key = Me._PreviousKey
                value = _
                    VictimRfidData.STATUS_KEY & "=" & Me._PreviousStatus & _
                    VictimRfidData.SEPARATOR & _
                    VictimRfidData.LOCATION_KEY & "=" & parts(1).Trim

                If key.ToUpper = VictimRfidData.ID_NONE Then
                    key &= "-" & noneCount
                    noneCount += 1
                End If

                Return New KeyValuePair(Of String, String)(key, value)

            End If

        End If

        Return MyBase.ToKeyValuePair(part)

    End Function

End Class
