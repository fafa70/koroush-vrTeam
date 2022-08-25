Imports System.Collections.Specialized

Public Class ArgsParser

    Private Class ArgInfo

        Public Sub New(ByVal name As String, ByVal description As String, ByVal switch As String, ByVal expectsValue As Boolean, ByVal defaultValue As String)
            Me._Name = name
            Me._Description = description
            Me._Switch = switch
            Me._ExpectsValue = expectsValue
            Me._DefaultValue = defaultValue
        End Sub

        Private _Name As String
        Public ReadOnly Property Name() As String
            Get
                Return Me._Name
            End Get
        End Property

        Private _Description As String
        Public ReadOnly Property Description() As String
            Get
                Return Me._Description
            End Get
        End Property

        Private _Switch As String
        Public ReadOnly Property Switch() As String
            Get
                Return Me._Switch
            End Get
        End Property

        Private _ExpectsValue As Boolean
        Public ReadOnly Property ExpectsValue() As Boolean
            Get
                Return Me._ExpectsValue
            End Get
        End Property

        Private _DefaultValue As String
        Public ReadOnly Property DefaultValue() As String
            Get
                Return Me._DefaultValue
            End Get
        End Property

    End Class


    Private _Args As Dictionary(Of String, ArgInfo)


    Public Sub New()
        Me._Args = New Dictionary(Of String, ArgInfo)
    End Sub

    Public Sub AddValueArg(ByVal switch As String, ByVal name As String, ByVal description As String, ByVal defaultValue As String)
        Me._Args.Add(switch, New ArgInfo(name, description, switch, True, defaultValue))
    End Sub

    Public Sub AddFlagArg(ByVal switch As String, ByVal name As String, ByVal description As String)
        Me._Args.Add(switch, New ArgInfo(name, description, switch, False, Boolean.FalseString))
    End Sub


    Private Enum ParserExpectation
        Switch
        Value
    End Enum

    Public Function Parse(ByVal args() As String, ByRef values As StringDictionary, ByRef msgs As StringCollection) As Boolean

        If IsNothing(values) Then
            values = New StringDictionary
        Else
            values.Clear()
        End If
        If IsNothing(msgs) Then
            msgs = New StringCollection
        Else
            msgs.Clear()
        End If

        'load defaults
        For Each argInfo As ArgInfo In Me._Args.Values
            values.Add(argInfo.Switch, argInfo.DefaultValue)
        Next

        'to keep track of the current parser state
        Dim expectation As ParserExpectation = ParserExpectation.Switch

        'now parse it
        Dim switch As String = ""
        For Each arg As String In args
            Select Case expectation
                Case ParserExpectation.Switch
                    If Not arg.StartsWith("-") Then
                        msgs.Add(String.Format("Expected switch instead of {0}", arg))
                        Exit For
                    Else
                        switch = arg.Substring(1)
                        If String.IsNullOrEmpty(switch) Then
                            msgs.Add(String.Format("Empty switch", switch))
                            Exit For
                        ElseIf Not Me._Args.ContainsKey(switch) Then
                            msgs.Add(String.Format("Unknown switch {0}", switch))
                            Exit For
                        Else
                            If Me._Args(switch).ExpectsValue Then
                                'do nothing yet, value will come in next iteration
                                expectation = ParserExpectation.Value
                            Else
                                'set the flag to true
                                values(switch) = Boolean.TrueString
                                expectation = ParserExpectation.Switch
                            End If
                        End If
                    End If

                Case ParserExpectation.Value
                    'the previous iteration stored the switch
                    values(switch) = arg
                    expectation = ParserExpectation.Switch

            End Select
        Next

        If expectation = ParserExpectation.Value Then
            msgs.Add("Last value is missing")
        End If

        Return msgs.Count = 0

    End Function

    Public Sub PrintHelp(ByVal out As System.IO.TextWriter)
        out.WriteLine("Expected arguments:")
        For Each argInfo As ArgInfo In Me._Args.Values
            out.WriteLine(String.Format("-{0} {1} : {2} - {3}", argInfo.Switch, argInfo.DefaultValue, argInfo.Name, argInfo.Description))
        Next


    End Sub

End Class
