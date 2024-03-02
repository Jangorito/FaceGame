using UnityEngine;
using System;
using extOSC;

public class DisplayImage : MonoBehaviour
{
    // Material to display the received image
    public Material displayMaterial;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the OSC receiver
        var receiver = gameObject.AddComponent<OSCReceiver>();
        receiver.LocalPort = 5005;
        receiver.Bind("/video", ReceivedVideoData);
    }

    // Method to handle received video data
   // Method to handle received video data
private void ReceivedVideoData(OSCMessage message)
{
    if (displayMaterial == null)
    {
        Debug.LogWarning("Display material is not assigned.");
        return;
    }

    if (message == null)
    {
        Debug.LogWarning("Received null OSC message.");
        return;
    }

    if (!message.ToBlob(out var value))
    {
        Debug.LogWarning("Received empty or invalid video data.");
        return;
    }

    try
    {
        // Convert the byte array to a texture
        Texture2D texture = new Texture2D(2, 2);
        if (!texture.LoadImage(value))
        {
            Debug.LogWarning("Failed to load image data.");
            return;
        }

        // Flip the texture vertically
        Color[] pixels = texture.GetPixels();
        Color[] flippedPixels = new Color[pixels.Length];
        int width = texture.width;
        int height = texture.height;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flippedPixels[x + (height - y - 1) * width] = pixels[x + y * width];
            }
        }
        texture.SetPixels(flippedPixels);
        texture.Apply();

        // Apply the texture to the display material
        displayMaterial.mainTexture = texture;

        // Adjust the scale of the object to match the aspect ratio of the texture
        float aspectRatio = (float)texture.width / texture.height;
        transform.localScale = new Vector3(aspectRatio, 1f, 1f);

        Debug.Log("Received and displayed image via OSC.");
    }
    catch (Exception e)
    {
        Debug.LogError("Error while processing image data: " + e.Message);
    }
}

}