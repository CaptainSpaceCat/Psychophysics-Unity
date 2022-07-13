using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class SystemTime : MonoBehaviour
{
    DateTime time;
    StreamWriter writer;
    // Start is called before the first frame update
    void Start()
    {
        writer = File.CreateText("C:\\work\\pupil\\pupil_src\\data_recieve_timestamp.txt");
    }
    
    private string Now()
    {
        time = DateTime.Now;
        return "" + time.Hour +":"+ time.Minute + ":" + time.Second + ":" + time.Millisecond;
    }

    public void Log(string msg)
    {
        writer.WriteLine(Now() + "|=|" + msg);
    }

    public void Close()
    {
        if (writer != null)
        {
            writer.Close();
        }
    }
}
