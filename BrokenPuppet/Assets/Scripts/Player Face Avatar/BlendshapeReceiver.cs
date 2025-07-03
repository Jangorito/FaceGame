using UnityEngine;
using extOSC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro; // Add this if not already present

public class FaceBlendshapeReceiver : MonoBehaviour
{
    public int oscPort = 9000; 
    public string oscAddress = "/FaceBlendshapes"; // The OSC address to listen for blendshape messages
    public SkinnedMeshRenderer faceRenderer; // The SkinnedMeshRenderer component that contains Unity's blendshapes
    public TextMeshProUGUI debugText; // Assign in Inspector

    private OSCReceiver receiver; // The OSCReceiver component to handle incoming OSC messages
    private Dictionary<string, float> lastBlendshapes = new Dictionary<string, float>(); // Store the last received value for each blendshape
    private Dictionary<string, float> neutralBlendshapes = new Dictionary<string, float>(); // Store neutral blendshape values for calibration
    // This will be used to determine the neutral position of each blendshape
    private bool isCalibrated = false;

    void Start() // called when the script is being loaded

    { 
        receiver = gameObject.AddComponent<OSCReceiver>(); // Add an OSCReceiver component to the GameObject this script is attached to
        receiver.LocalPort = oscPort; // Set the local port for the OSCReceiver to listen on
        receiver.Bind(oscAddress, OnBlendshapeMessage); // Bind Receiver to the address & set the callback method to handle incoming messages
    }

    void OnBlendshapeMessage(OSCMessage message)
    {
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
                // Subtract neutral value if calibrated
                if (isCalibrated && neutralBlendshapes.ContainsKey(blendshapeName))
                    value -= neutralBlendshapes[blendshapeName];

                value = Mathf.Max(0, value); // Clamp to zero (optional)

                int index = faceRenderer.sharedMesh.GetBlendShapeIndex(blendshapeName);
                if (index >= 0)
                    faceRenderer.SetBlendShapeWeight(index, value);

                // Store for debug panel
                lastBlendshapes[blendshapeName] = value;
            }
        }
        UpdateDebugPanel();
    }

    private void UpdateDebugPanel()
    {
        if (debugText == null) return;
        // Show only blendshapes with value > 1, sorted by value descending, top 8
        var active = lastBlendshapes
            .Where(kv => Mathf.Abs(kv.Value) > 1f)
            .OrderByDescending(kv => Mathf.Abs(kv.Value))
            .Take(8)
            .Select(kv => $"{kv.Key}: {kv.Value:F1}");
        debugText.text = string.Join("\n", active);
    }

    public void CalibrateNeutral()
    { // This method can be called to calibrate the neutral face
        neutralBlendshapes = new Dictionary<string, float>(lastBlendshapes);
        isCalibrated = true;
        Debug.Log("Neutral face calibrated.");
    }
}