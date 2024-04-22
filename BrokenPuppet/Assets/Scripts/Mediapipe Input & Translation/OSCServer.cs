using UnityEngine;
using System.Text;
using extOSC;

public class OSCServer : MonoBehaviour
{
    // Stores the movement of the Neck and Hip from mediapipe
    private Transform virtualNeck, virtualHip;

    // The fastest that a bone can move 
    private const float maxSpeed = 50f;

    private Body body;
    public Transform bodyParent;

    public GameObject Player1;
    public GameObject Player1Shadow;
    //public GameObject Player2;

    // flags if the server has received data from a client
    private bool bHasConnected = false;

    // Flags if script should output debugging info
    public bool shouldDebug;

    // Used to display debugging info
    private Logger logger;

    // The port that the OSC will listen on
    public int receiver_port  = 5005;

    // The channel that the OSC will listen on 
    public string receiver_channel = "/PythonData";

    private void Awake()
    {

        // set instance of the logger
        logger = new(shouldDebug);

        logger.LogMsg("OSCServer::Awake");

        // create the body object
        body = new(bodyParent);

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

        // Activate Player 1
        Player1.SetActive(true);

        // Activate Player 1 Shadow
        Player1Shadow.SetActive(true);

        // Activate Player 2
        //Player2.SetActive(true);
    }

    // Update is called once per frame
    void Update() {
        UpdateInstances();
    }

    private void Initialise()
    {
        // Initialize the receiver
        var receiver = gameObject.AddComponent<OSCReceiver>();

        // set the port
        receiver.LocalPort = receiver_port;

        // bind receiver to OSC channel
        receiver.Bind(receiver_channel, ReceivedMessage);
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
            logger.LogWar("Received ");
        }
        else
        {
            // Handle the case where the message does not contain valid data
            logger.LogWar("Received empty or invalid video data.");
        }
    }

    public bool HasClients() { 
        logger.LogMsg("OSCServer::HasClients");
        return bHasConnected; 
    }

    /* Order of Landmarks sent
         * Pose -> Face -> LeftHand -> RightHand 
         */
    private void ReceivedMessage(OSCMessage message)
    {
        // read message as long byte[] */
        if(message.ToBlob(out var value))
        {
            // Convert byte[] to String
            string inputData = Encoding.UTF8.GetString(value);

            // Split data into array of each line
            string[] lines = inputData.Split('\n');

            // Process each line in data sent
            foreach (string line in lines)
            {
                // ignore empty/EOF lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // process data of the line
                ParseInput(line);
            }
        }
    }

    /* Converts the String line to its respective Data */
    private void ParseInput(string line) {
        bHasConnected = true;

        // Split line into the input parts
        string[] parts = line.Split('|');

        // format should be: Type | Index | X | Y | Z
        // e.g. PL | 21 | 0.2372 | 0.35 | 0.01
        if (parts.Length != 5)
        {
            logger.LogMsg("Invalid Input detected: " + parts);

            // Ignore invalid input
            return;
        }

        // Calculate the global index of the given local index
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
                //index += (int)LenLandmark.Poses;

                // ignore Face input now
                return;

            // Pose landmarks are sent first so no offset needed
            case "PL":
                break;

            // Invalid data has been given
            default:
                return;
        }

        // Add new position to body position buffer: invert the z coord
        body.addValue(index, new Vector3(
            float.Parse(parts[2]), float.Parse(parts[3]), -float.Parse(parts[4])));
    }

    /* Uses the localPosition array to move the instances */
    private void UpdateInstances() {
        for (int i = 0; i < (int)LenLandmark.Total; i++) {

            // Do not update movement vecotr if landmark not recorded enough
            if (body.bPositions[i].enoughSamplesRecorded()) {

                // Take avarage of vectors recorded to gain new movement
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

        // Use left and right hip input to move hip
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
