using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;

namespace ModuleTwinFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // get these from KeyVault instead!
            //GET
            string hub = req.Query["hub"];
            string sasKey = req.Query["key"];
            string sasKeyName = req.Query["keyName"];
            string deviceId = req.Query["deviceId"];
            string moduleId = req.Query["moduleId"];
            string status = req.Query["status"];

            //POST
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            hub = hub ?? data?.hub;
            sasKey = sasKey ?? data?.key;
            sasKeyName = sasKeyName ?? data?.keyName;
            deviceId = deviceId ?? data?.deviceId;
            moduleId = moduleId ?? data?.moduleId;
            status = status ?? data?.status;


            try
            {
                using var registryManager = RegistryManager.CreateFromConnectionString($"HostName={hub};SharedAccessKeyName={sasKeyName};SharedAccessKey={sasKey}");
                var moduleTwin = await registryManager.GetTwinAsync(deviceId, moduleId);
                moduleTwin.Properties.Desired["status"] = status;
                await registryManager.UpdateTwinAsync(deviceId, moduleId, moduleTwin, moduleTwin.ETag);

                return new OkObjectResult("status set");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception thrown: {e.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
