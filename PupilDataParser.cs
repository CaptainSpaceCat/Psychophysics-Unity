using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PupilDataParser : MonoBehaviour
{

    public PupilLabs.SubscriptionsController subsCtrl;
    private PupilLabs.PupilListener listener;

    public event Action<int, double, float, Vector2> OnDataParsed;

    void OnEnable()
    {
        if (listener == null)
        {
            listener = new PupilLabs.PupilListener(subsCtrl);
        }

        listener.Enable();
        listener.OnReceivePupilData += ReceivePupilData;
    }

    private Vector2 WorldToDataPos(Vector3 pos)
    {
        return new Vector2(pos.x, pos.y);
    }

    void ReceivePupilData(Dictionary<string, object> dictionary)
    {
        int eyeidx = System.Int32.Parse(PupilLabs.Helpers.StringFromDictionary(dictionary, "id"));
        double pupilTimestamp = PupilLabs.Helpers.DoubleFromDictionary(dictionary, "timestamp");
        float confidence = PupilLabs.Helpers.FloatFromDictionary(dictionary, "confidence");
        Dictionary<object, object> subDic = PupilLabs.Helpers.DictionaryFromDictionary(dictionary, "ellipse");
        Vector2 ellipseCenter = WorldToDataPos(PupilLabs.Helpers.ObjectToVector(subDic["center"]));
        
        OnDataParsed(eyeidx, pupilTimestamp, confidence, ellipseCenter);
    }
}
