/*
 * createValidBox.cs
 * 
 * Description: Create box outline of region to which user should restrict their gaze
 * 
 * Parameters:
 *  - Material: material/color for outline
 *  - Pos X: Positive x boundary value relative to background origin
 *  - Neg X: Negative x boundary value relative to background origin
 *  - Pos Y: Positive y boundary value relative to background origin
 *  - Neg Y: Negative y boundary value relative to background origin
 *  
 *  Author: Paul Jolly
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createValidBox : MonoBehaviour
{

    // Unity user parameters
    public Material mat;
    public float posX = 0;
    public float negX = 0;
    public float posY = 0;
    public float negY = 0;

    Mesh mesh;

    void Start()
    {
        // Create mesh and specify vertices for valid box
        mesh = new Mesh();

        

        // Set material
        GetComponent<MeshRenderer>().material = mat;

        // DEBUG: Print vertices (Uncomment below)
        //Transform mainCam = Camera.main.transform;
        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    Vector3 vert_g = this.transform.TransformPoint(vertices[i]);
        //    Vector3 vert_c = mainCam.InverseTransformPoint(vert_g);

        //    print(vertices[i]); // Raw coordinates (local frame)
        //    print(vert_g);      // World coordinates
        //    print(vert_c);      // Main camera frame
        //}
    }

    private void Update()
    {
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-negX, posY, 0f);
        vertices[1] = new Vector3(posX, posY, 0f);
        vertices[2] = new Vector3(posX, -negY, 0f);
        vertices[3] = new Vector3(-negX, -negY, 0f);
        mesh.vertices = vertices;

        mesh.SetIndices(new int[] { 0, 1, 1, 2, 2, 3, 0, 3 }, MeshTopology.Lines, 0, true);
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
