using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestEurekaWebsockets.Messages;

namespace TestEurekaWebsockets
{
    class Program
    {
        private static int sequenceNumber;
        private static object sequenceNumberLock;

        static async Task Main(string[] args)
        {
            sequenceNumber = 1;
            sequenceNumberLock = new object();
            string instance = "instance:DZM2nz";

            using (var ws = new ClientWebSocket())
            {
                await ws.ConnectAsync(new Uri("wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0"), CancellationToken.None);

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken cancelToken = cancelTokenSource.Token;

                Task heartbeatTask = Task.Run(async () =>
                {
                    do
                    {
                        await Task.Delay(30000, cancelToken);

                        cancelToken.ThrowIfCancellationRequested();

                        int heartbeatSequenceNumber;

                        lock (sequenceNumberLock)
                        {
                            heartbeatSequenceNumber = sequenceNumber;
                            sequenceNumber++;
                        }

                        var pingMessage = new HeartbeatMessage(heartbeatSequenceNumber, "phoenix");
                        var pingJson = pingMessage.Serialize().ToString(Formatting.None);
                        await SendString(pingJson, ws, cancelToken);
                    } while (true);
                }, cancelToken);

                Task echoTask = Task.Run(async () =>
                {
                    do
                    {
                        string receivedText = await ReceiveString(ws, cancelToken);

                        Console.WriteLine("<< {0}", receivedText);
                    } while (!cancelToken.IsCancellationRequested);
                }, cancelToken);

                var joinMessage = new PhxJoinMessage(1, getAndIncrementSequenceNumber(), instance);
                await SendString(joinMessage.Serialize().ToString(Formatting.None), ws, cancelToken);

                string input = "";
                do
                {
                    input = Console.ReadLine();
                } while (input != "q");

                cancelTokenSource.Cancel();

                try
                {
                    await Task.WhenAll(heartbeatTask, echoTask);
                }
                catch (OperationCanceledException)
                {
                    // Suppress exception from cancelling tasks
                }
            }
        }

        private static async Task SendString(string data, WebSocket webSocket, CancellationToken cancelToken)
        {
            Console.WriteLine(">> {0}", data);
            await webSocket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, cancelToken);
        }

        private static async Task<string> ReceiveString(WebSocket webSocket, CancellationToken cancelToken)
        {
            const int BUFFER_SIZE = 2048;
            var buffer = new ArraySegment<byte>(new byte[BUFFER_SIZE]);

            WebSocketReceiveResult result;
            string receivedText;
            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await webSocket.ReceiveAsync(buffer, cancelToken);

                    cancelToken.ThrowIfCancellationRequested();

                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    receivedText = await reader.ReadToEndAsync();
                }
            }

            return receivedText;
        }

        private static int getAndIncrementSequenceNumber()
        {
            int result;

            lock(sequenceNumberLock)
            {
                result = sequenceNumber;
                sequenceNumber++;
            }

            return result;
        }
    }
}
