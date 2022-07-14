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

    private bool isCalibrating = false;
    private Vector2 currentEyePosition;
    private bool eyeAvailable = false;
    public float kMinConfidence = 0.65f;
    public int kChosenEye = 0;

    public Vector4 coefficient;
    public Vector2 intercept;

    void Awake()
    {
        currentEyePosition = Vector2.zero;
    }

    public bool UpdateParams()
    {
        StreamReader reader = new StreamReader("C:\\Users\\chich\\Desktop\\mapping_params.txt");
        float w = float.Parse(reader.ReadLine());
        float y = float.Parse(reader.ReadLine());
        float x = float.Parse(reader.ReadLine());
        float z = float.Parse(reader.ReadLine());
        float ix = float.Parse(reader.ReadLine());
        float iy = float.Parse(reader.ReadLine());

        coefficient = new Vector4(w, x, y, z);
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
            parser.OnDataParsed += ReceivePupilData;
            StartCoroutine(CalibrationRoutine());
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
        DataLogger.LogValidationPoint(WorldToDataPos(marker.transform.localPosition), currentEyePosition);
    }

    private IEnumerator CalibrationRoutine()
    {
        isCalibrating = true;
        DataLogger.NextCalibration();
        for (int i = 0; i < targets.GetTargetCount(); i++)
        {
            UpdateMarker(i, false);
            yield return new WaitForSeconds(settings.ignoreInitialSeconds);
            UpdateMarker(i, true);
            for (int n = 0; n < settings.samplesPerTarget; n++)
            {
                if (eyeAvailable)
                {
                    TakeSample();
                } else
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
    }

    public Vector2 LinearTransform(Vector2 pos)
    {
        float x = pos.x * coefficient.w + pos.y * coefficient.y;
        float y = pos.x * coefficient.x + pos.y * coefficient.z;
        return new Vector2(x, y) + intercept;
    }
}
