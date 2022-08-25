Public Interface ICommConversation

    ReadOnly Property ConversationID() As Integer
    Property ConversationCam() As Boolean
    Property incoming() As Boolean

    Sub StartConversation()
    Sub StopConversation()

    Sub SendText(ByVal text As String)
    Sub SendMessage(ByVal message As Message)

End Interface

