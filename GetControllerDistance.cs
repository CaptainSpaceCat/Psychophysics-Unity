/*
 * GetControllerDistance.cs
 * 
 * Description: Measure euclidean distance of controller from HMD in world coordinates
 * 
 * Parameters:
 *  - File Path: file to save measurements
 *  
 *  Inputs:
 *  - spacebar: take measurement
 *  
 *  Author: Paul Jolly
 */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GetControllerDistance : MonoBehaviour
{
    // Unity user parameters
    public string filePath = "C:\\Users\\chich\\Desktop\\distMeasurement.txt";

    // Private variables
    private StreamWriter writer;
    private int run_i = 1;

    void writeString(string toWrite)
    {
        StreamWriter writer = new StreamWriter(filePath, true);
        writer.WriteLine(toWrite);
        writer.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        writer = new StreamWriter(filePath, false);
        writer.WriteLine("Distance Measurements");
        writer.WriteLine("==================");
        writer.Close();
    }

    // Update is called once per frame
    void Update()  
    {
        if (Input.GetKeyDown("space"))
        {
            Vector3 headPos = GameObject.Find("VRCamera").transform.position;
            Vector3 leftPos = GameObject.Find("LeftHand").transform.position;
            Vector3 rightPos = GameObject.Find("RightHand").transform.position;
            print("Head Position: (" + headPos[0] + ", " + headPos[1] + ", " + headPos[2] + ")");
            print("Left Position: (" + leftPos[0] + ", " + leftPos[1] + ", " + leftPos[2] + ")");
            print("Right Position: (" + rightPos[0] + ", " + rightPos[1] + ", " + rightPos[2] + ")");

            float leftDist = Vector3.Distance(headPos, leftPos);
            float rightDist = Vector3.Distance(headPos, rightPos);

            print("Left Distance: " + leftDist);
            print("Right Distance: " + rightDist);

            string toWrite = "Run " + run_i + "\n";
            toWrite += "=====\n";
            toWrite += "Head Position: (" + headPos[0] + ", " + headPos[1] + ", " + headPos[2] + ")\n";
            toWrite += "Left Position: (" + leftPos[0] + ", " + leftPos[1] + ", " + leftPos[2] + ")\n";
            toWrite += "Left Dist = " + leftDist + "\n";
            toWrite += "Right Position: (" + rightPos[0] + ", " + rightPos[1] + ", " + rightPos[2] + ")\n";
            toWrite += "Right Dist = " + rightDist + "\n";
            toWrite += "==================";
            writeString(toWrite);
            run_i += 1;
        }
    }
}
