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
    public TextMeshProUGUI debuggingBSName; // Assign in Inspector for certain raw blendshapes
    private OSCReceiver receiver; // The OSCReceiver component to handle incoming OSC messages
    private Dictionary<string, float> lastBlendshapes = new Dictionary<string, float>(); // Store the last received value for each blendshape
    private Dictionary<string, float> neutralBlendshapes = new Dictionary<string, float>(); // Store neutral blendshape values for calibration
    // This will be used to determine the neutral position of each blendshape
    private bool isCalibrated = false;
    public bool isDebugMode = false; // Toggle for debug mode, can be set in Inspector
    private Dictionary<string, float> lastRawBlendshapes = new Dictionary<string, float>(); // Store the last received raw blendshape values
    private Dictionary<string, float> lastRawMPBlendshapes = new Dictionary<string, float>(); // Store the last received raw MediaPipe blendshape values
    public SortedList<string, List<string>> mediapipeToAvatarMapping = new SortedList<string, List<string>>(); // Mapping from MediaPipe blendshape names to Unity blendshape names
    public List<string> AvatarBlendshapeToInspect;
    public List<string> BlendshapesToCheck = new List<string>(); // List of Avatar blendshapes to fine tune
    public int blendshapeIndex = 0; // Index of the blendshape to be processed if in debug mode
    public string MPBlendshapeToInspect = ""; // Key for debug mode, used to access the current blendshape in debug mode
    public bool hasPrinted = false;
    void Start() // called when the script is being loaded
    {
        initialiseDictionary(); // Initialize the mapping dictionary
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
            string MpBlendshapeName = parts[0];
            if (float.TryParse(parts[1], out float value))
            {
                lastRawMPBlendshapes[MpBlendshapeName] = value;
            }
        }
        UpdateRawDebugPanel();
        ShowRawCertainBlendshapes(); // Update the certain raw blendshapes debug text TODO: This is not used in the current implementation, but can be useful for debugging
    }

    void OnBlendshapeMessage(OSCMessage message)
    {
        if (message.Values.Count == 0) return;
        string data = message.Values[0].StringValue;
        string[] pairs = data.Split('|');

        bool dMode = isDebugModeEnabled();
        if (dMode)
        {
            AvatarBlendshapeToInspect = mediapipeToAvatarMapping.Values[getCurrentBlendshapeIndex()];
            MPBlendshapeToInspect = mediapipeToAvatarMapping.Keys[getCurrentBlendshapeIndex()];
            string avatarBlendshapesString = getAvatarBlendshapeName(AvatarBlendshapeToInspect);

            if (hasPrinted == false)
            {
                Debug.Log($"We want to isolate '{MPBlendshapeToInspect}' with '{avatarBlendshapesString}' blendshape(s) in debug mode.");
                hasPrinted = true; // Set to true to prevent further debug messages for this blendshape
            }
        }

        // Store both raw & calibrated values
        foreach (var pair in pairs)
        {
            var parts = pair.Split(',');
            if (parts.Length != 2) continue;
            string CurAvatarBlendshapeName = parts[0];
            // CurAvatarBlendshapeName represents the Unity blendshape name we use to render the avatar through faceRenderer
            // many --> 1

            
            if (AvatarBlendshapeToInspect.Contains(CurAvatarBlendshapeName))
            {
                if (!hasPrinted)
                {
                    // Debug.Log(message.ToString());
                    Debug.Log($"Debug Mode: Processing blendshape '{CurAvatarBlendshapeName}' with index {blendshapeIndex}");
                    hasPrinted = false; // Reset hasPrinted to allow new debug messages

                }
            }

            if (float.TryParse(parts[1], out float value))
            {
                // Store raw MediaPipe blendshape values
                lastRawBlendshapes[CurAvatarBlendshapeName] = value;

                // process calibrated values
                if (isCalibrated && neutralBlendshapes.ContainsKey(CurAvatarBlendshapeName))
                    value -= neutralBlendshapes[CurAvatarBlendshapeName];

                value = Mathf.Max(0, value);

                // literally updating unity avatar
                int index = faceRenderer.sharedMesh.GetBlendShapeIndex(CurAvatarBlendshapeName);
                if (index >= 0)
                {
                    if (dMode) // If in debug mode, only update the blendshape specified by MPBlendshapeToInspect
                    {
                        // if (hasPrinted == false)
                        // {
                            // Debug.Log($"In Debug Mode - Only updating blendshape: {MPBlendshapeToInspect} by comparing with {CurAvatarBlendshapeName}");
                            // Debug.Log($"Current blendshape index: {blendshapeIndex}");
                            // hasPrinted = true; // Ensure this only prints once per message
                        // }

                        if (AvatarBlendshapeToInspect.Contains(CurAvatarBlendshapeName))
                        {
                            faceRenderer.SetBlendShapeWeight(index, value);
                            hasPrinted = true; // Set to true to prevent further debug messages for this blendshape
                            // if (hasPrinted) Debug.Log($"Debug Mode: Setting blendshape '{CurAvatarBlendshapeName}' to {value:F1}");
                        }
                    }
                    else // If not in debug mode, update all blendshapes
                    {
                        faceRenderer.SetBlendShapeWeight(index, value);
                    }
                }
                lastBlendshapes[CurAvatarBlendshapeName] = value;
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
            .Where(kv => kv.Key == MPBlendshapeToInspect)
            .Select(kv => $"{kv.Key}: {kv.Value:F1}");
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
    }
    public void CalibrateNeutral()
    { // This method can be called to calibrate the neutral face
        neutralBlendshapes = new Dictionary<string, float>(lastBlendshapes);
        isCalibrated = true;
        Debug.Log("Neutral face calibrated.");
    }
    public void debugMode()
    {
        isDebugMode = !isDebugMode;
        if (isDebugMode)
        {
            Debug.Log("Debug mode enabled.");
            debuggingBSName.text = getBlendshapeName();
        }
        else
        {
            Debug.Log("Debug mode disabled.");
        }
    }
    public void addBlendshape()
    {
        if (BlendshapesToCheck.Contains(getBlendshapeName()))
        {
            Debug.Log($"Blendshape '{getBlendshapeName()}' is already in the list.");
            return; // Exit if the blendshape is already in the list
        }
        Debug.Log($"Adding blendshape '{getBlendshapeName()}' to the list.");
        BlendshapesToCheck.Add(getBlendshapeName());
    }
    public void printBlendshapesToCheck()
    {
        if (BlendshapesToCheck.Count == 0)
        {
            Debug.Log("No blendshapes to check.");
            return;
        }
        Debug.Log($"Blendshapes to check: {getAvatarBlendshapeName(BlendshapesToCheck)}");
    }
// Blendshapes to check: browDownLeft, browDownRight, cheekPuff, cheekSquintLeft, cheekSquintRight, eyeBlinkLeft, eyeBlinkRight, eyeSquintLeft, eyeSquintRight, eyeWideLeft, eyeWideRight, mouthClose, mouthDimpleLeft, mouthDimpleRight, mouthFrownLeft, mouthFrownRight, mouthPucker, mouthRollLower, mouthRollUpper, mouthShrugLower, mouthShrugUpper, mouthStretchLeft, mouthUpperUpLeft, mouthUpperUpRight, noseSneerLeft, noseSneerRight,
    public bool isDebugModeEnabled()
    {
        return isDebugMode;
    }
    public void printRawMPs()
    {
        var output = lastRawMPBlendshapes
            .OrderByDescending(kv => Mathf.Abs(kv.Value))
            .Select(kv => $"{kv.Key}: {kv.Value * 1000:F1}");
        Debug.Log(string.Join("\n", output));
    }
    public void resetPreviousBlendshapes()
    {
        foreach (var blendshape in lastBlendshapes)
        {
            // Reset each blendshape to 0
            int index = faceRenderer.sharedMesh.GetBlendShapeIndex(blendshape.Key);
            if (index >= 0)
            {
                faceRenderer.SetBlendShapeWeight(index, 0f);
            }
        }
    }
    public void incrementBlendshapeIndex()
    {
        resetPreviousBlendshapes(); // Reset all blendshapes to 0 before incrementing
        blendshapeIndex++;
        if (blendshapeIndex >= 48) // Assuming there are 49 blendshapes (0-48)
        {
            blendshapeIndex = 0; // Reset to 0 if it exceeds the count
        }
        Debug.Log($"Current blendshape index: {blendshapeIndex}");
        debuggingBSName.text = getBlendshapeName(); // Update the debug text with the current blendshape name
        hasPrinted = false; // Reset hasPrinted to allow new debug messages
    }
    public void decrementBlendshapeIndex()
    {
        resetPreviousBlendshapes(); // Reset all blendshapes to 0 before decrementing
        blendshapeIndex--;
        if (blendshapeIndex < 0)
        {
            blendshapeIndex = 48; // Wrap around to the last index
        }
        Debug.Log($"Current blendshape index: {blendshapeIndex}");
        debuggingBSName.text = getBlendshapeName(); // Update the debug text with the current blendshape name
        hasPrinted = false; // Reset hasPrinted to allow new debug messages
    }
    public int getCurrentBlendshapeIndex()
    {
        return blendshapeIndex;
    }
    public string getAvatarBlendshapeName(List<string> AvatarBlendshapeToInspect)
    {    
        return string.Join(", ", AvatarBlendshapeToInspect); // Joins with ", "
    }
    public void initialiseDictionary()
    {
        mediapipeToAvatarMapping = new SortedList<string, List<string>>
        {
            // Brows
            { "browInnerUp", new List<string> { "Brow_Raise_Inner_L", "Brow_Raise_Inner_R" } },
            { "browDownLeft", new List<string> { "Brow_Drop_L" } },
            { "browDownRight", new List<string> { "Brow_Drop_R" } },
            { "browOuterUpLeft", new List<string> { "Brow_Raise_Outer_L" } },
            { "browOuterUpRight", new List<string> { "Brow_Raise_Outer_R" } },

            // Eyes
            { "eyeBlinkLeft", new List<string> { "Eye_Blink_L" } },
            { "eyeBlinkRight", new List<string> { "Eye_Blink_R" } },
            { "eyeSquintLeft", new List<string> {"Eye_Squint_L"} },
            { "eyeSquintRight", new List<string> {"Eye_Squint_R"} },
            { "eyeWideLeft", new List<string> {"Eye_Wide_L"} },
            { "eyeWideRight", new List<string> {"Eye_Wide_R"} },
            { "eyeLookOutLeft", new List<string> {"Eye_L_Look_L"} },
            { "eyeLookInLeft", new List<string> {"Eye_L_Look_R"} },
            { "eyeLookOutRight", new List<string> {"Eye_R_Look_R"} },
            { "eyeLookInRight", new List<string> {"Eye_R_Look_L"} },
            { "eyeLookUpLeft", new List<string> {"Eye_L_Look_Up"} },
            { "eyeLookUpRight", new List<string> {"Eye_R_Look_Up"} },
            { "eyeLookDownLeft", new List<string> {"Eye_L_Look_Down"} },
            { "eyeLookDownRight", new List<string> {"Eye_R_Look_Down"} },

            // Cheeks
            { "cheekPuff", new List<string> {"Cheek_Puff_L", "Cheek_Puff_R"} },
            { "cheekSquintLeft", new List<string> {"Cheek_Raise_L"} },
            { "cheekSquintRight", new List<string> {"Cheek_Raise_R"} },

            // Nose
            { "noseSneerLeft", new List<string> {"Nose_Sneer_L", "Nose_Nostril_Raise_L"} },
            { "noseSneerRight", new List<string> {"Nose_Sneer_R", "Nose_Nostril_Raise_R"} },

            // Jaw
            { "jawOpen", new List<string> {"Jaw_Open"} },
            { "jawForward", new List<string> {"Jaw_Forward"} },
            { "jawLeft", new List<string> {"Jaw_L"} },
            { "jawRight", new List<string> {"Jaw_R"} },

            // Mouth
            { "mouthSmileLeft", new List<string> {"Mouth_Smile_L"} },
            { "mouthSmileRight", new List<string> {"Mouth_Smile_R"} },
            { "mouthFrownLeft", new List<string> {"Mouth_Frown_L"} },
            { "mouthFrownRight", new List<string> {"Mouth_Frown_R"} },
            { "mouthDimpleLeft", new List<string> {"Mouth_Dimple_L"} },
            { "mouthDimpleRight", new List<string> {"Mouth_Dimple_R"} },
            { "mouthStretchLeft", new List<string> {"Mouth_Stretch_L"} },
            { "mouthStretchRight", new List<string> {"Mouth_Stretch_R"} },
            { "mouthPucker", new List<string> {"Mouth_Pucker_Up_L", "Mouth_Pucker_Up_R", "Mouth_Pucker_Down_L", "Mouth_Pucker_Down_R"} },
            { "mouthFunnel", new List<string> {"Mouth_Funnel_Up_L", "Mouth_Funnel_Up_R", "Mouth_Funnel_Down_L", "Mouth_Funnel_Down_R"} },
            { "mouthRollUpper", new List<string> {"Mouth_Roll_In_Upper_L", "Mouth_Roll_In_Upper_R"} },
            { "mouthRollLower", new List<string> {"Mouth_Roll_In_Lower_L", "Mouth_Roll_In_Lower_R"} },
            { "mouthShrugUpper", new List<string> {"Mouth_Shrug_Upper"} },
            { "mouthShrugLower", new List<string> {"Mouth_Shrug_Lower"} },
            { "mouthClose", new List<string> {"Mouth_Close"} },
            { "mouthUpperUpLeft", new List<string> {"Mouth_Up_Upper_L"} },
            { "mouthUpperUpRight", new List<string> {"Mouth_Up_Upper_R"} },
            { "mouthLowerDownLeft", new List<string> {"Mouth_Down_Lower_L"} },
            { "mouthLowerDownRight", new List<string> {"Mouth_Down_Lower_R"} },
            { "mouthPressLeft", new List<string> {"Mouth_Press_L"} },
            { "mouthPressRight", new List<string> {"Mouth_Press_R"} },
        };
    }
    public string getBlendshapeName()
    {
        return mediapipeToAvatarMapping.Keys[blendshapeIndex].ToString();
    }
    
}
