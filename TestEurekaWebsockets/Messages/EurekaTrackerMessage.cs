using Newtonsoft.Json.Linq;
using System;

namespace TestEurekaWebsockets.Messages
{
    public abstract class EurekaTrackerMessage
    {
        public int? TrackerNumber { get; }
        public int? SequenceNumber { get; }
        public string Target { get; }
        public abstract MessageType MessageType { get; }

        protected EurekaTrackerMessage(int? trackerNumber, int? sequenceNumber, string target)
        {
            this.TrackerNumber = trackerNumber;
            this.SequenceNumber = sequenceNumber;
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
                    throw new Exception(string.Format("Unknown message type {0}", rawMessage.MessageId));
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
