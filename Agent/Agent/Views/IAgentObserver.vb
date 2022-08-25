Imports UvARescue.Math
Imports System.Drawing

Public Interface IAgentObserver

    Sub NotifySensorUpdate(ByVal sensor As Sensor)
    Sub NotifyPoseEstimateUpdated(ByVal pose As Pose2D)
    Sub NotifyAlertReceived(ByVal alert As String)
    Sub NotifyTrackerImage(ByVal displayImage As Image)

    Sub NotifySignalStrengthReceived(ByVal pathloss As Double)

    Sub NotifyAgentStarted()
    Sub NotifyAgentSpawned()
    Sub NotifyAgentPaused()
    Sub NotifyAgentResumed()
    Sub NotifyAgentStopped()

End Interface
