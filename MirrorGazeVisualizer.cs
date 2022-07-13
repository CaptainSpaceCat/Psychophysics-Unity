/*
 * MirrorGazeVisualizer.cs
 * 
 * Description: Replicate gaze visualizer object on alternate camera
 * 
 * Parameters:
 *  - Alternate Cam: camera for secondary eye
 *  - Gaze Visualizer: Pupil labs gaze visualizer object
 *  - Projection Marker: Projection Marker transform to display
 *  - Gaze Direction Marker: Gaze Direction Marker transform to display
 *  - Mirrored Object: object to be mirrored (in blind spot)
 *  - Blind Object: object to display in blind spot
 *  - Blind Theta: blind spot eccentricity angle
 *  
 *  Author: Paul Jolly
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PupilLabs
{
    public class MirrorGazeVisualizer : MonoBehaviour
    {
        // Unity user parameters
        public Transform alternateCam; // Alternate camera
        public GameObject gazeVisualizer;
        public Transform projectionMarker;
        public Transform gazeDirectionMarker;
        public Transform mirroredObject;
        public GameObject[] mirroredObjs;
        public Transform blindObject;
        public float blindTheta = 15f;

        // Private variables
        Transform mainProjMark;
        Transform mainCam;
        GameObject hitMarker;
        GameObject dirHitMarker;
        GameObject[] objs;
        GameObject[] objsToMirror;
        int numObj = 5;
        float scale = 0.005f;

        void Start()
        {
            mainProjMark = gazeVisualizer.GetComponent<PupilLabs.GazeVisualizer>().projectionMarker;
            mirroredObjs = GameObject.Find("Background").GetComponent<Psychophysics.SetToGazePos>().objs;
            //mirroredObjs = GameObject.Find("Background").GetComponent<Psychophysics.TrajectoryExp>().objs;
            hitMarker = projectionMarker.gameObject;
            dirHitMarker = gazeDirectionMarker.gameObject;
            mainCam = Camera.main.transform;

            objs = new GameObject[numObj];
            for (int i = 0; i < numObj; i++)
            {
                objs[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                objs[i].transform.SetParent(this.transform, true);
                objs[i].transform.localScale = new Vector3(scale, scale, scale);
                objs[i].GetComponent<MeshRenderer>().enabled = false;
                if (i < 3)
                {
                    objs[i].GetComponent<Renderer>().material = (Material)Resources.Load("Materials/red");
                }
                else
                {
                    objs[i].GetComponent<Renderer>().material = (Material)Resources.Load("Materials/target");
                }
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                hitMarker.GetComponent<MeshRenderer>().enabled = !hitMarker.GetComponent<MeshRenderer>().enabled;
                dirHitMarker.GetComponent<MeshRenderer>().enabled = !dirHitMarker.GetComponent<MeshRenderer>().enabled;
            }

            // Plot projection marker in alternate camera frame
            Vector3 projMarkPosLocal = mainCam.InverseTransformPoint(mainProjMark.position);
            Vector3 projMarkPosGlobal = alternateCam.TransformPoint(projMarkPosLocal);
            projectionMarker.position = projMarkPosGlobal;

            // Plot gaze direction marker in alternate camera frame
            Vector3 gazeDirPosLocal = mainCam.InverseTransformPoint(mainProjMark.position);
            gazeDirPosLocal = new Vector3(gazeDirPosLocal.x, gazeDirPosLocal.y, gazeDirPosLocal.z);
            Vector3 gazeDirPosGlobal = alternateCam.TransformPoint(gazeDirPosLocal);
            gazeDirectionMarker.position = gazeDirPosGlobal;
            gazeDirectionMarker.LookAt(alternateCam.position);

            // Plot blind spot object in alternate camera frame
            Vector3 objPosLocal = mainCam.InverseTransformPoint(mirroredObject.position);
            Vector3 blindMarkerPos = alternateCam.TransformPoint(objPosLocal);
            blindObject.position = blindMarkerPos;

            //for (int i = 0;i < 4;i++)
            //{
            //    Vector3 mirrObjLocal = mainCam.InverseTransformPoint(mirroredObjs[i].transform.position);
            //    Vector3 mirrObjPos = alternateCam.TransformPoint(mirrObjLocal);
            //    objs[i].GetComponent<MeshRenderer>().enabled = mirroredObjs[i].GetComponent<MeshRenderer>().enabled;
            //    objs[i].transform.position = mirrObjPos;
            //}
            //objs[3].GetComponent<MeshRenderer>().enabled = mirroredObjs[4].GetComponent<MeshRenderer>().enabled;
        }
    }
}
