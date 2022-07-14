using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class CalibrationValidator : MonoBehaviour
{

    [Header("Scene References")]
    public Transform marker;
    public GazeRaycaster gazeRaycaster;
    public GameObject debugCube;
    public ResearcherDisplayController researchDisplay;
    public PupilLabs.CalibrationController calibrationController;
    public bool cheapMode = false;
    public bool debugGazeDir = false;
    public bool debugGazePoint = false;

    //settings
    private PupilLabs.CalibrationSettings settings;
    private PupilLabs.CalibrationTargets targets;

    // variables
    private Vector3 currLocalTargetPos;
    public bool validationCompleted = false;
    private Camera vrCam;

    //events
    public event Action OnValidationCompleted;

    private void Awake()
    {
        vrCam = Camera.main;

        settings = calibrationController.settings;
        targets = calibrationController.targets;
    }
    
    private void Update()
    {
        if (isValidating)
        {
            // move gaze target to target pos at index i
            marker.position = vrCam.transform.localToWorldMatrix.MultiplyPoint(currLocalTargetPos);
            marker.LookAt(vrCam.transform.position);

            //debugCube.transform.position = vrCam.transform.localToWorldMatrix.MultiplyPoint(currLocalTargetPos) + runningSampleAverage / (totalSamples - remainingSamples);
        }
    }

    private bool isValidating = false;
    private List<Vector3> deltaGraph;
    private Vector3 runningSampleAverage;
    private Coroutine validationRoutine;
    public void StartValidation()
    {
        DataLogger.NextValidation();

        validationCompleted = false;
        gazeRaycaster.OnRaycastSuccessful += OnGazeDataRecieved;
        gazeRaycaster.SetRaycastMode(1);
        deltaGraph = new List<Vector3>();
        if (!isValidating)
        {
            isValidating = true;
            if (cheapMode)
            {
                validationRoutine = StartCoroutine(CheapValidationRoutine());
            }
            else
            {
                validationRoutine = StartCoroutine(ValidationRoutine());
            }
        }
    }

    public List<Vector3> GetDeltaGraph()
    {
        return deltaGraph;
    }

    public void CancelValidation()
    {
        StopCoroutine(validationRoutine);
        isValidating = false;
        DataLogger.Close();

        gazeRaycaster.SetRaycastMode(0);
        gazeRaycaster.OnRaycastSuccessful -= OnGazeDataRecieved;

        //dump the marker far away so we cant see it
        //TODO just derender it or something
        marker.transform.position = Vector3.one * 900;
    }


    private int currTargetIdx;
    private IEnumerator ValidationRoutine()
    {
        GameObject displayParent = new GameObject();
        displayParent.name = "Sphere Parent";
        researchDisplay.ShowCalibrationTargets(displayParent.transform);
        marker.gameObject.SetActive(true);
        debugCube.gameObject.SetActive(true);
        for (int i = 0; i < targets.GetTargetCount(); i++)
        {
            currLocalTargetPos = targets.GetLocalTargetPosAt(i);
            currTargetIdx = i;

            // once everything is moved and set up, wait a short time for the user to move their eyes
            yield return new WaitForSeconds(settings.ignoreInitialSeconds);


            // set the remaining samples to be however many we decide we want in total per calibration point
            remainingSamples = totalSamples;

            // wait for the samples to be collected before moving on to the next calibration point
            while (remainingSamples > 0)
            {
                yield return null;
            }

            // take the sample average and add it as an element of the deltaGraph
            runningSampleAverage /= totalSamples;
            deltaGraph.Add(runningSampleAverage);
            DataLogger.LogValidationPoint(currLocalTargetPos, runningSampleAverage);
            researchDisplay.ShowValidationPoint(i, runningSampleAverage);
            runningSampleAverage = Vector3.zero;
        }

        marker.gameObject.SetActive(false);
        debugCube.gameObject.SetActive(false);
        isValidating = false;
        validationCompleted = true;
        DataLogger.Close();
        if (OnValidationCompleted != null)
        {
            OnValidationCompleted();
        }
    }

    private IEnumerator CheapValidationRoutine()
    {
        GameObject displayParent = new GameObject();
        displayParent.name = "Sphere Parent";
        researchDisplay.ShowCalibrationTargets(displayParent.transform);
        marker.gameObject.SetActive(true);
        debugCube.gameObject.SetActive(true);
        for (int i = 0; i < 9; i++)
        {
            currLocalTargetPos = targets.GetCheapValidationTargetAt(i);
            currTargetIdx = i;

            // once everything is moved and set up, wait a short time for the user to move their eyes
            yield return new WaitForSeconds(settings.ignoreInitialSeconds + 0.5f);
            
            // set the remaining samples to be however many we decide we want in total per calibration point
            remainingSamples = totalSamples;

            // wait for the samples to be collected before moving on to the next calibration point
            while (remainingSamples > 0)
            {
                yield return null;
            }

            // take the sample average and add it as an element of the deltaGraph
            runningSampleAverage /= totalSamples;
            deltaGraph.Add(runningSampleAverage);
            DataLogger.LogValidationPoint(currLocalTargetPos, runningSampleAverage);
            //researchDisplay.ShowValidationPoint(i, runningSampleAverage);
            runningSampleAverage = Vector3.zero;
        }

        marker.gameObject.SetActive(false);
        debugCube.gameObject.SetActive(false);
        isValidating = false;
        validationCompleted = true;
        DataLogger.Close();
        if (OnValidationCompleted != null)
        {
            OnValidationCompleted();
        }
    }

    public int totalSamples;
    private int remainingSamples = 0;
    public GameObject testGazeObject;
    
    private void OnGazeDataRecieved(Vector2 o, Vector2 point, float time)
    {
        Vector3 gazeDir = vrCam.transform.InverseTransformDirection(testGazeObject.transform.position - vrCam.transform.position).normalized;
        if (remainingSamples > 0)
        {
            Vector3 target = vrCam.transform.localToWorldMatrix.MultiplyPoint(currLocalTargetPos) - vrCam.transform.position;
            if (!debugGazeDir) {
                //gazeDir = gazeData.GazeDirection;
            } else
            {
                ShowDebugGazeLine(gazeDir);
            }
            if (true)//(GazeUtils.GazeIsStable(gazeDir)) //UNDO
            {
                Vector3 delta = CalculateRawGazeDelta(target, gazeDir);
                if (delta[0] < 900) // if it's above 900 it's because the CalculateRawGazeDelta function failed to raycast
                {
                    runningSampleAverage += delta;
                    DataLogger.LogValidationPoint(currLocalTargetPos, delta); //UNDO
                    remainingSamples--;
                    //researchDisplay.ShowValidationPoint(currTargetIdx, delta);
                }
            }
            
        }
    }

    private Vector3 CalculateRawGazeDelta(Vector3 targetDirection, Vector3 gazeDirection)
    {
        // get raw gaze direction vectors
        Vector3 target = targetDirection.normalized;
        Vector3 prediction = vrCam.transform.TransformDirection(gazeDirection).normalized;

        LayerMask backdropMask = LayerMask.GetMask("Overlay");
        //Vector3 worldSpaceGazeDir = vrCam.transform.TransformDirection(prediction);

        RaycastHit windowHit;
        if (Physics.Raycast(vrCam.transform.position, prediction,
            out windowHit, Mathf.Infinity, backdropMask))
        {
            if (debugGazePoint)
            {
                debugCube.transform.position = windowHit.point;
            }
            Vector3 delta = windowHit.collider.transform.InverseTransformPoint(windowHit.point);
            return new Vector3(delta.x, delta.z, 0f) - currLocalTargetPos;
        }
        return Vector3.one * 999;
    }

    private Vector3 CalculateGazeDelta(Vector3 targetDirection, Vector3 gazeDirection)
    {
        // get raw gaze direction vectors
        Vector3 target = targetDirection.normalized;
        Vector3 prediction = vrCam.transform.TransformDirection(gazeDirection).normalized;

        // convert to rotations
        Quaternion qTarget = Quaternion.LookRotation(target);
        Quaternion qPred = Quaternion.LookRotation(prediction);
        // rotate target 
        Quaternion normalizedPredictionRotation = qPred * Quaternion.Inverse(qTarget);
        // rotate forward vector to prediction
        Vector3 normalizedPrediction = normalizedPredictionRotation * Vector3.forward;
        normalizedPrediction = normalizedPrediction.normalized;
        // calculate delta
        Vector3 delta = Vector3.Dot(Vector3.forward, normalizedPrediction) * normalizedPrediction - Vector3.forward;

        return delta;
    }

    private void ShowDebugGazeLine(Vector3 gazeDir) {
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, vrCam.transform.position);
        lr.SetPosition(1, vrCam.transform.position + (vrCam.transform.TransformDirection(gazeDir).normalized)*30);
    }

}
