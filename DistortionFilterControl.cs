/*
 * DistortionFilterControl.cs
 * 
 * Description: General script to control "distortion filter" in real time.
 * 
 * Parameters:
 *  - Reset Color: Default color for view region
 *  - Divisor: Fraction of total pixels to be shaded with each pass
 *  
 *  Author: Paul Jolly
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Psychophysics
{
    public class DistortionFilterControl : MonoBehaviour
    {
        // Public Variables
        public Color resetColor;
        public int divisor;

        // Private Variables
        MeshRenderer meshRenderer;
        int texWidth = 1000, textHeight = 100;
        Texture2D dstTex;
        Color testColor;
        Color[] dstColors, rstColors, testColors;
        int[] options;
        int count = 0;
        bool done = false;
        float time;
        int numPasses = 0;

        Vector3 prevPosition;

        // Start is called before the first frame update
        void Start()
        {
            meshRenderer = this.GetComponent<MeshRenderer>();
            prevPosition = this.transform.parent.localPosition;

            // Initialize destination texture (to be passed to fragment shader)
            dstTex = new Texture2D(texWidth, texWidth);
            dstColors = dstTex.GetPixels();
            rstColors = dstTex.GetPixels();
            testColor = new Color(resetColor.r, resetColor.g, resetColor.b, 0);
            testColors = dstTex.GetPixels();
            options = new int[rstColors.Length];
            for (int i = 0; i < rstColors.Length; i++)
            {
                rstColors[i] = resetColor;
                dstColors[i] = resetColor;
                testColors[i] = testColor;
                options[i] = i;
            }
            dstTex.SetPixels(rstColors);
            dstTex.Apply();
            meshRenderer.material.SetTexture("_MainTex", dstTex); // Pass texture to fragment shader
        }

        // Update is called once per frame
        void Update()
        {
            if (!done)
            {
                numPasses += 1;
                for (int i = 0; i < dstColors.Length / divisor; i++)
                {
                    int pix_i = options[count % dstColors.Length];
                    dstColors[pix_i] = new Color(0,0,0,0);
                    count += 1;
                    if (count >= dstColors.Length)
                    {
                        done = true;
                        //print(Time.realtimeSinceStartup - time);
                        //print(numPasses);
                        numPasses = 0;
                        break;
                    }
                }

                // Set destination texture and pass to fragment shader
                dstTex.SetPixels(dstColors);
                dstTex.Apply();
                meshRenderer.material.SetTexture("_MainTex", dstTex);
            }

            if (Input.GetKey(KeyCode.A))
            {
                dstTex.SetPixels(testColors);
                dstTex.Apply();
                meshRenderer.material.SetTexture("_MainTex", dstTex);
            }
        }

        // Shuffle indices used to index pixels
        void Shuffle()
        {
            for (int i = 0; i < options.Length; i++)
            {
                int rnd = Random.Range(0, options.Length);
                int temp = options[rnd];
                options[rnd] = options[i];
                options[i] = temp;
            }
        }

        public IEnumerator resetFilter()
        {
            yield return null;
            count = 0;
            done = false;
            Shuffle();
            System.Array.Copy(rstColors, dstColors, dstColors.Length);
            dstTex.SetPixels(rstColors);
            dstTex.Apply();
            meshRenderer.material.SetTexture("_MainTex", dstTex);
            time = Time.realtimeSinceStartup;
        }
    }
}
