
'SEN {Type VictRFID} {Name VictRFID} {ID int} {Status String} {Location x,y,z}

'STATUS return value represents
' 0: cannot see tag
' 1: can see tag
' 2: can tell if false alarm
' 3: can tell id of victim
' 4: can tell status of victim

Public Enum VictimIdentificationLevel
	CannotSee = 0
	CanSee = 1
	CanSeeFalseAlarm = 2
	CanSeeVictimID = 3
	CanSeeVictimStatus = 4
End Enum


Public Class VictimRfidData
    Implements ISensorData

    Protected Friend Shared ReadOnly STATUS_KEY As String = "STATUS"
    Protected Friend Shared ReadOnly LOCATION_KEY As String = "LOCATION"
    Protected Friend Shared ReadOnly SEPARATOR As String = "/"

    Protected Friend Shared ReadOnly ID_FALSEALARM As String = "FALSEPOSITIVE"
    Protected Friend Shared ReadOnly ID_NONE As String = "NONE"
    Protected Friend Shared ReadOnly STATUS_NONE As String = "NONE"


    Public Sub New()
        Me._Victims = New List(Of VictimRfidTag)
    End Sub

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
        Me._Victims.Clear()
        Dim value As String
        For Each key As String In msg.Keys
            value = msg(key)
            If value.StartsWith(STATUS_KEY) Then
                Me._Victims.Add(New VictimRfidTag(key, value))
            End If
        Next
    End Sub

    Private _Victims As List(Of VictimRfidTag)
    Public ReadOnly Property Victims() As IEnumerable(Of VictimRfidTag)
        Get
            Return _Victims
        End Get
    End Property
    Public ReadOnly Property VictimCount() As Integer
        Get
            Return Me._Victims.Count
        End Get
    End Property



    Public Class VictimRfidTag

        Public Sub New(ByVal key As String, ByVal value As String)
            If key.ToUpper.StartsWith(ID_NONE) Then
                Me._ID = ID_NONE
            Else
                Me._ID = key
            End If

            Dim parts() As String = Strings.Split(value, SEPARATOR)
            Me._Status = parts(0).Substring(parts(0).IndexOf("=") + 1)

            'decision schema for level:
            '- falsealarm      -> we see a false alarm
            '- nostatus + noid -> we see something, maybe false alarm when we get closer
            '- nostatus + id   -> we see a victim 
            '- status + id     -> we see a victim + status
            '
            'Note: to see the status but no id is impossible.

            If Me.ID.ToUpper = ID_FALSEALARM Then
                Me._level = VictimIdentificationLevel.CanSeeFalseAlarm
            ElseIf Me.Status.ToUpper = STATUS_NONE Then
                If Me.ID.ToUpper = ID_NONE Then
                    Me._level = VictimIdentificationLevel.CanSee
                Else
                    Me._level = VictimIdentificationLevel.CanSeeVictimID
                End If
            Else
                Me._level = VictimIdentificationLevel.CanSeeVictimStatus
            End If

            parts = Strings.Split(parts(1).Substring(parts(1).IndexOf("=") + 1), ",")
            Me._X = Double.Parse(parts(0))
            Me._Y = Double.Parse(parts(1))
            Me._Z = Double.Parse(parts(2))

        End Sub

        Private _ID As String
        Public ReadOnly Property ID() As String
            Get
                Return _ID
            End Get
        End Property

        Private _Status As String
        Public ReadOnly Property Status() As String
            Get
                Return _Status
            End Get
        End Property

        Private _level As VictimIdentificationLevel
        Public ReadOnly Property Level() As VictimIdentificationLevel
            Get
                Return _level
            End Get
        End Property

        Private _X As Double
        Public ReadOnly Property X() As Double
            Get
                Return _X
            End Get
        End Property

        Private _Y As Double
        Public ReadOnly Property Y() As Double
            Get
                Return _Y
            End Get
        End Property

        Private _Z As Double
        Public ReadOnly Property Z() As Double
            Get
                Return _Z
            End Get
        End Property

    End Class

End Class
