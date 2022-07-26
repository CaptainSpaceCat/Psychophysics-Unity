using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// </summary>
public class DataSender : RunAbleThread
{

    public event Action<PupilData> OnPupilDataRecieved;
    public event Action<bool> OnCalibrationPointProcessed;
    /// <summary>
    ///     Request Hello message to server and receive message back. Do it 10 times.
    ///     Stop requesting when Running=false.
    /// </summary>
    protected override void Run()
    {
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
        using (RequestSocket client = new RequestSocket())
        {
            client.Connect("tcp://localhost:5555");
            dataQueue = new Queue<string>();
            while (Running)
            {
                while (DataToSend())
                {
                    string msg_out = dataQueue.Dequeue();
                    Debug.Log("Sending message " + msg_out);
                    client.SendFrame(msg_out);
                    OnCalibrationPointProcessed(WaitForResponse(client));
                }

                /*string msg_in = null;
                bool gotMessage = client.TryReceiveFrameString(out msg_in); // this returns true if it's successful
                if (gotMessage) {
                    //do something with the data
                    OnPupilDataRecieved(ParseIncomingData(msg_in));
                }*/
                
            }
        }

        NetMQConfig.Cleanup(); // this line is needed to prevent unity freeze after one use, not sure why yet
    }

    private bool WaitForResponse(RequestSocket client)
    {
        string response = "";
        while (Running && !client.TryReceiveFrameString(out response)) {}
        Debug.Log(response);
        return response == "ACK";
    }

    private PupilData ParseIncomingData(string incoming)
    {
        return new PupilData();
    }

    private bool DataToSend()
    {
        return dataQueue.Count > 0;
    }

    private Queue<string> dataQueue;
    public void SendData(Vector4 data)
    {
        dataQueue.Enqueue(data.ToString());
    }
}