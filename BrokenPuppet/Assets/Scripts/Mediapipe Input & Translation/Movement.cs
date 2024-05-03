using UnityEngine;
using UnityEngine.UI;
using extOSC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class Movement : MonoBehaviour
{
    private RectTransform rectangleTransform;
    public AvatarBody pipeServer;
    public float cursorSpeed = 5f; // Adjust cursor speed as needed
    private Vector2 wristCoords;
    private OSCReceiver receiver;
    public int port;
    public static int closePort = 0;
    void Start()
    {
        closePort = 0; ;
        rectangleTransform = GetComponent<RectTransform>();

        receiver = gameObject.AddComponent<OSCReceiver>();
        receiver.LocalPort = port;
        receiver.Bind("/PythonData", ReceivedLMData);
    }
      private void ReceivedLMData(OSCMessage message)
    {
        //Debug.LogError("helloooooooo\n");
        // Read Data from long byte[]
        if (closePort == 0)
        {
            if (message.ToBlob(out var value))
            {

                // Convert byte[] to string
                string data = Encoding.UTF8.GetString(value);

                // Separate Each line in string
                string[] lines = data.Split('\n');

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    processInput(line);
                }
            }
        }
        else
        {
            receiver.Close();
            receiver.LocalPort = 0;
            Debug.Log("CLOSING\n");
        }

    }
   private void processInput(string input) { 
    // Get each part of input
    string[] parts = input.Split('|');
    if(parts[0] =="LH")
    {
        if(int.Parse(parts[1])==15)
        {
            // Set wristCoords directly from the input
            wristCoords.x = float.Parse(parts[2]);
            wristCoords.y = float.Parse(parts[3]);
            wristCoords.y = -wristCoords.y; // Invert y-axis if needed
            //Debug.Log($"Wrist Coordinates: x: {wristCoords.x}, y: {wristCoords.y}");
            // Get the RectTransform component attached to the canvas
            RectTransform canvasRectTransform = GetComponent<RectTransform>();

            // Get the width and height of the canvas
            float canvasWidth = canvasRectTransform.sizeDelta.x;
            float canvasHeight = canvasRectTransform.sizeDelta.y;

            // Map the wrist coordinates to canvas coordinates
            Vector2 updatedPosition = new Vector2(
                (wristCoords.x * Screen.width)-1200,
                (wristCoords.y * Screen.height)+500);
             //Debug.Log($"Before clamp: x: {updatedPosition.x}, y: {updatedPosition.y}");

            // Clamp the updated position to canvas boundaries
            updatedPosition.x = Mathf.Clamp(updatedPosition.x, -500, 500);
            updatedPosition.y = Mathf.Clamp(updatedPosition.y, -270, 270);
            //Debug.Log($"after clamp: x: {updatedPosition.x}, y: {updatedPosition.y}");
            // Smoothly move the cursor towards the updated position
            Vector2 newPosition = Vector2.Lerp(rectangleTransform.anchoredPosition, updatedPosition, Time.deltaTime * cursorSpeed);
            rectangleTransform.anchoredPosition = newPosition;
        }
    }
}

    void Update()
    {
      
    }
}