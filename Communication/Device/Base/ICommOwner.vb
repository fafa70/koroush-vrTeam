Public Interface ICommOwner

    Sub NotifySignalStrengthReceived(ByVal toRobot As String, ByVal pathloss As Double)

    Sub NotifyDNSReceived(ByVal toRobot As String, ByVal port As Integer)

    Sub NotifyConversationStarted(ByVal conversation As ICommConversation, ByVal otherRobot As String)
    Sub NotifyMessageReceived(ByVal conversation As ICommConversation, ByVal message As Message)
    Sub NotifyConversationStopped(ByVal conversation As ICommConversation)

End Interface
