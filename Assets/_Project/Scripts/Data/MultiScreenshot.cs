using UnityEngine;
using System.IO;

public class MultiScreenshot : MonoBehaviour
{
    public Camera cam;

    private string folderPath;

    private void Awake()
    {
        folderPath = Application.dataPath + "/Screenshots/";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
    }

    private string GetTimestamp()
    {
        return System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    }

    public void CaptureAll()
    {
        string time = GetTimestamp();

        TakeScreenshot(1920, 1440, $"shot_4x3_{time}.png");
        TakeScreenshot(1920, 1080, $"shot_16x9_{time}.png");
        TakeScreenshot(2400, 1080, $"shot_20x9_{time}.png");

        Debug.Log("📸 Screenshots saved to: " + folderPath);
    }

    private void TakeScreenshot(int width, int height, string fileName)
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        cam.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        File.WriteAllBytes(folderPath + fileName, tex.EncodeToPNG());

        cam.targetTexture = null;
        RenderTexture.active = null;

        Destroy(rt);
        Destroy(tex);
    }
}
