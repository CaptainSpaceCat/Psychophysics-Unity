using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImageNetExperiment : Experiment
{
    public SpriteRenderer wallRenderer;
    private string spriteDir = "Images/grayscale/";

    public override void Next()
    {
        base.Next();
        wallRenderer.gameObject.SetActive(true);
        RandomizeSprite();
    }

    private void RandomizeSprite()
    {
        int choice = Random.Range(0, GetNumImages(spriteDir));
        wallRenderer.sprite = Resources.Load<Sprite>(spriteDir + choice);
    }

    public override void Clear()
    {
        wallRenderer.gameObject.SetActive(false);
        // clear objects associated with this experiment
    }

    public override void LogDataPoint(Vector2 point, float time)
    {
        // save data from this experiment
        DataLogger.LogGazePoint(point, time);
    }

    public override int GetRaycastMode()
    {
        //0 - disabled, 1 - following, 2 - locked
        return 1;
    }

    private int GetNumImages(string dir)
    {
        int n = 0;
        foreach (string fn in Directory.EnumerateFiles("Assets/Resources/" + dir, "*.png"))
        {
            n++;
        }
        return n;
    }
}
