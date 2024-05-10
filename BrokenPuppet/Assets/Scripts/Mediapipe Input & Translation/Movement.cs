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

    private Transform targetLandmark;

    private RectTransform rectangleTransform;
    public float cursorSpeed = 5f; // Adjust cursor speed as needed

    public bool bShouldDebug = true;
    Logger logger;

    private void Awake()
    {
        logger = new(bShouldDebug);
    }

    void Start()
    {

        receiver = Receiver.GetComponent<Receiver>();
        if (receiver == null) {
            logger.LogError("Receiver GameObject does not contain Receiver script");
            enabled = false;
            return;
        }
        inputData = receiver.GetBody(0);
        rectangleTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        targetLandmark = inputData.GetLandmark(Landmark.RIGHT_WRIST);
        Vector2 updatedPosition = new Vector2(
            (targetLandmark.position.x * Screen.width)-1200,
            (targetLandmark.position.y * Screen.height)+500
            );

        updatedPosition.x = Mathf.Clamp(updatedPosition.x, -500, 500);
        updatedPosition.y = Mathf.Clamp(updatedPosition.y, -270, 270);

        Vector2 newPosition=  Vector2.Lerp(
            rectangleTransform.anchoredPosition, updatedPosition, 
            Time.deltaTime * cursorSpeed
            );
        rectangleTransform.anchoredPosition = newPosition;
    }
}