using UnityEngine;
using extOSC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class FaceBlendshapeReceiver : MonoBehaviour
{
    public int oscPort = 9000;
    public string oscAddress = "/FaceBlendshapes"; // The OSC address to listen for blendshape messages
    public SkinnedMeshRenderer faceRenderer; // The SkinnedMeshRenderer component that contains Unity's blendshapes
    public TextMeshProUGUI debugText; // Assign in Inspector
    public TextMeshProUGUI rawDebugText; // Assign in Inspector for raw values
    public TextMeshProUGUI certainBlendshapesDebugText; // Assign in Inspector for certain blendshapes
    public TextMeshProUGUI certainRawBlendshapesDebugText;
    private OSCReceiver receiver; // The OSCReceiver component to handle incoming OSC messages
    private Dictionary<string, float> lastBlendshapes = new Dictionary<string, float>(); // Store the last received value for each blendshape
    private Dictionary<string, float> neutralBlendshapes = new Dictionary<string, float>(); // Store neutral blendshape values for calibration
    // This will be used to determine the neutral position of each blendshape
    private bool isCalibrated = false;
    private Dictionary<string, float> lastRawBlendshapes = new Dictionary<string, float>();
    private Dictionary<string, float> lastRawMPBlendshapes = new Dictionary<string, float>();

    void Start() // called when the script is being loaded

    {
        receiver = gameObject.AddComponent<OSCReceiver>(); // Add an OSCReceiver component to the GameObject this script is attached to
        receiver.LocalPort = oscPort; // Set the local port for the OSCReceiver to listen on
        receiver.Bind(oscAddress, OnBlendshapeMessage); // Bind Receiver to the address & set the callback method to handle incoming messages

        // New receiver for raw MediaPipe blendshapes
        receiver.Bind("/FaceBlendshapesRaw", OnRawMPBlendshapeMessage);
    }

    void OnRawMPBlendshapeMessage(OSCMessage message)
    {
        // Debug.Log("Received OSC message on /FaceBlendshapesRaw");
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
                lastRawMPBlendshapes[blendshapeName] = value;
                // Debug.Log(lastRawMPBlendshapes[blendshapeName]);

            }
        }
        UpdateRawDebugPanel();
        ShowRawCertainBlendshapes(); // Update the certain raw blendshapes debug text
    }

    void OnBlendshapeMessage(OSCMessage message)
    {
        if (message.Values.Count == 0) return;
        string data = message.Values[0].StringValue;
        string[] pairs = data.Split('|');

        // Store raw values
        foreach (var pair in pairs)
        {
            var parts = pair.Split(',');
            if (parts.Length != 2) continue;
            string blendshapeName = parts[0];
            if (float.TryParse(parts[1], out float value))
            {
                lastRawBlendshapes[blendshapeName] = value;
            }
        }

        // Now process calibrated values as before
        foreach (var pair in pairs)
        {
            var parts = pair.Split(',');
            if (parts.Length != 2) continue;
            string blendshapeName = parts[0];
            if (float.TryParse(parts[1], out float value))
            {
                if (isCalibrated && neutralBlendshapes.ContainsKey(blendshapeName))
                    value -= neutralBlendshapes[blendshapeName];

                value = Mathf.Max(0, value);

                int index = faceRenderer.sharedMesh.GetBlendShapeIndex(blendshapeName);
                if (index >= 0)
                    faceRenderer.SetBlendShapeWeight(index, value);

                lastBlendshapes[blendshapeName] = value;
            }
        }
        UpdateDebugPanel();
        ShowCertainBlendshapes(); // Update the certain blendshapes debug text
    }

    private void UpdateRawDebugPanel()
    {
        if (rawDebugText == null) return;

        var active = lastRawMPBlendshapes
            // .Where(kv => Mathf.Abs(kv.Value) > 1f)
            .OrderByDescending(kv => Mathf.Abs(kv.Value))
            .Take(8)
            .Select(kv => $"{kv.Key}: {kv.Value * 100f:F1}");
        rawDebugText.text = string.Join("\n", active);
    }

    private void ShowRawCertainBlendshapes()
    {
        if (certainRawBlendshapesDebugText == null) return;
        var active = lastRawMPBlendshapes
            .Where(kv => kv.Key == "browDownLeft" || kv.Key == "browDownRight")
            .Select(kv => $"{kv.Key}: {kv.Value * 100f:F1}");
        string output = string.Join("\n", active);
        if (string.IsNullOrEmpty(output))
            output = "can't find anything?";
        certainRawBlendshapesDebugText.text = output;
    }

    private void ShowCertainBlendshapes()
    {
        // This method will display only certain blendshapes in a debug panel
        if (certainBlendshapesDebugText == null)
        {
            certainBlendshapesDebugText.text = "can't find anything?"; // Ensure this is assigned in the Inspector
        }
        var active = lastRawMPBlendshapes
            .Where(kv => kv.Key == "Brow_Drop_L" || kv.Key == "Brow_Drop_R")
            .Select(kv => $"{kv.Key}: {kv.Value:F1}");
        certainBlendshapesDebugText.text = string.Join("\n", active);
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

        // Update raw debug text
        // if (rawDebugText != null)
        // {
        // var allBlendshapes = lastBlendshapes
        // .OrderByDescending(kv => Mathf.Abs(kv.Value))
        // .Select(kv => $"{kv.Key}: {kv.Value:F1}");
        // rawDebugText.text = string.Join("\n", allBlendshapes);
        // }
    }

    public void CalibrateNeutral()
    { // This method can be called to calibrate the neutral face
        neutralBlendshapes = new Dictionary<string, float>(lastBlendshapes);
        isCalibrated = true;
        Debug.Log("Neutral face calibrated.");
    }

    public void printRawMPs()
    {
        var output = lastRawMPBlendshapes
            .OrderByDescending(kv => Mathf.Abs(kv.Value))
            .Select(kv => $"{kv.Key}: {kv.Value * 1000 :F1}");
        Debug.Log(string.Join("\n", output));
    }
}
