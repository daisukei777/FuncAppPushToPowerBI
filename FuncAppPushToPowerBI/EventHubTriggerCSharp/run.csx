#r "Newtonsoft.Json"

using System;
using System.Net;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System.Text;


public static void Run(string myEventHubMessage, TraceWriter log)
{
    DateTime recieveTime = DateTime.UtcNow;
    string scale = "S3"; // set IoT Hub Scale
    string realTimePushURL = "POWER BI STREAM API";
    var CloudObject = new OurObject();

    try
    {
        //(This line is what it’s all about. Take the Message that trigger this function and load it into our object by deserializing it.)
        CloudObject = Newtonsoft.Json.JsonConvert.DeserializeObject<OurObject>(myEventHubMessage);
        CloudObject.recieveTime = recieveTime;
        CloudObject.scale = scale;
        CloudObject.latency = (recieveTime.Ticks - CloudObject.sendTime.Ticks) * 0.0000001;

        var messageString = Newtonsoft.Json.JsonConvert.SerializeObject(CloudObject);
        string postData = "[" + messageString + "]";

        log.Info($"C# Event Hub trigger function processed a message: {postData}");

        // sending request to Power BI streaming API
        WebRequest request = WebRequest.Create(realTimePushURL);
        request.Method = "POST";
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        request.ContentLength = byteArray.Length;
        Stream dataStream = request.GetRequestStream();
        dataStream.Write(byteArray, 0, byteArray.Length);
        dataStream.Close();
        WebResponse response = request.GetResponse();

        // Get the stream containing content returned by the server.
        dataStream = response.GetResponseStream();
        StreamReader reader = new StreamReader(dataStream);
        string responseFromServer = reader.ReadToEnd();

        // Clean up the streams
        reader.Close();
        dataStream.Close();
        response.Close();
    }
    catch (Exception ex)
    {
        log.Info($"Error deserializing: {ex.Message}");
        return;
    }
}


public class OurObject
{
    public string deviceId;
    public string scale;
    public double interval;
    public DateTime sendTime;
    public DateTime recieveTime;
    public double latency;
}
