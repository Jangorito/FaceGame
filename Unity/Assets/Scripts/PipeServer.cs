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
    Body body;
    public Transform model;

    private NamedPipeServerStream server;

    // Start is called before the first frame update
    void Start()
    {
        body = new Body(model);

        Thread startServerThread = new Thread(runServer);
        Debug.Log("Starting Pipe Server");
        startServerThread.Start();
        Debug.Log("Started Pipe Server");
    }

    // Update is called once per frame
    void Update()
    {
        body.update();
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
                int lenLine = 0;
                foreach (string l in lines)
                {
                    if (string.IsNullOrWhiteSpace(l))
                        continue;
                    body.updateLandmark(lenLine++, l);
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
}
