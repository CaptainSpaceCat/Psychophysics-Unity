using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GazeRaycaster : MonoBehaviour
{
    public BackdropShaderController shader;
    public PupilDataParser gazeParser;
    public CalibrationNotPupil calib;
    public NetworkingClient netClient;
    public Camera vrCam;
    public SystemTime sysTime;
    
    public bool debugGazeDir = false;
    public Vector3 testGazeDir;

    [Range(0f, 1f)]
    public float confidenceThreshold = 0.6f;
    public float degreesEccentricity;
    public Vector2 windowSize;

    OneEuroFilter<Vector3> gazeFilter;

    public bool filterOn = true;

    public float filterFrequency = 120.0f;
    public float filterMinCutoff = 1.0f;
    public float filterBeta = 0.0f;
    public float filterDcutoff = 1.0f;

    public event Action<Vector2, Vector2, float> OnRaycastSuccessful;

    private int raycastMode = 0;

    void Awake()
    {
        gazeFilter = new OneEuroFilter<Vector3>(filterFrequency);

        // set event listeners
        netClient.OnGazeDataReceived += ConsumeGazeData;
        savedMut = new Mutex();
    }

    private float prevDataTimestamp;
    public float dataTimeThreshold = 0.017f;
    private void Update()
    {
        if (debugGazeDir)
        {
            CalculateWindowPos(Vector2.zero, 1);
        }

        if (raycastMode > 0)
        {
            // "lag spike" fix
            // turns out the data sometimes stops coming for about .17 seconds
            // this is enough time for the user to foveate on the window and "cheat"
            // to fix this, we take timestamps of every time a data frame is collected and used to render the window
            // if more than 1/60th of a second passes before we get a new data frame, assume the data is temporarily blocked and shut the window
            if (Time.time - prevDataTimestamp > dataTimeThreshold)
            {
                shader.ShutWindow();
            }
        }

        if (gazeAvailable)
        {
            savedMut.WaitOne();
            CalculateWindowPos(savedCenter, savedConfidence);
            savedMut.ReleaseMutex();
        }
    }

    public void SetRaycastMode(int mode)
    {
        raycastMode = mode;
    }

    private bool gazeAvailable = false;
    private Vector2 savedCenter;
    private float savedConfidence;
    private Mutex savedMut;
    public GameObject dummytest;
    public void ConsumeGazeData(Vector2 center, float confidence)
    {
        dummytest.transform.localPosition = new Vector3(center.x + 1f, center.y, 2);
        //CalculateWindowPos(center, confidence);

        /*
        savedMut.WaitOne();
        savedCenter = center;
        savedConfidence = confidence;
        gazeAvailable = true;
        savedMut.ReleaseMutex();
        */
    }

    private float prevFilterTime = 0;
    public int kChosenEye = 0;
    public void CalculateWindowPos(Vector2 center, float confidence)
    {
        if (raycastMode == 0)// || eyeidx != kChosenEye)
        {
            return;
        }

        if (!debugGazeDir && confidence < confidenceThreshold)
        {
            // if we aren't debugging and aren't confident in where the user is looking,
            // just shut the window entirely to prevent cheating
            //TODO: shut the window if the change between this frame and last frame is too large, should help mitigate oscillations between 2 points
            shader.ShutWindow();
            return;
        }
        Vector3 gazeDirection = CenterPosToGazeDir(center);
        if (debugGazeDir)
        {
            gazeDirection = testGazeDir;
        }

        if (raycastMode == 1)
        {
            FollowMode(gazeDirection);
        } else if (raycastMode == 2)
        {
            //LockedMode(gazeDirection);
            Debug.LogError("Locked mode currently under construction. Why are you even using this?");
        }
    }

    private Vector3 CenterPosToGazeDir(Vector2 centerPos)
    {
        return new Vector3(centerPos.x, centerPos.y, 2); //TODO make this 2 not hardcoded, its based on the distance from the camera to the blocking plane
    }

    private void FollowMode(Vector3 rawGazeDir)
    {
        //sysTime.Log(gazeData.PupilTimestamp.ToString() + "-recieved");
        float filterDeltaTime = Time.time - prevFilterTime;
        if (prevFilterTime > 0)
        {
            //Debug.Log(filterDeltaTime);
            //gazeFilter.UpdateParams(1 / filterDeltaTime);
        }

        // Calculate the correct raycast vector based on gaze direction, eccentricity, and filter/debug status
        
        Vector3 gazeDir = rawGazeDir;
        if (filterOn)
        {
            gazeDir = gazeFilter.Filter(rawGazeDir);
            //Debug.Log("Actual: " + gazeDir + " Raw: " + rawGazeDir);
        }

        Vector3 horizontalAxis = Vector3.Cross(Vector3.left, gazeDir).normalized;
        Vector3 verticalAxis = Vector3.Cross(horizontalAxis, gazeDir).normalized;
        Vector3 rotatedGazeDir = Quaternion.AngleAxis(degreesEccentricity, horizontalAxis) * gazeDir;
        Vector3 worldSpaceGazeDir = vrCam.transform.TransformDirection(gazeDir);
        Vector3 worldSpaceRotatedGazeDir = vrCam.transform.TransformDirection(rotatedGazeDir);
        Vector3 cornerGazeDirTL = vrCam.transform.TransformDirection(Quaternion.AngleAxis(windowSize.x / 2, horizontalAxis) * Quaternion.AngleAxis(-windowSize.y / 2, verticalAxis) * rotatedGazeDir);
        Vector3 cornerGazeDirBR = vrCam.transform.TransformDirection(Quaternion.AngleAxis(-windowSize.x / 2, horizontalAxis) * Quaternion.AngleAxis(windowSize.y / 2, verticalAxis) * rotatedGazeDir);

        RaycastHit overlayHit, windowHit, dotHit, backdropHitTL, backdropHitBR;
        // set raycast layer to collide only with the background
        LayerMask overlayMask = LayerMask.GetMask("Overlay");

        // Raycast and set the location of the window to where the user is looking
        Physics.Raycast(vrCam.transform.position, worldSpaceGazeDir,
            out dotHit, Mathf.Infinity, overlayMask);
        Vector2 dotCoords = dotHit.textureCoord;

        Physics.Raycast(vrCam.transform.position, cornerGazeDirTL,
            out backdropHitTL, Mathf.Infinity, overlayMask);
        Physics.Raycast(vrCam.transform.position, cornerGazeDirBR,
            out backdropHitBR, Mathf.Infinity, overlayMask);

        LayerMask backdropMask = LayerMask.GetMask("Backdrop");
        if (Physics.Raycast(vrCam.transform.position, worldSpaceRotatedGazeDir,
            out windowHit, Mathf.Infinity, backdropMask))
        {
            Vector3 oHit = Vector3.zero;
            if (Physics.Raycast(vrCam.transform.position, worldSpaceRotatedGazeDir,
            out overlayHit, Mathf.Infinity, overlayMask))
            {
                oHit = overlayHit.collider.transform.InverseTransformPoint(overlayHit.point);
            }
            Vector3 wHit = windowHit.collider.transform.InverseTransformPoint(windowHit.point);
            OnRaycastSuccessful(new Vector2(oHit.x, oHit.z), new Vector2(wHit.x, wHit.y), Time.time);
        }
        prevDataTimestamp = Time.time;
        shader.Rerender(backdropHitTL.textureCoord, backdropHitBR.textureCoord, dotCoords);
        //sysTime.Log(gazeData.PupilTimestamp.ToString() + "-shaded");
        prevFilterTime = Time.time;
    }

    /*
    [Range(.99f, 1f)]
    public float gazeDeviationThreshold = 0.92f;
    private void LockedMode(PupilLabs.GazeData gazeData)
    {
        // Vector stuff
        Vector3 gazeDir = gazeData.GazeDirection;
        if (debugGazeDir)
        {
            gazeDir = testGazeDir;
        }

        Vector3 worldSpaceGazeDir = vrCam.transform.TransformDirection(gazeDir);
        Vector3 goalDir = Vector3.forward;

        Vector3 horizontalAxis = Vector3.Cross(Vector3.left, goalDir).normalized;
        Vector3 verticalAxis = Vector3.Cross(horizontalAxis, goalDir).normalized;
        Vector3 rotatedGoalDir = Quaternion.AngleAxis(degreesEccentricity, horizontalAxis) * goalDir;
        Vector3 worldSpaceGoalDir = vrCam.transform.TransformDirection(goalDir);
        Vector3 worldSpaceRotatedGoalDir = vrCam.transform.TransformDirection(rotatedGoalDir);


        Vector3 cornerGazeDirTL = vrCam.transform.TransformDirection(Quaternion.AngleAxis(windowSize.x / 2, horizontalAxis) * Quaternion.AngleAxis(-windowSize.y / 2, verticalAxis) * rotatedGoalDir);
        Vector3 cornerGazeDirBR = vrCam.transform.TransformDirection(Quaternion.AngleAxis(-windowSize.x / 2, horizontalAxis) * Quaternion.AngleAxis(windowSize.y / 2, verticalAxis) * rotatedGoalDir);

        // Raycasts
        LayerMask overlayMask = LayerMask.GetMask("Overlay");

        RaycastHit windowHit, dotHit, backdropHitTL, backdropHitBR;
        Physics.Raycast(vrCam.transform.position, worldSpaceRotatedGoalDir,
            out windowHit, Mathf.Infinity, overlayMask);
        Physics.Raycast(vrCam.transform.position, worldSpaceGoalDir,
            out dotHit, Mathf.Infinity, overlayMask);

        Physics.Raycast(vrCam.transform.position, cornerGazeDirTL,
            out backdropHitTL, Mathf.Infinity, overlayMask);
        Physics.Raycast(vrCam.transform.position, cornerGazeDirBR,
            out backdropHitBR, Mathf.Infinity, overlayMask);

        // Control
        if ((!debugGazeDir && gazeData.Confidence < confidenceThreshold) || (Vector3.Dot(goalDir.normalized, gazeDir.normalized) < gazeDeviationThreshold))
        {
            shader.ShutWindow();
            return;
        }
        shader.Rerender(backdropHitTL.textureCoord, backdropHitBR.textureCoord, dotHit.textureCoord);
        OnRaycastSuccessful(gazeData, Vector2.zero, windowHit.collider.transform.InverseTransformPoint(windowHit.point), Time.time);
    }
    */
}



