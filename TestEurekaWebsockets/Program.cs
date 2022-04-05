using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using TestEurekaWebsockets.Messages;

namespace TestEurekaWebsockets
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Out.Write("Instance id: ");
            Console.Out.Flush();
            string instanceId = String.Format("instance:{0}", Console.ReadLine().Trim());
            Console.WriteLine();

            const string TRACKER_ADDRESS = "wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0";
            EurekaTrackerClient client = new EurekaTrackerClient(new Uri(TRACKER_ADDRESS));

            EurekaTrackerClient.MessageHandler echoTask = async (client, message) =>
            {
                await Console.Out.WriteLineAsync(String.Format("{1} << {0}", RenderMessage(message), DateTime.Now));
            };

            client.RegisterListener(instanceId, echoTask);
            client.RegisterListener("phoenix", echoTask);

            CancellationTokenSource cts = new CancellationTokenSource();

            Task clientTask = await client.StartClient();
            Task exitTask = Task.Run(async () =>
            {
                while (true)
                {
                    string input = await Console.In.ReadLineAsync();

                    if (input == "q")
                    {
                        cts.Cancel();
                        client.StopClient();
                        break;
                    }
                }
            });

            PhxJoinMessage joinMessage = new PhxJoinMessage(instanceId);
            await client.SendMessage(joinMessage, cts.Token);

            try
            {
                await clientTask;
            }
            catch (OperationCanceledException)
            {
                // Suppress exception for cancelled operations
            }
        }

        private static string RenderMessage(EurekaTrackerMessage message)
        {
            return message.Serialize().ToString(Formatting.None);
        }
    }
}
