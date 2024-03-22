using UnityEngine;
using UnityEngine.UI;
using extOSC;
using System;

public class displayImageP2 : MonoBehaviour
{
    // UI Image to display the received image
    public Image displayImage; // Renamed from displayImageP2

    // OSC Receiver
    private OSCReceiver receiver;

    void Start()
    {
        receiver = gameObject.AddComponent<OSCReceiver>();
        receiver.LocalPort = 5018;
        receiver.Bind("/video", ReceivedVideoData);

    }

    // Method to handle received video data
    private void ReceivedVideoData(OSCMessage message)
    {
        Debug.LogError("being called");

        if (displayImage == null)
        {
            Debug.LogError("Display image is not assigned.");
            return;
        }

        if (message == null)
        {
            Debug.LogError("Received null OSC message.");
            return;
        }

        if (!message.ToBlob(out var value))
        {
            Debug.LogError("Received empty or invalid video data.");

            // Create a white texture
            Texture2D whiteTexture = CreateWhiteTexture();
            Sprite whiteSprite = Sprite.Create(whiteTexture, new Rect(0, 0, whiteTexture.width, whiteTexture.height), Vector2.one * 0.5f);
            displayImage.sprite = whiteSprite;

            return;
        }

        try
        {
            // Convert the byte array to a texture
            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(value))
            {
                Debug.LogError("Failed to load image data.");
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

            Debug.LogError("Received and displayed image via OSC.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error while processing image data: " + e.Message);
        }
    }

    private Texture2D CreateWhiteTexture()
    {
        Texture2D texture = new Texture2D(2, 2);
        Color[] pixels = new Color[4];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
