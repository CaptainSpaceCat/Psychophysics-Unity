using System.Collections.Concurrent;
using System.Threading;
using NetMQ;
using UnityEngine;
using NetMQ.Sockets;
using System;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CustomGazeData
{
    public Vector2 center;
    public float confidence;
    public float timestamp;
    public List<int> timestamps = new List<int>();

    public CustomGazeData()
    {
    }

    public void AddTimestamp(int stamp)
    {
        timestamps.Add(stamp);
    }

    public void DumpTimestamps()
    {
        string path = "Assets/out.txt";
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        string line = "";
        foreach (float stamp in timestamps)
        {
            line += stamp.ToString() + ' ';
        }
        writer.WriteLine(line);
        writer.Close();
    }

    public bool IsStale()
    {
        //return false;
        int now = DateTime.Now.Millisecond * 1000;
        int start = timestamps[0];
        if (now < start)
        {
            now += 1000000;
        }
        return now - start >= 15000;
    }
}


public class NetMQListener
{
    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();
    private CancellationTokenSource Source;
    private bool CanListen;

    // This function runs forever in a separate thread
    // Its only purpose is to pull data from Python and queue it up
    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var subSocket = new SubscriberSocket())
        {
            subSocket.Options.ReceiveHighWatermark = 1000;
            subSocket.Connect("tcp://localhost:5557");
            subSocket.Subscribe("");
            // loop forever until the thread is no longer needed
            while (!_listenerCancelled)
            {
                //byte[] frameBytes;
                string frameString;
                if (!subSocket.TryReceiveFrameString(out frameString)) continue;
                int unity_recieve_timestamp = DateTime.Now.Millisecond * 1000;
                //we recieve the data from Python and enqueue it into our messageQueue
                AddMessage(frameString + ' ' + unity_recieve_timestamp.ToString());
            }
            subSocket.Close();
        }
        NetMQConfig.Cleanup();
    }

    /// <summary>
    /// Store message and immediately start reading
    /// </summary>
    /// <param name="message"></param>
    private void AddMessage(string message)
    {
        _messageQueue.Enqueue(message);
        if (Source != null)
        {
            Source.Cancel();
        }
    }

    /// <summary>
    /// Listen infinitely for any incoming messages and refresh the token
    /// </summary>
    async void WaitForInput()
    {
        while (CanListen)
        {
            if (Source != null)
                Source.Dispose();

            Source = new CancellationTokenSource();
            bool tokenCancelled = await Task.Run(() => WaitUntilTokenIsCancelled(Source.Token));

            if (tokenCancelled)
            {
                ReadMessages();
            }
        }
    }

    /// <summary>
    /// Will return true when task has been cancelled, and will block
    /// background thread till it has
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> WaitUntilTokenIsCancelled(CancellationToken token)
    {
        return Task.Delay(-1, token)
                .ContinueWith(tsk => tsk.Exception == default);
    }

    void ReadMessages()
    {
        while (!_messageQueue.IsEmpty)
        {
            string message_out;
            if (_messageQueue.TryDequeue(out message_out))
            {
                // This is hooked up to the rendering code which changes the shader variables 
                // to render the square
                _messageDelegate(message_out);
            }
            else
            {
                break;
            }
        }
    }

    public NetMQListener(MessageDelegate messageDelegate)
    {
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(ListenerWork);
    }

    public void Start()
    {
        _listenerCancelled = false;
        CanListen = true;

        WaitForInput();
        _listenerWorker.Start();
    }

    public void Stop()
    {
        CanListen = false;

        _listenerCancelled = true;
        _listenerWorker.Join();
    }
}

public class ClientObjectEdited : MonoBehaviour
{
    public Vector3 offset;
    private NetMQListener _netMqListener;
    public Text text;
    public float avg = 0;
    public float max = 0;
    private float total = 0;
    private int samps = 0;

    public GazeRaycaster gazer;
    //private CustomGazeData gazeData = new CustomGazeData();

    private void HandleMessage(string message)
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

        gazer.ConsumeGazeData(gazeData);
    }

    private void Start()
    {
        _netMqListener = new NetMQListener(HandleMessage);
        _netMqListener.Start();
    }
    
    private void OnDestroy()
    {
        //_netMqListener.Stop();
    }
}