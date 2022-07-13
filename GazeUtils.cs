using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GazeUtils
{
    public static float GetDeltaAngle(Vector3 a, Vector3 b)
    {
        return Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(a, b) / (Vector3.Magnitude(a) * Vector3.Magnitude(b)));
    }

    private static Queue<Vector3> window = new Queue<Vector3>();
    private static int kWindowLen = 10;
    private static float stableGazeThreshold = 1f;

    public static bool GazeIsStable(Vector3 gazeDir)
    {
        bool result = false;
        if (window.Count == kWindowLen)
        {
            bool flag = true;
            for (int i = 0; i < kWindowLen; i++)
            {
                Vector3 compare = window.Dequeue();
                float delta = GetDeltaAngle(gazeDir, compare);
                if (delta > stableGazeThreshold)
                {
                    flag = false;
                }
                window.Enqueue(compare);
            }
            result = flag;
        }

        window.Enqueue(gazeDir);
        if (window.Count > kWindowLen)
        {
            window.Dequeue();
        }

        return result;
    }

    public static void ResetStabilityWindow()
    {
        window.Clear();
    }
}
