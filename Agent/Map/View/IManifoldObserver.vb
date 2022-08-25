Public Interface IManifoldObserver

    Sub NotifyPatchInserted(ByVal patch As Patch)
    Sub NotifyRelationInserted(ByVal relation As Relation)

    Sub NotifyAgentMoved(ByVal agent As Agent, ByVal pose As Math.Pose2D)

    Sub NotifyVictimUpdated(ByVal victim As VictimObservation)

    Sub NotifyCleared()

End Interface
