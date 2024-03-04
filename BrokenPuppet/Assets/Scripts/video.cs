using UnityEngine;
using UnityEngine.UI;
using extOSC;
using System;
public class DisplayImage : MonoBehaviour
{
    // UI Image to display the received image
    public Image displayImage;
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
        if (displayImage == null)
        {
            Debug.LogWarning("Display image is not assigned.");
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
            
            
            texture.SetPixels(pixels);
            texture.Apply();

            // Convert the texture to sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

            // Apply the sprite to the UI Image
            displayImage.sprite = sprite;

            Debug.Log("Received and displayed image via OSC.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error while processing image data: " + e.Message);
        }
    }

}