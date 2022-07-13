/*
 * SetToGazePos.cs
 * 
 * Description: Attach center/left/right objects to gaze position and render at specified eccentricities 
 * 
 * Parameters:
 *  - Hit Marker: Gaze Visualizer hit marker
 *  - Direction Hit Marker: Gaze Visualizer direction hit marker
 *  - Eccentricity Angle: Offset angle from gaze point
 *  - Center: Select to include center object
 *  - Center Object: center object to display
 *  - Left: Select to include left object
 *  - Left Object: left object to display
 *  - Right: Select to include right object
 *  - Right Object: right object to display
 *  
 *  Inputs:
 *  - z: Print details
 *  
 *  Author: Paul Jolly
 */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Psychophysics
{
    public class SetToGazePos : MonoBehaviour
    {
        // Unity user parameters
        public GameObject hitMarker;
        public GameObject dirHitMarker;
        public float blindSpotAngle = 15;
        public bool gazePoints = false;
        public float separationAngle = 10f;
        public bool center;
        public GameObject centerObj;
        public bool left;
        public GameObject leftObject;
        public bool right;
        public GameObject rightObject;
        public GameObject[] objs;
        public float delayTime = 0.5f;
        public float distThres = 0.02f;


        [Header("Record Settings")]
        public bool record = false;
        public string saveDir = "C:\\Users\\chich\\Documents\\psychophysics\\MATLAB\\TrajData";

        // Private variables
        Transform gazeDirectionMarker;
        Transform mainCam;
        Transform background;
        int numObj = 2;
        float scale = 0.005f;
        Vector3 gazePosPrev;

        float offset_L = 0;
        float offset_R = 0;
        float blindRad = 0;
        float offset, zDist, sepRad;
        bool toggleOff = false;

        void Start()
        {
            gazeDirectionMarker = GameObject.Find("Gaze Visualizer").GetComponent<PupilLabs.GazeVisualizer>().projectionMarker;
            mainCam = Camera.main.transform;
            Vector3 camPos = mainCam.position;
            background = this.transform;
            this.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/transparent");
            blindRad = blindSpotAngle * Mathf.PI / 180f;
            //print("Cam pos: " + mainCam.InverseTransformPoint(camPos));

            if (!center)
            {
                centerObj.SetActive(false);
            }
            else
            {
                hitMarker.GetComponent<MeshRenderer>().enabled = false;
                dirHitMarker.GetComponent<MeshRenderer>().enabled = false;
                leftObject.GetComponent<MeshRenderer>().enabled = false;
                rightObject.GetComponent<MeshRenderer>().enabled = false;

            }


            
            if (gazePoints)
            {
                zDist = mainCam.InverseTransformPoint(this.transform.position).z;
                sepRad = separationAngle * Mathf.PI / 180f;
                offset = zDist * Mathf.Tan(sepRad);
                objs = new GameObject[numObj];
                for (int i = 0; i < numObj; i++)
                {
                    objs[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    objs[i].GetComponent<Renderer>().material = (Material)Resources.Load("Materials/target");
                    objs[i].transform.SetParent(this.transform, true);
                    objs[i].transform.localScale = new Vector3(scale, scale, scale);
                }
                objs[0].transform.localPosition = new Vector3(offset, 0.0f, 0.0f);
                //objs[0].GetComponent<MeshRenderer>().enabled = false;
                objs[1].transform.localPosition = new Vector3(-offset, 0.0f, 0.0f);
                //objs[1].GetComponent<MeshRenderer>().enabled = false;
            }
            gazePosPrev = mainCam.InverseTransformPoint(gazeDirectionMarker.position);

        }

        void FixedUpdate()
        {
            int eye = GameObject.Find("Gaze Visualizer").GetComponent<PupilLabs.GazeVisualizer>().eye;
            Vector3 gazePosLocal = mainCam.InverseTransformPoint(gazeDirectionMarker.position);
            Vector3 backgroundLocal = mainCam.InverseTransformPoint(background.position);

            float d = backgroundLocal.z;
            float a = Mathf.Sqrt(Mathf.Pow(gazePosLocal.x, 2) + Mathf.Pow(gazePosLocal.z, 2));
            float diff = Mathf.Sqrt(Mathf.Pow(a, 2) - Mathf.Pow(d, 2));

            float offset_1 = d * Mathf.Tan(Mathf.Acos(d / a) + blindRad) - diff;
            float offset_2 = diff + d * Mathf.Tan(blindRad - Mathf.Acos(d / a));

            if (gazePosLocal.x >= backgroundLocal.x)
            {
                offset_L = -offset_2;
                offset_R = offset_1;
            }
            else
            {
                offset_L = -offset_1;
                offset_R = offset_2;
            }

            if (left || right)
            {
                if (eye == 2)
                {
                    leftObject.GetComponent<MeshRenderer>().enabled = true;
                    rightObject.GetComponent<MeshRenderer>().enabled = false;
                }
                else
                {
                    leftObject.GetComponent<MeshRenderer>().enabled = false;
                    rightObject.GetComponent<MeshRenderer>().enabled = true;
                }

                Vector3 leftPosGlobal = mainCam.TransformPoint(new Vector3(gazePosLocal.x + offset_L, gazePosLocal.y, gazePosLocal.z));
                Vector3 rightPosGlobal = mainCam.TransformPoint(new Vector3(gazePosLocal.x + offset_R, gazePosLocal.y, gazePosLocal.z));

                leftObject.transform.position = leftPosGlobal;
                rightObject.transform.position = rightPosGlobal;


                if (Input.GetKeyDown(KeyCode.Z))
                {
                    print("Target Pos: {" + gazePosLocal.x + ", " + gazePosLocal.y + ", " + gazePosLocal.z + "}" +
                    "; Offset_L: " + offset_L + "; Offset_R: " + offset_R + "; Offset_1: " + offset_1 + "; Offset_2: " + offset_2 +
                    ";\na: " + a + "; d: " + d);
                }
                else if (Input.GetKeyDown(KeyCode.G))
                {
                    hitMarker.GetComponent<MeshRenderer>().enabled = !hitMarker.GetComponent<MeshRenderer>().enabled;
                    dirHitMarker.GetComponent<MeshRenderer>().enabled = !dirHitMarker.GetComponent<MeshRenderer>().enabled;
                }
            }

            if (center)
            {
                Vector3 posGlobal = mainCam.TransformPoint(new Vector3(gazePosLocal.x + offset_R, gazePosLocal.y, gazePosLocal.z));
                centerObj.transform.position = posGlobal;

                if (Vector3.Distance(gazePosLocal, gazePosPrev) > distThres)
                {

                    if (!toggleOff)
                    {
                        toggleOff = true;
                        StartCoroutine(toggleTimer());
                    }
                }
                gazePosPrev = gazePosLocal;
            }
        }
        IEnumerator toggleTimer()
        {
            //toggleOff = true;
            centerObj.SetActive(false);
            yield return new WaitForSeconds(delayTime);
            centerObj.SetActive(true);
            toggleOff = false;
        }
    }
}

