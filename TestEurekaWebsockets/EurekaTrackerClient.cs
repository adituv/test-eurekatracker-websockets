using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestEurekaWebsockets.Messages;

namespace TestEurekaWebsockets
{
    public class EurekaTrackerClient
    {
        private int SequenceNumber;
        private Dictionary<string, List<MessageHandler>> Listeners;
        private ClientWebSocket WebSocket;

        private CancellationTokenSource CancellationTokenSource;

        public Uri Address { get; init; }

        public delegate Task MessageHandler(EurekaTrackerClient sender, EurekaTrackerMessage message);

        public EurekaTrackerClient(Uri address)
        {
            this.CancellationTokenSource = null;
            this.SequenceNumber = 0;
            this.Listeners = new Dictionary<string, List<MessageHandler>>();
            this.WebSocket = new ClientWebSocket();

            this.Address = address;
        }

        public void RegisterListener(string target, MessageHandler listener)
        {
            lock(this.Listeners)
            {
                if (!this.Listeners.ContainsKey(target))
                {
                    this.Listeners.Add(target, new List<MessageHandler>());
                }

                var listenerChain = this.Listeners[target];

                lock(listenerChain)
                {
                    if (listenerChain.Contains(listener))
                    {
                        throw new ArgumentException("Listener is already registered");
                    }

                    listenerChain.Add(listener);
                }
            }
        }

        public void UnregisterListener(string target, MessageHandler listener)
        {
            lock (this.Listeners)
            {
                if (this.Listeners.TryGetValue(target, out List<MessageHandler> listenerChain))
                {
                    lock (listenerChain)
                    {
                        listenerChain.Remove(listener);
                    }
                }
            }
        }

        public async Task<Task> StartClient()
        {
            // Start a client with no external CancellationToken
            return await StartClient(CancellationToken.None);
        }

        public async Task<Task> StartClient(CancellationToken cancellationToken)
        {
            this.CancellationTokenSource = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(this.CancellationTokenSource.Token, cancellationToken);

            await this.WebSocket.ConnectAsync(this.Address, linkedCts.Token);

            if (this.WebSocket.State != WebSocketState.Open)
            {
                throw new Exception("Failed to connect to websocket");
            }

            return Task.WhenAll(ListenerTask(linkedCts.Token), HeartbeatTask(linkedCts.Token));
        }

        public void StopClient()
        {
            if (this.WebSocket.State == WebSocketState.Open || this.WebSocket.State == WebSocketState.Connecting)
            {
                if (this.CancellationTokenSource == null)
                {
                    throw new NullReferenceException(nameof(this.CancellationTokenSource));
                }

                this.CancellationTokenSource.Cancel();
            }
        }

        public async Task SendMessage(EurekaTrackerMessage message, CancellationToken cancellationToken)
        {
            message.SequenceNumber = Interlocked.Increment(ref this.SequenceNumber);

            var data = message.Serialize().ToString(Formatting.None);
            await Console.Out.WriteLineAsync(string.Format("{1} >> {0}", data, DateTime.Now));
            await this.WebSocket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, cancellationToken);
        }


        private async Task<EurekaTrackerMessage> ReceiveMessage(CancellationToken cancelToken)
        {
            if (this.WebSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("Websocket is not open");
            }

            const int BUFFER_SIZE = 2048;
            var buffer = new ArraySegment<byte>(new byte[BUFFER_SIZE]);

            JToken receivedMessage;
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await this.WebSocket.ReceiveAsync(buffer, cancelToken);
                    cancelToken.ThrowIfCancellationRequested();
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    throw new WebSocketClosedException();
                }

                ms.Seek(0, SeekOrigin.Begin);

                using (var sr = new StreamReader(ms))
                using (var jtr = new JsonTextReader(sr)) {
                    receivedMessage = JToken.ReadFrom(jtr);
                }
            }

            return EurekaTrackerMessage.FromRaw(RawEurekaTrackerMessage.FromToken(receivedMessage));
        }

        private async Task ListenerTask(CancellationToken cancellationToken)
        {
            do
            {
                EurekaTrackerMessage message = await ReceiveMessage(cancellationToken);

                if (this.Listeners.TryGetValue(message.Target, out List<MessageHandler> listenerChain))
                {
                    await Task.WhenAll(listenerChain.Select(async x => await x(this, message)));
                }
            } while (!cancellationToken.IsCancellationRequested);
        }

        private async Task HeartbeatTask(CancellationToken cancellationToken)
        {
            const string HEARTBEAT_TARGET = "phoenix";
            const int PING_TIMEOUT = 300;

            DateTime lastHeartbeatReceived = DateTime.UtcNow;
            DateTime lastHeartbeatSent = DateTime.UtcNow;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            RegisterListener(HEARTBEAT_TARGET, async (client, message) =>
            {
                lastHeartbeatReceived = DateTime.UtcNow;
            });
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            do
            {
                DateTime nextHeartbeatTime = lastHeartbeatSent + TimeSpan.FromSeconds(30);
                TimeSpan heartbeatDelay = nextHeartbeatTime < DateTime.UtcNow ? TimeSpan.FromSeconds(0) : nextHeartbeatTime - DateTime.UtcNow;

                await Task.Delay(heartbeatDelay, cancellationToken);

                TimeSpan timeSinceLastHeartbeat = DateTime.UtcNow - lastHeartbeatReceived;
                if (timeSinceLastHeartbeat > TimeSpan.FromSeconds(PING_TIMEOUT))
                {
                    throw new TimeoutException("No heartbeat reply received");
                }

                cancellationToken.ThrowIfCancellationRequested();

                var pingMessage = new HeartbeatMessage(HEARTBEAT_TARGET);
                lastHeartbeatSent = DateTime.UtcNow;
                await SendMessage(pingMessage, cancellationToken);
            } while (!cancellationToken.IsCancellationRequested);
        }
    }
}
