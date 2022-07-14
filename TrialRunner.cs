using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialRunner : MonoBehaviour
{
    public GazeRaycaster gazeRaycaster;
    public TumblingEController tumblingE;
    public ResearcherDisplayController researchView;
    public WallTextureController wallController;
    public WallRendererCapturer wallSnapshot;
    public BackdropShaderController shader;
    public CalibrationNotPupil calib;

    public Experiment[] ExperimentOptions;
    public int ExperimentChoice;

    public void StartTrial()
    {
        shader.SetOpaque();

        if (!calib.UpdateParams())
        {
            Debug.LogError("Failed to update calibration parameters");
            return;
        }

        gazeRaycaster.OnRaycastSuccessful += ProcessGazePoint;
        gazeRaycaster.SetRaycastMode(ExperimentOptions[ExperimentChoice].GetRaycastMode());

        ExperimentOptions[ExperimentChoice].Next();
        DataLogger.SaveSnapshot(wallSnapshot.TakeSnapshot());

        shader.SetWindow();
    }

    public void StopTrial()
    {
        shader.SetOpaque();
        gazeRaycaster.SetRaycastMode(0);
        gazeRaycaster.OnRaycastSuccessful -= ProcessGazePoint;

        ExperimentOptions[ExperimentChoice].Clear();
        
        shader.SetTransparent();
    }

    private void ProcessGazePoint(Vector2 overlayPoint, Vector2 windowPoint, float time)
    {
        ExperimentOptions[ExperimentChoice].OnRaycastSuccessful(overlayPoint, windowPoint);
        ExperimentOptions[ExperimentChoice].LogDataPoint(windowPoint, time);
    }
}
