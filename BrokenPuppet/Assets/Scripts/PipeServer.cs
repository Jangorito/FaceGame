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

    private Vector3[] instances = new Vector3[LANDMARK_COUNT];
    private Vector3[] localPosition = new Vector3[LANDMARK_COUNT];
    private float maxSpeed = 50f;


    // Start is called before the first frame update
    void Start() {
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

        int index = int.Parse(parts[0]);
        Vector3 position = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), -float.Parse(parts[3]));
        localPosition[index] = position;        
    }

    /* Uses the localPosition array to move the instances */
    private void updateInstances() {
        for (int i = 0; i < LANDMARK_COUNT; i++) {
            instances[i] = Vector3.MoveTowards(instances[i], localPosition[i],
                Time.deltaTime * maxSpeed);
        }
    }

    public Vector3 getLandmark(Landmark mark) {
        return instances[(int)mark];
    }

}
