using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DataLogger
{
    private static string outDir = "C:\\Users\\chich\\Desktop\\Output";
    private static StreamWriter logWriter;
    private static int experimentIndex;
    private static int validationIndex;
    private static int blockIndex = 0;
    private static float experimentStartTime;

    public static int NextBlock()
    {
        //blockIndex = 0;
        while (Directory.Exists(outDir + "\\block_" + blockIndex))
        {
            blockIndex++;
        }
        Directory.CreateDirectory(outDir + "\\block_" + blockIndex);
        experimentIndex = -1;
        validationIndex = 0;
        return blockIndex;
    }

    public static void NextTrial(float startTime)
    {
        Close();
        experimentIndex++;
        experimentStartTime = startTime;
        logWriter = File.CreateText(GetDataPath("gazeData.txt"));
    }

    public static void NextValidation()
    {
        logWriter = File.CreateText(GetValidationPath());
        validationIndex++;
    }

    public static void NextCalibration()
    {
        logWriter = File.CreateText(GetCalibrationPath());
    }

    private static string GetDataPath(string suffix)
    {
        if (!Directory.Exists(outDir + "\\block_" + blockIndex + "\\" + experimentIndex))
        {
            Directory.CreateDirectory(outDir + "\\block_" + blockIndex + "\\" + experimentIndex);
        }
        return outDir + "\\block_" + blockIndex + "\\" + experimentIndex + "\\" + suffix;
    }

    private static string GetValidationPath()
    {
        return outDir + "\\validation_" + blockIndex + "_" + validationIndex + ".txt";
    }

    private static string GetCalibrationPath()
    {
        return outDir + "\\calibration.txt";
    }

    //we pass in the timestamp collected at the time of the raycast to eliminate any extra milliseconds spend sending the data to the logger
    public static void LogGazePoint(Vector2 hitPos, float time)
    {
        logWriter.WriteLine("(" + hitPos.x + "," + hitPos.y + "):" + (time - experimentStartTime));
    }

    public static void LogValidationPoint(Vector2 truePoint, Vector2 sampledDelta)
    {
        logWriter.WriteLine(
            "(" + truePoint.x + "," + truePoint.y + "):" +
            "(" + sampledDelta.x + "," + sampledDelta.y + ")"
            );
    }

    public static void SaveSnapshot(Texture2D tex)
    {
        byte[] bytes;
        bytes = tex.EncodeToPNG();

        var imgFile = File.Open(GetDataPath("image.png"), FileMode.Create);
        var bin = new BinaryWriter(imgFile);
        bin.Write(bytes);
        imgFile.Close();
    }

    public static void Close()
    {
        if (logWriter != null)
        {
            logWriter.Close();
        }
    }
}
