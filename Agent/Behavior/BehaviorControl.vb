Public Enum BehaviorType

    TeleOperation
    ConservativeTeleOp
    AutonomousExploration
    'fafa add new behaviour
    semiAutonomous
    ' JDH RoboCup 2011 Removed behaviours (from GUI only) to reduce risk of wrong choice
    'DriveSquare
    'DriveCircle

    ' RIKNOTE: added for assignment 2 and 3
    'FollowCorridorBehaviorType
    'ObstacleAvoidanceBehaviorType
    'FollowPath
    'ExploreTraversibility

    FollowWaypoint

    ' JDHNOTE: added for Deployment at German Open, testing
    'DeploymentTest
End Enum

''' <summary>
''' Behavior Control manages a finite state machine where each state
''' represent a particular behavior. The implementation is such that only
''' one behavior can be active at a time.
''' </summary>
''' <remarks></remarks>
Public Class BehaviorControl

    Private _Behaviors As New Dictionary(Of BehaviorType, Behavior)


#Region " Constructor "

    Public Sub New(ByVal agent As BehaviorAgent, ByVal motionControl As MotionControl)
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\BehaviorControl.vb::New() called")

        If IsNothing(agent) Then Throw New ArgumentNullException("agent")
        If IsNothing(motionControl) Then Throw New ArgumentNullException("motionControl")

        Me._Agent = agent
        Me._Manifold = agent.Manifold
        Me._MotionControl = motionControl

        ' register all supported behaviors
        Me._Behaviors.Add(BehaviorType.TeleOperation, New TeleOperation(Me))
        Me._Behaviors.Add(BehaviorType.ConservativeTeleOp, New ConservativeTeleOp(Me))
        Me._Behaviors.Add(BehaviorType.AutonomousExploration, New AutonomousExploration(Me))
        Me._Behaviors.Add(BehaviorType.semiAutonomous, New semiAutonomous(Me))
        '   Me._Behaviors.Add(BehaviorType.DriveSquare, New DriveSquare(Me))
        '  Me._Behaviors.Add(BehaviorType.DriveCircle, New DriveCircle(Me))

        ' RIKNOTE: added for assignment 2 and 3
        ' Me._Behaviors.Add(BehaviorType.FollowCorridorBehaviorType, New FollowCorridorBehavior(Me))
        'Me._Behaviors.Add(BehaviorType.ObstacleAvoidanceBehaviorType, New ObstacleAvoidanceBehavior(Me))
        'Me._Behaviors.Add(BehaviorType.FollowPath, New FollowPath(Me))

        Me._Behaviors.Add(BehaviorType.FollowWaypoint, New FollowWaypoint(Me))
        'Me._Behaviors.Add(BehaviorType.DeploymentTest, New DeploymentBehavior(Me))


    End Sub

#End Region

#Region " Properties "

    Private _Agent As BehaviorAgent
    Public ReadOnly Property Agent() As BehaviorAgent
        Get
            Return Me._Agent
        End Get
    End Property

    Private _Manifold As Manifold
    Public ReadOnly Property Manifold() As Manifold
        Get
            Return Me._Manifold
        End Get
    End Property
    Private _imanifold As IManifoldView
    Public ReadOnly Property manifoldView() As IManifoldView

        Get
            Return Me._imanifold
        End Get
    End Property
    Private _MotionControl As MotionControl
    Public ReadOnly Property MotionControl() As MotionControl
        Get
            Return Me._MotionControl
        End Get
    End Property

#End Region

#Region " Motion Control Wrapper Functions "

    Public Sub ActivateMotion(ByVal motionType As MotionType, ByVal exclusive As Boolean)
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\BehaviorControl.vb::ActivateMotion() called, motionType: {0}", motionType)
        Me._MotionControl.ActivateMotion(motionType, exclusive)
    End Sub

    Public Function IsActiveMotion(ByVal motionType As MotionType) As Boolean
        Return Me._MotionControl.IsActiveMotion(motionType)
    End Function

    Public Sub DeActivateMotion(ByVal motionType As MotionType)
        Me._MotionControl.DeActivateMotion(motionType)
    End Sub

    Public Sub DeActivateAllMotions()
        Me._MotionControl.DeActivateAllMotions()
    End Sub
    

#End Region

#Region " Current Behavior Management "

    ''' <summary>
    ''' Forward sensor updates to currently active behavior
    ''' </summary>
    ''' <param name="sensor"></param>
    ''' <remarks></remarks>
    Protected Friend Overridable Sub NotifySensorUpdate(ByVal sensor As Sensor)
        If Not IsNothing(Me._CurrentBehavior) Then
            Me._CurrentBehavior.NotifySensorUpdate(sensor)
        End If
    End Sub

    Protected Friend Overridable Sub NotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        If Not IsNothing(Me._CurrentBehavior) Then
            Me._CurrentBehavior.NotifyPoseEstimateUpdated(pose)
        End If
    End Sub



    ''' <summary>
    ''' Invoked when the direct or indirect connection to the operator was 
    ''' (re-)established. 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Friend Overridable Sub NotifyOperatorConnected()
        If Not IsNothing(Me._CurrentBehavior) Then
            Me._CurrentBehavior.NotifyOperatorConnected()
        End If
    End Sub

    ''' <summary>
    ''' Invoked when the direct or indirect connection to the operator was 
    ''' lost.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Friend Overridable Sub NotifyOperatorDisconnected()
        If Not IsNothing(Me._CurrentBehavior) Then
            Me._CurrentBehavior.NotifyOperatorDisconnected()
        End If
    End Sub

    ''' <summary>
    ''' Invoked for every direct connection with another robot or operator that
    ''' was (re-)established.
    ''' </summary>
    ''' <param name="agentName"></param>
    ''' <remarks></remarks>
    Protected Friend Overridable Sub NotifyAgentConnected(ByVal agentName As String)
        If Not IsNothing(Me._CurrentBehavior) Then
            Me._CurrentBehavior.NotifyAgentConnected(agentName)
        End If
    End Sub

    ''' <summary>
    ''' Invoked for every direct connection with another robot or operator that
    ''' was lost.
    ''' </summary>
    ''' <param name="agentName"></param>
    ''' <remarks></remarks>
    Protected Friend Overridable Sub NotifyAgentDisconnected(ByVal agentName As String)
        If Not IsNothing(Me._CurrentBehavior) Then
            Me._CurrentBehavior.NotifyAgentDisconnected(agentName)
        End If
    End Sub


    Private _CurrentBehavior As Behavior

    ''' <summary>
    ''' Only one behavior can be active at a time: the 'current' behavior.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property CurrentBehavior() As Behavior
        Get
            Return Me._CurrentBehavior
        End Get
    End Property

    ''' <summary>
    ''' Invoked to change the current behavior.
    ''' </summary>
    ''' <param name="behaviorType"></param>
    ''' <remarks></remarks>
    Public Sub SwitchToBehavior(ByVal behaviorType As BehaviorType)
        Console.WriteLine("[RIKNOTE] Agent\Behavior\BehaviorControl.vb::SwitchToBehavior() called")
        If Me._Behaviors.ContainsKey(behaviorType) Then
            Console.WriteLine("Behaviour exists")

            'get new behavior
            Dim behavior As Behavior = Me._Behaviors(behaviorType)
            If Not behavior Is Me._CurrentBehavior Then

                'deactive current behavior
                If Not IsNothing(Me._CurrentBehavior) Then
                    Me._CurrentBehavior.DeActivate()
                    Console.WriteLine("i deactivate it")
                Else
                    Console.WriteLine("it is same")
                End If

                Me._CurrentBehavior = behavior

                'activate new behavior
                If Not IsNothing(behavior) Then
                    Console.WriteLine("    [RIKNOTE] Agent\Behavior\BehaviorControl.vb::SwitchToBehavior() calling behavior.Activate()")
                    behavior.Activate()
                End If

            End If
        End If
    End Sub

#End Region

End Class
