using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class WallTextureController : MonoBehaviour
{
    public SpriteRenderer wallRenderer;
    public Sprite defaultSprite;
    private Sprite currentSprite;
    public TextMeshPro wallText;
    public TextMeshPro wallTextResearch;
    public int dailyIdx;

    private void Awake()
    {
        Default();
        Display();
    }

    public enum ExperimentType
    {
        // Change or add to this enum to create new experiments
        ImageNet,
        Rectangles,
        Text,
    }
    public ExperimentType experiment;

    // Change or add to this switch statement to create new experiments
    private string GetExperimentDirectory(ExperimentType type)
    {
        switch (type) {
            case ExperimentType.ImageNet:
                return "Images/grayscale/";
            case ExperimentType.Rectangles:
                return "Images/rectangles/";
            case ExperimentType.Text:
                return "Images/text/";
        }
        throw new System.Exception("Error: Experimental type not recognized");
    }

    private int GetNumImages(string dir)
    {
        int n = 0;
        foreach (string fn in Directory.EnumerateFiles(dir, "*.png"))
        {
            n++;
        }
        return n;
    }

    public string AutoHyphenate(string message)
    {
        string output = "";
        bool fencepost = true;
        foreach (char c in message)
        {
            if (c == ' ')
            {
                fencepost = true;
            } else if (!fencepost)
            {
                output += "\u00AD";
            } else
            {
                fencepost = false;
            }
            output += c;
        }
        return output;
    }

    public string GetRandomText()
    {
        TextAsset txt = Resources.Load("TextCorpus/daily_" + dailyIdx) as TextAsset;
        string fullText = txt.text;
        int strlen = fullText.Length;
        int idx = (int)Random.Range(0, strlen-250);
        return AutoHyphenate(fullText.Substring(fullText.IndexOf(' ', idx)+1, 250));
    }

    public Sprite LoadNextSprite()
    {
        int max = 0;
        wallText.text = "";
        switch (experiment)
        {
            case ExperimentType.ImageNet:
                max = 300;
                break;
            case ExperimentType.Rectangles:
                max = 15;
                break;
            case ExperimentType.Text:
                max = 1;
                wallText.text = GetRandomText();
                wallText.ForceMeshUpdate();
                string pruned = wallText.text.Substring(0, wallText.firstOverflowCharacterIndex);
                pruned = pruned.Substring(0, pruned.LastIndexOf(" "));
                wallText.text = pruned;
                break;
        }
        wallTextResearch.text = wallText.text;
        string dir = GetExperimentDirectory(experiment);
        int choice = (int)Random.Range(0, max);
        Sprite nextSprite = Resources.Load<Sprite>(dir + choice);
        currentSprite = nextSprite;
        return nextSprite;
    }

    public void Default()
    {
        currentSprite = defaultSprite;
        wallText.text = "";
        wallTextResearch.text = "";
    }

    public void Display()
    {
        if (currentSprite != null)
        {
            wallRenderer.sprite = currentSprite;
        }
        else
        {
            wallRenderer.sprite = defaultSprite;
        }
    }
}
