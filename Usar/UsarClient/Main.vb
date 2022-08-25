Imports System.Collections.Specialized
Imports System.IO

Imports UvARescue.Agent
Imports UvARescue.Slam
Imports UvARescue.Tools

Imports UvARescue.Usar.Lib


Module Main

    Public Sub Main(ByVal args() As String)

        Dim parser As New ArgsParser
        With parser
            .AddValueArg("tc", "teamconfig", "the config file with the team and network settings", "")
            .AddValueArg("ac", "agentconfig", "the config file with the agent settings", "")
            .AddValueArg("n", "name", "the name of this agent", "")
        End With

        Dim values As StringDictionary = Nothing
        Dim msgs As StringCollection = Nothing
        If Not parser.Parse(args, values, msgs) Then

            Console.WriteLine("Invalid Arguments, error while parsing")
            For Each msg As String In msgs
                Console.WriteLine("- " & msg)
            Next
            Console.WriteLine()

            parser.PrintHelp(Console.Out)

            Exit Sub
        End If

        If values("n").Equals("") OrElse values("ac").Equals("") OrElse values("tc").Equals("") Then
            Console.WriteLine("Missing argument")
            For Each msg As String In msgs
                Console.WriteLine("- " & msg)
            Next
            Console.WriteLine()

            parser.PrintHelp(Console.Out)

            Exit Sub
        End If
            'args parsed successfully
            Dim teamConfigFile As String = Path.Combine(My.Application.Info.DirectoryPath, values("tc"))
            Dim agentConfigFile As String = Path.Combine(My.Application.Info.DirectoryPath, values("ac"))
            Dim agentName As String = values("n")

            Dim teamConfig As New TeamConfig(teamConfigFile)
            Dim agentConfig As New UsarAgentConfig(agentName)
            agentConfig.Load(agentConfigFile)

            Dim manifold As New Manifold
            Dim agent As Agent.Agent = agentConfig.CreateAgent(manifold, teamConfig, AgentStartMode.FromConsolePrompt)
        '  Dim driver As IDriver = agentConfig.CreateDriver(agent, teamConfig)

        '    driver.Start()

            Dim line As String = Console.ReadLine()
            Select Case line.ToLower
                Case "q", "quit", "exit", "stop"
                'driver.Stop()
                    Quit()
            End Select

    End Sub

    Public Sub Quit()
        Console.WriteLine()
        Console.WriteLine()
        Console.WriteLine("Press [Enter] to Quit")
        Console.ReadLine()
    End Sub

End Module
