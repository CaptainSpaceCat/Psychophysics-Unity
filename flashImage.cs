/*
 * flashImage.cs
 * 
 * Description: Flash image experiment. Can be either single image or image set per trial.
 * 
 * Parameters:
 *  - Resource Directory: image directory (in Resources)
 *  - Flash Time: image "on" time in seconds
 *  - Pause Time: time between images in an image set in seconds (per trial)
 *  - Single: select for single image presentation
 *  
 *  Inputs:
 *  - spacebar: next image set
 *  
 *  Author: Paul Jolly
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flashImage : MonoBehaviour
{
    // Unity user parameters
    public string resourceDirectory = "words";
    public float flashTime = 0.2f;
    public float pauseTime = 1.0f;
    public bool single = false;

    // Private variables
    private SpriteRenderer spriteR;
    private Sprite[] sprites;
    private List<int> inds;
    private int image = 0;
    private int randi = 0;
    private int ind = 0;
    private bool pause = false;
    private bool next = true;
    int count = 0;

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        sprites = Resources.LoadAll<Sprite>(resourceDirectory);

        if (single)
        {
            inds = createIntList(sprites.Length);
        } else
        {
            inds = createIntList(sprites.Length / 3);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space") && next)
        {
            next = false;
            if (inds.Count > 0)
            {
                if (image == 0)
                {
                    count++;
                    int randInd = Random.Range(0, inds.Count);
                    ind = inds[randInd];
                    inds.RemoveAt(randInd);

                    spriteR.sprite = sprites[3 * ind];
                    print(sprites[3 * ind]);
                    StartCoroutine(timer());
                    image = 1;
                    pause = true;
                }                
            }
        }

        if (!single && image > 0 && next)
        {
            if (image == 1)
            {
                randi = Random.Range(1, 3);
                spriteR.sprite = sprites[3 * ind + randi];
                print(sprites[3 * ind + randi]);
                image = 2;
            } else if (image == 2)
            {
                spriteR.sprite = sprites[3 * ind + (3 - randi)];
                print(sprites[3 * ind + (3 - randi)]);
                image = 0;
            }
            next = false;
            StartCoroutine(timer());
            pause = true;
        }

        if (pause && spriteR.sprite == null)
        {
            StartCoroutine(pauseTimer());
            pause = false;
        }

        if (count == sprites.Length)
        {
            print("Done");
        }
    }

    // Helper Functions
    IEnumerator timer()
    {
        yield return new WaitForSeconds(flashTime);
        spriteR.sprite = null;
    }

    IEnumerator pauseTimer()
    {
        yield return new WaitForSeconds(pauseTime);
        next = true;
    }

    List<int> createIntList(int maxInd)
    {
        List<int> listNumbers = new List<int>();
        for (int i = 0; i < maxInd; i++)
        {
            listNumbers.Add(i);
        }
        return listNumbers;
    }
}
