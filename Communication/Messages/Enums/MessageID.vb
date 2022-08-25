Public Enum MessageID As Byte

    Text

    Greeting
    ActorCommand
    PoseUpdate


    'both agents tell each other who they are
    '- the agent with the lower number will become the Master
    '- the agent with the higher number will become the Slave
    SyncStart '+ agentname

    SyncMaster
    SyncSlave

    SyncCommit

    ' message for camera view request
    CamReq
    CamRepl

    ' message for change of behavior
    BehaviorChange

    ' message for targetlocation from the AirRobot
    NewTargetLocation

    ' message for signal strength to teammates (for comm link layer in usarcommander)
    TeammateSignalStrength

End Enum


