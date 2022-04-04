using System;
using Newtonsoft.Json.Linq;

namespace TestEurekaWebsockets.Messages
{
    public class RawEurekaTrackerMessage
    {
        public int? TrackerNumber { get; set; }
        public int? SequenceNumber { get; set; }
        public string Target { get; set; }
        public string MessageId { get; set; }
        public JObject MessageParams { get; set; }

        public static RawEurekaTrackerMessage FromToken(JToken token)
        {
            RawEurekaTrackerMessage result = new RawEurekaTrackerMessage();
            JArray array = token as JArray;

            // TODO: real error handling
            if (array == null)
            {
                // Invalid token - not an array
                throw new Exception("Invalid JToken - not an array");
            }
            if (array.Count != 5)
            {
                // Invalid token - incorrect array length
                throw new Exception("Invalid JToken - incorrect array length");
            }

            result.TrackerNumber = array[0].Type == JTokenType.Null ? null : array[0].Value<int>();
            result.SequenceNumber = array[1].Type == JTokenType.Null ? null : array[1].Value<int>();
            result.Target = array[2].Value<string>();
            result.MessageId = array[3].Value<string>();
            result.MessageParams = array[4] as JObject;

            return result;
        }

        public JToken ToToken()
        {
            return new JArray
            {
                TrackerNumber?.ToString(),
                SequenceNumber?.ToString(),
                Target,
                MessageId,
                MessageParams
            };
        }
    }
}
