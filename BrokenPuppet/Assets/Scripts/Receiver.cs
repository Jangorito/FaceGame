using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
using static UnityEditor.Experimental.GraphView.GraphView;
using System.Numerics;

public class Receiver : MonoBehaviour
{
    public static OSCReceiver p1Receiver;
    public static OSCReceiver p2Receiver;

    public static GameObject p1_body;
    public static GameObject p1_vneck;
    public static GameObject p1_vhip;

    public static GameObject p2_body;
    public static GameObject p2_vneck;
    public static GameObject p2_vhip;

    private static AvatarBody p1Input;
    private static AvatarBody p2Input;

    static bool bInitialised = false;

    private void Awake()
    {


        if (bInitialised) {
            p1Input = new(0, p1_body, p1_vneck, p1_vhip);
            p2Input = new(1, p2_body, p2_vneck, p2_vhip);
            p1Receiver.Bind("/PythonData", p1Input.ReceivedMessage);
            p2Receiver.Bind("/PythonData", p2Input.ReceivedMessage);
            return;
        }

        p1Input = new(0, p1_body, p1_vneck, p1_vhip);
        p2Input = new(1, p2_body, p2_vneck, p2_vhip);

        // Make empty gameobjects
        p1_body = new GameObject("P1-Body");
        p1_vneck = new GameObject("P1-VNeck");

        p1Receiver = gameObject.AddComponent<OSCReceiver>();
        p2Receiver = gameObject.AddComponent<OSCReceiver>();

        // Bind the receivers to the ports
        p1Receiver.LocalPort = 5005;
        p2Receiver.LocalPort = 5015;
        
        DontDestroyOnLoad(p1Receiver.gameObject);
        DontDestroyOnLoad(p2Receiver.gameObject);
        DontDestroyOnLoad(gameObject);

        DontDestroyOnLoad(p1_body);
        DontDestroyOnLoad(p1_vneck);
        DontDestroyOnLoad(p1_vhip);
        DontDestroyOnLoad(p2_body);
        DontDestroyOnLoad(p2_vneck);
        DontDestroyOnLoad(p2_vhip);

        // Bind the received message function
        p1Receiver.Bind("/PythonData", p1Input.ReceivedMessage);
        p2Receiver.Bind("/PythonData", p2Input.ReceivedMessage);

        bInitialised = true;
    }

    private void Start()
    {
    }

    private void Update()
    {
        p1Input.updateBody();
        p2Input.updateBody();
    }

    public AvatarBody GetBody(int iClientID) {
        switch (iClientID)
        {
            case (0):
                return p1Input;
            case (1):
                return p2Input;
            default:
                return p1Input;
        }
    }
}
