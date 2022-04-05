using Newtonsoft.Json.Linq;
using System;

namespace TestEurekaWebsockets.Messages
{
    public abstract class EurekaTrackerMessage
    {
        public virtual int? TrackerNumber { get; }
        public int? SequenceNumber { get; internal set; }
        public string Target { get; }
        public abstract MessageType MessageType { get; }

        protected EurekaTrackerMessage(int? trackerNumber, string target)
        {
            this.TrackerNumber = trackerNumber;
            this.Target = target;
        }

        protected EurekaTrackerMessage(RawEurekaTrackerMessage rawMessage)
        {
            this.TrackerNumber = rawMessage.TrackerNumber;
            this.SequenceNumber = rawMessage.SequenceNumber;
            this.Target = rawMessage.Target;
        }
        public static EurekaTrackerMessage FromRaw(RawEurekaTrackerMessage rawMessage)
        {
            switch (rawMessage.MessageId)
            {
                case "phx_reply":
                    return new PhxReplyMessage(rawMessage);
                default:
                    return new UnknownTrackerMessage(rawMessage);
            }
        }

        public JToken Serialize()
        {
            return new JArray
            {
                new JValue(TrackerNumber?.ToString()),
                new JValue(SequenceNumber?.ToString()),
                new JValue(Target),
                new JValue(MessageType.ToString().ToLowerInvariant()),
                this.SerializeMessageParams()
            };
        }

        protected abstract JObject SerializeMessageParams();
    }
}
