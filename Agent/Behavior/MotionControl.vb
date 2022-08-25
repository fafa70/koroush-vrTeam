Public Enum MotionType

    Following 'own directory of Fares
    AvoidTeamMate_Following
    AvoidVictim_Following
    ObstacleAvoidance_Following
    RandomWalk_Following
    CorridorWalk_Following
    semiBehaviour
    DeploymentMotion  ' JDHNOTE: introduced German Open 2009
    AvoidTeamMate
    AvoidVictim
    ConservativeTeleOpMotion
    CorridorWalk
    ExploreTerrain
    FollowPathTraversability
    FrontierExploration
    NoMotion
    ObstacleAvoidance   ' RIKNOTE: added for assignment 3 (OA)
    RandomWalk
    Retreat
    WalkCircle
    WalkSquare



End Enum

''' <summary>
''' Motion control is implemented as a finite state machine where each state represents
''' a particular motion. The implementation is such that multiple motions can be active
''' at the same time. 
''' 
''' The idea behind the support for multiple concurrent motions is that different motions
''' could deal with different effectors simultaneously. E.g. an 'explore' motion that deals 
''' with driving, while a 'search victims' motion deals with the camera and a 'leave trail' motion
''' drops an RFID tag every 30 secs.
''' </summary>
''' <remarks></remarks>
Public Class MotionControl
    Private _Motions As New Dictionary(Of MotionType, Motion)


#Region " Constructor "

    Public Sub New(ByVal agent As BehaviorAgent)
        Me._Agent = agent

        'register supported motions
        SyncLock Me._Motions
            ' AVINOTE: lets get alphabetic

            'Me._Motions.Add(MotionType.NoMotion, New NoMotion(Me))
            'Me._Motions.Add(MotionType.FrontierExploration, New FrontierExploration(Me))
            '            Me._Motions.Add(MotionType.ConservativeTeleOpMotion, New ConservativeTeleOpMotion(Me))
            'Me._Motions.Add(MotionType.RandomWalk, New RandomWalk(Me))
            ' RIKNOTE: CorridorWalk was already present
            '            Me._Motions.Add(MotionType.CorridorWalk, New CorridorWalk(Me))
            'Me._Motions.Add(MotionType.Retreat, New Retreat(Me))
            'Me._Motions.Add(MotionType.WalkSquare, New WalkSquare(Me))
            'Me._Motions.Add(MotionType.WalkCircle, New WalkCircle(Me))

            ' RIKNOTE: added for assignment 3 (OA)
            'Me._Motions.Add(MotionType.ObstacleAvoidance, New ObstacleAvoidance(Me))

            '            Me._Motions.Add(MotionType.AvoidTeamMate, New AvoidTeamMate(Me))
            '            Me._Motions.Add(MotionType.AvoidVictim, New AvoidVictim(Me))

            Me._Motions.Add(MotionType.Following, New Following(Me))
            Me._Motions.Add(MotionType.AvoidVictim_Following, New AvoidVictim_Following(Me))
            Me._Motions.Add(MotionType.AvoidTeamMate_Following, New AvoidTeamMate_Following(Me))
            Me._Motions.Add(MotionType.CorridorWalk_Following, New CorridorWalk_Following(Me))
            Me._Motions.Add(MotionType.RandomWalk_Following, New RandomWalk_Following(Me))
            Me._Motions.Add(MotionType.ObstacleAvoidance_Following, New ObstacleAvoidance_Following(Me))
            Me._Motions.Add(MotionType.semiBehaviour, New semiBehaviour(Me))
            Me._Motions.Add(MotionType.AvoidTeamMate, New AvoidTeamMate(Me))
            Me._Motions.Add(MotionType.AvoidVictim, New AvoidVictim(Me))
            Me._Motions.Add(MotionType.ConservativeTeleOpMotion, New ConservativeTeleOpMotion(Me))
            Me._Motions.Add(MotionType.CorridorWalk, New CorridorWalk(Me))
            Me._Motions.Add(MotionType.DeploymentMotion, New DeploymentMotion(Me))
            Me._Motions.Add(MotionType.ExploreTerrain, New ExploreTerrain(Me))
            'fafa actives FollowPathTraversability and change its name to  ExploreTraversibility
            ' Me._Motions.Add(MotionType.FollowPathTraversability, New ExploreTraversibility(Me))
            Me._Motions.Add(MotionType.FrontierExploration, New FrontierExploration(Me))
            Me._Motions.Add(MotionType.NoMotion, New NoMotion(Me))
            Me._Motions.Add(MotionType.ObstacleAvoidance, New ObstacleAvoidance(Me))
            Me._Motions.Add(MotionType.RandomWalk, New RandomWalk(Me))
            Me._Motions.Add(MotionType.Retreat, New Retreat(Me))
            'Me._Motions.Add(MotionType.WalkCircle, New WalkCircle(Me))
            'Me._Motions.Add(MotionType.WalkSquare, New WalkSquare(Me))
        End SyncLock
    End Sub

#End Region

#Region " Properties "

    Private _Agent As BehaviorAgent
    Public ReadOnly Property Agent() As BehaviorAgent
        Get
            Return Me._Agent
        End Get
    End Property

#End Region

#Region " Motion Control "

    ''' <summary>
    ''' Forward sensor update to all currently active motions.
    ''' </summary>
    ''' <param name="sensor"></param>
    ''' <remarks></remarks>
    Protected Friend Overridable Sub NotifySensorUpdate(ByVal sensor As Sensor)
        SyncLock Me._Motions
            SyncLock Me._ActiveMotions
                For Each motion As Motion In Me._ActiveMotions.ToArray 'Arnoud: local copy, to prevent crashes on modifications in underlying NotifyUpdates
                    motion.NotifySensorUpdate(sensor)
                Next
            End SyncLock
        End SyncLock

        If sensor.SensorType = StatusSensor.SENSORTYPE_STATUS Then
            Me.ProcessStatusData(DirectCast(sensor, StatusSensor).CurrentData)
        End If

    End Sub

    Protected Overridable Sub ProcessStatusData(ByVal current_status As StatusData)
        If current_status.Battery < 1 Then
            ' RIKNOTE: fake a "battery empty" situation?
            ActivateMotion(MotionType.NoMotion, True)
        End If
    End Sub

    Protected Friend Overridable Sub NotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        SyncLock Me._Motions
            SyncLock Me._ActiveMotions
                For Each motion As Motion In Me._ActiveMotions.ToArray 'Arnoud: local copy, to prevent crashes on modifications in underlying NotifyUpdates
                    motion.NotifyPoseEstimateUpdated(pose)
                Next
            End SyncLock
        End SyncLock
    End Sub



    Private _ActiveMotions As New List(Of Motion)

    ''' <summary>
    ''' All currently active motions.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    Public ReadOnly Property ActiveMotions() As List(Of Motion)
        Get
            SyncLock Me._Motions
                SyncLock Me._ActiveMotions
                    Return Me._ActiveMotions
                End SyncLock
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Checks if a motion is active.
    ''' </summary>
    ''' <param name="motionType">motion to be checked</param>
    ''' <returns>true when active</returns>
    Public Function IsActiveMotion(ByVal motionType As MotionType) As Boolean
        SyncLock Me._Motions
            If Me._Motions.ContainsKey(motionType) Then
                Dim motion As Motion = Me._Motions(motionType)
                SyncLock Me._ActiveMotions
                    If Me._ActiveMotions.Contains(motion) Then
                        Return True
                    End If
                End SyncLock

            End If

            Return False
        End SyncLock
    End Function

    ''' <summary>
    ''' Invoked to activate a particular motion.
    ''' Use the exclusive parameter to specify if all other active motions
    ''' should be deactivated first.
    ''' </summary>
    ''' <param name="motionType"></param>
    ''' <param name="exclusive"></param>
    Public Sub ActivateMotion(ByVal motionType As MotionType, ByVal exclusive As Boolean)
        If exclusive Then Me.DeActivateAllMotions()
        SyncLock Me._Motions
            Console.WriteLine("[RIKNOTE] Me._Motions.ContainsKey(ObstacleAvoidance) == {0}", Me._Motions.ContainsKey(motionType.ObstacleAvoidance))
            If Me._Motions.ContainsKey(motionType) Then
                Dim motion As Motion = Me._Motions(motionType)
                SyncLock Me._ActiveMotions
                    If Not Me._ActiveMotions.Contains(motion) Then
                        Me._ActiveMotions.Add(motion)
                        motion.Activate()
                    End If
                End SyncLock

            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Invoked to deactivate a particular motion.
    ''' </summary>
    ''' <param name="motionType"></param>
    ''' <remarks></remarks>
    Public Sub DeActivateMotion(ByVal motionType As MotionType)
        SyncLock Me._Motions
            If Me._Motions.ContainsKey(motionType) Then
                Dim motion As Motion = Me._Motions(motionType)
                SyncLock Me._ActiveMotions
                    If Me._ActiveMotions.Contains(motion) Then
                        motion.DeActivate()
                        Me._ActiveMotions.Remove(motion)
                    End If
                End SyncLock

            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Invoked to deactivate all currently active motions in one call.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub DeActivateAllMotions()
        SyncLock Me._Motions
            SyncLock Me._ActiveMotions

                For Each motion As Motion In Me._ActiveMotions
                    motion.DeActivate()
                Next
                Me._ActiveMotions.Clear()
            End SyncLock
        End SyncLock
    End Sub

#End Region

End Class
