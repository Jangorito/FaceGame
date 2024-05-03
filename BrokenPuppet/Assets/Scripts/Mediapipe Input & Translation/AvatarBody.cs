using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
using System.Text;

public class AvatarBody
{

    public int m_iClientID = -1;

    private bool m_bIsConnected = false;

    private Transform m_VirtualNeck;

    private Transform m_VirtualHip;

    private Body m_Body;

    private const float m_fSpeed = 70;

    Logger logger;
    private const bool m_bShouldDebug = false;
    
    public AvatarBody(int iClientID) {
        logger = new Logger(m_bShouldDebug);

        m_iClientID = iClientID;

        m_Body = new Body(new GameObject("I see you reading this").transform);

        m_VirtualNeck = new GameObject("VirtualNeck"+getClientID()).transform;
        m_VirtualHip = new GameObject("VirtualHip"+getClientID()).transform;
    }

    public Transform GetLandmark(Landmark mark) {
        return m_Body.instances[(int)mark].transform;
    }

    public Transform GetVirtualHip() {
        return m_VirtualHip;
    }

    public Transform GetVirtualNeck() {
        return m_VirtualNeck;
    }

    private int getClientID() { return m_iClientID; }

    public bool IsConnected() { return m_bIsConnected; }
    
    public void SetIsConnected(bool val) { m_bIsConnected = val; }

    public void ReceivedMessage(OSCMessage message) {
        SetIsConnected(true);

        // Read Data from long byte[]
        if (message.ToBlob(out var value)) {

            // Convert byte[] to string
            string data = Encoding.UTF8.GetString(value);

            // Separate Each line in string
            string[] lines = data.Split('\n');

            foreach (string line in lines) {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                processInput(line);
            }
        }
    }

    private void processInput(string input) { 
        
        // Get each part of input
        string[] parts = input.Split('|');

        // format should be: Type | Index | X | Y | Z
        if (parts.Length != 5)
            return;

        // Calculate the landmark offset
        int index = int.Parse(parts[1]);
        switch (parts[0]) {
            case "RH":
                index += ((int)LenLandmark.LeftHand + (int)LenLandmark.Face + (int)LenLandmark.Poses);
                break;
            case "LH":
                index += ((int)LenLandmark.Face + (int)LenLandmark.Poses);
                break;
            case "FL":
                return;
            case "PL":
                break;
            
            // Invalid data has been given
            default:
                return;
        }

        m_Body.addValue(index, new Vector3(
            float.Parse(parts[2]), float.Parse(parts[3]), -float.Parse(parts[4])));
    }

    public void updateBody() {
        for (int i = 0; i < (int)LenLandmark.Total; i++) {
            // Do not update movement vector if landmark not recorded enough
            if (m_Body.bPositions[i].enoughSamplesRecorded()) {
                // Take average of vectors recorded to gain new movement
                m_Body.instances[i].transform.position = Vector3.MoveTowards(
                m_Body.instances[i].transform.position, m_Body.bPositions[i].getBuffer(),
                Time.deltaTime * m_fSpeed);

                // Clear the Accumulated Buffer of input vectors
                m_Body.bPositions[i].resetSamples();
            }
        }

        // Use shoulder input to move neck
        m_VirtualNeck.transform.position = (m_Body.instances[(int)Landmark.RIGHT_SHOULDER].transform.position + 
            m_Body.instances[(int)Landmark.LEFT_SHOULDER].transform.position) / 2f;

        // use left and right hip input to move hip
        m_VirtualHip.transform.position = (m_Body.instances[(int)Landmark.RIGHT_HIP].transform.position + 
            m_Body.instances[(int)Landmark.LEFT_HIP].transform.position) / 2f;
    }
}
