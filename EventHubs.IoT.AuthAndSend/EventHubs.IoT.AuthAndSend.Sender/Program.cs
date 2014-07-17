using System;
using System.IO;
using System.Net;
using System.Threading;

namespace EventHubs.IoT.AuthAndSend.Sender
{
    class Program
    {
        static void Main(string[] args)
        { 
            // Generate a SAS key with the Signature Generator.: https://github.com/sandrinodimattia/RedDog/releases
            var sas = "SharedAccessSignature sr=https%3a%2f%2freddogeventhub.servicebus.windows.net%2ftemperature%2fpublishers%2fbathroom%2fmessages&sig=OsO5iA%2btDyxGLmFCcHNHmTRtMTr03VyZjAtdC5FFPEw%3d&se=1405562933&skn=SenderDevice";

            // Namespace info.
            var serviceNamespace = "reddogeventhub";
            var hubName = "temperature";
            var deviceName = "bathroom";
            
            
            Console.WriteLine("Starting device: {0}", deviceName);

            var uri = new Uri(String.Format("https://{0}.servicebus.windows.net/{1}/publishers/{2}/messages", serviceNamespace, hubName, deviceName));

            // Keep sending.
            while (true)
            {
                var eventData = new
                {
                    Temperature = new Random().Next(20, 50)
                };

                var req = WebRequest.CreateHttp(uri);
                req.Method = "POST";
                req.Headers.Add("Authorization", sas);
                req.ContentType = "application/atom+xml;type=entry;charset=utf-8";

                using (var writer = new StreamWriter(req.GetRequestStream()))
                {
                    writer.Write("{ Temperature: " + eventData.Temperature + "}");
                }

                using (var response = req.GetResponse() as HttpWebResponse)
                {
                    Console.WriteLine("Sent temperature using legacy HttpWebRequest: {0}", eventData.Temperature);
                    Console.WriteLine(" > Response: {0}", response.StatusCode);
                    
                }

                Thread.Sleep(new Random().Next(1000, 5000));
            }
        }
    }
}
