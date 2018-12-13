using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET;
using System.Linq;
using Microsoft.Azure.Devices;
using System.Text;
using System;
using System.Web.Http;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Configuration;

namespace AflexCityFunctions
{
    public static class AlexaFunctions
    {
        //[FunctionName("LEDActions")]
        //public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        //{
        //    log.Info("C# HTTP trigger function processed a request.");

        //    string name = req.Query["name"];

        //    string requestBody = new StreamReader(req.Body).ReadToEnd();
        //    dynamic data = JsonConvert.DeserializeObject(requestBody);
        //    name = name ?? data?.name;

        //    return name != null
        //        ? (ActionResult)new OkObjectResult($"Hello, {name}")
        //        : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        //}

        static ServiceClient serviceClient;
        static string connectionString = "";



        static string dotnetDevice = "";
        static string cameraDevice = "";
        static string turnLED1_On = "{\"Name\" : \"TurnLed1On\", \"Parameters\" : {}}";
        static string turnLED1_Off = "{\"Name\" : \"TurnLed1Off\", \"Parameters\" : {}}";
        static string turnLED2_On = "{\"Name\" : \"TurnLed2On\", \"Parameters\" : {}}";
        static string turnLED2_Off = "{\"Name\" : \"TurnLed2Off\", \"Parameters\" : {}}";
        static string turnWarmerOn = "{\"Name\" : \"TurnWarmerOn\", \"Parameters\" : {}}";
        static string turnWarmerOff = "{\"Name\" : \"TurnWarmerOff\", \"Parameters\" : {}}";

        static string turnCameraOn = "on";
        static string turnCameraOff = "off";

        //static string turnCameraOn = "{\"Name\" : \"on\", \"Parameters\" : {}}";
        //static string turnCameraOff = "{\"Name\" : \"TurnCameraOff\", \"Parameters\" : {}}";

        static string corridorintent = "corridor";
        static string bedroomintent = "bedroom";
        static string smartwarmerintent = "smartwarmer";
        static string cameraintent = "naico";


        static string turnDeviceStatusOn = "turn on";
        static string turnDeviceStatusOff = "turn off";
        static string switchDeviceStatusOn = "switch on";
        static string switchDeviceStatusOff = "switch off";
        static string deviceStart = "start";
        static string deviceStop = "stop";


        [FunctionName("Alexa")]
        public static SkillResponse Run(
        [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = null)]
        [FromBody]SkillRequest request, TraceWriter log)
        {
            SkillResponse response = null;
            PlainTextOutputSpeech outputSpeech = new PlainTextOutputSpeech();
            try
            {
                serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

                string deviceStatus = (request.Request as IntentRequest)?.Intent.Slots.FirstOrDefault(s => s.Key == "status").Value?.Value;
                string deviceType = (request.Request as IntentRequest)?.Intent.Slots.FirstOrDefault(s => s.Key == "devicetype").Value?.Value;

                string intentName = (request.Request as IntentRequest)?.Intent.Name;
                if (deviceType == null)
                {
                    outputSpeech.Text = "Device type is Missing";
                }
                else if (deviceStatus == null)
                {
                    outputSpeech.Text = "Device status is Missing";
                }
                else if (intentName == null)
                {
                    outputSpeech.Text = "Device location (intent name) is Missing";
                }
                else if (deviceType.Equals(DeviceType.Light))
                {
                    HandleDeviceOperations(intentName, deviceStatus);
                    outputSpeech.Text = intentName + " " + deviceType + " turned " + deviceStatus;
                }
                else if (deviceType.Equals(DeviceType.Fragrance))
                {
                    HandleDeviceOperations(intentName, deviceStatus);
                    outputSpeech.Text = intentName + " " + deviceType + " turned " + deviceStatus;
                }
                else if (deviceType.Equals(DeviceType.Camera))
                {
                    HandleDeviceOperations(intentName, deviceStatus);
                    outputSpeech.Text = intentName + " " + deviceType + " turned " + deviceStatus;
                }

                response = ResponseBuilder.Tell(outputSpeech);

            }
            catch (Exception ex)
            {
                outputSpeech.Text = "Something went wrong";
            }
            return response;
        }


        private async static Task SendDeviceMessage(string message, string device)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(message));
            //  await serviceClient.SendAsync(deviceID, commandMessage); cameraDevice
            await serviceClient.SendAsync(device, commandMessage);
        }

        private static void HandleDeviceOperations(string deviceLocation, string deviceStatus)
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var deviceList = ConfigurationManager.AppSettings.AllKeys;
            //once you have the path you get the directory with:
            var directory = System.IO.Path.GetDirectoryName(path);

            //var deviceDictionary = LoadDevices(directory + @"\DeviceConfig.xml");
          //  var deviceList = deviceDictionary.Select(x => x.Key).ToList();
            if (deviceList.Where(x => x == deviceLocation).Count() > 0)
            {
                var deviceKey = deviceList.Where(x => x == deviceLocation).SingleOrDefault();
                var type = ConfigurationManager.AppSettings[deviceKey];
                if (!string.IsNullOrEmpty(type))
                {
                    if (deviceStatus.ToLower().Equals(turnDeviceStatusOn) || deviceStatus.ToLower().Equals(switchDeviceStatusOn) || deviceStatus.ToLower().Equals(deviceStart))
                    {

                        switch (type)
                        {

                            case "WARMER":
                                SendDeviceMessage(turnWarmerOn, dotnetDevice).Wait();
                                break;
                            case "CAMERA":
                                SendDeviceMessage(turnCameraOn, cameraDevice).Wait();
                                break;
                            case "LED1":
                                SendDeviceMessage(turnLED1_On, dotnetDevice).Wait();
                                break;
                            case "LED2":
                                SendDeviceMessage(turnLED2_On, dotnetDevice).Wait();
                                break;
                        }
                        //if (deviceLocation.Equals(corridorintent))
                        //{
                        //    SendDeviceMessage(turnLED1_On, dotnetDevice).Wait();
                        //}
                        //else if (deviceLocation.Equals(bedroomintent))
                        //{
                        //    SendDeviceMessage(turnLED2_On, dotnetDevice).Wait();
                        //}
                        //else if (deviceLocation.Equals(smartwarmerintent))
                        //{
                        //    SendDeviceMessage(turnWarmerOn, dotnetDevice).Wait();
                        //}
                        //else if (deviceLocation.Equals(cameraintent))
                        //{
                        //    SendDeviceMessage(turnCameraOn, cameraDevice).Wait();
                        //}
                    }
                    else if (deviceStatus.ToLower().Equals(turnDeviceStatusOff) || deviceStatus.ToLower().Equals(switchDeviceStatusOff) || deviceStatus.ToLower().Equals(deviceStop))
                    {


                        switch (type)
                        {

                            case "WARMER":
                                SendDeviceMessage(turnWarmerOff, dotnetDevice).Wait();
                                break;
                            case "CAMERA":
                                SendDeviceMessage(turnCameraOff, cameraDevice).Wait();
                                break;
                            case "LED1":
                                SendDeviceMessage(turnLED1_Off, dotnetDevice).Wait();
                                break;
                            case "LED2":
                                SendDeviceMessage(turnLED2_Off, dotnetDevice).Wait();
                                break;
                        }


                        //SendDeviceMessage(turnLED1_Off, dotnetDevice).Wait();

                        //if (deviceLocation.Equals(corridorintent))
                        //{
                        //    SendDeviceMessage(turnLED1_Off, dotnetDevice).Wait();
                        //}
                        //else if (deviceLocation.Equals(bedroomintent))
                        //{
                        //    SendDeviceMessage(turnLED2_Off, dotnetDevice).Wait();
                        //}
                        //else if (deviceLocation.Equals(smartwarmerintent))
                        //{
                        //    SendDeviceMessage(turnWarmerOff, dotnetDevice).Wait();
                        //}
                        //else if (deviceLocation.Equals(cameraintent))
                        //{
                        //    SendDeviceMessage(turnCameraOff, cameraDevice).Wait();
                        //}
                    }
                }
            }
        }
        private static Dictionary<string, string> LoadDevices(string xmlFile)
        {
            return XDocument.Load(xmlFile)
                .Descendants("devices")
                .Descendants("device")
                .ToDictionary(p => p.Attribute("name").Value,
                p => p.Attribute("type").Value);
        }

        public static class DeviceType
        {
            public static string Light = "light";
            public static string Camera = "camera";
            public static string Fragrance = "fragrance";
        }
    }
}
