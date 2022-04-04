using NUnit.Framework;
using Newtonsoft.Json.Linq;
using TestEurekaWebsockets.Messages;
using System;

namespace TestEurekaWebsocketsTests
{
    public class RawEurekaTrackerMessageTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ToTokenReturnsExpectedResult()
        {
            var sampleMessageParams = new JObject();
            sampleMessageParams.Add("response", new JObject());
            sampleMessageParams.Add("status", "ok");

            RawEurekaTrackerMessage rawMessage = new RawEurekaTrackerMessage
            {
                TrackerNumber = null,
                SequenceNumber = 69,
                Target = "phoenix",
                MessageId = "phx_reply",
                MessageParams = sampleMessageParams
            };

            JToken expectedToken = new JArray()
            {
                new JValue((string)null),
                new JValue("69"),
                new JValue("phoenix"),
                new JValue("phx_reply"),
                new JObject() {
                    { "response", new JObject() },
                    { "status", "ok" }
                }
            };

            Assert.AreEqual(expectedToken, rawMessage.ToToken());
        }

        [Test]
        public void FromTokenNonArrayThrowsException()
        {
            Assert.Throws(typeof(Exception), () => RawEurekaTrackerMessage.FromToken(new JValue("hi")));
        }

        public void FromTokenShortArrayThrowsException()
        {
            
        }
    }
}