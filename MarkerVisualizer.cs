using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerVisualizer : MonoBehaviour
{
    public GameObject greenEye;
    public GameObject redEye;

    // Start is called before the first frame update
    void Start()
    {
        SetGreen(true);
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform);
    }

    public void SetGreen(bool state)
    {
        greenEye.SetActive(state);
        redEye.SetActive(!state);
    }
}
