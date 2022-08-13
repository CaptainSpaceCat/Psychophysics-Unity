using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AsyncListener : MonoBehaviour
{
    static ConcurrentQueue<string> Messages;
    static CancellationTokenSource Source;
    static Thread ListenThread;

    static DateTime MessageWasSent;
    static DateTime MessageWasReceived;

    public GazeRaycaster raycaster;

    private void Start()
    {
        Main();
    }

    static void Main()
    {
        Messages = new ConcurrentQueue<string>();

        WaitForInput();

        ListenThread = new Thread(() => ListenForMessages());
        ListenThread.Start();

    }

    static void ListenForMessages()
    {
        AsyncIO.ForceDotNet.Force();
        using (var subSocket = new SubscriberSocket())
        {
            subSocket.Options.ReceiveHighWatermark = 1000;
            subSocket.Connect("tcp://localhost:5557");
            subSocket.Subscribe("");
            while (true)
            {
                string frameString;
                while (!subSocket.TryReceiveFrameString(out frameString)) { Thread.Sleep(1); /*TODO maybe delete this*/ }

                AddMessage(frameString);
            }
            subSocket.Close();
        }
        NetMQConfig.Cleanup();
    }



    static void AddMessage(string message)
    {
        Messages.Enqueue(message + ' ' + (DateTime.Now.Millisecond*1000).ToString());
        MessageWasSent = DateTime.Now;

        Source.Cancel();
    }

    static async void WaitForInput()
    {
        while (true) {
            if (Source != null)
                Source.Dispose();

            Source = new CancellationTokenSource();

            if (await Wait(Source.Token))
            {
                MessageWasReceived = DateTime.Now;

                ReadMessages();
            }
        }
    }

    static Task<bool> Wait(CancellationToken token)
    {
        return Task.Delay(-1, token)
                .ContinueWith(tsk => tsk.Exception == default);
    }

    static void ReadMessages()
    {
        bool read = Messages.TryDequeue(out string message);

        if (read)
        {
            //Debug.Log("Read message: " + message);
            //Debug.Log("Time taken to catch message: " + MessageWasReceived.Subtract(MessageWasSent).TotalMilliseconds + "ms");
            HandleMessage(message);
        }
    }

    static void HandleMessage(string message)
    {
        int testts = DateTime.Now.Millisecond * 1000;
        string[] split = message.Split(' ');
        Vector2 center = new Vector2(float.Parse(split[0]), float.Parse(split[1]));
        float confidence = float.Parse(split[2]);
        int[] timestamps = {
            int.Parse(split[3]),
            int.Parse(split[4]),
            int.Parse(split[5]),
            int.Parse(split[6]),
        };

        //TODO populate a premade struct
        //CustomGazeData gazeData = new CustomGazeData();
        CustomGazeData gazeData = new CustomGazeData();
        gazeData.center = center;
        gazeData.confidence = confidence;
        for (int i = 0; i < timestamps.Length; i++)
        {
            gazeData.AddTimestamp(timestamps[i]);
        }
        gazeData.AddTimestamp(testts);
        gazeData.AddTimestamp(DateTime.Now.Millisecond * 1000);

        gazeData.DumpTimestamps();
        //raycaster.ConsumeGazeData(gazeData);
        Debug.Log((MessageWasReceived - MessageWasSent).TotalMilliseconds);
    }
}
