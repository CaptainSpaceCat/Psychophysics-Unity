using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackdropShaderController : MonoBehaviour
{

    Material shaderMat;

    private void Awake()
    {
        shaderMat = GetComponent<Renderer>().material;

        // Set the window initially to be transparent
        // This allows the user to see the calibration targets
        SetTransparent();
    }

    public void Rerender(Vector2 windowCoordsA, Vector2 windowCoordsB, Vector2 dotCoords)
    {
        shaderMat.SetFloat("_WindowAX", windowCoordsA.x);
        shaderMat.SetFloat("_WindowAY", windowCoordsA.y);
        shaderMat.SetFloat("_WindowBX", windowCoordsB.x);
        shaderMat.SetFloat("_WindowBY", windowCoordsB.y);
        shaderMat.SetFloat("_DotCenterX", dotCoords.x);
        shaderMat.SetFloat("_DotCenterY", dotCoords.y);
    }

    public void SetOpaque()
    {
        shaderMat.SetInt("_Transparency", 0);
    }

    public void SetWindow()
    {
        shaderMat.SetInt("_Transparency", 1);
    }

    public void SetTransparent()
    {
        shaderMat.SetInt("_Transparency", 2);
    }

    public void ShutWindow()
    {
        shaderMat.SetFloat("_WindowAX", 0);
        shaderMat.SetFloat("_WindowAY", 0);
        shaderMat.SetFloat("_WindowBX", 0);
        shaderMat.SetFloat("_WindowBY", 0);
    }
}
