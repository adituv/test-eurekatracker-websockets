using Newtonsoft.Json.Linq;

namespace TestEurekaWebsockets.Messages
{
    public class PhxJoinMessage : EurekaTrackerMessage
    {
        // When joining a tracker, the current sequence number is used as a reference to the instance
        public override int? TrackerNumber => this.SequenceNumber;

        public string Password { get; init; }
        public override MessageType MessageType => MessageType.PHX_JOIN;

        public PhxJoinMessage(string target, string password = null)
            : base(null, target)
        {
            this.Password = password;
        }

        protected PhxJoinMessage(RawEurekaTrackerMessage rawMessage)
            : base(rawMessage)
        {
            this.Password = rawMessage.MessageParams.Value<string>("password");
        }

        protected override JObject SerializeMessageParams()
        {
            var result = new JObject();

            if (Password != null)
            {
                result.Add("password", new JValue(Password));
            }

            return result;
        }
    }
}
