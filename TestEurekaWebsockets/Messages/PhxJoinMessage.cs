using Newtonsoft.Json.Linq;

namespace TestEurekaWebsockets.Messages
{
    public class PhxJoinMessage : EurekaTrackerMessage
    {
        public string Password { get; init; }
        public override MessageType MessageType => MessageType.PHX_JOIN;

        public PhxJoinMessage(int trackerNumber, int sequenceNumber, string target, string password = null)
            : base(trackerNumber, sequenceNumber, target)
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
