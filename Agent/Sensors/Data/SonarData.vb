'SEN {Time 45.14} {Type Sonar} {Name F1 Range 4.4690} {Name F2 Range 1.9387} 

<Serializable()> _
Public Class SonarData
	Implements ISensorData

    Private Const FRONTKEY As String = "F"
    Private Const REARKEY As String = "R"
    Private Const SKEY As String = "S" 'ATRVJr
    Private Const SONARKEY As String = "SONAR" 'Nomad


	Public Sub New()
		Me._front = New Dictionary(Of Integer, Double)
		Me._rear = New Dictionary(Of Integer, Double)
	End Sub

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
        ' Console.WriteLine("[RIKNOTE] Agent\Sensors\Data\SonarData.vb::Load() called")
        For Each key As String In msg.Keys
            If key.ToUpper.StartsWith(FRONTKEY) Then
                ' Console.WriteLine("    [RIKNOTE] Agent\Sensors\Data\SonarData.vb::Load() front key = {0}", CInt(key.Substring(FRONTKEY.Length)))
                Me._front(CInt(key.Substring(FRONTKEY.Length))) = Double.Parse(msg(key))
            ElseIf key.ToUpper.StartsWith(REARKEY) Then
                ' Console.WriteLine("    [RIKNOTE] Agent\Sensors\Data\SonarData.vb::Load(), rear key = {0}", CInt(key.Substring(REARKEY.Length)))
                'fafa comments line below for activation of Nao
                ' Me._rear(CInt(key.Substring(REARKEY.Length))) = Double.Parse(msg(key))
            ElseIf key.ToUpper.StartsWith(SONARKEY) Then
                Dim id As Integer = CInt(key.Substring(SONARKEY.Length))

                ' Console.WriteLine("    [RIKNOTE] Agent\Sensors\Data\SonarData.vb::Load(), sonar key = {0}", CInt(key.Substring(SONARKEY.Length)))

                'The Nomad is rectangular. Sonar4-Sonar0 and Sonar15-Sonar12 are looking forward,
                If id < 5 Then
                    Me._front(5 - CInt(key.Substring(SONARKEY.Length))) = Double.Parse(msg(key)) 'Sonar0 is F5, Sonar4 = F1
                ElseIf id > 11 Then
                    Me._front(21 - CInt(key.Substring(SONARKEY.Length))) = Double.Parse(msg(key)) 'Sonar15 is F6, Sonar12 is F9 (making a nice arc from left to right)
                Else
                    Me._rear(12 - CInt(key.Substring(SONARKEY.Length))) = Double.Parse(msg(key)) 'Sonar11 is R1, Sonar5 is R7
                End If
            ElseIf key.ToUpper.StartsWith(SKEY) Then
                Dim id As Integer = CInt(key.Substring(SKEY.Length))

                ' Console.WriteLine("    [RIKNOTE] Agent\Sensors\Data\SonarData.vb::Load(), s key = {0}", CInt(key.Substring(SKEY.Length)))

                'The ATRVJr is rectangular. S1-S9 and S14-S17 are looking forward, S11 and S12 to the back and S9-S10 & S13-S14 to the sides
                If id < 10 Then
                    Me._front(CInt(key.Substring(SKEY.Length)) + 4) = Double.Parse(msg(key)) 'S1 is F5
                ElseIf id > 13 Then
                    Me._front(CInt(key.Substring(SKEY.Length)) - 13) = Double.Parse(msg(key)) 'S14 is F1 (making a nice arc from left to right)
                Else
                    Me._rear(CInt(key.Substring(SKEY.Length)) - 9) = Double.Parse(msg(key)) 'S10 is R1
                End If


                End If
        Next
    End Sub

	Private _front As Dictionary(Of Integer, Double)
	Public ReadOnly Property Front(ByVal index As Integer) As Double
		Get
			Return Me._front(index)
		End Get
	End Property
	Public ReadOnly Property FrontCount() As Integer
		Get
			Return _front.Count
		End Get
	End Property

	Private _rear As Dictionary(Of Integer, Double)
	Public ReadOnly Property Rear(ByVal index As Integer) As Double
		Get
            Return Me._rear(index)
		End Get
	End Property
	Public ReadOnly Property RearCount() As Integer
		Get
			Return _rear.Count
		End Get
	End Property

End Class
