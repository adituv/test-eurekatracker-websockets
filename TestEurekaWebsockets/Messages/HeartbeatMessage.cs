using Newtonsoft.Json.Linq;
namespace TestEurekaWebsockets.Messages
{
    public class HeartbeatMessage : EurekaTrackerMessage
    {
        public HeartbeatMessage(int? sequenceNumber, string target) : base(null, sequenceNumber, target)
        {
        }

        protected HeartbeatMessage(RawEurekaTrackerMessage rawMessage) : base(rawMessage)
        {
        }

        public override MessageType MessageType => MessageType.HEARTBEAT;

        protected override JObject SerializeMessageParams()
        {
            return new JObject();
        }
    }
}
