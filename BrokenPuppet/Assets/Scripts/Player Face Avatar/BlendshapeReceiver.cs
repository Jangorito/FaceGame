using UnityEngine;
using extOSC;

public class FaceBlendshapeReceiver : MonoBehaviour
{
    public int oscPort = 9000; 
    public string oscAddress = "/FaceBlendshapes"; // The OSC address to listen for blendshape messages
    public SkinnedMeshRenderer faceRenderer; // The SkinnedMeshRenderer component that contains Unity's blendshapes

    private OSCReceiver receiver; // The OSCReceiver component to handle incoming OSC messages

    void Start() // called when the script is being loaded

    { 
        receiver = gameObject.AddComponent<OSCReceiver>(); // Add an OSCReceiver component to the GameObject this script is attached to
        receiver.LocalPort = oscPort; // Set the local port for the OSCReceiver to listen on
        receiver.Bind(oscAddress, OnBlendshapeMessage); // Bind Receiver to the address & set the callback method to handle incoming messages
    }

    void OnBlendshapeMessage(OSCMessage message)
    { // This method parses the incoming OSC message and applies the blendshape values to the SkinnedMeshRenderer
    
        if (message.Values.Count == 0) return;
        string data = message.Values[0].StringValue;
        string[] pairs = data.Split('|');
        foreach (var pair in pairs)
        {
            var parts = pair.Split(',');
            if (parts.Length != 2) continue;
            string blendshapeName = parts[0];
            if (float.TryParse(parts[1], out float value))
            {
                int index = faceRenderer.sharedMesh.GetBlendShapeIndex(blendshapeName);
                if (index >= 0)
                    faceRenderer.SetBlendShapeWeight(index, value);
            }
        }
    }
}