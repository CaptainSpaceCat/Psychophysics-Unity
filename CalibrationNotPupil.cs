using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationNotPupil : MonoBehaviour
{
    public PupilLabs.CalibrationSettings settings;
    public PupilLabs.CalibrationTargets targets;
    public MarkerVisualizer marker;

    public PupilLabs.SubscriptionsController subsCtrl;
    private PupilLabs.PupilListener listener;

    private bool isCalibrating = false;
    private Vector2 currentEyePosition;
    private bool eyeAvailable = false;
    public float kMinConfidence = 0.6f;
    public int kChosenEye = 0;

    void OnEnable()
    {
        if (listener == null)
        {
            listener = new PupilLabs.PupilListener(subsCtrl);
        }

        listener.Enable();
        listener.OnReceivePupilData += ReceivePupilData;

        currentEyePosition = Vector2.zero;
    }

    void OnDisable()
    {
        listener.Disable();
        listener.OnReceivePupilData -= ReceivePupilData;
    }


    private void ReceivePupilData(Dictionary<string, object> dictionary)
    {
        int eyeidx = System.Int32.Parse(PupilLabs.Helpers.StringFromDictionary(dictionary, "id"));
        if (eyeidx == kChosenEye)
        {
            float confidence = PupilLabs.Helpers.FloatFromDictionary(dictionary, "confidence");
            if (confidence < kMinConfidence)
            {
                eyeAvailable = false;
                return;
            }
            eyeAvailable = true;
            //double PupilTimestamp = PupilLabs.Helpers.DoubleFromDictionary(dictionary, "timestamp");
            Dictionary<object, object> subDic = PupilLabs.Helpers.DictionaryFromDictionary(dictionary, "ellipse");
            Vector3 ellipseCenter = PupilLabs.Helpers.ObjectToVector(subDic["center"]);
            currentEyePosition = WorldToDataPos(ellipseCenter);
        }
    }

    public void StartCalibration()
    {
        if (!isCalibrating)
        {
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
        isCalibrating = false;
    }
}
