using UnityEngine;
using System;
using System.IO;
using System.Text;
using extOSC;

public class OSCServer : MonoBehaviour
{
    private const int LANDMARK_COUNT = 543;

    private Transform virtualNeck, virtualHip;
    private float maxSpeed = 50f;
    private Body body;
    public Transform bodyParent;

    private string messages;
    public string blob;


    // Start is called before the first frame update
    void Start() {

        body = new Body(bodyParent, LANDMARK_COUNT);
        virtualNeck = new GameObject("VirtualNeck").transform;
        virtualHip = new GameObject("VirtualHip").transform;

        Debug.Log("Initialising OSC Bindings");
        Initialise();
        Debug.Log("OSC Bindings Initialised");
    }

    // Update is called once per frame
    void Update() {
        updateInstances();

    }
    private void Initialise()
    {
        // Initialize the receiver
        var receiver = gameObject.AddComponent<OSCReceiver>();
        receiver.LocalPort = 5005;
        receiver.Bind("/PythonData", ReceivedMessage);
        receiver.Bind("/video", ReceivedVideoData);
    }

    public static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
private void ReceivedVideoData(OSCMessage message)
{
    // Check if the message contains data
    if (message.ToBlob(out var value))
    {
        // Convert the byte array to a string
        string videoData = Encoding.UTF8.GetString(value);

        // Process the video data as needed
        // For example, you could display the video data, save it to a file, etc.

        // Here, we'll just log the received video data
        //Debug.Log("Received video data: " + videoData);
        Debug.LogWarning("Received ");
    }
    else
    {
        // Handle the case where the message does not contain valid data
        Debug.LogWarning("Received empty or invalid video data.");
    }
}

    private void ReceivedMessage(OSCMessage message)
    {
        /* Data send is long byte[] */
        if(message.ToBlob(out var value))
        {
            /* Convert byte[] to String */
            String data = Encoding.UTF8.GetString(value);
            /* Convert String to String[] for each new line */
            String[] lines = data.Split('\n');
            /* Process each line in data sent */
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                parseInput(line);
            }
        }
    }

    /* Converts the String line to its respective Data */
    private void parseInput(string line) {
        int lenPoses = 33;
        int lenFace = 478;
        int lenHand = 21;
        int index;
        /* Order of Landmarks sent
         * Pose -> Face -> LeftHand -> RightHand 
         */
        string[] parts = line.Split('|');
        if (parts.Length != 5)
        {
            Debug.Log("Invalid Input detected: " + parts);
            return;
        }
        /* calculate the offset index to add new position to buffer */
        index = int.Parse(parts[1]);
        switch (parts[0])
        {
            case "RH":
                index += (lenHand + lenFace + lenPoses);
                break;
            case "LH":
                index += (lenPoses + lenFace);
                break;
            case "FL":
                return;
                //index += (lenPoses);
                break;
            case "PL":
                break;
            default:
                break;
        }
    /* Add new position to position buffer */
    body.addValue(index, new Vector3(float.Parse(parts[2]), float.Parse(parts[3]), -float.Parse(parts[4])));
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
