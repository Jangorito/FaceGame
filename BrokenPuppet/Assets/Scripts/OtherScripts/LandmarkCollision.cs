using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
using System.Text;

public class LandmarkCollision : MonoBehaviour
{
    private float startTime;
    private bool threeSecondsPassed = false;
    public float xPosL;
    public float zPosL;
    public float yPosL;

    public float xPosR;
    public float zPosR;
    public float yPosR;

    public Vector3 handPositionRight; // Changed from Vec3 to Vector3
    public Vector3 handPositionLeft;  // Changed from Vec3 to Vector3

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        // Assuming GetLandmark is a method that returns a Vector3 representing the position
        // Update method had missing closing brace
        handPositionRight = transform.position; // Assuming you want to assign some position, corrected for context
        handPositionLeft = transform.position;  // Assuming you want to assign some position, corrected for context

        xPosL = handPositionLeft.x;
        yPosL = handPositionLeft.y;
        zPosL = handPositionLeft.z;

        xPosR = handPositionRight.x;
        yPosR = handPositionRight.y;
        zPosR = handPositionRight.z;

        // Distance calculation using Vector3.Distance
        if (Vector3.Distance(handPositionRight, handPositionLeft) < 0.01f)
        {
            if (CheckIfThreeSecondsPassed())
            {
                Debug.Log("Collision alert evacuate immediately before you die");
            }
        }
    }

    private void ResetTimer()
    {
        startTime = Time.time;
        threeSecondsPassed = false;
    }

    // Function to check if 3 seconds have passed
    private bool CheckIfThreeSecondsPassed()
    {
        if (!threeSecondsPassed && Time.time - startTime >= 3.0f)
        {
            threeSecondsPassed = true;
            return true;
        }
        return false;
    }
}