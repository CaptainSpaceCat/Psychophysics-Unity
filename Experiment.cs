using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment : MonoBehaviour
{
    public virtual void Next()
    {
        DataLogger.NextTrial(Time.time);
    }

    public virtual void Clear()
    {
        throw new MissingReferenceException("\'Clear\' method not overriden from Experiment");
    }

    public virtual void LogDataPoint(Vector2 point, float time)
    {
        throw new MissingReferenceException("\'LogDataPoint\' method not overriden from Experiment");
    }

    public virtual int GetRaycastMode()
    {
        return 0;
    }

    public virtual void OnRaycastSuccessful(Vector2 o, Vector2 w) { }
}
