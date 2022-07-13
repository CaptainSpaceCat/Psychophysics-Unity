using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePosition : MonoBehaviour
{
    public int pos = 0;
    float currentTime;
    int kSamples = 100;
    int kDimX = 10;

    // Start is called before the first frame update
    void Start()
    {
        currentTime = UnityEngine.Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (UnityEngine.Time.time - currentTime > 3 && GameObject.Find("Calibration Controller").GetComponent<PupilLabs.CalibrationController>().CalibDone)
        {
            int idx = pos % kSamples;
            int y = idx / kDimX;
            int x = idx % kDimX;

            if (y % 2 == 1)
            {
                x = kDimX - x - 1;
            }

            this.transform.localPosition = new Vector3(-1.8f + (float)x / (float)kDimX * 3.6f, 0f, 1.8f - (float)y / (float)kDimX * 3.6f);

            currentTime = UnityEngine.Time.time;
            pos += 1;
        }
        else if (!GameObject.Find("Calibration Controller").GetComponent<PupilLabs.CalibrationController>().CalibDone)
        {
            pos = 0;
            currentTime = UnityEngine.Time.time + 5f;
        }
    }
}
