# Updating an IoT Edge device Module Twin with an Azure Function

### Disclaimer: This sample does not incorporate any security best-practices! The main goal of this demo is to highlight the technical requirements and steps required to achieve thist task. The Azure Function allows anonymous access, so *ANYONE* can modify the module! 

### Requirements:

1. configured Azure IoT Hub
    1. create a shared access policy for registry read, registry write and service connect
2. configured Azure IoT Edge device (this should work for non-edge devices as well)
3. configured module registry

### How to run this demo

#### Prepare the IoT Edge project

1. install the Azure IoT Edge Tools & Docker
2. create an ```.env``` file in the ModuleTwin.Device folder. Add the following content:
```
CONTAINER_REGISTRY_URI_iotedgedevregistry=<your_registry_hostname>
CONTAINER_REGISTRY_USERNAME_iotedgedevregistry=<your_registry_username>
CONTAINER_REGISTRY_PASSWORD_iotedgedevregistry=<your_registry_key>
```
3. Configure your local IoT Edge Simulator by providing the IoT Hub connection string


#### Prepare the Azure Function project

1. Install the Azure Function Core Tools & Docker
2. create a ```local.settings.json``` file in the ModuleTwinFunction. There will be no credentials stored, but the file configures local storage and logging
```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet"
    }
}
```

#### Run the demo

1. Fire up the IoT Edge project using the simulator
2. Start the Azure Functions project
3. Get Postman and create a POST request to ```http://localhost:30060/api/Function1```
4. Provide Azure IoT Hub details in the body (technically, you could also fire a GET and provide the parameters in the query)
```json
{
    "hub" : "your_iot_hub_uri",
    "deviceId" : "your_iot_edge_simulator_device",
    "moduleId" : "ModuleTwinEdge",
    "key" : "your_iot_hub_shared_access_key",
    "keyName" : "your_iot_hub_shared_access_key_name",
    "status" : "your_status_value"
}
```
5. Send the request. The module will receive a new Desired Property (status), do something and update the Reported Property afterwards

### Notes:

#### IoT Edge Module implementation

1. The module has a route defined, however it is not being used. In a "real-world" sample this is probably helpful
2. Only one Desired Property "status" is being read. A TwinCollection patch is being created and applied to the Reported Properties 
```cs
    await moduleClient.UpdateReportedPropertiesAsync(new TwinCollection()[keyName] = desiredProperties[keyName]);
```
3. The switch case allows to extend the status value and react accordingly. Of course, the property name can be modified.
```cs
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

    ...
}
```

#### Function implementation

1. The authorization level of the Function is set to ```AuthorizationLevel.Anonymous```! Add authorization logic, and/or place the Function behind an API Management instance which takes care of the authentication.
2. The Function is supporting both GET and POST. You likely want to go just with POST.
3. The credentials (and payload) are taken from the JSON-body. You likely don't want to the credentials all the time, so it's a good idea to store IoT Hub URI, Shared Access Key, Shared Access Key Name (and the module name) somewhere safe. Your Function's Application Settings or Azure KeyVault are a good choice.
