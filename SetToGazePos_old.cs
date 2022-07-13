using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetToGazePos_old : MonoBehaviour
{
    Transform gazeDirectionMarker;
    Transform mainCam;
    public Transform background;
    public float visualAngle = 15;
    public float offset = 0;
    // Start is called before the first frame update
    void Start()
    {
        gazeDirectionMarker = GameObject.Find("Gaze Visualizer").GetComponent<PupilLabs.GazeVisualizer>().projectionMarker;
        mainCam = Camera.main.transform;
        Vector3 camPos = mainCam.position;
        //print("Cam pos: " + mainCam.InverseTransformPoint(camPos));
    }

    // Update is called once per frame
    void Update()
    {
        int eye = GameObject.Find("Gaze Visualizer").GetComponent<PupilLabs.GazeVisualizer>().eye;
        Vector3 gazePosLocal = mainCam.InverseTransformPoint(gazeDirectionMarker.position);
        Vector3 backgroundLocal = mainCam.InverseTransformPoint(background.position);

        float d = backgroundLocal.z;
        float a = Mathf.Sqrt(Mathf.Pow(gazePosLocal.x,2) + Mathf.Pow(gazePosLocal.z,2));
        float diff = Mathf.Sqrt(Mathf.Pow(a, 2) - Mathf.Pow(d, 2));
        float blindAngle = visualAngle * Mathf.PI / 180f;

        float offset_1 = d * Mathf.Tan(Mathf.Acos(d / a) + blindAngle) - diff;
        float offset_2 = diff + d * Mathf.Tan(blindAngle - Mathf.Acos(d / a));

        if (eye == 2)
        {
            if (gazePosLocal.x >= backgroundLocal.x)
            {
                offset = -offset_2;
            }
            else
            {
                offset = -offset_1;
            }
        }
        else
        {
            if (gazePosLocal.x <= backgroundLocal.x)
            {
                offset = offset_2;
            }
            else
            {
                offset = offset_1;
            }
        }
        //print("Background: " + backgroundLocal);
        Vector3 gazePosGlobal = mainCam.TransformPoint(new Vector3(gazePosLocal.x + offset, gazePosLocal.y,gazePosLocal.z));
        this.transform.position = gazePosGlobal;


        if (Input.GetKeyDown(KeyCode.Z)) // Left 
        {
            print("Target Pos: {" + gazePosLocal.x + ", " + gazePosLocal.y + ", " + gazePosLocal.z + "}" +
            "; Offset: " + offset + "; Offset_1: " + offset_1 + "; Offset_2: " + offset_2 +
            ";\na: " + a + "; d: " + d);
        }
    }
}
