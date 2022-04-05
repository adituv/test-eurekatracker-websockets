namespace TestEurekaWebsockets.Messages
{
    public enum MessageType
    {
        UNKNOWN,

        // Connection messages
        PHX_JOIN,
        PHX_REPLY,
        PHX_LEAVE,
        PHX_CLOSE,
        HEARTBEAT,

        // Tracker operation
        SET_KILL_TIME,
        SET_PREPPED,
        SET_INSTANCE_INFORMATION,

        // State synchronisation
        INITIAL_PAYLOAD,
        PAYLOAD,

        // Viewer presence
        PRESENCE_STATE,
        PRESENCE_DIFF,
    }
}
