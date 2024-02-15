using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Text;

public class PipeServer : MonoBehaviour
{
    private const int LANDMARK_COUNT = 543;
    private NamedPipeServerStream server;

    private Transform virtualNeck, virtualHip;
    private float maxSpeed = 50f;
    private Body body;
    public Transform bodyParent;

    // Start is called before the first frame update
    void Start() {

        body = new Body(bodyParent, LANDMARK_COUNT);
        virtualNeck = new GameObject("VirtualNeck").transform;
        virtualHip = new GameObject("VirtualHip").transform;


        Thread startServerThread = new Thread(runServer);
        Debug.Log("Starting Pipe Server");
        startServerThread.Start();
        Debug.Log("Started Pipe Server");
    }

    // Update is called once per frame
    void Update() {
        updateInstances();

    }

    private void runServer() {
        /* Open the named Pipe */
        server = new NamedPipeServerStream("UnityMediaPipeBody");

        Debug.Log("Waiting for connection...");
        server.WaitForConnection();
        Debug.Log("Connected");

        var br = new BinaryReader(server);
        while (true) { 
            try
            {
                var len = (int)br.ReadUInt32();
                var str = new string(br.ReadChars(len));

                string[] lines = str.Split('\n');
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    parseInput(line);
                }

            } catch (EndOfStreamException)
            {
                Debug.Log("Client has disconnected");
                break; /* Client has Disconnected */
            }
        }

        Debug.Log("Client Disconnected.");
        server.Close();
        server.Dispose();
    }

    /* Converts the String line to its respective Data */
    private void parseInput(string line) {
        string[] parts = line.Split('|');
        /* Index: 0, x: 1, y: 2, z: 3 */
        if (parts.Length != 4) {
            Debug.Log("Invalid Input Detected - " + line);
            return;
        }

        /* Add new position to position buffer */
        body.bPositions[int.Parse(parts[0])].addValue(new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), -float.Parse(parts[3])));
    }

    /* Uses the localPosition array to move the instances */
    private void updateInstances() {
        for (int i = 0; i < LANDMARK_COUNT; i++) {
            /* do not add movement vector if landmark not recorded enough */
            if (body.bPositions[i].enoughSamplesRecorded()) {
                /* add avarage movement recorded to current position */
                body.instances[i].transform.position = Vector3.MoveTowards(
                    body.instances[i].transform.position, body.bPositions[i].getBuffer(), Time.deltaTime * maxSpeed);
                
                body.bPositions[i].resetSamples();
            }
        }

        virtualNeck.transform.position = (body.instances[(int)Landmark.RIGHT_SHOULDER].transform.position + 
            body.instances[(int)Landmark.LEFT_SHOULDER].transform.position) / 2f;
        virtualHip.transform.position = (body.instances[(int)Landmark.RIGHT_HIP].transform.position + 
            body.instances[(int)Landmark.LEFT_HIP].transform.position) / 2f;


    }

    /* returns position of landmark given */
    public Transform getLandmark(Landmark mark) {
            return body.instances[(int)mark].transform;
    }

    public Transform getVirtualHip() {
        return virtualHip;
    }

    public Transform getVirtualNeck() {
        return virtualNeck;
    }
}
