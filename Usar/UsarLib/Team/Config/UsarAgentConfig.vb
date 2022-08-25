Imports UvARescue.Agent
Imports UvARescue.ImageAnalysis
Imports UvARescue.Slam

Public Enum AgentStartMode
    FromCommanderGUI
    FromConsolePrompt
End Enum


Public Class UsarAgentConfig
    Inherits AgentConfig

    Public Sub New(ByVal agentName As String)
        MyBase.New(agentName)
    End Sub

    Public Function CreateAgent(ByVal manifold As Manifold, ByVal teamConfig As TeamConfig, ByVal startMode As AgentStartMode) As Agent.Agent

        Dim agent As Agent.Agent

        'Dim mnf As New Manifold 'this line is used to debug Comm, then each agent will have it's own manifold
        Dim mnf As Manifold = manifold 'this is as it should be

        'ignore SpawnFromCommander setting when the agent is started from the console
        If startMode = AgentStartMode.FromConsolePrompt OrElse Me.SpawnFromCommander Then
            'this agent should be spawned from the commander, so instantiate
            'a 'true' agent 

            'figure out which agent ...
            Select Case Me.AgentType

                Case "operator"
                    agent = New UsarOperatorAgent(manifold, Me.AgentName, Me, teamConfig)

                Case "slam", "skin"
                    Dim matcher As IScanMatcher
                    Select Case Me.ScanMatcher

                        Case "IDC"
                            matcher = New IdcScanMatcher
                        Case "WSM"
                            matcher = New WeightedScanMatcher
                        Case "MbICP"
                            matcher = New MbIcpScanMatcher
                        Case "ICP"
                            matcher = New IcpScanMatcher
                        Case "MultiICP"
                            matcher = New MultiIcpScanMatcher
                        Case "QuadIDC"
                            matcher = New QuadIdcScanMatcher
                        Case "QuadWSM"
                            matcher = New QuadWeightedScanMatcher
                        Case "QuadICP"
                            matcher = New QuadIcpScanMatcher

                        Case Else
                            matcher = New WeightedScanMatcher

                    End Select


                    Dim mmode As MappingMode = UvARescue.Slam.MappingMode.ScanMatching
                    Select Case Me.MappingMode
                        Case UvARescue.Slam.MappingMode.DeadReckoning.ToString
                            mmode = UvARescue.Slam.MappingMode.DeadReckoning
                    End Select

                    Dim smode As PoseEstimationSeedMode = PoseEstimationSeedMode.None
                    Select Case Me.SeedMode
                        Case PoseEstimationSeedMode.GroundTruth.ToString
                            'only enable using groundtruth in debug mode!
                            smode = PoseEstimationSeedMode.GroundTruth
                        Case PoseEstimationSeedMode.INS.ToString
                            smode = PoseEstimationSeedMode.INS
                        Case PoseEstimationSeedMode.Odometry.ToString
                            smode = PoseEstimationSeedMode.Odometry
                        Case PoseEstimationSeedMode.GPS.ToString
                            smode = PoseEstimationSeedMode.GPS
                        Case PoseEstimationSeedMode.Encoder.ToString
                            smode = PoseEstimationSeedMode.Encoder
                    End Select

                    Dim slam As ISlamStrategy

#If Not COMPETITION_MODE Then

                    If Me.UseNoise Then
                        slam = New NoisedSlam(mnf, matcher, mmode, smode, Me.NoiseSigma)
                    Else
                        slam = New ManifoldSlam(mnf, matcher, mmode, smode)
                    End If
#Else

                    'to make sure we run the competition runs with optimal setting
                    Console.WriteLine("[NOTE] - FORCED SLAM TO OPTIMAL SETTINGS:")
                    Console.WriteLine("[NOTE] - > INITIAL POSE ESTIMATES FROM INS")
                    Console.WriteLine("[NOTE] - > SCANMATCHER IS FAST-WSM")
                    Console.WriteLine("[NOTE] - > NO ADDITIONAL GAUSSIAN NOISE")

                    slam = New ManifoldSlam(mnf, New WeightedScanMatcher, UvARescue.Slam.MappingMode.ScanMatching, PoseEstimationSeedMode.INS)

#End If


                    If Me.AgentType = "skin" Then

                        Dim skinDet As IImageAnalysis
                        Select Case Me.SkinDetectorMode
                            Case "Detection"
                                skinDet = New SkinDetector(DirectCast(slam, ManifoldSlam), CInt(Me.DetectorTheta))
                            Case Else
                                skinDet = New SkinDetectorTeacher(False, Me.TeacherMode)
                        End Select

                        agent = New UsarSkinDetAgent(slam, mnf, skinDet, Me.AgentName, Me, teamConfig)

                    Else
                        agent = New UsarSlamAgent(slam, mnf, Me.AgentName, Me, teamConfig)
                    End If


                Case "follow"
                    agent = New UsarFollowAgent(mnf, Me.AgentName, Me, teamConfig)
                    Console.WriteLine("follow is choosen")
                Case Else
                    agent = New UsarAgent(mnf, Me.AgentName, Me, teamConfig)

            End Select

        Else
            'this agent is not spawned from the commander, i.e.: it is spawned remotely
            'the commander should start a proxy that waits for messages 
            'comming in through the WSS

            agent = New UsarProxyAgent(mnf, Me.AgentName, Me, teamConfig)

        End If

        Return agent

    End Function

    Public Function CreateDriver(ByVal agent As Agent.Agent, ByVal teamConfig As TeamConfig) As IDriver
        Dim driver As IDriver
        If Me.LogPlayback Then
            Dim handler As LogDriver.ParseDelegate = AddressOf LineParsers.ParseUsarSimLine
            Select Case Me.LogFileFormat
                Case "Player"
                    handler = AddressOf LineParsers.ParsePlayerLine
                Case "Carmen"
                    handler = AddressOf LineParsers.ParseCarmenLine
                Case "Cogniron"
                    handler = AddressOf LineParsers.ParseCognironLine
                Case "Matlab"
                    handler = AddressOf LineParsers.ParseMatlabDataLine
                Case "NomadWithHokuyo"
                    handler = AddressOf LineParsers.ParseNomadHokuyoLine
                Case "Longwood"
                    handler = AddressOf LineParsers.ParseLongwoodLine
            End Select

            driver = New LogDriver(agent, Me.LogFile, handler)

        Else
            If TypeOf agent Is UsarProxyAgent Then
                driver = New LiveProxyDriver(agent, teamConfig, Me.UseImageServer, Me.UseLogger)
            Else
                driver = New LiveDriver(agent, teamConfig, Me.UseImageServer, Me.UseLogger)

            End If

        End If
        Return driver
    End Function

End Class
