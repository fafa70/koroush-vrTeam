Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Tools

Imports System.Math

Public Enum MappingMode

    'use scanmatcher to estimate poses
    ScanMatching

    'blindly trust sensors
    DeadReckoning

End Enum



Public Enum PoseEstimationSeedMode

    'don not use any initial estimate
    None

    'use the GroundTruth sensor to obtain current pose
    GroundTruth

    'use odometry to compute current pose
    Odometry

    'use INS to obtain current pose
    INS

    'use GPS to obtain current pose
    GPS

    'use Encoder to obtain current pose
    Encoder

End Enum



''' <summary>
''' ManifoldSLAM as inspired by Howard et al.
''' </summary>
''' <remarks></remarks>
Public Class ManifoldSlam
    Implements ISlamStrategy


    'matching thresholds, should be more convervative than extension thresholds
    Protected MATCH_MAXTRANSLATION As Single = 20 'mm
    Protected MATCH_MAXROTATION As Single = 2 'degrees

    'extension thresholds
    Protected EXTEND_MAXTRANSLATION As Single = 200 'mm
    Protected EXTEND_MAXROTATION As Single = 5 'degrees


    Protected Manifold As Manifold
    Protected Matcher As IScanMatcher

    Protected MappingMode As MappingMode
    Protected SeedMode As PoseEstimationSeedMode



#Region " Constructor "

    Public Sub New(ByVal manifold As Manifold, ByVal matcher As IScanMatcher, ByVal mappingMode As MappingMode, ByVal seedMode As PoseEstimationSeedMode)

        If mappingMode = Slam.MappingMode.ScanMatching AndAlso IsNothing(matcher) Then Throw New ArgumentNullException("matcher")

        Me.Manifold = manifold
        Me.Matcher = matcher
        Me.MappingMode = mappingMode
        Me.SeedMode = seedMode

    End Sub

#End Region

#Region " Config "

    Protected Overridable Sub ApplyConfig(ByVal config As Config) Implements ISlamStrategy.ApplyConfig

        Me.Matcher.ApplyConfig(config)

        Me.EXTEND_MAXTRANSLATION = Single.Parse(config.GetConfig("slam", "extend-maxtranslation", CStr(Me.EXTEND_MAXTRANSLATION)))
        Me.EXTEND_MAXROTATION = Single.Parse(config.GetConfig("slam", "extend-maxrotation", CStr(Me.EXTEND_MAXROTATION)))

    End Sub

#End Region

#Region " Receive Sensor notifications from Agent "

    ''' <summary>
    ''' The Agent will forward any sensor updates to Slam through this member
    ''' </summary>
    ''' <param name="sensor"></param>
    ''' <remarks></remarks>
    Public Overridable Sub ProcessSensorUpdate(ByVal sensor As Sensor) Implements ISlamStrategy.ProcessSensorUpdate
        ' RIKNOTE: feed the SLAM with new sensor data
        Select Case sensor.SensorType

            Case LaserRangeSensor.SENSORTYPE_RANGESCANNER
                Me.ProcessLaserRangeData(sensor.SensorName, sensor.Agent, DirectCast(sensor, LaserRangeSensor).PopData)

            Case VictimSensor.SENSORTYPE_VICTIM ' 2007 Competition
                Me.ProcessVictimData(sensor.Agent, DirectCast(sensor, VictimSensor).CurrentData)

            Case OdometrySensor.SENSORTYPE_ODOMETRY
                If sensor.Agent.RobotModel.Equals("AIBO") OrElse sensor.Agent.RobotModel.Equals("P3AT") Then
                    Me.ProcessOdometryData(sensor.Agent, DirectCast(sensor, OdometrySensor).CurrentData)
                End If

                'Case InsSensor.SENSORTYPE_INS
                '    If sensor.Agent.RobotModel.Equals(New String(CType("AirRobot", Char()))) Then
                '        Me.ProcessINSData(sensor.Agent, DirectCast(sensor, InsSensor).CurrentData)
                '    End If

            Case GroundTruthSensor.SENSORTYPE_GROUNDTRUTH
                If sensor.Agent.RobotModel.Equals("AirRobot") Then
                    Me.ProcessINSData(sensor.Agent, DirectCast(sensor, GroundTruthSensor).CurrentData)
                End If

            Case CameraSensor.SENSORTYPE_CAMERA
                Me.ProcessOmnicamData(sensor.Agent, DirectCast(sensor, CameraSensor).PopData)

        End Select
    End Sub

#End Region

#Region " Laser Range Data handling / SLAM "

    Private _OldPose As Pose2D = Nothing
    Protected Overridable Sub ProcessLaserRangeData(ByVal sensorName As String, ByVal agent As Agent.Agent, ByVal laser As LaserRangeData)

        'JDH May 2010 attempt to limit laser range data processing (hoping to eliminate source of delay)

        ' ATTEMPT 1:  Each agent is scheduled an equal portion of time to process laser range data
        'Dim numAgents As Integer = Strings.Split(agent.TeamConfig.TeamMembers, ",").Length
        'Dim processingWindow As Integer = CInt(200 / numAgents)

        'I assume that agent numbers are: Comstation 0; Robot1 1; Robot2 2; etc...
        'Dim bottomRange As Integer = (agent.Number - 1) * processingWindow
        'Dim topRange As Integer = bottomRange + processingWindow
        'If ((CInt(DateTime.Now().Millisecond) Mod 200) < bottomRange Or (CInt(DateTime.Now().Millisecond) Mod 200) >= topRange) Then
        'Console.WriteLine(String.Format("{0} {1} {2}", CInt(DateTime.Now().Millisecond), numAgents, agent.Number))
        'Exit Sub
        'End If

        ' ATTEMPT 2:  For 100ms let agents process laser range data, for next 100ms do not (and let map catch up)
        If ((CInt(DateTime.Now().Millisecond) Mod 200) > 100) Then
            Exit Sub
        End If

        ' Both attempts improve the delay problem, but attempt 2 is much more effective for regular updates.
        'End JDH May 2010 attempt to limit laser range data processing


        'It is possible that a sensor is mounted on a tilt.  Not interested in that here.
        If (sensorName = "TiltedScanner") Then
            Exit Sub
        End If


        'since the sensor data is in meters and the scanmatcher works on millimeters
        'we need to multiply the sensor measurements by a factor 1000
        Dim factor As Single = 1000

        'acquire the current localization of the agent
        Me.Manifold.AcquireReaderLock()
        Dim curPatch As Patch = Me.Manifold.GetCurrentPatch(agent)
        Dim curPose As Pose2D = Me.Manifold.GetCurrentPose(agent)
        Me.Manifold.ReleaseReaderLock()

        If IsNothing(curPose) Then
            'initialize to the agent's starting pose (already converted to millimeters)
            curPose = agent.CurrentPoseEstimate

            Select Case Me.SeedMode
                Case PoseEstimationSeedMode.GroundTruth
                    'acquire global ground-truth estimate
                    If Not IsNothing(agent.GroundTruthPose) Then
                        curPose.X = CType(agent.GroundTruthPose.X * factor, Single)
                        curPose.Y = CType(agent.GroundTruthPose.Y * factor, Single)
                        curPose.Rotation = CType(agent.GroundTruthPose.Yaw, Single)
                    End If

                Case PoseEstimationSeedMode.Odometry
                    'construct global pose from startpose + odometry
                    If Not IsNothing(agent.OdometryPose) Then
                        curPose.X = CType(agent.OdometryPose.X * factor, Single)
                        curPose.Y = CType(agent.OdometryPose.Y * factor, Single)
                        curPose.Rotation = CType(agent.OdometryPose.Rotation, Single)
                    End If
                    'project odometry on the startpose
                    curPose = curPose.ToGlobal(agent.StartPose)

                Case PoseEstimationSeedMode.INS
                    'acquire global INS estimate
                    If Not IsNothing(agent.InsPose) Then
                        curPose.X = CType(agent.InsPose.X * factor, Single)
                        curPose.Y = CType(agent.InsPose.Y * factor, Single)
                        curPose.Rotation = CType(agent.InsPose.Yaw, Single)
                    End If

                Case PoseEstimationSeedMode.GPS
                    'acquire global position estimate
                    If Not IsNothing(agent.GPSSensor) AndAlso agent.GPSSensor.Acquired AndAlso Not IsNothing(agent.GpsPose) Then
                        curPose.X = CType(agent.GpsPose.X * factor, Single)
                        curPose.Y = CType(agent.GpsPose.Y * factor, Single)
                    End If
                    'No rotation estimate from GPS

                    If Not IsNothing(agent.InsPose) Then
                        If Not IsNothing(agent.GPSSensor) AndAlso Not agent.GPSSensor.Acquired Then
                            curPose.X = CType(agent.InsPose.X * factor, Single)
                            curPose.Y = CType(agent.InsPose.Y * factor, Single)
                        End If
                        curPose.Rotation = CType(agent.InsPose.Yaw, Single)
                    End If
                    If Not IsNothing(agent.GroundTruthPose) Then
                        curPose.Rotation = CType(agent.GroundTruthPose.Yaw, Single)
                    End If

                Case PoseEstimationSeedMode.Encoder
                    'acquire global position estimate
                    If Not IsNothing(agent.EncoderPose) Then
                        curPose.X = CType(agent.EncoderPose.X * factor, Single)
                        curPose.Y = CType(agent.EncoderPose.Y * factor, Single)
                        curPose.Rotation = CType(agent.EncoderPose.Rotation, Single)
                    End If

                    If Not IsNothing(agent.OdometryPose) Then
                        If Not IsNothing(agent.EncoderSensor) Then
                            curPose.X = CType(agent.OdometryPose.X * factor, Single)
                            curPose.Y = CType(agent.OdometryPose.Y * factor, Single)
                            curPose.Rotation = CType(agent.OdometryPose.Rotation, Single)
                        End If
                    End If

                    'project odometry on the startpose
                    curPose = curPose.ToGlobal(agent.StartPose)


                Case Else
                    'this case should not be reached, but fail gracefully anyways
                    curPose = agent.CurrentPoseEstimate
            End Select
        End If

        Dim oldTime As DateTime

        If Not IsNothing(curPatch) Then
            oldTime = curPatch.Timestamp 'Get Old Time from Current TimeStamp (MRT)
        Else
            oldTime = Now
        End If

        'create a (possibly temporary) patch at the current pose with the new scan
        Dim newPatch As New Patch(Me.Manifold, New ScanObservation(factor, laser), curPose, agent.Name)
        'Pass oldTime along to newPatch for speed-calculation (MRT)
        'pass Size too, to be able to render a mobilityMap using this (MRT) 


        If IsNothing(curPatch) Then
            Dim bumbing As Boolean = False


            If IsNothing(_OldPose) Then
                'Wait until another sensor estimates the current position
                Select Case Me.SeedMode
                    Case PoseEstimationSeedMode.GroundTruth
                        If Not IsNothing(agent.GroundTruthPose) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.Encoder
                        If Not IsNothing(agent.EncoderPose) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.Odometry
                        If Not IsNothing(agent.OdometryPose) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.INS
                        If Not IsNothing(agent.InsPose) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.GPS
                        'acquire global position estimate
                        If Not IsNothing(agent.GPSSensor) AndAlso agent.GPSSensor.Acquired = False Then
                            _OldPose = curPose
                        End If
                    Case Else
                        'this case should not be reached, but fail gracefully anyways
                        _OldPose = agent.CurrentPoseEstimate
                End Select
                bumbing = True
            Else
                'Wait with the first Patch until the robot is not longer bumbing

                bumbing = bumbing OrElse Abs(curPose.X - _OldPose.X) > MATCH_MAXTRANSLATION
                bumbing = bumbing OrElse Abs(curPose.Y - _OldPose.Y) > MATCH_MAXTRANSLATION

                If bumbing Then
                    ' Console.WriteLine(String.Format("[SLAM] bumbing ({0:f2},{1:f2})->({2:f2},{3:f2})", _OldPose.X, _OldPose.Y, curPose.X, curPose.Y))
                End If

                Dim dradians As Double = (curPose.GetNormalizedRotation - _OldPose.GetNormalizedRotation)
                Dim dangle As Double = dradians / PI * 180
                bumbing = bumbing OrElse Abs(dangle) > MATCH_MAXROTATION

                If bumbing Then
                    ' Console.WriteLine(String.Format("[SLAM] bumbing with angle {0:f2} deg", dangle))
                End If

                'Arnoud: could also check in other dimension of INS or GroundTruth sensor

            End If

            'first incoming patch, initialize manifold and agent's localization
            If bumbing = False Then
                Me.Manifold.AcquireWriterLock()
                Me.Manifold.Extend(newPatch)
                Me.Manifold.LocalizeAgent(agent, newPatch, curPose, agent._SignalStrengthMatrix)
                Me.Manifold.ReleaseWriterLock()
            End If

        Else
            'match current and new scans using the scanmatcher
            Try

                'get an initial pose estimate for the current displacement
                Dim seed As Pose2D = Me.GetInitialPoseEstimate(agent, curPatch, newPatch)

                Dim match As Boolean
                If Me.SeedMode = PoseEstimationSeedMode.None Then
                    'always match
                    match = True
                Else
                    'check if the robot moved at least a certain amount according to the seed
                    Dim dpose As Pose2D = seed.ToGlobal(curPatch.EstimatedOrigin).ToLocal(curPose)
                    match = Me.ExceedsThresholds(dpose, MATCH_MAXTRANSLATION, MATCH_MAXROTATION)
                End If

                If match Then
                    'Console.WriteLine(String.Format("[ProcessLaserRangeData] newPatch with {0} length", newPatch.Scan.Length))
                    'Console.WriteLine(String.Format("[ProcessLaserRangeData] curPatch with {0} length", curPatch.Scan.Length))


                    'get new pose estimate from scan matcher
                    Dim newRelation As Relation = Me.RelatePatches(agent, curPatch, newPatch, seed)
                    Dim newPose As Pose2D = newRelation.OdometryPose.ToGlobal(curPatch.EstimatedOrigin)


                    'extend the manifold when thresholds exceed
                    Dim extend As Boolean = Me.ExceedsThresholds(newRelation.OdometryPose, EXTEND_MAXTRANSLATION, EXTEND_MAXROTATION)

                    'new extension check based on nearest patch instead of current patch
                    Dim useNearest As Boolean = False
                    If useNearest Then

                        'find nearest patch
                        Dim nearestPatch As Patch = Me.Manifold.FindNearestPatch(newPose.Position, EXTEND_MAXTRANSLATION)

                        If IsNothing(nearestPatch) Then
                            extend = True

                        Else

                            'compute pose wrt nearestPatch
                            Dim nearestPose As Pose2D = newPose.ToLocal(nearestPatch.EstimatedOrigin)

                            'determine if should extend based on nearest pose 
                            extend = Me.ExceedsThresholds(nearestPose, EXTEND_MAXTRANSLATION, EXTEND_MAXROTATION)

                        End If

                    End If


                    If extend Then

                        'set the estimated origin of the patch
                        newPatch.EstimatedOrigin = newPose

                        'extend manifold and localize agent to the origin of the new patch
                        Me.Manifold.AcquireWriterLock()
                        Me.Manifold.Extend(newPatch, newRelation)
                        Me.Manifold.LocalizeAgent(agent, newPatch, newPose, agent._SignalStrengthMatrix)
                        Me.Manifold.ReleaseWriterLock()

                    Else
                        'agent is still on the same patch, only update pose estimate
                        Me.Manifold.AcquireWriterLock()
                        Me.Manifold.LocalizeAgent(agent, curPatch, newPose, agent._SignalStrengthMatrix)
                        Me.Manifold.ReleaseWriterLock()

                    End If

                Else

                    'COMMENTED - DON'T LOCALIZE AND AVOID POSE UPDATE BROADCASTING
                    'skipped matching, localize the agent to the initial estimate
                    'Console.WriteLine("[SLAM] Skipped scanmatching")
                    'Dim newPose As Pose2D = seed.ToGlobal(curPatch.EstimatedOrigin)

                    'Me.Manifold.AcquireWriterLock()
                    'Me.Manifold.LocalizeAgent(agent, curPatch, newPose)
                    'Me.Manifold.ReleaseWriterLock()

                End If


            Catch ex As Exception
                Debug.WriteLine(ex.ToString)

            Finally
                'make sure that we release any lock
                Me.Manifold.ReleaseLock()

            End Try

        End If

    End Sub

    Protected Overridable Function GetInitialPoseEstimate(ByVal agent As Agent.Agent, ByVal fromPatch As Patch, ByVal toPatch As Patch) As Pose2D
        Dim seedPose As New Pose2D(0, 0, 0)
        If Me.SeedMode = PoseEstimationSeedMode.None Then
            'return empty seed
            seedPose = New Pose2D(0, 0, 0)

        Else
            'first construct global seedpose from sensor data
            Select Case Me.SeedMode
                Case PoseEstimationSeedMode.GroundTruth
                    'acquire global ground-truth estimate
                    With agent.GroundTruthPose
                        seedPose.X = CType(.X * 1000, Single)
                        seedPose.Y = CType(.Y * 1000, Single)
                        seedPose.Rotation = CType(.Yaw, Single)
                    End With

                Case PoseEstimationSeedMode.Encoder
                    'construct global pose from startpose + odometry
                    With agent.EncoderPose
                        seedPose.X = CType(.X * 1000, Single)
                        seedPose.Y = CType(.Y * 1000, Single)
                        seedPose.Rotation = CType(.Rotation, Single)
                    End With
                    'project encoder-value on the startpose
                    seedPose = seedPose.ToGlobal(agent.StartPose)

                Case PoseEstimationSeedMode.Odometry
                    'construct global pose from startpose + odometry
                    With agent.OdometryPose
                        seedPose.X = CType(.X * 1000, Single)
                        seedPose.Y = CType(.Y * 1000, Single)
                        seedPose.Rotation = CType(.Rotation, Single)
                    End With
                    'project odometry on the startpose
                    seedPose = seedPose.ToGlobal(agent.StartPose)

                Case PoseEstimationSeedMode.INS
                    'acquire global INS estimate
                    With agent.InsPose

                        If Not agent.AgentConfig.RobotModel.ToLower = "element" Then
                        seedPose.X = CType(.X * 1000, Single)
                        seedPose.Y = CType(.Y * 1000, Single)
                        seedPose.Rotation = CType(.Yaw, Single)
                        Else
                            seedPose.X = Me.Manifold.GetCurrentPose(agent).X
                            seedPose.Y = Me.Manifold.GetCurrentPose(agent).Y
                            seedPose.Rotation = CType(.Yaw, Single)
                        End If
                    End With

                Case PoseEstimationSeedMode.GPS
                    'acquire global position estimate
                    If Not IsNothing(agent.GPSSensor) AndAlso agent.GPSSensor.Acquired Then
                        seedPose.X = CType(agent.GpsPose.X * 1000, Single)
                        seedPose.Y = CType(agent.GpsPose.Y * 1000, Single)
                    End If
                    'No rotation estimate from GPS

                    If Not IsNothing(agent.InsPose) Then
                        If Not IsNothing(agent.GPSSensor) AndAlso agent.GPSSensor.Acquired = False Then
                            seedPose.X = CType(agent.InsPose.X * 1000, Single)
                            seedPose.Y = CType(agent.InsPose.Y * 1000, Single)
                        End If
                        seedPose.Rotation = CType(agent.InsPose.Yaw, Single)
                    End If
                    If Not IsNothing(agent.GroundTruthPose) Then
                        seedPose.Rotation = CType(agent.GroundTruthPose.Yaw, Single)
                    End If

                Case Else
                    'this case should not be reached, but fail gracefully anyways
                    seedPose = fromPatch.EstimatedOrigin

            End Select

            'project seedpose to origin of frompatch
            seedPose = seedPose.ToLocal(fromPatch.EstimatedOrigin)

        End If

        Return seedPose

    End Function

    Protected Overridable Function ExceedsThresholds(ByVal dpose As Pose2D, ByVal maxTranslation As Double, ByVal maxRotation As Double) As Boolean

        Dim extend As Boolean = False
        extend = extend OrElse Abs(dpose.X) > maxTranslation
        extend = extend OrElse Abs(dpose.Y) > maxTranslation

        Dim dradians As Double = dpose.GetNormalizedRotation
        Dim dangle As Double = dradians / PI * 180
        extend = extend OrElse Abs(dangle) > maxRotation

        Return extend

    End Function



    Private _Counter As Integer = 0
    Dim _CovarianceThreshold As Single = 10000.0F


    Protected Overridable Function RelatePatches(ByVal agent As Agent.Agent, ByVal fromPatch As Patch, ByVal toPatch As Patch, ByVal seed As Pose2D) As Relation

        Dim estimate As Pose2D
        Dim covariance As Matrix3

        Select Case Me.MappingMode
            Case Slam.MappingMode.ScanMatching
                'use scanmatcher
                Dim match As MatchResult = Me.Matcher.Match(Me.Manifold, fromPatch, toPatch, seed)
                estimate = match.EstimatedOdometryPose
                covariance = match.Covariance

                If Not match.Converged Then
                    estimate = seed
                End If

                If match.NumCorrespondencies = 0 Then
                    estimate = seed
                End If

                If Abs(match.Covariance(0, 0)) > _CovarianceThreshold OrElse Abs(match.Covariance(1, 1)) > _CovarianceThreshold OrElse Abs(match.Covariance(2, 2)) > _CovarianceThreshold Then
                    estimate = seed
                End If

                'update counter 
                Me._Counter += 1


                ' Console.WriteLine(String.Format("[SLAM]-- dpose = {0} ({1} iters / {2} msecs / {3} corr.)", match.EstimatedOdometry.ToString(2, 2), match.NumIterations, match.NumMilliseconds, match.NumCorrespondencies))
                ' Console.WriteLine(String.Format("[SLAM]-- trace = {0:f5} {1:f5} {2:f5}", match.Covariance(0, 0), match.Covariance(1, 1), match.Covariance(2, 2)))

                'debug print
                'With match
                'Debug.WriteLine(String.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}", _
                '    Me._Counter, _
                '    match.NumCorrespondencies, _
                '    match.NumIterations, _
                '    match.NumMilliseconds, _
                '    .Distance, _
                '    .EstimatedOdometry.X, .EstimatedOdometry.Y, .EstimatedOdometry.Rotation, _
                '    .Covariance(0, 0), .Covariance(1, 1), .Covariance(2, 2)))
                'End With



            Case Else
                'DeadReckoning - blindly trust the sensor data
                estimate = seed
                covariance = New Matrix3

        End Select

        Return New Relation(Me.Manifold, fromPatch, toPatch, estimate, covariance)
    End Function

#End Region

#Region " Victim Data Handling "

    Protected Overridable Sub ProcessVictimData(ByVal agent As Agent.Agent, ByVal victim As VictimData)

        If victim.PartCount > 2 Then

            Me.Manifold.AcquireReaderLock()
            Dim curPatch As Patch = Me.Manifold.GetCurrentPatch(agent)
            Dim curPose As Pose2D = Me.Manifold.GetCurrentPose(agent)
            Me.Manifold.ReleaseReaderLock()

            If Not IsNothing(curPatch) Then
                Me.Manifold.AcquireWriterLock()
                curPatch.ProcessVictimData(curPose, victim)
                Me.Manifold.ReleaseWriterLock()
            End If

        End If

    End Sub

    Public Sub ProcessVictimPicture(ByVal agent As Agent.Agent, ByVal position As Vector2, ByVal picture As Drawing.Bitmap, ByVal skinPercentage As Double)

        Me.Manifold.AcquireReaderLock()
        Dim curPatch As Patch = Me.Manifold.GetCurrentPatch(agent)
        Dim curPose As Pose2D = Me.Manifold.GetCurrentPose(agent)
        Me.Manifold.ReleaseReaderLock()

        If Not IsNothing(curPatch) Then
            Me.Manifold.AcquireWriterLock()
            curPatch.ProcessVictimPicture(curPose, position.X, position.Y, picture, skinPercentage)
            Me.Manifold.ReleaseWriterLock()
        End If

    End Sub

#End Region

#Region " INS Data handling for the Airrobot "

    Protected Overridable Sub ProcessINSData(ByVal agent As Agent.Agent, ByVal ins As InsData)

        If IsNothing(agent) Then
            Return
        End If

        'Should check if a LaserRange is mounted
        If agent.IsMounted(LaserRangeSensor.SENSORTYPE_RANGESCANNER) Then
            ' No need to create patches with empty laserrange data when a real data is present
            Return
        End If


        'since the sensor data is in meters and the scanmatcher works on millimeters
        'we need to multiply the sensor measurements by a factor 1000
        'HANNE: I assume the INS does meters
        'Arnoud (May 11, 2009): seems to be milimeters (seems also to be shared code with GetInitialPoseEstimate)
        Dim factor As Single = 1000

        'acquire the current localization of the agent
        Me.Manifold.AcquireReaderLock()
        Dim curPatch As Patch = Me.Manifold.GetCurrentPatch(agent)
        Dim curPose As Pose2D = Me.Manifold.GetCurrentPose(agent)
        Me.Manifold.ReleaseReaderLock()

        If IsNothing(curPose) Then
            'initialize to the agent's starting pose (already converted to millimeters)
            curPose = agent.CurrentPoseEstimate

            Select Case Me.SeedMode
                Case PoseEstimationSeedMode.GroundTruth
                    'acquire global ground-truth estimate
                    If Not IsNothing(agent.GroundTruthPose) Then
                        curPose.X = CType(agent.GroundTruthPose.X * factor, Single)
                        curPose.Y = CType(agent.GroundTruthPose.Y * factor, Single)
                        curPose.Rotation = CType(agent.GroundTruthPose.Yaw, Single)
                    End If

                Case PoseEstimationSeedMode.Odometry
                    'construct global pose from startpose + odometry
                    With agent.OdometryPose
                        curPose.X = CType(.X * factor, Single)
                        curPose.Y = CType(.Y * factor, Single)
                        curPose.Rotation = CType(.Rotation, Single)
                    End With
                    'project odometry on the startpose
                    curPose = curPose.ToGlobal(agent.StartPose)

                Case PoseEstimationSeedMode.INS
                    'acquire global INS estimate
                    With agent.InsPose
                        curPose.X = CType(.X * factor, Single)
                        curPose.Y = CType(.Y * factor, Single)
                        curPose.Rotation = CType(.Yaw, Single)
                    End With

                Case PoseEstimationSeedMode.GPS
                    'acquire global position estimate
                    If Not IsNothing(agent.GpsPose) AndAlso agent.GPSSensor.Acquired Then
                        curPose.X = CType(agent.GpsPose.X * factor, Single)
                        curPose.Y = CType(agent.GpsPose.Y * factor, Single)
                    End If
                    'No rotation estimate from GPS

                    If Not IsNothing(agent.InsPose) Then
                        If Not IsNothing(agent.GPSSensor) AndAlso Not agent.GPSSensor.Acquired Then
                            curPose.X = CType(agent.InsPose.X * factor, Single)
                            curPose.Y = CType(agent.InsPose.Y * factor, Single)
                        End If
                        curPose.Rotation = CType(agent.InsPose.Yaw, Single)
                    End If
                    If Not IsNothing(agent.GroundTruthPose) Then

                        curPose.Rotation = CType(agent.GroundTruthPose.Yaw, Single)
                    End If

                Case PoseEstimationSeedMode.Encoder
                    'acquire global position estimate
                    If Not IsNothing(agent.EncoderSensor) Then
                        curPose.X = CType(agent.EncoderSensor.X * factor, Single)
                        curPose.Y = CType(agent.EncoderSensor.Y * factor, Single)
                        curPose.Rotation = CType(agent.EncoderSensor.Theta, Single)
                    End If

                    If Not IsNothing(agent.OdometryPose) Then
                        If Not IsNothing(agent.EncoderSensor) Then
                            curPose.X = CType(agent.OdometryPose.X * factor, Single)
                            curPose.Y = CType(agent.OdometryPose.Y * factor, Single)
                            curPose.Rotation = CType(agent.OdometryPose.Rotation, Single)
                        End If
                    End If

                    'project odometry on the startpose
                    curPose = curPose.ToGlobal(agent.StartPose)


                Case Else
                    'this case should not be reached, but fail gracefully anyways
                    curPose = agent.CurrentPoseEstimate
            End Select
        End If

        If IsNothing(curPose) Then
            Console.WriteLine("[SLAM] ProcessINSdata: WARNING creating patch with empty LaserRange data without curPose")
        End If

        'create a (possibly temporary) patch at the current pose with the new scan
        Dim laser As LaserRangeData
        laser = New LaserRangeData(0.2, 19.8, 0, 0)
        Dim newPatch As New Patch(Me.Manifold, New ScanObservation(factor, laser), curPose, agent.Name)

        If IsNothing(curPatch) Then
            Dim bumbing As Boolean = False


            If IsNothing(_OldPose) Then
                'Wait until another sensor estimates the current position
                Select Case Me.SeedMode
                    Case PoseEstimationSeedMode.GroundTruth
                        If Not IsNothing(agent.GroundTruthPose) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.Encoder
                        If Not IsNothing(agent.EncoderSensor) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.Odometry
                        If Not IsNothing(agent.OdometryPose) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.INS
                        If Not IsNothing(agent.InsPose) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.GPS
                        'acquire global position estimate
                        If Not IsNothing(agent.GPSSensor) AndAlso agent.GPSSensor.Acquired = False Then
                            _OldPose = curPose
                        End If
                    Case Else
                        'this case should not be reached, but fail gracefully anyways
                        _OldPose = agent.CurrentPoseEstimate
                End Select
                bumbing = True
            Else
                'Wait with the first Patch until the robot is not longer bumbing

                bumbing = bumbing OrElse Abs(curPose.X - _OldPose.X) > MATCH_MAXTRANSLATION
                bumbing = bumbing OrElse Abs(curPose.Y - _OldPose.Y) > MATCH_MAXTRANSLATION

                If bumbing Then
                    ' Console.WriteLine(String.Format("[SLAM] bumbing ({0:f2},{1:f2})->({2:f2},{3:f2})", _OldPose.X, _OldPose.Y, curPose.X, curPose.Y))
                End If

                Dim dradians As Double = (curPose.GetNormalizedRotation - _OldPose.GetNormalizedRotation)
                Dim dangle As Double = dradians / PI * 180
                bumbing = bumbing OrElse Abs(dangle) > MATCH_MAXROTATION

                If bumbing Then
                    ' Console.WriteLine(String.Format("[SLAM] bumbing with angle {0:f2} deg", dangle))
                End If

                'Arnoud: could also check in other dimension of INS or GroundTruth sensor

            End If

            'first incoming patch, initialize manifold and agent's localization
            If bumbing = False Then
                Me.Manifold.AcquireWriterLock()
                Me.Manifold.Extend(newPatch)
                Me.Manifold.LocalizeAgent(agent, newPatch, curPose, agent._SignalStrengthMatrix)
                Me.Manifold.ReleaseWriterLock()
                End If

        Else
            'match current and new scans using the scanmatcher
            Try

                'get an initial pose estimate for the current displacement
                Dim seed As Pose2D = Me.GetInitialPoseEstimate(agent, curPatch, newPatch)

                Dim match As Boolean
                If Me.SeedMode = PoseEstimationSeedMode.None Then
                    'always match
                    match = True
                Else
                    'check if the robot moved at least a certain amount according to the seed
                    Dim dpose As Pose2D = seed.ToGlobal(curPatch.EstimatedOrigin).ToLocal(curPose)
                    match = Me.ExceedsThresholds(dpose, MATCH_MAXTRANSLATION, MATCH_MAXROTATION)
                End If

                If match Then

                    'get new pose estimate from scan matcher
                    Dim newRelation As Relation = Me.RelatePatches(agent, curPatch, newPatch, seed)
                    Dim newPose As Pose2D = newRelation.OdometryPose.ToGlobal(curPatch.EstimatedOrigin)


                    'extend the manifold when thresholds exceed
                    Dim extend As Boolean = Me.ExceedsThresholds(newRelation.OdometryPose, EXTEND_MAXTRANSLATION, EXTEND_MAXROTATION)

                    'new extension check based on nearest patch instead of current patch
                    Dim useNearest As Boolean = False
                    If useNearest Then

                        'find nearest patch
                        Dim nearestPatch As Patch = Me.Manifold.FindNearestPatch(newPose.Position, EXTEND_MAXTRANSLATION)

                        If IsNothing(nearestPatch) Then
                            extend = True

                        Else

                            'compute pose wrt nearestPatch
                            Dim nearestPose As Pose2D = newPose.ToLocal(nearestPatch.EstimatedOrigin)

                            'determine if should extend based on nearest pose 
                            extend = Me.ExceedsThresholds(nearestPose, EXTEND_MAXTRANSLATION, EXTEND_MAXROTATION)

                        End If

                    End If


                    If extend Then

                        'set the estimated origin of the patch
                        newPatch.EstimatedOrigin = newPose

                        'extend manifold and localize agent to the origin of the new patch
                Me.Manifold.AcquireWriterLock()
                        Me.Manifold.Extend(newPatch, newRelation)
                        Me.Manifold.LocalizeAgent(agent, newPatch, newPose, agent._SignalStrengthMatrix)
                        Me.Manifold.ReleaseWriterLock()

                    Else
                        'agent is still on the same patch, only update pose estimate
                        Me.Manifold.AcquireWriterLock()
                        Me.Manifold.LocalizeAgent(agent, curPatch, newPose, agent._SignalStrengthMatrix)
                        Me.Manifold.ReleaseWriterLock()

                    End If

                Else

                    'COMMENTED - DON'T LOCALIZE AND AVOID POSE UPDATE BROADCASTING
                    'skipped matching, localize the agent to the initial estimate
                    'Console.WriteLine("[SLAM] Skipped scanmatching")
                    'Dim newPose As Pose2D = seed.ToGlobal(curPatch.EstimatedOrigin)

                    'Me.Manifold.AcquireWriterLock()
                    'Me.Manifold.LocalizeAgent(agent, curPatch, newPose)
                    'Me.Manifold.ReleaseWriterLock()

                End If


            Catch ex As Exception
                Debug.WriteLine(ex.ToString)

            Finally
                'make sure that we release any lock
                Me.Manifold.ReleaseLock()

            End Try

        End If
    End Sub


#End Region
#Region "Process Odometry for AIBO and P3AT"
    Protected Overridable Sub ProcessOdometryData(ByVal agent As Agent.Agent, ByVal odometry As OdometryData)

        'since the sensor data is in meters and the scanmatcher works on millimeters
        'we need to multiply the sensor measurements by a factor 1000
        'HANNE: I asume the INS does meters
        Dim factor As Single = 1

        'acquire the current localization of the agent
        Me.Manifold.AcquireReaderLock()
        Dim curPatch As Patch = Me.Manifold.GetCurrentPatch(agent)
        Dim curPose As Pose2D = Me.Manifold.GetCurrentPose(agent)
        Me.Manifold.ReleaseReaderLock()

        If IsNothing(curPose) Then
            'initialize to the agent's starting pose (already converted to millimeters)
            curPose = agent.CurrentPoseEstimate

            Select Case Me.SeedMode
                Case PoseEstimationSeedMode.GroundTruth
                    'acquire global ground-truth estimate
                    If Not IsNothing(agent.GroundTruthPose) Then
                        curPose.X = CType(agent.GroundTruthPose.X * factor, Single)
                        curPose.Y = CType(agent.GroundTruthPose.Y * factor, Single)
                        curPose.Rotation = CType(agent.GroundTruthPose.Yaw, Single)
                    End If

                Case PoseEstimationSeedMode.Odometry
                    'construct global pose from startpose + odometry
                    If Not IsNothing(agent.OdometryPose) Then
                        curPose.X = CType(agent.OdometryPose.X * factor, Single)
                        curPose.Y = CType(agent.OdometryPose.Y * factor, Single)
                        curPose.Rotation = CType(agent.OdometryPose.Rotation, Single)
                    End If
                    'project odometry on the startpose
                    curPose = curPose.ToGlobal(agent.StartPose)

                Case PoseEstimationSeedMode.INS
                    'acquire global INS estimate
                    If Not IsNothing(agent.InsPose) Then
                        curPose.X = CType(agent.InsPose.X * factor, Single)
                        curPose.Y = CType(agent.InsPose.Y * factor, Single)
                        curPose.Rotation = CType(agent.InsPose.Yaw, Single)
                    End If

                Case PoseEstimationSeedMode.GPS
                    'acquire global position estimate
                    If Not IsNothing(agent.GPSSensor) AndAlso agent.GPSSensor.Acquired AndAlso Not IsNothing(agent.GpsPose) Then
                        curPose.X = CType(agent.GpsPose.X * factor, Single)
                        curPose.Y = CType(agent.GpsPose.Y * factor, Single)
                    End If
                    'No rotation estimate from GPS

                    If Not IsNothing(agent.InsPose) Then
                        If Not IsNothing(agent.GPSSensor) AndAlso Not agent.GPSSensor.Acquired Then
                            curPose.X = CType(agent.InsPose.X * factor, Single)
                            curPose.Y = CType(agent.InsPose.Y * factor, Single)
                        End If
                        curPose.Rotation = CType(agent.InsPose.Yaw, Single)
                    End If
                    If Not IsNothing(agent.GroundTruthPose) Then

                        curPose.Rotation = CType(agent.GroundTruthPose.Yaw, Single)
                    End If

                Case PoseEstimationSeedMode.Encoder
                    'acquire global position estimate
                    If Not IsNothing(agent.EncoderPose) Then
                        curPose.X = CType(agent.EncoderPose.X * factor, Single)
                        curPose.Y = CType(agent.EncoderPose.Y * factor, Single)
                        curPose.Rotation = CType(agent.EncoderPose.Rotation, Single)
                    End If

                    If Not IsNothing(agent.OdometryPose) Then
                        If Not IsNothing(agent.EncoderSensor) Then
                            curPose.X = CType(agent.OdometryPose.X * factor, Single)
                            curPose.Y = CType(agent.OdometryPose.Y * factor, Single)
                            curPose.Rotation = CType(agent.OdometryPose.Rotation, Single)
                        End If
                    End If

                    'project odometry on the startpose
                    curPose = curPose.ToGlobal(agent.StartPose)


                Case Else
                    'this case should not be reached, but fail gracefully anyways
                    curPose = agent.CurrentPoseEstimate
            End Select
        End If

        'create a (possibly temporary) patch at the current pose with the new scan
        Dim laser As LaserRangeData
        laser = New LaserRangeData(0.2, 19.8, 0, 0)
        Dim newPatch As New Patch(Me.Manifold, New ScanObservation(factor, laser), curPose, agent.Name)

        If IsNothing(curPatch) Then
            Dim bumbing As Boolean = False


            If IsNothing(_OldPose) Then
                'Wait until another sensor estimates the current position
                Select Case Me.SeedMode
                    Case PoseEstimationSeedMode.GroundTruth
                        If Not IsNothing(agent.GroundTruthPose) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.Encoder
                        If Not IsNothing(agent.EncoderPose) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.Odometry
                        If Not IsNothing(agent.OdometryPose) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.INS
                        If Not IsNothing(agent.InsPose) Then
                            _OldPose = curPose
                        End If

                    Case PoseEstimationSeedMode.GPS
                        'acquire global position estimate
                        If Not IsNothing(agent.GPSSensor) AndAlso agent.GPSSensor.Acquired = False Then
                            _OldPose = curPose
                        End If
                    Case Else
                        'this case should not be reached, but fail gracefully anyways
                        _OldPose = agent.CurrentPoseEstimate
                End Select
                bumbing = True
            Else
                'Wait with the first Patch until the robot is not longer bumbing

                bumbing = bumbing OrElse Abs(curPose.X - _OldPose.X) > MATCH_MAXTRANSLATION
                bumbing = bumbing OrElse Abs(curPose.Y - _OldPose.Y) > MATCH_MAXTRANSLATION

                If bumbing Then
                    ' Console.WriteLine(String.Format("[SLAM] bumbing ({0:f2},{1:f2})->({2:f2},{3:f2})", _OldPose.X, _OldPose.Y, curPose.X, curPose.Y))
                End If

                Dim dradians As Double = (curPose.GetNormalizedRotation - _OldPose.GetNormalizedRotation)
                Dim dangle As Double = dradians / PI * 180
                bumbing = bumbing OrElse Abs(dangle) > MATCH_MAXROTATION

                If bumbing Then
                    ' Console.WriteLine(String.Format("[SLAM] bumbing with angle {0:f2} deg", dangle))
                End If

                'Arnoud: could also check in other dimension of INS or GroundTruth sensor

            End If

            'first incoming patch, initialize manifold and agent's localization
            If bumbing = False Then
                Me.Manifold.AcquireWriterLock()
                Me.Manifold.Extend(newPatch)
                Me.Manifold.LocalizeAgent(agent, newPatch, curPose, agent._SignalStrengthMatrix)
                Me.Manifold.ReleaseWriterLock()
            End If

        Else
            'match current and new scans using the scanmatcher
            Try

                'get an initial pose estimate for the current displacement
                Dim seed As Pose2D = Me.GetInitialPoseEstimate(agent, curPatch, newPatch)

                Dim match As Boolean
                If Me.SeedMode = PoseEstimationSeedMode.None Then
                    'always match
                    match = True
                Else
                    'check if the robot moved at least a certain amount according to the seed
                    Dim dpose As Pose2D = seed.ToGlobal(curPatch.EstimatedOrigin).ToLocal(curPose)
                    match = Me.ExceedsThresholds(dpose, MATCH_MAXTRANSLATION, MATCH_MAXROTATION)
                End If

                If match Then

                    'get new pose estimate from scan matcher
                    Dim newRelation As Relation = Me.RelatePatches(agent, curPatch, newPatch, seed)
                    Dim newPose As Pose2D = newRelation.OdometryPose.ToGlobal(curPatch.EstimatedOrigin)


                    'extend the manifold when thresholds exceed
                    Dim extend As Boolean = Me.ExceedsThresholds(newRelation.OdometryPose, EXTEND_MAXTRANSLATION, EXTEND_MAXROTATION)

                    'new extension check based on nearest patch instead of current patch
                    Dim useNearest As Boolean = False
                    If useNearest Then

                        'find nearest patch
                        Dim nearestPatch As Patch = Me.Manifold.FindNearestPatch(newPose.Position, EXTEND_MAXTRANSLATION)

                        If IsNothing(nearestPatch) Then
                            extend = True

                        Else

                            'compute pose wrt nearestPatch
                            Dim nearestPose As Pose2D = newPose.ToLocal(nearestPatch.EstimatedOrigin)

                            'determine if should extend based on nearest pose 
                            extend = Me.ExceedsThresholds(nearestPose, EXTEND_MAXTRANSLATION, EXTEND_MAXROTATION)

                        End If

                    End If


                    If extend Then

                        'set the estimated origin of the patch
                        newPatch.EstimatedOrigin = newPose

                        'extend manifold and localize agent to the origin of the new patch
                        Me.Manifold.AcquireWriterLock()
                        Me.Manifold.Extend(newPatch, newRelation)
                        Me.Manifold.LocalizeAgent(agent, newPatch, newPose, agent._SignalStrengthMatrix)
                        Me.Manifold.ReleaseWriterLock()

                    Else
                        'agent is still on the same patch, only update pose estimate
                        Me.Manifold.AcquireWriterLock()
                        Me.Manifold.LocalizeAgent(agent, curPatch, newPose, agent._SignalStrengthMatrix)
                        Me.Manifold.ReleaseWriterLock()

                    End If

                Else

                    'COMMENTED - DON'T LOCALIZE AND AVOID POSE UPDATE BROADCASTING
                    'skipped matching, localize the agent to the initial estimate
                    'Console.WriteLine("[SLAM] Skipped scanmatching")
                    'Dim newPose As Pose2D = seed.ToGlobal(curPatch.EstimatedOrigin)

                    'Me.Manifold.AcquireWriterLock()
                    'Me.Manifold.LocalizeAgent(agent, curPatch, newPose)
                    'Me.Manifold.ReleaseWriterLock()

                End If


            Catch ex As Exception
                Debug.WriteLine(ex.ToString)

            Finally
                'make sure that we release any lock
                Me.Manifold.ReleaseLock()

            End Try

        End If
    End Sub


#End Region

#Region " Camera Data Handling "

    Protected Overridable Sub ProcessOmnicamData(ByVal agent As Agent.Agent, ByVal camera As CameraData)
        'Me.Manifold.AcquireReaderLock()
        'Dim curPatch As Patch = Me.Manifold.GetCurrentPatch(agent)
        'Dim curPose As Pose2D = agent.CurrentPoseEstimate
        'Me.Manifold.ReleaseReaderLock()

        ''If the current patch is not Nothing and does not already have omnicam data, update the patch
        'If (Not IsNothing(curPatch) AndAlso IsNothing(curPatch.Omnicam)) Then
        '    Me.Manifold.AcquireWriterLock()
        '    curPatch.ProcessCameraData(camera, curPose)
        '    Me.Manifold.ReleaseWriterLock()
        'End If
    End Sub
#End Region
End Class
