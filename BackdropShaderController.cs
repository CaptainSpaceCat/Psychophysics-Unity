using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackdropShaderController : MonoBehaviour
{

    Material shaderMat;
    private float[] spool = new float[1024];
    public float noiseDelay = 0.01f;
    private Coroutine noiseRoutine;
    private bool noiseRoutineRunning = false;
    private bool noiseActive = false;

    private void Awake()
    {
        shaderMat = GetComponent<Renderer>().material;

        // Set the window initially to be transparent
        // This allows the user to see the calibration targets
        SetTransparent();
    }

    private IEnumerator NoiseGrid()
    {
        noiseRoutineRunning = true;
        while (true)
        {
            RandomizeSpool();
            shaderMat.SetFloatArray("_NoiseSpool", spool);
            yield return new WaitForSeconds(noiseDelay);
        }
    }

    private void RandomizeSpool()
    {
        for (int i = 0; i < spool.Length; i++)
        {
            spool[i] = Random.value;
        }
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

    public void ToggleNoiseActive()
    {
        if (noiseActive)
        {
            SetNoiseActive(false);
        }
        else
        {
            SetNoiseActive(true);
        }
    }

    public void SetNoiseActive(bool state)
    {
        if (state)
        {
            shaderMat.SetFloat("_NoiseSize", 0.088f);
            if (!noiseRoutineRunning)
            {
                noiseRoutine = StartCoroutine(NoiseGrid());
            }
        }
        else
        {
            shaderMat.SetFloat("_NoiseSize", 0f);
            if (noiseRoutineRunning)
            {
                StopCoroutine(noiseRoutine);
                noiseRoutineRunning = false;
            }
        }
        noiseActive = state;
    }

    public void SetOpaque()
    {
        shaderMat.SetInt("_Transparency", 0);
        SetNoiseActive(false);
    }

    public void SetWindow()
    {
        shaderMat.SetInt("_Transparency", 1);
        //SetNoiseActive(true);
    }

    public void SetTransparent()
    {
        shaderMat.SetInt("_Transparency", 2);
        SetNoiseActive(false);
    }

    public void ShutWindow()
    {
        shaderMat.SetFloat("_WindowAX", 0);
        shaderMat.SetFloat("_WindowAY", 0);
        shaderMat.SetFloat("_WindowBX", 0);
        shaderMat.SetFloat("_WindowBY", 0);
    }
}
