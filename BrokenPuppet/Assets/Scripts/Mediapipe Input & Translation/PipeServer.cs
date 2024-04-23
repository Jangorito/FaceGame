using UnityEngine;
using System;
using System.IO;
using System.Text;
using extOSC;

public class OSCServer : MonoBehaviour
{
    public GameObject Player1;
    public GameObject Player2;
    public GameObject ShadowPuppet;

    private Transform virtualNeck, virtualHip;
    private float maxSpeed = 50f;
    private Body body;
    public Transform bodyParent;

    private string messages;
    public string blob;

    // Used to Display debugging info
    Logger logger;
    
    // Flags if should display debugging info
    bool bShouldDebug;

    // Flags if server received any data
    bool bHasConnected = false;

    private void Awake()
    {
        // make instance of the logger
        logger = new(bShouldDebug);

        logger.LogMsg("PipeServer::Awake");

        // create new body object
        body = new Body(bodyParent);

        // set the neck and hip
        virtualNeck = new GameObject("VirtualNeck").transform;
        virtualHip = new GameObject("VirtualHip").transform;

        // establish OSC bindings
        logger.LogMsg("Initialising OSC Bindings");
        Initialise();
        logger.LogMsg("OSC Bindings Initialised");
    }

    // Start is called before the first frame update
    void Start() {
        logger.LogMsg("OSCServer::Start");

        // Activate Shadow Avatar
        ShadowPuppet.SetActive(true);

        // Activate Player 1
        Player1.SetActive(true);

        // Activate Player 2
        Player2.SetActive(true);
    }

    // Update is called once per frame
    void Update() {
        updateInstances();

    }
    private void Initialise()
    {
        // Initialize the receiver
        var receiver = gameObject.AddComponent<OSCReceiver>();

        // set the port
        receiver.LocalPort = 5005;

        // bind receiver to OSC channel
        receiver.Bind("/PythonData", ReceivedMessage);
   }

    public bool HasClients() {
        logger.LogMsg("OSCServer::HasClients");
        return bHasConnected;
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
        bHasConnected = true;

        // Split line into the input parts
        string[] parts = line.Split('|');

        // format should be: Type | Index | X | Y | Z
        if (parts.Length != 5)
        {
            logger.LogMsg("Invalid input detected: " + parts);
            return;
        }
        /* calculate the offset index to add new position to buffer */
        int index = int.Parse(parts[1]);
        switch (parts[0])
        {
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
    // Add new position to body position buffer; invert the z coord
    body.addValue(index, new Vector3(
        float.Parse(parts[2]), float.Parse(parts[3]), -float.Parse(parts[4])));
    }

    /* Uses the localPosition array to move the instances */
    private void updateInstances() {
        for (int i = 0; i < (int)LenLandmark.Total; i++) {
            // Do not update movement vector if landmark not recorded enough
            if (body.bPositions[i].enoughSamplesRecorded()) {
                // Take average of vectors recorded to gain new movement
                body.instances[i].transform.position = Vector3.MoveTowards(
                    body.instances[i].transform.position, body.bPositions[i].getBuffer(), 
                    Time.deltaTime * maxSpeed);
                
                // Clear the Accumulated Buffer of input vectors
                body.bPositions[i].resetSamples();
            }
        }
        
        // Use shoulder input to move neck
        virtualNeck.transform.position = (body.instances[(int)Landmark.RIGHT_SHOULDER].transform.position + 
            body.instances[(int)Landmark.LEFT_SHOULDER].transform.position) / 2f;

        // use left and right hip input to move hip
        virtualHip.transform.position = (body.instances[(int)Landmark.RIGHT_HIP].transform.position + 
            body.instances[(int)Landmark.LEFT_HIP].transform.position) / 2f;
    }

    // Get transform representing landmark
    public Transform GetLandmark(Landmark mark) {
            return body.instances[(int)mark].transform;
    }

    public Transform GetVirtualHip() {
        return virtualHip;
    }

    public Transform GetVirtualNeck() {
        return virtualNeck;
    }
}
