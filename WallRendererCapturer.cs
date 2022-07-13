using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WallRendererCapturer : MonoBehaviour
{
    private Camera cam;
    public int width;
    public int height;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 3.75f;
        cam.aspect = (float)width / height;
    }

    public Texture2D TakeSnapshot()
    {
        RenderTexture tempRT = new RenderTexture(width, height, 24);
        cam.targetTexture = tempRT;
        cam.orthographic = true;
        cam.orthographicSize = 3.75f;
        cam.Render();

        RenderTexture.active = tempRT;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.aspect = (float)width / height;
        tex.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        RenderTexture.active = null;

        return tex;
    }

    public void SaveSnapshot(Texture2D tex, string path)
    {
        byte[] bytes;
        bytes = tex.EncodeToPNG();

        var imgFile = File.Open(path + "\\image.png", FileMode.Create);
        var bin = new BinaryWriter(imgFile);
        bin.Write(bytes);
        imgFile.Close();
    }
}
