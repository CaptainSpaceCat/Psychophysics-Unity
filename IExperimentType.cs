using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExperimentType
{
    void Next();
    void Clear();
    void LogDataPoint(Vector2 point, float time);
}
