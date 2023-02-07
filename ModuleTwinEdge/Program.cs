using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace ModuleTwinEdge
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient 
        /// </summary>
        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);
        }

        private static async Task OnDesiredPropertiesUpdate(TwinCollection twinCollection, object userContext)
        {
            const string keyName = "status";
            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null) { throw new ArgumentNullException(nameof(userContext)); }

            var moduleTwin = await moduleClient.GetTwinAsync();
            var desiredProperties = moduleTwin.Properties.Desired;

            if (desiredProperties.Contains(keyName))
            {
                switch (desiredProperties[keyName])
                {
                    case "on":
                        //do things
                        break;
                    case "off":
                        //do other things
                        break;
                    default:
                        break;
                }

                await moduleClient.UpdateReportedPropertiesAsync(new TwinCollection()[keyName] =
                    desiredProperties[keyName]);
            }
        }
    }
}
