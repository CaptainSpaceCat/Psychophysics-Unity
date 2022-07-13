/*
 * toggleEye.cs
 * 
 * Description: Individually select eye camera feed input
 * 
 * Parameters:
 *  - Alternate Cam: camera for secondary eye
 *  - Use Both: select to always use combined gaze direction from both eyes in calculating gaze position
 *  
 *  Inputs:
 *  - e: Set main scene camera to both eyes
 *  - q: Set main scene camera to left eye and alternate camera to right
 *  - w: Set main scene camera to right eye and alternate camera to left
 *  
 *  Author: Paul Jolly
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Psychophysics
{
    public class toggleEye : MonoBehaviour
    {
        // Unity user parameters
        public Camera alternateCam;
        public bool useBoth = false;
        public int eye = 0;

        // Private variables
        Camera mainCam;
        GameObject GazeVisualizer;

        void Start()
        {
            mainCam = Camera.main;
            GazeVisualizer = GameObject.Find("Gaze Visualizer");
            if (GazeVisualizer)
                GazeVisualizer.GetComponent<PupilLabs.GazeVisualizer>().eye = 0;
            eye = 0;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Slash)) // Both Eyes
            {
                if (GazeVisualizer)
                    GazeVisualizer.GetComponent<PupilLabs.GazeVisualizer>().eye = 0;

                mainCam.stereoTargetEye = StereoTargetEyeMask.Both;
                alternateCam.stereoTargetEye = StereoTargetEyeMask.None;
                eye = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Period)) // Left Eye
            {
                if (!useBoth && GazeVisualizer)
                    GazeVisualizer.GetComponent<PupilLabs.GazeVisualizer>().eye = 2;

                mainCam.stereoTargetEye = StereoTargetEyeMask.Left;
                alternateCam.stereoTargetEye = StereoTargetEyeMask.Right;
                eye = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Comma)) // Right Eye
            {
                if (!useBoth && GazeVisualizer)
                    GazeVisualizer.GetComponent<PupilLabs.GazeVisualizer>().eye = 1;

                mainCam.stereoTargetEye = StereoTargetEyeMask.Right;
                alternateCam.stereoTargetEye = StereoTargetEyeMask.Left;
                eye = 1;
            }
        }
    }
}
