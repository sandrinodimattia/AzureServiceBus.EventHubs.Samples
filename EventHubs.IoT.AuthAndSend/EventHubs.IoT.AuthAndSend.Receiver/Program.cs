using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace EventHubs.IoT.AuthAndSend.Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            var partitionCount = 8;
            var serviceNamespace = "reddogeventhub";
            var hubName = "temperature";
            var receiverKeyName = "TemperatureProcessor";
            var receiverKey = "Bk3bmYV1kiLYRPBac8OuAQYa1WlTRQEa5Vi1/WaGTh0=";

            Console.WriteLine("Starting temperature processor with {0} partitions.", partitionCount);

            CancellationTokenSource cts = new CancellationTokenSource();

            for (int i = 0; i <= partitionCount - 1; i++)
            {
                Task.Factory.StartNew((state) =>
                {
                    Console.WriteLine("Starting worker to process partition: {0}", state);

                    var factory = MessagingFactory.Create(ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, ""), new MessagingFactorySettings()
                    {
                        TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(receiverKeyName, receiverKey),
                        TransportType = TransportType.Amqp
                    });

                    var receiver = factory.CreateEventHubClient(hubName)
                        .GetDefaultConsumerGroup()
                        .CreateReceiver(state.ToString(), DateTime.UtcNow);


                    while (true)
                    {
                        // Receive could fail, I would need a retry policy etc...
                        var messages = receiver.Receive(10);
                        foreach (var message in messages)
                        {
                            var eventBody = Newtonsoft.Json.JsonConvert.DeserializeObject<TemperatureEvent>(Encoding.Default.GetString(message.GetBytes()));

                            Console.WriteLine("{0} [{1}] Temperature: {2}", DateTime.Now, message.PartitionKey, eventBody.Temperature);
                        }

                        if (cts.IsCancellationRequested)
                        {
                            Console.WriteLine("Stopping: {0}", state);
                            receiver.Close();
                        }
                    }
                }, i);
            }

            Console.ReadLine();
            cts.Cancel();
            Console.WriteLine("Wait for all receivers to close and then press ENTER.");
            Console.ReadLine();
        }
    }
}
