using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TumblingEExperiment : Experiment
{
    public GameObject tumblingE;
    public TextMeshProUGUI eText;
    [Range(.05f, .25f)]
    public float size;

    public override void Next()
    {
        base.Next();
        tumblingE.SetActive(true);
        RandomizeE();
    }

    public override void Clear()
    {
        // clear objects associated with this experiment
        tumblingE.SetActive(false);
    }

    public override void LogDataPoint(Vector2 point, float time)
    {
        // save data from this experiment
        DataLogger.LogGazePoint(point, time);
    }

    public override void OnRaycastSuccessful(Vector2 overlay, Vector2 window)
    {
        tumblingE.transform.localPosition = new Vector3(overlay.x, overlay.y, 2f);
    }

    private void RandomizeE()
    {
        // rotate the E
        int configuration = Random.Range(0, 4);
        tumblingE.transform.localRotation = Quaternion.Euler(0f, 0f, configuration * 90);
        eText.fontSize = size;
        
    }

    public override int GetRaycastMode()
    {
        //0 - disabled, 1 - following, 2 - locked
        return 1;
    }
}
