using System;
using System.Net;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System.Text;

/// <summary>
/// getting a message from device and then call Power BI API. 
/// </summary>
/// <param name="myEventHubMessage"></param>
public static void Run(string myEventHubMessage, TraceWriter log)
{
    DateTime recieveTime = DateTime.UtcNow;
    string scale = "S1";  // Please replace IOT Hubs scal value

    log.Info($" processed a recieve message: {myEventHubMessage}");

    string realTimePushURL = "POWER_BI_API";
    var input = new InputMessage();
    var output = new OutputMessage();

    try
    {
        //(This line is what it’s all about. Take the Message that trigger this function and load it into our object by deserializing it.)
        input = Newtonsoft.Json.JsonConvert.DeserializeObject<InputMessage>(myEventHubMessage);

        output.deviceId = input.deviceId;
        output.interval = input.interval;
        output.sendTime = input.sendTime.ToString("MM / dd / yyyy hh: mm: ss.fff tt");
        output.recieveTime = recieveTime.ToString("MM / dd / yyyy hh: mm: ss.fff tt");
        output.scale = scale;
        output.latency = (recieveTime.Ticks - input.sendTime.Ticks) * 0.0000001;

        var messageString = Newtonsoft.Json.JsonConvert.SerializeObject(output);
        string postData = "[" + messageString + "]";
        SendData(postData, realTimePushURL);

        log.Info($" processed a post message: {postData}");

    }
    catch (Exception ex)
    {
        log.Info($"Error deserializing: {ex.Message}");
        return;
    }
}

/// <summary>
/// Call Power BI Streaming API to store data.
/// </summary>
/// <param name="json"></param>
/// <param name="apiurl"></param>
static async void SendData(string json, string apiurl)
{
    // sending request to Power BI streaming API
    WebRequest request = WebRequest.Create(apiurl);
    request.Method = "POST";
    byte[] byteArray = Encoding.UTF8.GetBytes(json);
    request.ContentLength = byteArray.Length;
    Stream dataStream = request.GetRequestStream();
    dataStream.Write(byteArray, 0, byteArray.Length);
    dataStream.Close();
    WebResponse response = await request.GetResponseAsync();

    // Get the stream containing content returned by the server.
    dataStream = response.GetResponseStream();
    StreamReader reader = new StreamReader(dataStream);
    string responseFromServer = reader.ReadToEnd();

    // Clean up the streams
    reader.Close();
    dataStream.Close();
    response.Close();
}

public class InputMessage
{
    public string deviceId;
    public double interval;
    public DateTime sendTime;
}

public class OutputMessage
{
    public string deviceId;
    public string scale;
    public double interval;
    public string sendTime;
    public string recieveTime;
    public double latency;
}