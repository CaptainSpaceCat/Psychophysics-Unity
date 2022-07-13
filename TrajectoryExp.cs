/*
 * TrajectoryExp.cs
 * 
 * Description: Eye saccade trajectory experiment - records the 1-D eye saccade trajectory
 * between two points and writes to file.
 * 
 * Parameters:
 *  - Save Directory: directory where recorded data will be saved
 *  - Deg of Visual Angle: offset angle from center fixation point of left/right targets
 *  - Test Time: experiment duration time (corresponds to length of recording)
 *  
 *  Inputs (Control Mode):
 *  - o: Start experiment (recording)
 *  - i: Show objects (toggle on/off)
 *  - u: Show gaze visualizer (toggle on/off)
 *  
 *  Author: Paul Jolly
 */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Psychophysics
{
    public class TrajectoryExp : MonoBehaviour
    {
        // Unity user parameters
        public string saveDir = "C:\\Users\\chich\\Documents\\psychophysics\\MATLAB\\TrajData";
        public float testTime = 10f;
        public float separationAngle = 10f;
        public float blindSpotAngle = 15f;
        public bool blindTest = true;
        public float delayTime = 0.5f;
        public bool gridOn = true;
        public float distThres = 1.0f;
        public GameObject[] objs;

        [Header("Blind Spot Settings")]
        public bool followGaze = false;
        public GameObject gazeMirror;

        // Private variables
        string fileName;
        Transform gazeProjMarker, gazeDirMarker, gazeProjMarker_m, gazeDirMarker_m;
        Transform mainCam;
        GameObject calibController;
        //GameObject[] objs;
        GameObject[] mirrObjs;
        bool record = false;
        bool showObjs = false;
        float startTime, currentTime;
        float scale = 0.005f;
        int numObj = 3;
        float offset, offset_L, offset_R, zDist, sepRad, blindRad, outerRad, innerRad;
        Vector3 gazePos, gazePosPrev, leftPosGlobal, rightPosGlobal;
        bool toggleOff = false;
        //bool dumpData = false;
        string toWrite = "";

        void Start()
        {
            // Get GameObjects/Transforms
            mainCam = Camera.main.transform;
            gazeProjMarker = GameObject.Find("Gaze Visualizer").GetComponent<PupilLabs.GazeVisualizer>().projectionMarker;
            gazeDirMarker = GameObject.Find("Gaze Visualizer").GetComponent<PupilLabs.GazeVisualizer>().gazeDirectionMarker;
            gazeProjMarker_m = gazeMirror.GetComponent<PupilLabs.MirrorGazeVisualizer>().projectionMarker;
            gazeDirMarker_m = gazeMirror.GetComponent<PupilLabs.MirrorGazeVisualizer>().gazeDirectionMarker;


            calibController = GameObject.Find("Calibration Controller");

            // Calculate position parameters
            zDist = mainCam.InverseTransformPoint(this.transform.position).z;
            sepRad = separationAngle * Mathf.PI / 180f;
            offset = zDist * Mathf.Tan(sepRad);
            print("x-Coordinate of fixation point: " + offset);

            // Create and position fixation point and left/right targets
            if (blindTest)
                numObj += 2;
            objs = new GameObject[numObj];
            for (int i = 0; i < numObj; i++)
            {
                objs[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                objs[i].transform.SetParent(this.transform, true);
                objs[i].transform.localScale = new Vector3(scale, scale, scale);
                if (i < 3)
                {
                    objs[i].GetComponent<Renderer>().material = (Material)Resources.Load("Materials/red");
                } else
                {
                    objs[i].GetComponent<Renderer>().material = (Material)Resources.Load("Materials/target");
                }
                
            }
            objs[0].transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            objs[0].GetComponent<MeshRenderer>().enabled = false;
            objs[1].transform.localPosition = new Vector3(offset, 0.0f, 0.0f);
            objs[1].GetComponent<MeshRenderer>().enabled = false;
            objs[2].transform.localPosition = new Vector3(-offset, 0.0f, 0.0f);
            objs[2].GetComponent<MeshRenderer>().enabled = false;

            // Setup alternate camera objects
            if (blindTest)
            {
                this.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Materials/opaque");
                GameObject[] mirroredObjs = new GameObject[numObj];

                for (int i = 0; i < numObj; i++)
                {
                    mirroredObjs[i] = objs[i];
                }

                gazeMirror.GetComponent<PupilLabs.MirrorGazeVisualizer>().mirroredObjs = mirroredObjs;

                // Place blind spot targets
                objs[3].transform.localScale = new Vector3(scale, scale, scale);
                objs[3].GetComponent<MeshRenderer>().enabled = false;
                objs[4].transform.localScale = new Vector3(scale, scale, scale);
                objs[4].GetComponent<MeshRenderer>().enabled = false;
                blindRad = blindSpotAngle * Mathf.PI / 180f;
            }

            if (!followGaze && blindTest)
            {
                gazePos = mainCam.InverseTransformPoint(objs[1].transform.position); 
                float a = Mathf.Sqrt(Mathf.Pow(gazePos.x, 2) + Mathf.Pow(gazePos.z, 2));
                float diff = Mathf.Sqrt(Mathf.Pow(a, 2) - Mathf.Pow(zDist, 2));
                float offset_1 = zDist * Mathf.Tan(Mathf.Acos(zDist / a) + blindRad) - diff;
                float offset_2 = diff + zDist * Mathf.Tan(blindRad - Mathf.Acos(zDist / a));

                outerRad = offset + offset_1;
                innerRad = offset - offset_2;
                objs[3].transform.localPosition = new Vector3(innerRad, 0.0f, 0.0f);
                objs[4].transform.localPosition = new Vector3(outerRad, 0.0f, 0.0f);
            } else
            {
                gazePosPrev = mainCam.InverseTransformPoint(gazeProjMarker.position);
                //delayTime = 0.4f;
            }
        }

        void Update()
        {
            if (followGaze)
            {
                gazePos = mainCam.InverseTransformPoint(objs[0].transform.position);
            }
            else if (objs[1].GetComponent<MeshRenderer>().enabled)
            {
                gazePos = mainCam.InverseTransformPoint(objs[1].transform.position);
            }
            else if (objs[2].GetComponent<MeshRenderer>().enabled)
            {
                gazePos = mainCam.InverseTransformPoint(objs[2].transform.position);
            }
            else
            {
                gazePos = mainCam.InverseTransformPoint(objs[0].transform.position);
            }

            if (followGaze)
            {
                gazePos = mainCam.InverseTransformPoint(gazeProjMarker.position);
                Vector3 backgroundLocal = mainCam.InverseTransformPoint(this.transform.position);
                float a = Mathf.Sqrt(Mathf.Pow(gazePos.x, 2) + Mathf.Pow(gazePos.z, 2));
                float diff = Mathf.Sqrt(Mathf.Pow(a, 2) - Mathf.Pow(zDist, 2));
                float offset_1 = zDist * Mathf.Tan(Mathf.Acos(zDist / a) + blindRad) - diff;
                float offset_2 = diff + zDist * Mathf.Tan(blindRad - Mathf.Acos(zDist / a));

                if (gazePos.x >= backgroundLocal.x)
                {
                    offset_L = -offset_2;
                    offset_R = offset_1;
                }
                else
                {
                    offset_L = -offset_1;
                    offset_R = offset_2;
                }
                leftPosGlobal = mainCam.TransformPoint(new Vector3(gazePos.x + offset_L, gazePos.y, gazePos.z));
                rightPosGlobal = mainCam.TransformPoint(new Vector3(gazePos.x + offset_R, gazePos.y, gazePos.z));
                objs[3].transform.position = leftPosGlobal;
                objs[4].transform.position = rightPosGlobal;

                //print(Vector3.Distance(gazePos, gazePosPrev));
                //if (Vector3.Distance(gazePos, gazePosPrev) > distThres)
                //{

                //    if (!toggleOff)
                //    {
                //        toggleOff = true;
                //        StartCoroutine(toggleTimer());
                //    }
                //}
                gazePosPrev = gazePos;
            }

            if (Input.GetKeyDown(KeyCode.O) && !record)
            {
                record = true;
                showObjs = false;
                objs[1].GetComponent<MeshRenderer>().enabled = false;
                objs[2].GetComponent<MeshRenderer>().enabled = false;
                gazeProjMarker.GetComponent<MeshRenderer>().enabled = false;
                gazeDirMarker.GetComponent<MeshRenderer>().enabled = false;
                gazeProjMarker_m.GetComponent<MeshRenderer>().enabled = false;
                gazeDirMarker_m.GetComponent<MeshRenderer>().enabled = false;
                startTime = Time.realtimeSinceStartup;
                currentTime = Time.realtimeSinceStartup;
                fileName = saveDir + "\\trajTest_" + System.DateTime.Now.ToString("yy-MM-dd_HH-mm-ss") + ".txt";
                print("Recording");
            }

            if (Input.GetKeyDown(KeyCode.I) && !record)
            {
                showObjs = !showObjs;
            }

            if (Input.GetKeyDown(KeyCode.U) && !record)
            {
                gazeProjMarker.GetComponent<MeshRenderer>().enabled = !gazeProjMarker.GetComponent<MeshRenderer>().enabled;
                gazeDirMarker.GetComponent<MeshRenderer>().enabled = !gazeDirMarker.GetComponent<MeshRenderer>().enabled;
                gazeProjMarker_m.GetComponent<MeshRenderer>().enabled = !gazeProjMarker_m.GetComponent<MeshRenderer>().enabled;
                gazeDirMarker_m.GetComponent<MeshRenderer>().enabled = !gazeDirMarker_m.GetComponent<MeshRenderer>().enabled;
            }

            
            //if (dumpData)
            //{
            //    writeString(toWrite);
            //    dumpData = false;
            //}
        }

        void FixedUpdate()
        {
            if (record)
            {
                Vector3 pos = mainCam.InverseTransformPoint(gazeProjMarker.position);
                toWrite = (Time.realtimeSinceStartup - startTime).ToString() + ",";
                toWrite += (objs[0].GetComponent<MeshRenderer>().enabled ? 1 : 0).ToString() + ",";
                toWrite += (objs[1].GetComponent<MeshRenderer>().enabled ? 1 : 0).ToString() + ",";
                toWrite += (objs[2].GetComponent<MeshRenderer>().enabled ? 1 : 0).ToString() + ",";
                if (blindTest)
                    toWrite += (objs[4].GetComponent<MeshRenderer>().enabled ? 1 : 0).ToString() + ",";
                toWrite += pos.x.ToString() + "," + pos.y.ToString() + "," + pos.z.ToString() + "\n";
                writeString(toWrite);
            }

            if (!calibController.GetComponent<PupilLabs.CalibrationController>().CalibDone)
            {
                for (int i = 0; i < numObj; i++)
                {
                    objs[i].GetComponent<MeshRenderer>().enabled = false;
                }
                record = false;
            }
            else if (record)
            {
                if (Time.realtimeSinceStartup - currentTime > 1)
                {
                    if (objs[0].GetComponent<MeshRenderer>().enabled)
                    {
                        objs[0].GetComponent<MeshRenderer>().enabled = false;
                        objs[1].GetComponent<MeshRenderer>().enabled = true;
                        //objs[3].GetComponent<MeshRenderer>().enabled = true;
                        if (blindTest)
                            objs[4].GetComponent<MeshRenderer>().enabled = true;
                    }
                    else
                    {
                        StartCoroutine(delayTimer());                        
                        objs[1].GetComponent<MeshRenderer>().enabled = !objs[1].GetComponent<MeshRenderer>().enabled;
                        objs[2].GetComponent<MeshRenderer>().enabled = !objs[2].GetComponent<MeshRenderer>().enabled;
                    }

                    currentTime = Time.realtimeSinceStartup;
                }

                // If total time done, end recording
                if (Time.realtimeSinceStartup - startTime > testTime)
                {
                    record = false;
                    print("Done Recording");
                    //dumpData = true;
                }
            }
            else
            {
                objs[0].GetComponent<MeshRenderer>().enabled = true;
                objs[1].GetComponent<MeshRenderer>().enabled = showObjs;
                objs[2].GetComponent<MeshRenderer>().enabled = showObjs;
                //objs[4].GetComponent<MeshRenderer>().enabled = followGaze;

            }
        }

        void writeString(string toWrite)
        {
            StreamWriter writer = new StreamWriter(fileName, true);
            writer.WriteLine(toWrite);
            writer.Close();
        }

        IEnumerator delayTimer() //0.24
        {
            if (blindTest)
                objs[4].GetComponent<MeshRenderer>().enabled = false;

            yield return new WaitForSeconds(delayTime);

            if (!followGaze && blindTest)
            {
                Vector3 newPos = -1 * objs[3].transform.localPosition;
                objs[3].transform.localPosition = -1 * objs[4].transform.localPosition;
                objs[4].transform.localPosition = newPos;
            }

            if (blindTest)
                objs[4].GetComponent<MeshRenderer>().enabled = true;
        }

        IEnumerator toggleTimer()
        {
            //toggleOff = true;
            if (blindTest)
                objs[4].GetComponent<MeshRenderer>().enabled = false;
            yield return new WaitForSeconds(delayTime);
            if (blindTest)
                objs[4].GetComponent<MeshRenderer>().enabled = true;
            toggleOff = false;
        }
    }
}
