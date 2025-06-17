using UnityEngine;
using UnityEngine.UI;
using extOSC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;



public class Movement : MonoBehaviour
{

    // GameObject containing the OSCReceiver
    public GameObject Receiver;

    // Script containing the AvatarBody
    private Receiver receiver;

    // The Class containing the actual mediapipe input
    private AvatarBody inputData;

    private Transform targetLandmark; // the right wrist landmark

    private RectTransform rectangleTransform; // RectTransform of the UI element to move
    public float cursorSpeed = 5f; // Adjust cursor speed as needed

    private int timeoutCounter = 0; // Counter for timeout handling

    public bool bShouldDebug = true;
    Logger logger;

    private void Awake()
    {
        logger = new(bShouldDebug);
    }

    void Start()
    {

        receiver = Receiver.GetComponent<Receiver>();
        if (receiver == null)
        {
            logger.LogError("Receiver GameObject does not contain Receiver script");
            enabled = false;
            return;
        }
        inputData = receiver.GetBody(0);
        rectangleTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        timeoutCounter++;
        targetLandmark = inputData.GetLandmark(Landmark.RIGHT_WRIST);
        if (targetLandmark.position.x == 0){
            // If the target landmark is not set or has no valid position, skip the update
            if (timeoutCounter == 1)
            {
                logger.LogMsg("Target landmark position is invalid, skipping update.");
            }
            // logger.LogMsg("Target landmark position is invalid, skipping update.");
            // Debug.Log("Target landmark position is invalid, skipping update.");
            return;
        }

        // Vector2 updatedPosition = new Vector2(
        //     (targetLandmark.position.x * Screen.width)-672, // 672 is the width of the screen, value used to be 1200
        //     (targetLandmark.position.y * Screen.height)+489 // 489 is the height of the screen, value used to be 500
        //     ); // 

        // updatedPosition.x = Mathf.Clamp(updatedPosition.x, -500, 500);
        // updatedPosition.y = Mathf.Clamp(updatedPosition.y, -270, 270);


        // Map normalized [0,1] to anchoredPosition with (0,0) at center
        float x = targetLandmark.position.x * Screen.width;
        float y = (targetLandmark.position.y + 0.5f) * Screen.height;
        Vector2 updatedPosition = new Vector2(x, -y);


        // Optional: Clamp to keep cursor within visible area
        updatedPosition.x = Mathf.Clamp(updatedPosition.x, -Screen.width / 2f, Screen.width / 2f);
        updatedPosition.y = Mathf.Clamp(updatedPosition.y, -Screen.height / 2f, Screen.height / 2f);

        // Log the updated position for debugging
        if (timeoutCounter % 333 == 0) // Log every 333 frames
        {
            logger.LogMsg($"Target Landmark Position: {targetLandmark.position} (x: {x}, y: {y})\n" +
                          $"Updated Position after Clamp: {updatedPosition}\n" +
                          $"Screen Size: {Screen.width}x{Screen.height}");
        } 

        Vector2 newPosition = Vector2.Lerp(
            rectangleTransform.anchoredPosition, updatedPosition,
            Time.deltaTime * cursorSpeed
            );
        rectangleTransform.anchoredPosition = newPosition;
    }
}