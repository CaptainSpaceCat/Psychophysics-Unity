using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RecordGaze : MonoBehaviour
{
    public string filePath = "C:\\Users\\chich\\Desktop\\aditya1.txt";

    void writeString(string toWrite)
    {
        StreamWriter writer = new StreamWriter(filePath, true);
        writer.WriteLine(toWrite);
        writer.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Transform gazeDirectionMarker = GameObject.Find("Gaze Visualizer").GetComponent<PupilLabs.GazeVisualizer>().gazeDirectionMarker;
        float lastConfidence = GameObject.Find("Gaze Visualizer").GetComponent<PupilLabs.GazeVisualizer>().lastConfidence;

        string toWrite = GameObject.Find("Target").GetComponent<UpdatePosition>().pos.ToString() + ",";
        toWrite += gazeDirectionMarker.position.x.ToString() + "," + gazeDirectionMarker.position.y.ToString() + "," + gazeDirectionMarker.position.z.ToString() + ",";
        toWrite += lastConfidence + ",";
        toWrite += GameObject.Find("Target").transform.position.x.ToString() + "," + GameObject.Find("Target").transform.position.y.ToString() + "," + GameObject.Find("Target").transform.position.z.ToString();

        Debug.Log("I'm running");
        Debug.Log(toWrite);

        if (GameObject.Find("Calibration Controller").GetComponent<PupilLabs.CalibrationController>().CalibDone)
            writeString(toWrite);
    }
}
