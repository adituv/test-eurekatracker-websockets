using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TestEurekaWebsockets
{
    public class WebSocketClosedException : Exception
    {
        public WebSocketClosedException() : base()
        {
        }

        public WebSocketClosedException(string message) : base(message)
        {
        }

        public WebSocketClosedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WebSocketClosedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
