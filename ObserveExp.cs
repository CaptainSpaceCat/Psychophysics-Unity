/*
 * ObserveExp.cs
 * 
 * Description: Main script for Observation Experiment.
 * 
 * Parameters:
 *  
 * Inputs:
 *  - Left Alt: toggle blind spot calibration
 *  - Left Ctrl: toggle angle adjust mode
 *  - Up/Down Arrow: adjust vertical scale of blind spot object
 *  - Left/Right Arrow: 
 *    -- (not angle adjust mode) adjust horizontal scale of blind spot object
 *    -- (angle adjust mode) adjust eccentricity angle of blind spot object
 *  
 *  - Enter: 
 *  - Left Shift: show/hide scene
 *  - Space: next image
 *  - Right Shift: toggle automode
 *  - Backquote (`): toggle gaze direction marker 
 *  
 *  - Keypad 0: increment cheat count
 *  - Keypad 5: randomly place view-window
 *  - Keypad (rest): move view-window in corresponding direction (left, right, up, down, diagonals)
 *  
 *  - Letters: class selection
 *    -- Scene/Shape Experiments: ASDF => 0,1,2,3
 *    -- Letters Experiment: corresponding letter
 *  
 *  Author: Paul Jolly
 */

using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using UnityEngine;

namespace Psychophysics
{
    public class ObserveExp : MonoBehaviour
    {
        public string saveDir = "C:\\Users\\chich\\Documents\\psychophysics\\MATLAB\\ObsData";
        public Transform scene;
        public Transform target;
        public Transform targetRing;
        public AudioClip sound;
        public GameObject viewWindow;
        public enum Experiment{ Scenes, Shapes, Letters };

        [Header("Experiment Settings")]
        public Experiment expType;
        public bool sceneFile = true;
        public string sceneFileName = "test0.txt";
        public int numImages = 372;
        public int numTrials = 10;
        public float[] eccentricityAngles;
        public bool distortion = false;

        [Header("Transition Settings")]
        public int jitterCount = 5;
        public float fineDelta = 0.1f;
        public float coarseDelta = 0.3f;
        public float sigma = 0.1f;
        public float auditoryDelay = 0.0f;
        public float blindSpotDelay = 0.035f;

        [Header("Valid Region")]
        public float maxX = 0.65f;
        public float minX = -0.55f;
        public float maxY = 0.4f;
        public float minY = -0.6f;
        
        Transform mainCam; 
        private AudioSource source { get { return GetComponent<AudioSource>(); } }
        string dataFile;
        string fullFile;
        string toWrite = "";
        string eyePositions = "";
        string vwPositions = "";
        string posTimeStamps = "";
        int eye;

        Transform gazeProjMarker, gazeDirMarker;
        GameObject calibController;
        GameObject nextTarget, blindObj;
        float[] eccAngles;
        int ang_i = 0;
        float offset, blindOffset, zDist, eccAng;
        float blindDeg = 16.1f;
        float blindRad = 16.1f * Mathf.PI / 180f;
        float hiddenOffset_L = 0.3f;
        float hiddenOffset_R = -0.3f;
        Vector3 newPos;
        float increment = 0.01f;
        bool angleAdjust = false;
        bool calibration = false;
        bool coarseMode = false;
        float delta;

        List<Dictionary<int, int>> trials;
        private bool autoMode = false;
        private float startTime, prevTime;
        int imgNum;
        int[] imgInds;
        int trialNum = 0;
        bool record = false;

        int round = 0;
        int selectedClass = 0;
        int movementCount = 0;
        int cheatCount = 0;
        //int count = 0;

        string[] shapes = new string[] { "c_", "r_", "s_", "t_" };
        string[] letters = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        int trueClass;

        // Start is called before the first frame update
        void Start()
        {
            // General setup
            mainCam = Camera.main.transform;
            zDist = mainCam.InverseTransformPoint(this.transform.position).z;
            //gazeProjMarker = GameObject.Find("Gaze Visualizer").GetComponent<PupilLabs.GazeVisualizer>().projectionMarker;
            //gazeDirMarker = GameObject.Find("Gaze Visualizer").GetComponent<PupilLabs.GazeVisualizer>().gazeDirectionMarker;
            //gazeProjMarker.GetComponent<MeshRenderer>().enabled = false;
            //gazeDirMarker.GetComponent<MeshRenderer>().enabled = false;
            //calibController = GameObject.Find("Calibration Controller");
            gameObject.AddComponent<AudioSource>();
            source.clip = sound;
            source.playOnAwake = false;
            sceneFileName = "C:\\Users\\chich\\Documents\\psychophysics\\Psychophysics\\Assets\\Resources\\TestGroups_ILSVRC\\" + sceneFileName;

            if (distortion)
                GameObject.Find("TransparencyFilter").GetComponent<Psychophysics.DistortionFilterControl>().resetColor.a = 255;
            else
                GameObject.Find("TransparencyFilter").GetComponent<Psychophysics.DistortionFilterControl>().resetColor.a = 0;

            // Current target and view window position
            //target.localPosition = new Vector3(Random.Range(minX, maxX), 0.0f, Random.Range(minY, maxY));
            target.localPosition = new Vector3((minX + maxX) / 2f, 0.0f, (minY + maxY) / 2f);
            targetRing.localPosition = target.localPosition;
            offset = calculateOffset(target.localPosition, eccAng);
            viewWindow.transform.localPosition = new Vector3(target.localPosition.x - offset, target.localPosition.y, target.localPosition.z);

            // Create next target object
            float scale = 0.05f;
            nextTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nextTarget.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/red");
            nextTarget.transform.SetParent(this.transform, true);
            nextTarget.transform.localScale = new Vector3(scale, scale, scale);
            nextTarget.GetComponent<MeshRenderer>().enabled = false;

            // Create blindspot object
            scale = 0.01f;
            blindObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            blindObj.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/green");
            blindObj.transform.SetParent(this.transform, true);
            blindObj.transform.localScale = new Vector3(-0.04f, scale, 0.07f);
            //blindObj.GetComponent<MeshRenderer>().enabled = false;

            blindOffset = calculateOffset(target.localPosition, blindRad);
            blindObj.transform.localPosition = new Vector3(target.localPosition.x + blindOffset, target.localPosition.y, target.localPosition.z);
            blindObj.transform.localRotation = Quaternion.identity;

            // Load default image
            scene.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/white-square");
            //scene.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/letters/A");

            // ##### Create trials (based on experiment type) ###################################################################################
            eccAngles = new float[eccentricityAngles.Length];
            for (int i=0; i<eccentricityAngles.Length; i++)
            {
                eccAngles[i] = eccentricityAngles[i] * Mathf.PI / 180f;
            }

            if (expType.Equals(Experiment.Scenes))
            {
                trials = createTrials(eccAngles, sceneFile);
            } else if (expType.Equals(Experiment.Shapes))
            {
                imgInds = new int[numTrials];
                for (int i = 0; i < numTrials; i++)
                {
                    imgInds[i] = Random.Range(0, 3000);
                }
            } else if (expType.Equals(Experiment.Letters))
            {
                imgInds = new int[numTrials];
                for (int i = 0; i < numTrials; i++)
                {
                    imgInds[i] = Random.Range(0, 26);
                }
            }
            

            //foreach (int img in trials[0].Keys)
            //{
            //    print(img.ToString() + ": " + trials[0][img].ToString() + ", " + trials[1][img].ToString());
            //}
        }

        // Update is called once per frame
        void Update()
        {
            // General setup
            //scene.GetComponent<SpriteRenderer>().enabled = calibController.GetComponent<PupilLabs.CalibrationController>().CalibDone;
            //target.GetComponent<MeshRenderer>().enabled = calibController.GetComponent<PupilLabs.CalibrationController>().CalibDone;
            //targetRing.GetComponent<MeshRenderer>().enabled = calibController.GetComponent<PupilLabs.CalibrationController>().CalibDone;

            //if (!calibController.GetComponent<PupilLabs.CalibrationController>().CalibDone)
            //    blindObj.GetComponent<MeshRenderer>().enabled = false;
            //else if (!viewWindow.activeSelf)
            //    blindObj.GetComponent<MeshRenderer>().enabled = true;

            //viewWindow.SetActive(calibController.GetComponent<PupilLabs.CalibrationController>().CalibDone);

            // Blind spot calibration
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                calibration = !calibration;
            }

            if (calibration)
            {
                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    angleAdjust = !angleAdjust;
                }

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    blindObj.transform.localScale = new Vector3(blindObj.transform.localScale.x, blindObj.transform.localScale.y, blindObj.transform.localScale.z + increment);
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    blindObj.transform.localScale = new Vector3(blindObj.transform.localScale.x, blindObj.transform.localScale.y, blindObj.transform.localScale.z - increment);
                }
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (angleAdjust)
                    {
                        blindDeg += 0.1f;
                        print(blindDeg);
                        blindRad = blindDeg * Mathf.PI / 180f;
                        blindOffset = calculateOffset(target.localPosition, blindRad);
                        blindObj.transform.localPosition = new Vector3(target.localPosition.x + blindOffset, target.localPosition.y, target.localPosition.z);
                    }
                    else
                    {
                        blindObj.transform.localScale = new Vector3(blindObj.transform.localScale.x + increment, blindObj.transform.localScale.y, blindObj.transform.localScale.z);
                    }

                }
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (angleAdjust)
                    {
                        blindDeg -= 0.1f;
                        print(blindDeg);
                        blindRad = blindDeg * Mathf.PI / 180f;
                        blindOffset = calculateOffset(target.localPosition, blindRad);
                        blindObj.transform.localPosition = new Vector3(target.localPosition.x + blindOffset, target.localPosition.y, target.localPosition.z);
                    }
                    else
                    {
                        blindObj.transform.localScale = new Vector3(blindObj.transform.localScale.x - increment, blindObj.transform.localScale.y, blindObj.transform.localScale.z);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                coarseMode = !coarseMode;
            }

            if (coarseMode)
                delta = coarseDelta;
            else
                delta = fineDelta;

            // Window movement control
            if (Input.GetKeyDown(KeyCode.Keypad5) && !autoMode)
            {
                float newX = RandomGaussian(newPos.x, minX, maxX);
                float newZ = RandomGaussian(newPos.z, minY, maxY);
                newPos = new Vector3(newX, 0.0f, newZ);

                nextTarget.transform.localPosition = newPos;
                nextTarget.GetComponent<MeshRenderer>().enabled = true;
                StartCoroutine(beepTimer());
            }
            else if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                newPos = new Vector3(newPos.x - delta, 0.0f, newPos.z - delta);
                nextTarget.transform.localPosition = newPos;
                nextTarget.GetComponent<MeshRenderer>().enabled = true;
                StartCoroutine(beepTimer());
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                newPos = new Vector3(newPos.x, 0.0f, newPos.z - delta);
                nextTarget.transform.localPosition = newPos;
                nextTarget.GetComponent<MeshRenderer>().enabled = true;
                StartCoroutine(beepTimer());
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                newPos = new Vector3(newPos.x + delta, 0.0f, newPos.z - delta);
                nextTarget.transform.localPosition = newPos;
                nextTarget.GetComponent<MeshRenderer>().enabled = true;
                StartCoroutine(beepTimer());
            }
            else if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                newPos = new Vector3(newPos.x - delta, 0.0f, newPos.z);
                nextTarget.transform.localPosition = newPos;
                nextTarget.GetComponent<MeshRenderer>().enabled = true;
                StartCoroutine(beepTimer());
            }
            else if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                newPos = new Vector3(newPos.x + delta, 0.0f, newPos.z);
                nextTarget.transform.localPosition = newPos;
                nextTarget.GetComponent<MeshRenderer>().enabled = true;
                StartCoroutine(beepTimer());
            }
            else if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                newPos = new Vector3(newPos.x - delta, 0.0f, newPos.z + delta);
                nextTarget.transform.localPosition = newPos;
                nextTarget.GetComponent<MeshRenderer>().enabled = true;
                StartCoroutine(beepTimer());
            }
            else if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                newPos = new Vector3(newPos.x, 0.0f, newPos.z + delta);
                nextTarget.transform.localPosition = newPos;
                nextTarget.GetComponent<MeshRenderer>().enabled = true;
                StartCoroutine(beepTimer());
            }
            else if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                newPos = new Vector3(newPos.x + delta, 0.0f, newPos.z + delta);
                nextTarget.transform.localPosition = newPos;
                nextTarget.GetComponent<MeshRenderer>().enabled = true;
                StartCoroutine(beepTimer());
            }

            // Keys used: Keypad, `, Enter, RightShift, Space, LeftShift, A, S, D, F
            // Automode
            if (Input.GetKeyDown(KeyCode.RightShift))
            {
                autoMode = !autoMode;
                prevTime = Time.realtimeSinceStartup;
            }

            // Toggle gaze direction marker
            //if (Input.GetKeyDown(KeyCode.BackQuote))
            //{
            //    gazeProjMarker.GetComponent<MeshRenderer>().enabled = !gazeProjMarker.GetComponent<MeshRenderer>().enabled;
            //    gazeDirMarker.GetComponent<MeshRenderer>().enabled = !gazeDirMarker.GetComponent<MeshRenderer>().enabled;
            //}

            // Increment cheat count
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                cheatCount += 1;
            }

            // Toggle recording
            if (Input.GetKeyDown(KeyCode.Return))
            {
                record = !record;
                string timeStamp = System.DateTime.Now.ToString("yy-MM-dd_HH-mm-ss");
                dataFile = saveDir + "\\obsTest_" +  timeStamp + ".txt";
                fullFile = saveDir + "\\obsTest_full_" + timeStamp + ".txt";
                print("Recording: " + record.ToString());
                print("Data File: " + dataFile);
                print("Pos File: " + fullFile);
                startTime = Time.realtimeSinceStartup;
            }

            // Show/hide scene
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (scene.GetComponent<SpriteRenderer>().maskInteraction == SpriteMaskInteraction.None)
                    scene.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                else
                    scene.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
            }

            // Choose class
            if (expType.Equals(Experiment.Scenes) || expType.Equals(Experiment.Shapes))
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    selectedClass = 0;
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    selectedClass = 1;
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    selectedClass = 2;
                }
                else if (Input.GetKeyDown(KeyCode.F))
                {
                    selectedClass = 3;
                }
            } else if (expType.Equals(Experiment.Letters))
            {
                if (Input.inputString.Length > 0 && char.IsLetter(Input.inputString[0]))
                {
                    selectedClass = Input.inputString[0] - 97;
                }
            }


            // Next Image
                if (Input.GetKeyDown(KeyCode.Space))
            {
                if (record && trialNum > 0)
                {
                    float time = Time.realtimeSinceStartup - startTime;
                    toWrite = imgNum.ToString() + ",";                   
                    toWrite += ang_i.ToString() + ",";
                    toWrite += time.ToString() + ",";
                    toWrite += selectedClass.ToString() + ",";           
                    toWrite += movementCount.ToString() + ",";
                    toWrite += trueClass.ToString() + ",";
                    toWrite += cheatCount.ToString();
                    writeString(toWrite, dataFile);

                    eyePositions += ",(" + target.localPosition.x.ToString() + "," + target.localPosition.z.ToString() + ")";
                    vwPositions += ",(" + viewWindow.transform.localPosition.x.ToString() + "," + viewWindow.transform.localPosition.z.ToString() + ")";
                    posTimeStamps += "," + time.ToString();
                    toWrite += "\n" + posTimeStamps + "\n" + eyePositions + "\n" + vwPositions;
                    writeString(toWrite, fullFile);
                }

                if (trialNum >= numTrials)
                {
                    trialNum = 0;
                    round += 1;
                    scene.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/white-square");
                    scene.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
                    Shuffle();                                            // experiment dependent
                }
                else
                {
                    scene.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    imgNum = imgInds[trialNum];                           // experiment dependent

                    string img = "";
                    if (expType.Equals(Experiment.Scenes))
                    {
                        img = "Images/GrayImages_ILSVRC/" + imgNum.ToString();
                        ang_i = trials[round][imgNum];
                    }
                    else if (expType.Equals(Experiment.Shapes))
                    {
                        trueClass = Random.Range(0, 4);
                        img = "Images/shapes/" + shapes[trueClass] + imgNum.ToString();
                    }
                    else if (expType.Equals(Experiment.Letters))
                    {
                        trueClass = Random.Range(0, 26);
                        img = "Images/letters/" + letters[trueClass];
                    }

                    print(img);
                    scene.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(img);
                    startTime = Time.realtimeSinceStartup;
                    trialNum += 1;
                    movementCount = 0;
                    cheatCount = 0;

                    eccAng = eccAngles[ang_i];

                    if (eccAng > 0)
                    {
                        if (eye == 2)
                        {
                            scene.localPosition = new Vector3(hiddenOffset_L, scene.localPosition.y, scene.localPosition.z);
                        }
                        else
                        {
                            scene.localPosition = new Vector3(hiddenOffset_R, scene.localPosition.y, scene.localPosition.z);
                            Vector3 sceneCenter = new Vector3((minX + maxX) / 2f + hiddenOffset_R, 0.0f, (minY + maxY) / 2f);
                            float gazeOffset = calculateOffset(sceneCenter, eccAng, true);
                            newPos = new Vector3(sceneCenter.x - gazeOffset, sceneCenter.y, sceneCenter.z);
                        }
                    }
                    else
                    {
                        scene.localPosition = new Vector3(0.05f, scene.localPosition.y, scene.localPosition.z);
                        newPos = new Vector3((minX + maxX) / 2f, 0.0f, (minY + maxY) / 2f);
                    }

                    //newPos = new Vector3(Random.Range(minX, maxX), 0.0f, Random.Range(minY, maxY));
                    //nextTarget.transform.localPosition = newPos;
                    //nextTarget.GetComponent<MeshRenderer>().enabled = true;
                    //movementCount = -1;
                    //StartCoroutine(beepTimer());

                    target.localPosition = newPos;
                    targetRing.localPosition = newPos;
                    offset = calculateOffset(newPos, eccAng);
                    blindOffset = calculateOffset(newPos, blindRad);
                    StartCoroutine(GameObject.Find("TransparencyFilter").GetComponent<Psychophysics.DistortionFilterControl>().resetFilter());
                    viewWindow.transform.localPosition = new Vector3(newPos.x - offset, newPos.y, newPos.z);
                    blindObj.transform.localPosition = new Vector3(newPos.x + blindOffset, newPos.y, newPos.z);
                    StartCoroutine(blindSpotDelayTimer());

                    if (record)
                    {
                        eyePositions = "(" + target.localPosition.x.ToString() + "," + target.localPosition.z.ToString() + ")";
                        vwPositions = "(" + viewWindow.transform.localPosition.x.ToString() + "," + viewWindow.transform.localPosition.z.ToString() + ")";
                        posTimeStamps = "0";
                    }
                    
                }
            }
        }

        //void FixedUpdate()
        //{
        //    if (autoMode)
        //    {
        //        if (Time.realtimeSinceStartup - prevTime > 1)
        //        {
        //            float newX = RandomGaussian(newPos.x, minX, maxX);
        //            float newZ = RandomGaussian(newPos.z, minY, maxY);

        //            if (count < jitterCount)
        //            {
        //                newPos = new Vector3(newX, 0.0f, newZ);
        //                count += 1;
        //            }
        //            else
        //            {
        //                newPos = new Vector3(Random.Range(minX, maxX), 0.0f, Random.Range(minY, maxY));
        //                count = 0;
        //            }
        //            nextTarget.transform.localPosition = newPos;
        //            nextTarget.GetComponent<MeshRenderer>().enabled = true;
        //            StartCoroutine(beepTimer());
        //            prevTime = Time.realtimeSinceStartup;
        //        }
        //    }

        //    //if (record)
        //    //{
        //    //    // Eye Position Data
        //    //    Vector3 pos = mainCam.InverseTransformPoint(gazeProjMarker.position);
        //    //    Vector3 targetPos = target.localPosition;
        //    //    toWrite = (Time.realtimeSinceStartup - startTime).ToString() + ",";
        //    //    toWrite += targetPos.x.ToString() + "," + targetPos.y.ToString() + "," + targetPos.z.ToString() + ",";
        //    //    toWrite += pos.x.ToString() + "," + pos.y.ToString() + "," + pos.z.ToString();
        //    //    writeString(toWrite);
        //    //}

        //}

        float RandomGaussian(float currVal, float min, float max)
        {
            float u, v, S, newVal;

            do
            {
                do
                {
                    u = 2.0f * Random.value - 1.0f;
                    v = 2.0f * Random.value - 1.0f;
                    S = u * u + v * v;
                }
                while (S >= 1.0f || S == 0f);

                // Standard Normal Distribution
                float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
                newVal = currVal + std * sigma;
            }
            while (newVal < min || newVal > max);

            return newVal;
        }

        float calculateOffset(Vector3 pos, float eccAngRad, bool inverse=false)
        {
            Vector3 backgroundLocal = mainCam.InverseTransformPoint(this.transform.position);
            Vector3 posLocal = mainCam.InverseTransformPoint(this.transform.TransformPoint(pos));
            //eye = GameObject.Find("Gaze Visualizer").GetComponent<PupilLabs.GazeVisualizer>().eye;
            eye = GameObject.Find("VR Rig").GetComponent<Psychophysics.toggleEye>().eye;

            float d = backgroundLocal.z;
            float a = Mathf.Sqrt(Mathf.Pow(posLocal.x, 2) + Mathf.Pow(posLocal.z, 2));
            float diff = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(a, 2) - Mathf.Pow(d, 2)));
            
            float offset_1 = d * Mathf.Tan(Mathf.Acos(d / a) + eccAngRad) - diff;
            float offset_2 = diff + d * Mathf.Tan(eccAngRad - Mathf.Acos(d / a));

            if (float.IsNaN(offset_1) || float.IsNaN(offset_2))
            {
                offset_1 = d * Mathf.Tan(Mathf.Acos(Mathf.Floor(d / a)) + eccAngRad) - diff;
                offset_2 = diff + d * Mathf.Tan(eccAngRad - Mathf.Acos(Mathf.Floor(d / a)));
            }

            if (!inverse)
            {
                if (pos.x >= backgroundLocal.x)
                {
                    if (eye == 2)
                        return -offset_2;
                    else
                        return offset_1;
                }
                else
                {
                    if (eye == 2)
                        return -offset_1;
                    else
                        return offset_2;
                }
            } else
            {
                if (pos.x <= backgroundLocal.x)
                {
                    if (eye == 2)
                        return offset_1;
                    else
                        return -offset_2;
                }
                else
                {
                    if (eye == 2)
                        return offset_2;
                    else
                        return -offset_1;
                }
            }
        }

        List<Dictionary<int, int>> createTrials(float[] angles, bool readFromFile=true)
        {
            List<Dictionary<int, int>> trials = new List<Dictionary<int, int>>();

            int numAngles = angles.Length;

            if (readFromFile)
            {
                imgInds = loadTestSet(sceneFileName, numTrials);
            }
            else
            {
                imgInds = new int[numImages];
                for (int i = 0; i < numImages; i++)
                {
                    imgInds[i] = i;
                }
                Shuffle();

                int[] imgOptions = imgInds;
                imgInds = new int[numTrials];
                for (int i = 0; i < numTrials; i++)
                {
                    imgInds[i] = imgOptions[i];
                }
            }

            Dictionary<int, int> round_1 = new Dictionary<int, int>();
            Dictionary<int, int> round_2 = new Dictionary<int, int>();
            foreach (int img in imgInds)
            {
                int ang_i = Random.Range(0, 2);
                round_1.Add(img, ang_i);
                round_2.Add(img, 1 - ang_i);
            }
                
            trials.Add(round_1);
            trials.Add(round_2);

            return trials;
        }

        void Shuffle()
        {
            for (int i = 0; i < imgInds.Length; i++)
            {
                int rnd = Random.Range(0, imgInds.Length);
                int temp = imgInds[rnd];
                imgInds[rnd] = imgInds[i];
                imgInds[i] = temp;
            }
        }

        IEnumerator beepTimer()
        {
            StartCoroutine(resetFilter());
            yield return new WaitForSeconds(auditoryDelay);
            source.PlayOneShot(sound);
            nextTarget.GetComponent<MeshRenderer>().enabled = false;
            movementCount += 1;
            target.localPosition = newPos;
            targetRing.localPosition = newPos;
            offset = calculateOffset(newPos, eccAng);
            blindOffset = calculateOffset(newPos, blindRad);
            viewWindow.transform.localPosition = new Vector3(newPos.x - offset, newPos.y, newPos.z);
            blindObj.transform.localPosition = new Vector3(newPos.x + blindOffset, newPos.y, newPos.z);
            blindObj.GetComponent<MeshRenderer>().enabled = false;
            StartCoroutine(blindSpotDelayTimer());

            if (record)
            {
                eyePositions += ",(" + target.localPosition.x.ToString() + "," + target.localPosition.z.ToString() + ")";
                vwPositions += ",(" + viewWindow.transform.localPosition.x.ToString() + "," + viewWindow.transform.localPosition.z.ToString() + ")";
                posTimeStamps += "," + (Time.realtimeSinceStartup - startTime).ToString();
            }
        }

        IEnumerator resetFilter()
        {
            yield return new WaitForSeconds(auditoryDelay-0.01f);
            StartCoroutine(GameObject.Find("TransparencyFilter").GetComponent<Psychophysics.DistortionFilterControl>().resetFilter());
        }

        IEnumerator blindSpotDelayTimer()
        {
            yield return new WaitForSeconds(blindSpotDelay);
            blindObj.GetComponent<MeshRenderer>().enabled = true;
        }

        void writeString(string toWrite, string fileName)
        {
            print("asdf");
            StreamWriter writer = new StreamWriter(fileName, true);
            writer.WriteLine(toWrite);
            writer.Close();
        }

        int[] loadTestSet(string fileName, int numTrials)
        {
            int[] inds = new int[numTrials];
            string line;
            StreamReader reader = new StreamReader(fileName);

            int i = 0;

            using (reader)
            {
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        inds[i] = System.Convert.ToInt32(line);
                        i += 1;
                    }
                }
                while (line != null);

                reader.Close();
            }

            return inds;
        }
    }
}
