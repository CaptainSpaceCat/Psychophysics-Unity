using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectangleExperiment : Experiment
{
    public GameObject rectangle;

    public override void Next()
    {
        base.Next();
        rectangle.SetActive(true);
        RandomizeRectangle();
    }

    public override void Clear()
    {
        // clear objects associated with this experiment
        rectangle.SetActive(false);
    }

    public override void LogDataPoint(Vector2 point, float time)
    {
        // save data from this experiment
        DataLogger.LogGazePoint(point, time);
    }

    private void RandomizeRectangle()
    {
        // rotate the rectangle
        int configuration = Random.Range(0, 2);
        float angle = Random.Range(-15f, 15f);
        rectangle.transform.localRotation = Quaternion.Euler(0f, 0f, angle + configuration * 90);

        // and change its scale
        float sX = Random.Range(1.5f, 3f);
        float multiplier = Random.Range(1.5f, 2.5f);
        rectangle.transform.localScale = new Vector3(sX, sX * multiplier);

        // and position
        float pX = Random.Range(-3f, 3f) * (1 - configuration);
        float pY = Random.Range(-2f, 2f) * configuration;
        rectangle.transform.localPosition = new Vector3(pX, pY, 19);
    }

    public override int GetRaycastMode()
    {
        //0 - disabled, 1 - following, 2 - locked
        return 1;
    }
}
