using UnityEngine;
using UnityEngine.UI;
using extOSC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor.PackageManager;

public class Movement : MonoBehaviour
{
    private RectTransform rectangleTransform;
    public AvatarBody pipeServer;
    public float cursorSpeed = 5f; // Adjust cursor speed as needed
    private Vector2 wristCoords;
    private OSCReceiver receiver;
    private int closePort = 0;
    public int ClosePort { get => closePort; set => closePort = value; }

    void Start()
    {
        rectangleTransform = GetComponent<RectTransform>();

        receiver = gameObject.AddComponent<OSCReceiver>();
        receiver.LocalPort = 5005;
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
            Debug.Log("CLOSING\n");
        }

    }
    private void processInput(string input) { 
               // Get each part of input
        string[] parts = input.Split('|');
        if(parts[0] =="LH")
        {
            Debug.Log("hello");
             if(int.Parse(parts[1])==15)
                {
                        wristCoords.x = float.Parse(parts[2]);
                        wristCoords.y = float.Parse(parts[3]);
                                wristCoords.y = -wristCoords.y; // Invert y-axis if needed

                        Vector2 updatedPosition = new Vector2(
                            wristCoords.x * Screen.width,
                            wristCoords.y * Screen.height);

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