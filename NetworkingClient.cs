using System;
using UnityEngine;

public class NetworkingClient : MonoBehaviour
{
    private DataSender _sender;
    public event Action<PupilData> OnPupilDataRecieved;
    public event Action<bool> OnCalibrationPointProcessed;

    private void Start()
    {
        _sender = new DataSender();
        _sender.OnPupilDataRecieved += Pipe;
        _sender.OnCalibrationPointProcessed += CalibCallback;
        _sender.Start();
    }

    private void OnDestroy()
    {
        _sender.Stop();
    }

    public void SendData(Vector4 data)
    {
        _sender.SendData(data);
    }

    private void Pipe(PupilData data)
    {
        OnPupilDataRecieved(data);
    }

    private void CalibCallback(bool state)
    {
        OnCalibrationPointProcessed(state);
    }
}