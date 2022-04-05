using Newtonsoft.Json.Linq;
namespace TestEurekaWebsockets.Messages
{
    public class HeartbeatMessage : EurekaTrackerMessage
    {
        public HeartbeatMessage(string target) : base(null, target)
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
