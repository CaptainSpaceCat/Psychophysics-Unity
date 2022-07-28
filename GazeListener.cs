using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeListener : RunAbleThread
{
    public event Action<string> OnGazeDataReceived;

    protected override void Run()
    {
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
        using (RequestSocket client = new RequestSocket())
        {
            client.Connect("tcp://localhost:5556");
            while (Running)
            {
                RequestGazeData(client);
                string response = "";
                while (Running && !client.TryReceiveFrameString(out response)) { }
                OnGazeDataReceived(response);
            }
            client.SendFrame("EOF");
        }

       

        NetMQConfig.Cleanup(); // this line is needed to prevent unity freeze after one use, not sure why yet
    }

    private void RequestGazeData(RequestSocket client)
    {
        client.SendFrame("LOL");
    }
}