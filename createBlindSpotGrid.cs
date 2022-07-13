/*
 * createBlindSpotGrid.cs
 * 
 * Description: Create and control blind spot dots to investigate blind spot region
 * 
 * Parameters:
 *  ---- General
 *  - Blind Theta: angle of deviation between each dot
 *  - Grid Mode: select to turn on grid mode
 *  ---- Grid Mode Params
 *  - Rows: # rows (before symmetry across x-axis)
 *  - Columns: # columns on either side of y-axis (must be greater than 0)
 *  - Vertical Separation: vertical distance between rows
 *  ---- Control Mode Params
 *  - Delta: incremental movement distance
 *  
 *  Inputs (Control Mode):
 *  - z: Move left
 *  - c: Move right
 *  - s: Move up
 *  - x: Move down
 *  - d: Scale up
 *  - f: Scale down
 *  - k: Toggle left object on/off
 *  - l: Toggle right object on/off
 *  - v: Print locations
 *  
 *  Author: Paul Jolly
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createBlindSpotGrid : MonoBehaviour
{
    // Unity user parameters
    public float blindTheta = 15f;
    public bool gridMode = true;

    [Header("Grid Mode Settings")]
    public int rows = 1;
    public int columns = 1;
    public float vertSeparation = 0.5f;

    [Header("Control Mode Settings")]
    public float delta = 0.1f;

    // Private Variables
    Transform mainCam;
    int totalPoints = 0;
    GameObject[] objs;
    float d, theta;
    float scale = 0.02f;

    void Start()
    {
        mainCam = Camera.main.transform;
        d = mainCam.InverseTransformPoint(this.transform.position).z;
        theta = blindTheta * Mathf.PI / 180f;

        // Grid Mode -------------------------------------------------------
        if (gridMode)
        {
            totalPoints = (2 * columns + 1) * (2 * (rows - 1) + 1);
            objs = new GameObject[totalPoints];

            // Create Objects
            for (int i = 0; i < totalPoints; i++)
            {
                objs[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                objs[i].GetComponent<Renderer>().material = (Material)Resources.Load("Materials/target");
                objs[i].transform.SetParent(this.transform, true);
                objs[i].transform.localScale = new Vector3(scale, scale, scale);
            }

            // Place objects
            objs[0].transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            for (int r = 1; r < rows; r++)
            {
                objs[2 * columns + 2 * (r - 1) + 1].transform.localPosition = new Vector3(0.0f, 0.0f, r * vertSeparation);
                objs[2 * columns + 2 * (r - 1) + 2].transform.localPosition = new Vector3(0.0f, 0.0f, -r * vertSeparation);
            }

            for (int c = 0; c < columns; c++)
            {
                float offset = d * Mathf.Tan((c + 1) * theta);
                objs[2 * c + 1].transform.localPosition = new Vector3(offset, 0.0f, 0.0f);
                objs[2 * c + 2].transform.localPosition = new Vector3(-offset, 0.0f, 0.0f);

                for (int r = 0; r < rows - 1; r++)
                {
                    objs[2 * columns + 2 * (rows - 1) + 4 * (rows - 1) * c + 4 * r + 1].transform.localPosition = new Vector3(offset, 0.0f, (r + 1) * vertSeparation);
                    objs[2 * columns + 2 * (rows - 1) + 4 * (rows - 1) * c + 4 * r + 2].transform.localPosition = new Vector3(offset, 0.0f, -(r + 1) * vertSeparation);
                    objs[2 * columns + 2 * (rows - 1) + 4 * (rows - 1) * c + 4 * r + 3].transform.localPosition = new Vector3(-offset, 0.0f, (r + 1) * vertSeparation);
                    objs[2 * columns + 2 * (rows - 1) + 4 * (rows - 1) * c + 4 * r + 4].transform.localPosition = new Vector3(-offset, 0.0f, -(r + 1) * vertSeparation);
                }
            }
        }
        // Control Mode ----------------------------------------------------
        else 
        {
            totalPoints = 3;
            objs = new GameObject[totalPoints];

            // Create Objects
            for (int i = 0; i < totalPoints; i++)
            {
                objs[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                objs[i].GetComponent<Renderer>().material = (Material)Resources.Load("Materials/target");
                objs[i].transform.SetParent(this.transform, true);
                objs[i].transform.localScale = new Vector3(scale, scale, scale);
            }

            // Place Objects
            objs[0].transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 targPosLocal = objs[0].transform.localPosition;
            Vector3 targPos = objs[0].transform.position;

            print("targLocal = " + targPosLocal);
            print("targPos = " + targPos);
            float a = Mathf.Sqrt(Mathf.Pow(targPos.x, 2) + Mathf.Pow(targPos.z, 2));
            float diff = Mathf.Sqrt(Mathf.Pow(a, 2) - Mathf.Pow(d, 2));
            float offset = d * Mathf.Tan(Mathf.Acos(d / a) + theta) - diff;

            objs[1].transform.localPosition = new Vector3(targPosLocal.x - offset, targPosLocal.y, targPosLocal.z);
            objs[2].transform.localPosition = new Vector3(targPosLocal.x + offset, targPosLocal.y, targPosLocal.z);
        }
    }

    void Update()
    {
        if (!gridMode)
        {
            Vector3 targPosLocal = objs[0].transform.localPosition;
            bool update = false;

            // Key Controls
            if (Input.GetKeyDown(KeyCode.Z)) // Left
            {
                objs[0].transform.localPosition = new Vector3(targPosLocal.x - delta, targPosLocal.y, targPosLocal.z);
                update = true;
            } else if (Input.GetKeyDown(KeyCode.C)) // Right
            {
                objs[0].transform.localPosition = new Vector3(targPosLocal.x + delta, targPosLocal.y, targPosLocal.z);
                update = true;
            } else if (Input.GetKeyDown(KeyCode.S)) // Up
            {
                objs[0].transform.localPosition = new Vector3(targPosLocal.x, targPosLocal.y, targPosLocal.z + delta);
                update = true;
            } else if (Input.GetKeyDown(KeyCode.X)) // Down
            {
                objs[0].transform.localPosition = new Vector3(targPosLocal.x, targPosLocal.y, targPosLocal.z - delta);
                update = true;
            } else if (Input.GetKeyDown(KeyCode.D)) // Scale down
            {
                scale -= 0.01f;
                update = true;
            } else if (Input.GetKeyDown(KeyCode.F)) // Scale up
            {
                scale += 0.01f;
                update = true;
            }
            else if (Input.GetKeyDown(KeyCode.K)) // Toggle left
            {
                objs[1].SetActive(!objs[1].activeInHierarchy);
            }
            else if (Input.GetKeyDown(KeyCode.L)) // Toggle right
            {
                objs[2].SetActive(!objs[2].activeInHierarchy);
            }
            else if (Input.GetKeyDown(KeyCode.V)) // Print
            {
                targPosLocal = objs[0].transform.localPosition;
                Vector3 objPosLocal_L = objs[1].transform.localPosition;
                Vector3 objPosLocal_R = objs[2].transform.localPosition;

                print("Target Pos: {" + targPosLocal.x + ", "+targPosLocal.y + ", " + targPosLocal.z + "}"+
                    "\nLeft Pos: {" + objPosLocal_L.x + ", " + objPosLocal_L.y + ", " + objPosLocal_L.z + "}" +
                    "; Right Pos: {" + objPosLocal_R.x + ", " + objPosLocal_R.y + ", " + objPosLocal_R.z + "}" +
                    "; Scale: " + scale);
            }

            // Update locations
            if (update)
            {
                targPosLocal = objs[0].transform.localPosition;
                Vector3 targPos = mainCam.InverseTransformPoint(objs[0].transform.position);
                float a = Mathf.Sqrt(Mathf.Pow(targPos.x, 2) + Mathf.Pow(targPos.z, 2));
                float diff = Mathf.Sqrt(Mathf.Pow(a, 2) - Mathf.Pow(d, 2));


                float offset_1 = d * Mathf.Tan(Mathf.Acos(d / a) + theta) - diff;
                float offset_2 = diff + d * Mathf.Tan(theta - Mathf.Acos(d / a));

                scale = Mathf.Max(0.0f, scale);
                objs[1].transform.localScale = new Vector3(scale, scale, scale);
                objs[2].transform.localScale = new Vector3(scale, scale, scale);

                float offset_L;
                float offset_R;
                if (targPosLocal.x >= this.transform.localPosition.x)
                {
                    offset_L = -offset_2;
                    offset_R = offset_1;
                }
                else
                {
                    offset_L = -offset_1;
                    offset_R = offset_2;
                }


                objs[1].transform.localPosition = new Vector3(targPosLocal.x + offset_L, targPosLocal.y, targPosLocal.z);
                objs[2].transform.localPosition = new Vector3(targPosLocal.x + offset_R, targPosLocal.y, targPosLocal.z);


                // DEBUG: Print all detailed coordinates (Uncomment below)
                //Vector3 objPosLocal_L = objs[1].transform.localPosition;
                //Vector3 objPosLocal_R = objs[2].transform.localPosition;
                //print("Target Pos: {" + targPosLocal.x + ", " + targPosLocal.y + ", " + targPosLocal.z + "}" +
                //    "; Offset_1: " + offset_1 + "; Offset_2: " + offset_2 +
                //    "\nLeft Pos: {" + objPosLocal_L.x + ", " + objPosLocal_L.y + ", " + objPosLocal_L.z + "}" +
                //    "; Right Pos: {" + objPosLocal_R.x + ", " + objPosLocal_R.y + ", " + objPosLocal_R.z + "}" +
                //    "; Offset_L: " + offset_L + "; Offset_R: " + offset_R + "; a: " + a + "; d: " + d + "; diff: " + diff);
            }
        }
    }
}
