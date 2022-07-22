using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GreenLetterTest : MonoBehaviour
{

    public GazeRaycaster gazeRaycaster;
    public GameObject greenTextObj;
    public TextMeshProUGUI greenText;
    public Text buttonText;

    /*private string[] words =
    {
        "STAR",
        "RATS",
        "FART",
        "LOUD",
        "NOON",
        "DING",
        "FORK",
        "LARD",
        "AGES",
        "FLYS",
        "POUT",
        "WORD",
        "EARS",
        "CORE",
        "APPS",
        "KING",
        "DRIP",
        "SASH",
        "WAIT"
    };*/
    private string[] words = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

    private int wordIdx = -1;
    public void RandomizeText()
    {
        int oldIdx = wordIdx;
        while (oldIdx == wordIdx) {
            wordIdx = Random.Range(0, words.Length);
        }
        greenText.text = words[wordIdx];
        buttonText.text = words[wordIdx];
    }

    public void SetTextActivated(bool state)
    {
        if (state)
        {
            gazeRaycaster.OnRaycastSuccessful += RenderGreenText;
            RandomizeText();
        } else
        {
            gazeRaycaster.OnRaycastSuccessful -= RenderGreenText;
            greenText.text = "";
        }
    }

    private void RenderGreenText(Vector2 oHit, Vector2 wHit, float time)
    {
        greenTextObj.transform.localPosition = new Vector3(oHit.x, oHit.y, 2);
    }
}
