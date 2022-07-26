using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PupilData
{
    public float Confidence;
    public Vector2 Position;

    public PupilData(Vector2 pos, float conf)
    {
        Position = pos;
        Confidence = conf;
    }

    public PupilData()
    {
        Position = Vector2.zero;
        Confidence = 0;
    }
}
