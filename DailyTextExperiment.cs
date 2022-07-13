using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DailyTextExperiment : Experiment
{
    public TextMeshPro textMesh;
    public int dailyIdx;

    public override void Next()
    {
        base.Next();
        textMesh.gameObject.SetActive(true);
        RandomizeText();
    }

    public override void Clear()
    {
        // clear objects associated with this experiment
        textMesh.gameObject.SetActive(false);
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

    private void RandomizeText()
    {
        textMesh.SetText(GetRandomText());
        textMesh.ForceMeshUpdate();
        string pruned = textMesh.text.Substring(0, textMesh.firstOverflowCharacterIndex);
        pruned = pruned.Substring(0, pruned.LastIndexOf(' '));
        textMesh.SetText(pruned);
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
            }
            else if (!fencepost)
            {
                output += "\u00AD";
            }
            else
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
        int idx = (int)Random.Range(0, strlen - 250);
        return AutoHyphenate(fullText.Substring(fullText.IndexOf(' ', idx) + 1, 250));
    }
}
