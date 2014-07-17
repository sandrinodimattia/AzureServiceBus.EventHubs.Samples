using System;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace EventHubs.IoT.AuthAndSent.ClientSender
{
    class Program
    {
        static void Main(string[] args)
        {
            // Generate a SAS key with the Signature Generator.: https://github.com/sandrinodimattia/RedDog/releases
            // Be sure to choose the AMQP option.
            var sas = "SharedAccessSignature sr=sb%3a%2f%2freddogeventhub.servicebus.windows.net%2ftemperature%2fpublishers%2fgarage&sig=sxgUcKBpmKU%2fsvg5%2bPzmo1%2bOBtCqVH3ZAGiAFNz3rY4%3d&se=1405563678&skn=SenderDevice";

            // Namespace info.
            var serviceNamespace = "reddogeventhub";
            var hubName = "temperature";
            var deviceName = "garage";


            Console.WriteLine("Starting device: {0}", deviceName);

            // Keep sending.
            while (true)
            {
                var eventData = new
                {
                    Temperature = new Random().Next(20, 50)
                };
                
                var factory = MessagingFactory.Create(ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, ""), new MessagingFactorySettings
                {
                    TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(sas),
                    TransportType = TransportType.Amqp
                });

                var client = factory.CreateEventHubClient(String.Format("{0}/publishers/{1}", hubName, deviceName));

                var data = new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventData)));
                data.PartitionKey = deviceName;

                client.Send(data);

                Console.WriteLine("Sent temperature using EventHubClient: {0}", eventData.Temperature);

                Thread.Sleep(new Random().Next(1000, 5000));
            }
        }
    }
}
