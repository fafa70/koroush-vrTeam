'SEN {Type Tachometer} {Name TachTest} {Vel 0.9998,0.9998,0.7998,0.7998} {Pos 3.3132,3.3132,6.2160,6.2160}

<Serializable()> _
Public Class TachometerData
    Implements ISensorData

    Public Sub New(ByVal numberOfWheels As Integer)

        Me._wheelAngle = New Dictionary(Of Integer, Double)
        Me._wheelSpeed = New Dictionary(Of Integer, Double)

        For i As Integer = 0 To numberOfWheels - 1
            Me._wheelAngle(i) = 0.0 'radians
            Me._wheelSpeed(i) = 0.0 'radians / second
        Next
    End Sub

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
        Dim parts() As String = Strings.Split(msg("Pos"), ",")

        For i As Integer = 0 To Me.NumberOfWheels - 1

            If i > parts.Length - 1 Then
                Continue For
            End If

            If parts(i) = "-1.#IND" Then
                Console.WriteLine("Tachometer received INF numbers")
            ElseIf parts(i).Length = 0 Then
                Console.WriteLine("Tachometer received empty 'Pos' information")
            Else
                Me._wheelAngle(i) = Double.Parse(parts(i)) 'radians
            End If

        Next

        parts = Strings.Split(msg("Vel"), ",")

        For i As Integer = 0 To Me.NumberOfWheels - 1

            If i > parts.Length - 1 Then
                Continue For
            End If

            If parts(i) = "-1.#IND" Then
                Console.WriteLine("Tachometer received INF numbers")
            ElseIf parts(i).Length = 0 Then
                Console.WriteLine("Tachometer received empty 'Vel' information")
            Else
                Me._wheelSpeed(i) = Double.Parse(parts(i)) 'radians
            End If

        Next
        If msg.ContainsKey("Time") Then
            Me._MeasuredTime = Double.Parse(msg("Time"))
        End If
    End Sub

    Public ReadOnly Property NumberOfWheels() As Integer
        Get
            Return Me._wheelAngle.Count
        End Get
    End Property

    Private _MeasuredTime As Double 'timestamp in seconds
    Public ReadOnly Property MeasuredTime() As Double
        Get
            Return _MeasuredTime
        End Get
    End Property

    Private _wheelAngle As Dictionary(Of Integer, Double)
    Public ReadOnly Property wheelAngle(ByVal index As Integer) As Double 'radians
        Get
            If index > Me._wheelAngle.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[Tachometer]: WARNING 'Angle of Wheel({0}) not known (yet)'", index))
                Return 0.0123
            Else
                Return Me._wheelAngle(index)
            End If
        End Get
        'This protection is needed, because Sensor can be constructed before the Agent
    End Property

    Private _wheelSpeed As Dictionary(Of Integer, Double)
    Public ReadOnly Property wheelSpeed(ByVal index As Integer) As Double 'radians / seconds
        Get
            If index > Me._wheelSpeed.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[Tachometer]: WARNING 'Angular Speed of Wheel({0}) not known (yet)'", index))
                Return 0.0123
            Else
                Return Me._wheelSpeed(index)
            End If
        End Get
    End Property

End Class
