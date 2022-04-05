using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEurekaWebsockets.Messages
{
    public class UnknownTrackerMessage : EurekaTrackerMessage
    {
        public JObject MessageParams { get; init; }

        public UnknownTrackerMessage(RawEurekaTrackerMessage rawMessage) : base(rawMessage)
        {
            this.MessageType = MessageType.UNKNOWN;
            this.MessageParams = rawMessage.MessageParams;
        }

        public override MessageType MessageType { get; }

        protected override JObject SerializeMessageParams()
        {
            return MessageParams;
        }
    }
}
