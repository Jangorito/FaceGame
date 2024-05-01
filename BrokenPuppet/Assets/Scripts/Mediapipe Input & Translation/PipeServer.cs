using UnityEngine;
using System;
using System.IO;
using System.Text;
using extOSC;

public class OSCServer : MonoBehaviour
{
    public GameObject Player1;
    public GameObject Player2;

    Logger logger;

    public bool m_bShouldDebug = false;

    // Start is called before the first frame update
    void Start() {
        logger = new(m_bShouldDebug);

        initialise_receiver(5005, Player1);
        initialise_receiver(5015, Player2);
    }

    private void initialise_receiver(int iPort, GameObject Player) {
        var receiver = gameObject.AddComponent<OSCReceiver>();

        receiver.LocalPort = iPort;

        var player = Player.GetComponent<Avatar>();

        if (player != null)
            receiver.Bind("/PythonData", player.getAvatarBody().ReceivedMessage);
        else
            logger.LogMsg("OSCServer::initialise_receiver | Failed to bind on port: " + iPort);
    }


    private void received(OSCMessage message) {
        logger.LogMsg("OSCServer::received | Received OSC Message");
    }
}