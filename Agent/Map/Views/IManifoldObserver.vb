Public Interface IManifoldObserver

    Sub NotifyPatchInserted(ByVal patch As Patch)
    Sub NotifyRelationInserted(ByVal relation As Relation)

    Sub NotifyAgentMoved(ByVal agent As Agent, ByVal pose As Math.Pose2D, ByVal commMatrix As Double(,))

    Sub NotifyVictimUpdated(ByVal victim As VictimObservation)

    'Sub NotifyOmnicamUpdated(ByVal patch As Patch)

    Sub NotifyCleared()

End Interface
