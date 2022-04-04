using Newtonsoft.Json.Linq;

namespace TestEurekaWebsockets.Messages
{
    public class PhxReplyMessage : EurekaTrackerMessage
    {
        public string Status { get; init; }
        public override MessageType MessageType => MessageType.PHX_REPLY;

        public PhxReplyMessage(int? trackerNumber, int? sequenceNumber, string target, string status) : base(trackerNumber,sequenceNumber,target)
        {
            this.Status = status;
        }

        internal PhxReplyMessage(RawEurekaTrackerMessage rawMessage) : base(rawMessage)
        {
            this.Status = rawMessage.MessageParams.Value<string>("status");
        }

        protected override JObject SerializeMessageParams()
        {
            return new JObject() {
                { "status", new JValue(Status) }
            };
        }
    }
}
