using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PupilLabs;
using UnityEngine.UI;

public class ExperimentControlFlow : MonoBehaviour
{
    
    //object references
    public CalibrationController calibrator;
    public CalibrationNotPupil calibNotPupil;
    public CalibrationValidator validator;
    public TrialRunner trialRunner;
    public ResearcherDisplayController researchController;
    public GameObject testingRoom;
    public Camera vrCam;
    public GameObject statusText;
    public FrameVisualizer eyeFrame;

    //flags
    private bool vrView = false;
    private bool calibrationExists = false;
    private ExperimentalControlState controlState = ExperimentalControlState.Waiting;
    public Vector3 testingRoomOffset;

    //buttons
    public Button calibrationButton;
    public Button validationButton;
    public Button trialButton;
    public Button cancelButton;
    public Button greenTextButton;
    public Button vrViewButton;
    public Button vrHeadLockButton;
    public Button dashboardViewButton;
    public Button eyeFrameButton;
    public Button blockButton;

    public enum ExperimentalControlState
    {
        Waiting,
        Calibrating,
        Testing,
        Validating,
    }


    // initialization
    void Awake()
    {
        // set event listeners
        /*
        calibrator.OnCalibrationStarted += OnCalibrationStarted;
        calibrator.OnCalibrationSucceeded += OnCalibrationSucceeded;
        calibrator.OnCalibrationFailed += OnCalibrationFailed;
        */
        calibNotPupil.OnCalibrationSucceeded += OnCalibrationSucceeded;
        calibNotPupil.OnCalibrationStarted += OnCalibrationStarted;

        //validator.OnValidationCompleted += OnValidationCompleted;
        calibNotPupil.OnValidationCompleted += OnValidationCompleted;

        //initialize new block
        NextBlock();
    }

    public void OnAnyTask()
    {
        researchController.ClearInfographics();


        eyeFrame.gameObject.SetActive(true);
        ToggleShowEyes();
        statusText.SetActive(false);
        //calibrationPlane.SetActive(false);
    }


    // control flow
    public void Calibrate()
    {
        OnAnyTask();
        //calibrationPlane.SetActive(true);
        //calibrator.ToggleCalibration();
        calibNotPupil.StartCalibration();

        //trialRunner.StartTrial();
    }

    public void RunTrial()
    {
        OnAnyTask();
        //TODO make this only happen in the tumbling E experiment
        //calibrationPlane.SetActive(true);

        if (controlState == ExperimentalControlState.Testing)
        { // if we're already in a trial, we're just trying to move to the next trial, so cancel the old one first
            trialRunner.StopTrial();
        }

        controlState = ExperimentalControlState.Testing;
        trialRunner.StartTrial();

        calibrationButton.interactable = false;
        validationButton.interactable = false;
        trialButton.interactable = true;
        cancelButton.interactable = true;
    }

    public void Validate()
    {
        OnAnyTask();
        controlState = ExperimentalControlState.Validating;
        //validator.StartValidation();
        calibNotPupil.StartValidation();

        calibrationButton.interactable = false;
        validationButton.interactable = false;
        trialButton.interactable = false;
        cancelButton.interactable = true;
    }

    public void SwapView()
    {
        vrView = !vrView;
        researchController.MirrorSubjectView(vrView);
        vrViewButton.gameObject.SetActive(!vrView);
        dashboardViewButton.gameObject.SetActive(vrView);
    }

    public void RecenterVR()
    {
        testingRoom.transform.SetParent(vrCam.transform);
        testingRoom.transform.localPosition = Vector3.down * 0.1f;
        testingRoom.transform.localRotation = Quaternion.identity; //TODO make it so it's always flat even if the user is looking up or down
        testingRoom.transform.SetParent(null);

        vrHeadLockButton.interactable = true;
    }

    public void HeadLockVR()
    {
        testingRoom.transform.SetParent(vrCam.transform);
        testingRoom.transform.localPosition = testingRoomOffset;
        testingRoom.transform.localRotation = Quaternion.identity;

        vrHeadLockButton.interactable = false;
    }


    public void ToggleShowEyes()
    {
        eyeFrame.gameObject.SetActive(!eyeFrame.gameObject.activeSelf);
        if (eyeFrame.gameObject.activeSelf)
        {
            eyeFrameButton.GetComponentInChildren<Text>().text = "Hide Eye Frames";
        } else {
            eyeFrameButton.GetComponentInChildren<Text>().text = "Show Eye Frames";
        }
    }

    public void NextBlock()
    {
        blockButton.GetComponentInChildren<Text>().text = "Block " + DataLogger.NextBlock();
    }

    public void CancelAction()
    {
        OnAnyTask();
        switch (controlState)
        {
            case ExperimentalControlState.Calibrating:
                //calibrator.ToggleCalibration();
                calibNotPupil.CancelCalibration();
                break;
            case ExperimentalControlState.Testing:
                trialRunner.StopTrial();
                break;
            case ExperimentalControlState.Validating:
                //validator.CancelValidation();
                calibNotPupil.CancelValidation();
                break;
        }

        controlState = ExperimentalControlState.Waiting;

        calibrationButton.interactable = true;
        validationButton.interactable = calibrationExists;
        trialButton.interactable = calibrationExists;
        cancelButton.interactable = false;
        greenTextButton.interactable = false;
    }

    public void Debug()
    {
        calibrationButton.interactable = true;
        validationButton.interactable = true;
        trialButton.interactable = true;
        cancelButton.interactable = true;
        greenTextButton.interactable = true;
    }


    // callbacks
    public void OnCalibrationStarted()
    {
        controlState = ExperimentalControlState.Calibrating;

        calibrationButton.interactable = false;
        validationButton.interactable = false;
        trialButton.interactable = false;
        cancelButton.interactable = true;
    }

    public void OnCalibrationSucceeded()
    {
        calibrationExists = true;

        calibrationButton.interactable = true;
        validationButton.interactable = true;
        trialButton.interactable = true;
        cancelButton.interactable = false;
        greenTextButton.interactable = true;
    }

    public void OnCalibrationFailed()
    {
        calibrationExists = false;

        calibrationButton.interactable = true;
        validationButton.interactable = false;
        trialButton.interactable = false;
        cancelButton.interactable = false;
    }

    public void OnValidationCompleted()
    {
        calibrationButton.interactable = true;
        validationButton.interactable = true;
        trialButton.interactable = true;
        cancelButton.interactable = false;
    }
}
