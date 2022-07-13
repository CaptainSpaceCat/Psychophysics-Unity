using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeLocker : MonoBehaviour
{
    public BackdropShaderController shader;
    public PupilLabs.GazeController gazeController;
    public Camera vrCam;
    public bool debugGazeDir = false;
    public Vector3 testGazeDir;
    public TumblingEController tumblingE;

    [Range(0f, 1f)]
    public float confidenceThreshold = 0.6f;
    [Range(.99f, 1f)]
    public float gazeDeviationThreshold = 0.92f;
    public float degreesEccentricity;
    public Vector2 windowSize;


    // Start is called before the first frame update
    void Start()
    {
        gazeController.OnReceive3dGaze += ConsumeGazeData;
    }

    public void RandomizeE()
    {
        tumblingE.Randomize();
    }

    public void ConsumeGazeData(PupilLabs.GazeData gazeData)
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

        //Debug.Log(Vector3.Dot(goalDir.normalized, gazeDir.normalized));
        
        // Control
        if ((!debugGazeDir && gazeData.Confidence < confidenceThreshold) || (Vector3.Dot(goalDir.normalized, gazeDir.normalized) < gazeDeviationThreshold))
        {
            shader.ShutWindow();
            return;
        }
        shader.Rerender(backdropHitTL.textureCoord, backdropHitBR.textureCoord, dotHit.textureCoord);
        tumblingE.transform.position = windowHit.point;
    }
}