/*
 * MeasureObjects.cs
 * 
 * Description: Get global, local, and main camera positions of transforms/objects set in parameters
 *  
 *  Inputs:
 *  - m: print measurements
 *  
 *  Author: Paul Jolly
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureObjects : MonoBehaviour
{
    // Unity user parameters
    public Transform object1;
    public Transform object2;
    public Transform object3;
    public GameObject go1;

    // Private variables
    Transform mainCam;
    
    void Start()
    {
        mainCam = Camera.main.transform;
    }

    void Update()
    {
        Vector3 obj1Vec = object1.position;
        Vector3 obj2Vec = object2.position;
        Vector3 obj3Vec = object3.position;

        Vector3 obj1Vec_l = object1.localPosition;
        Vector3 obj2Vec_l = object2.localPosition;
        Vector3 obj3Vec_l = object3.localPosition;

        Vector3 obj1Vec_c = mainCam.InverseTransformPoint(obj1Vec);
        Vector3 obj2Vec_c = mainCam.InverseTransformPoint(obj2Vec);
        Vector3 obj3Vec_c = mainCam.InverseTransformPoint(obj3Vec);

        string obj1Pos = "(" + obj1Vec.x + ", " + obj1Vec.y + ", " + obj1Vec.z + ")";
        string obj2Pos = "(" + obj2Vec.x + ", " + obj2Vec.y + ", " + obj2Vec.z + ")";
        string obj3Pos = "(" + obj3Vec.x + ", " + obj3Vec.y + ", " + obj3Vec.z + ")";

        string obj1Pos_l = "(" + obj1Vec_l.x + ", " + obj1Vec_l.y + ", " + obj1Vec_l.z + ")";
        string obj2Pos_l = "(" + obj2Vec_l.x + ", " + obj2Vec_l.y + ", " + obj2Vec_l.z + ")";
        string obj3Pos_l = "(" + obj3Vec_l.x + ", " + obj3Vec_l.y + ", " + obj3Vec_l.z + ")";

        string obj1Pos_c = "(" + obj1Vec_c.x + ", " + obj1Vec_c.y + ", " + obj1Vec_c.z + ")";
        string obj2Pos_c = "(" + obj2Vec_c.x + ", " + obj2Vec_c.y + ", " + obj2Vec_c.z + ")";
        string obj3Pos_c = "(" + obj3Vec_c.x + ", " + obj3Vec_c.y + ", " + obj3Vec_c.z + ")";

        if (Input.GetKeyDown(KeyCode.M))
        {
            print("Object 1 -- Local: " + obj1Pos_l + " Global: " + obj1Pos);
            print("Object 2 -- Local: " + obj2Pos_l + " Global: " + obj2Pos);
            print("Object 3 -- Local: " + obj3Pos_l + " Global: " + obj3Pos);

            print("Camera Obj1: " + obj1Pos_c);
            print("Camera Obj2: " + obj2Pos_c);
            print("Camera Obj3: " + obj3Pos_c);

            print(go1.GetComponent<Renderer>().bounds.size);
        }
    }
}
