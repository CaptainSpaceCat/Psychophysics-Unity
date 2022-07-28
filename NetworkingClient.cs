using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections;
using UnityEngine;

public class NetworkingClient : MonoBehaviour
{
    private DataSender _sender;
    private GazeListener _listener;
    public event Action<Vector2, float> OnGazeDataReceived;
    public event Action<bool> OnCalibrationPointProcessed;
    public PullSocket testClient;

    private void Start()
    {
        _sender = new DataSender();
        _sender.OnCalibrationPointProcessed += CalibCallback;
        _sender.OnEOF += Cleanup;

        //_listener = new GazeListener();
        //_listener.OnGazeDataReceived += Pipe;
        //AsyncIO.ForceDotNet.Force();
        //testClient = new PullSocket();
        //testClient.Connect("tcp://localhost:5557");
        //testClient.SubscribeToAnyTopic();
        //testClient.ReceiveReady += TestPipe;

        StartCoroutine(FakeThread());
    }

    private void TestPipe(object sender, NetMQSocketEventArgs e)
    {
        string message;
        if (e.Socket.TryReceiveFrameString(out message))
        {
            Debug.Log("Test " + message);
            Pipe(message);
        }
    }

    public void StartCalibration()
    {
        _sender.Start();
    }

    public void StartStreamingGaze()
    {
        //_listener.Start();  
    }

    private void OnDestroy()
    {
        _sender.Stop();
        //_listener.Stop();
    }

    public void SendData(string data)
    {
        _sender.SendData(data);
    }

    private void Pipe(string rawdata)
    {
        string[] split = rawdata.Split(' ');
        Vector2 center = new Vector2(float.Parse(split[0]), float.Parse(split[1]));
        float confidence = float.Parse(split[2]);
        OnGazeDataReceived(center, confidence);
    }

    private void CalibCallback(bool state)
    {
        OnCalibrationPointProcessed(state);
    }

    private void Cleanup()
    {
        _sender.Stop();
    }

    private bool Running = true;
    private IEnumerator FakeThread()
    {
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
        using (PullSocket client = new PullSocket())
        {
            client.Connect("tcp://localhost:5557");

            for (int i = 0; Running; i++)
            {
                //Debug.Log("Sending Hello");
                //client.SendFrame("Hello");
                // ReceiveFrameString() blocks the thread until you receive the string, but TryReceiveFrameString()
                // do not block the thread, you can try commenting one and see what the other does, try to reason why
                // unity freezes when you use ReceiveFrameString() and play and stop the scene without running the server
                //                string message = client.ReceiveFrameString();
                //                Debug.Log("Received: " + message);
                string message = null;
                bool gotMessage = false;
                while (Running)
                {
                    gotMessage = client.TryReceiveFrameString(out message); // this returns true if it's successful
                    if (gotMessage) break;
                    yield return null;
                }

                if (gotMessage) Pipe(message);//Debug.Log("Received " + message);
            }
        }

        NetMQConfig.Cleanup(); // this line is needed to prevent unity freeze after one use, not sure why yet
    }

}