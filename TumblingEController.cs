using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TumblingEController : MonoBehaviour
{
    [Range(0, 0.25f)]
    public float size;
    public TextMeshProUGUI tmp;

    public void Randomize()
    {
        transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f * Random.Range(0, 4)));
    }

    public void SetHidden(bool state)
    {
        if (state)
        {
            tmp.SetText("");
        } else
        {
            tmp.SetText("E");
        }
    }

    
    private void Update()
    {
        tmp.fontSize = size;
    }
}
