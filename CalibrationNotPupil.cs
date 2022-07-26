using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CalibrationNotPupil : MonoBehaviour
{
    public PupilLabs.CalibrationSettings settings;
    public PupilLabs.CalibrationTargets targets;
    public MarkerVisualizer marker;
    public PupilDataParser parser;

    public NetworkingClient dataSender;

    private bool isCalibrating = false;
    private Vector2 currentEyePosition;
    private bool eyeAvailable = false;
    public float kMinConfidence = 0.65f;
    public int kChosenEye = 0;

    public Vector4 coefficient;
    public Vector2 intercept;

    public event Action OnCalibrationStarted;
    public event Action OnCalibrationSucceeded;
    public event Action OnValidationCompleted;
    private Coroutine calibRoutine;
    private Coroutine validRoutine;

    private List<Vector4> dataPoints;

    void Awake()
    {
        currentEyePosition = Vector2.zero;
        dataPoints = new List<Vector4>();
        dataSender.OnCalibrationPointProcessed += LastPointCallback;
    }

    private bool lastPointResultReady = false;
    private bool lastPointAccepted;
    private void LastPointCallback(bool result)
    {
        lastPointAccepted = result;
        lastPointResultReady = true;
    }

    public bool UpdateParams()
    {
        StreamReader reader = new StreamReader("C:\\Users\\chich\\Desktop\\mapping_params.txt");
        float x = float.Parse(reader.ReadLine());
        float y = float.Parse(reader.ReadLine());
        float z = float.Parse(reader.ReadLine());
        float w = float.Parse(reader.ReadLine());
        float ix = float.Parse(reader.ReadLine());
        float iy = float.Parse(reader.ReadLine());

        coefficient = new Vector4(x, y, z, w);
        intercept = new Vector2(ix, iy);
        return true;
    }

    private void ReceivePupilData(int eyeidx, double timestamp, float confidence, Vector2 ellipseCenter)
    {
        if (eyeidx == kChosenEye)
        {
            if (confidence < kMinConfidence)
            {
                eyeAvailable = false;
                return;
            }
            currentEyePosition = ellipseCenter;
            eyeAvailable = true;
        }
    }

    public void StartCalibration()
    {
        if (!isCalibrating)
        {
            OnCalibrationStarted();
            parser.OnDataParsed += ReceivePupilData;
            calibRoutine = StartCoroutine(CalibrationRoutine());
        }
    }

    public void StartValidation()
    {
        if (!isCalibrating)
        {
            if (UpdateParams())
            {
                //OnCalibrationStarted();
                parser.OnDataParsed += ReceivePupilData;
                validRoutine = StartCoroutine(ValidationRoutine());
            }
        }
    }

    private void UpdateMarker(int idx, bool active)
    {
        if (idx == -1)
        {
            marker.transform.position = Vector3.down;
            return;
        }
        marker.transform.localPosition = targets.GetLocalTargetPosAt(idx);
        marker.SetGreen(active);
    }

    private Vector2 WorldToDataPos(Vector3 pos)
    {
        return new Vector2(pos.x, pos.y);
    }

    
    private void TakeSample()
    {
        //DataLogger.LogValidationPoint(WorldToDataPos(marker.transform.localPosition), currentEyePosition);
        Vector2 markerPos = WorldToDataPos(marker.transform.localPosition);
        Vector4 collated = new Vector4(
            markerPos.x,
            markerPos.y,
            currentEyePosition.x,
            currentEyePosition.y
            );

        /*dataPoints.Add();*/
        dataSender.SendData(collated);
    }

    private IEnumerator CalibrationRoutine()
    {
        isCalibrating = true;
        DataLogger.NextCalibration();
        dataPoints.Clear();
        for (int i = 0; i < targets.GetTargetCount(); i++)
        {
            UpdateMarker(i, false);
            yield return new WaitForSeconds(settings.ignoreInitialSeconds);
            UpdateMarker(i, true);
            for (int n = 0; n < settings.samplesPerTarget; n++)
            {
                float start = Time.time;
                TakeSample();
                while (!lastPointResultReady)
                {
                    yield return null;
                }
                float timeTaken = Time.time - start;
                Debug.Log("> Network time taken: " + timeTaken.ToString());
                lastPointResultReady = false;
                if (!lastPointAccepted)
                {
                    n--;
                }
                yield return new WaitForSeconds(1f / settings.SampleRate);
            }
        }
        UpdateMarker(-1, false);
        DataLogger.Close();
        parser.OnDataParsed -= ReceivePupilData;
        isCalibrating = false;
        OnCalibrationSucceeded();
    }

    private IEnumerator ValidationRoutine()
    {
        isCalibrating = true;
        DataLogger.NextValidation();
        for (int i = 0; i < targets.GetTargetCount(); i++)
        {
            Vector2 runningSampleAvg = Vector2.zero;
            int runningSampleCount = 0;

            UpdateMarker(i, false);
            yield return new WaitForSeconds(settings.ignoreInitialSeconds);
            UpdateMarker(i, true);
            for (int n = 0; n < settings.samplesPerTarget; n++)
            {
                if (eyeAvailable)
                {
                    runningSampleAvg += LinearTransform(currentEyePosition) - WorldToDataPos(marker.transform.localPosition);
                    runningSampleCount += 1;
                }
                else
                {
                    n--;
                }
                yield return new WaitForSeconds(1f / settings.SampleRate);
            }

            DataLogger.LogValidationPoint(WorldToDataPos(marker.transform.localPosition), runningSampleAvg / runningSampleCount);
        }
        UpdateMarker(-1, false);
        DataLogger.Close();
        parser.OnDataParsed -= ReceivePupilData;
        isCalibrating = false;
        OnValidationCompleted();
    }

    public void CancelCalibration()
    {
        StopCoroutine(calibRoutine);
        UpdateMarker(-1, false);
    }

    public void CancelValidation()
    {
        StopCoroutine(validRoutine);
        UpdateMarker(-1, false);
    }

    public Vector2 LinearTransform(Vector2 pos)
    {
        float x = pos.x * coefficient.x + pos.y * coefficient.y;
        float y = pos.x * coefficient.z + pos.y * coefficient.w;
        return new Vector2(x, y) + intercept;
    }
}
