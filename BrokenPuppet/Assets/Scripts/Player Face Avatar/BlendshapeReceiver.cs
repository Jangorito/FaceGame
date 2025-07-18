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
    private Dictionary<string, float> neutralMPValues = new Dictionary<string, float>(); // Store neutral blendshape values for calibration
                                                                                         // This will be used to determine the neutral position of each blendshape
    private Dictionary<string, float> minMPValues = new Dictionary<string, float>(); // Store neutral blendshape values for calibration
    private Dictionary<string, float> maxMPValues = new Dictionary<string, float>(); // Store neutral blendshape values for calibration
    private Dictionary<string, float> _toggledAvatarExtremeStates = new Dictionary<string, float>(); // Stores 0f or 100f for toggled extreme states of avatar blendshapes
    private bool isMinMaxCalibrated = false; // Flag to check if min/max calibration has been done
    private bool isNeutralCalibrated = false;
    public bool isDebugMode = false; // Toggle for debug mode, can be set in Inspector
    private Dictionary<string, float> lastLongFormRawBlendshapes = new Dictionary<string, float>(); // Store the last received raw blendshape values
    private Dictionary<string, float> lastRawMPBlendshapes = new Dictionary<string, float>(); // Store the last received raw MediaPipe blendshape values
    public SortedList<string, List<string>> mediapipeToAvatarMapping = new SortedList<string, List<string>>(); // Mapping from MediaPipe blendshape names to Unity blendshape names
    public List<string> blendshapesToDisplayInDebug = new List<string>
    {
        "cheekPuff",
        "mouthClose",
        "cheekSquintLeft",
        "cheekSquintRight",
        "noseSneerLeft",
        "noseSneerRight",
        
    };
    public List<string> dModeAvatarBlendshapesToInspect;
    public List<string> BlendshapesToCheck = new List<string>(); // List of Avatar blendshapes to fine tune
    public int blendshapeIndex = 0; // Index of the blendshape to be processed if in debug mode
    public string dModeMPKeyToInspect = ""; // Key for debug mode, used to access the current blendshape in debug mode
    public bool hasPrinted = false;
    private bool isManualOverrideActive = false;
    void Start() // called when the script is being loaded
    {
        initialiseDictionary(); // Initialize the mapping dictionary
        receiver = gameObject.AddComponent<OSCReceiver>(); // Add an OSCReceiver component to the GameObject this script is attached to
        receiver.LocalPort = oscPort; // Set the local port for the OSCReceiver to listen on
        receiver.Bind("/10sfBlendshapes", OnBlendshapeMessage); // Bind Receiver to the address & set the callback method to handle incoming messages

        // New receiver for raw MediaPipe blendshapes
        receiver.Bind("/FaceBlendshapesRaw", OnRawMPBlendshapeMessage);
    }

    void OnRawMPBlendshapeMessage(OSCMessage message)
    {
        // Debug.Log("Received OSC message on /FaceBlendshapesRaw");
        if (message.Values.Count == 0) return;
        string data = message.Values[0].StringValue;
        string[] pairs = data.Split('|');

        bool dMode = isDebugModeEnabled();
        // If dMode is true, calculate the target MP and Avatar blendshapes once here
        if (dMode)
        {
            dModeMPKeyToInspect = mediapipeToAvatarMapping.Keys[getCurrentBlendshapeIndex()];
            // MediaPipe blendshape to inspect

            if (mediapipeToAvatarMapping.TryGetValue(dModeMPKeyToInspect, out List<string> avatarNames))
            {
                dModeAvatarBlendshapesToInspect = avatarNames;
                // Avatar blendshapes to inspect
            }

            string avatarBlendshapesString = getAvatarBlendshapeName(dModeAvatarBlendshapesToInspect);
            // Join the avatar blendshapes into a string for logging

            if (hasPrinted == false) // This flag needs careful management
            {
                Debug.Log($"Debug Mode: Isolating MP '{dModeMPKeyToInspect}' mapped to avatar '{avatarBlendshapesString}'.");
                hasPrinted = true; // Set to true after initial print
            }
        }

        foreach (var pair in pairs)
        {
            var parts = pair.Split(',');
            if (parts.Length != 2) continue;

            string currentMPBlendshapeName = parts[0];
            // the MediaPipe blendshape name

            if (!float.TryParse(parts[1], out float rawMPValue)) continue;
            // Try to parse the value, if it fails, skip this pair

            lastRawMPBlendshapes[currentMPBlendshapeName] = rawMPValue;
            // Store the raw MediaPipe blendshape value, both for debbugging and calibration


            // --- Calibration logic for raw MediaPipe blendshapes ---

            // float calibratedValue = rawMPValue;
            // // Start with the raw value

            // // Firstly, neutral calibration check
            // if (isNeutralCalibrated && neutralMPValues.ContainsKey(currentMPBlendshapeName))
            // {
            //     calibratedValue -= neutralMPValues[currentMPBlendshapeName];
            // }

            // // then, check for min/max calibration...TODO: This is not implemented yet as we still need to finalse methods for min/max calibration, namely, the many to one mapping

            // if (isMinMaxCalibrated && minMPValues.ContainsKey(currentMPBlendshapeName) && maxMPValues.ContainsKey(currentMPBlendshapeName))
            // {
            //     float min = minMPValues[currentMPBlendshapeName];
            //     float max = maxMPValues[currentMPBlendshapeName];

            //     if (Mathf.Approximately(min, max))
            //     {
            //         calibratedValue = (min > 0) ? 100 : 0; // Avoid division by zero, snap to extreme
            //     }
            //     else
            //     {
            //         // Remap from [min, max] to [0, 100]
            //         calibratedValue = Mathf.InverseLerp(min, max, calibratedValue) * 100f;
            //     }
            // }
            // else
            // {
            //     // If no min/max calibration, simply scale to 0-100 (assuming MediaPipe is 0-1)
            //     calibratedValue *= 100f;
            // }

            // // The final clamping to ensure the value is within 0-100
            // calibratedValue = Mathf.Clamp(calibratedValue, 0f, 100f);
            float calibratedValue = CalibrateMPValueToAvatarWeight(currentMPBlendshapeName, rawMPValue);


            // --- Apply the calibrated value to the Unity blendshape ---
            if (!isManualOverrideActive)
            {
                if (mediapipeToAvatarMapping.TryGetValue(currentMPBlendshapeName, out List<string> avatarBlendshapeNames))
                {
                    foreach (string avatarName in avatarBlendshapeNames)
                    {
                        int index = faceRenderer.sharedMesh.GetBlendShapeIndex(avatarName);
                        if (index >= 0)
                        {
                            if (dMode) // If in debug mode, only update the isolated blendshape(s)
                            {
                                if (dModeAvatarBlendshapesToInspect.Contains(avatarName))
                                {
                                    faceRenderer.SetBlendShapeWeight(index, calibratedValue);
                                    // Debug.Log($"DMode: Set {avatarName} to {calibratedValue:F2}");
                                }
                            }
                            else // Not in debug mode, update all
                            {
                                faceRenderer.SetBlendShapeWeight(index, calibratedValue);
                            }
                        }
                        // Always store the *calibrated* value for the *avatar* blendshape
                        lastBlendshapes[avatarName] = calibratedValue;
                    }
                }
            }
        }
        UpdateDebugPanel();
        UpdateRawDebugPanel();
        ShowRawCertainBlendshapes(); // Update the certain raw blendshapes debug text TODO: This is not used in the current implementation, but can be useful for debugging
        // ShowCertainBlendshapes(); // Update the certain blendshapes debug text
    }

    void OnBlendshapeMessage(OSCMessage message)
    {
        Debug.Log("Received OSC message on /10sfBlendshapes");
        if (message.Values.Count == 0) return;
        string data = message.Values[0].StringValue;
        string[] pairs = data.Split('|');

        foreach (var pair in pairs)
        {
            var parts = pair.Split(',');
            if (parts.Length != 2) continue;
            string LongFormRawMPs = parts[0];
            
            if (!float.TryParse(parts[1], out float LongFormrawMPValue)) continue;

            lastLongFormRawBlendshapes[LongFormRawMPs] = LongFormrawMPValue;
        }
        // ShowCertainBlendshapes(); // Update the certain blendshapes debug text
    }

    //     bool dMode = isDebugModeEnabled();
    //     if (dMode)
    //     {
    //         dModeAvatarBlendshapesToInspect = mediapipeToAvatarMapping.Values[getCurrentBlendshapeIndex()];
    //         dModeMPKeyToInspect = mediapipeToAvatarMapping.Keys[getCurrentBlendshapeIndex()];
    //         string avatarBlendshapesString = getAvatarBlendshapeName(dModeAvatarBlendshapesToInspect);

        //         if (hasPrinted == false)
        //         {
        //             Debug.Log($"We want to isolate '{dModeMPKeyToInspect}' with '{avatarBlendshapesString}' blendshape(s) in debug mode.");
        //             hasPrinted = true; // Set to true to prevent further debug messages for this blendshape
        //         }
        //     }

        //     // Store both raw & calibrated values
        //     foreach (var pair in pairs)
        //     {
        //         var parts = pair.Split(',');
        //         if (parts.Length != 2) continue;
        //         string CurAvatarBlendshapeName = parts[0];
        //         // CurAvatarBlendshapeName represents the Unity blendshape name we use to render the avatar through faceRenderer
        //         // many --> 1


        //         if (dModeAvatarBlendshapesToInspect.Contains(CurAvatarBlendshapeName))
        //         {
        //             if (!hasPrinted)
        //             {
        //                 // Debug.Log(message.ToString());
        //                 Debug.Log($"Debug Mode: Processing blendshape '{CurAvatarBlendshapeName}' with index {blendshapeIndex}");
        //                 hasPrinted = false; // Reset hasPrinted to allow new debug messages

        //             }
        //         }

        //         if (float.TryParse(parts[1], out float value))
        //         {
        //             // Store raw MediaPipe blendshape values
        //             lastRawBlendshapes[CurAvatarBlendshapeName] = value;

        //             // process calibrated values
        //             if (isNeutralCalibrated && neutralMPValues.ContainsKey(CurAvatarBlendshapeName))
        //                 value -= neutralMPValues[CurAvatarBlendshapeName];

        //             value = Mathf.Max(0, value);

        //             // literally updating unity avatar
        //             int index = faceRenderer.sharedMesh.GetBlendShapeIndex(CurAvatarBlendshapeName);
        //             if (index >= 0)
        //             {
        //                 if (dMode) // If in debug mode, only update the blendshape specified by dModeMPKeyToInspect
        //                 {
        //                     // if (hasPrinted == false)
        //                     // {
        //                     // Debug.Log($"In Debug Mode - Only updating blendshape: {dModeMPKeyToInspect} by comparing with {CurAvatarBlendshapeName}");
        //                     // Debug.Log($"Current blendshape index: {blendshapeIndex}");
        //                     // hasPrinted = true; // Ensure this only prints once per message
        //                     // }

        //                     if (dModeAvatarBlendshapesToInspect.Contains(CurAvatarBlendshapeName))
        //                     {
        //                         faceRenderer.SetBlendShapeWeight(index, value);
        //                         hasPrinted = true; // Set to true to prevent further debug messages for this blendshape
        //                         // if (hasPrinted) Debug.Log($"Debug Mode: Setting blendshape '{CurAvatarBlendshapeName}' to {value:F1}");
        //                     }
        //                 }
        //                 else // If not in debug mode, update all blendshapes
        //                 {
        //                     faceRenderer.SetBlendShapeWeight(index, value);
        //                 }
        //             }
        //             lastBlendshapes[CurAvatarBlendshapeName] = value;
        //         }
        //     }
        //     UpdateDebugPanel();
        //     ShowCertainBlendshapes(); // Update the certain blendshapes debug text
        // }

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
        var active = lastLongFormRawBlendshapes
            .Where(kv => kv.Key == dModeMPKeyToInspect)
            .Select(kv => $"{kv.Key}: {kv.Value:F1}");
        string output = string.Join("\n", active);
        if (string.IsNullOrEmpty(output))
            output = "can't find anything?";
        certainRawBlendshapesDebugText.text = output;
    }
    // private void ShowCertainBlendshapes()
    // {
    //     // This method will display only certain blendshapes in a debug panel
    //     // if (certainBlendshapesDebugText == null)
    //     // {
    //     //     certainBlendshapesDebugText.text = "can't find anything?"; // Ensure this is assigned in the Inspector
    //     // }
    //     var active = lastLongRawMPBlendshapes
    //         // .Where(kv => kv.Key == "Nose_Sneer_L" || kv.Key == "Nose_Nostril_Raise_L" || kv.Key == "Nose_Sneer_R" || kv.Key == "Nose_Nostril_Raise_R" )
    //         .Where(kv => blendshapesToDisplayInDebug.Contains(kv.Key))
    //         .Select(kv => $"{kv.Key}: {kv.Value:F1}");
    //     certainBlendshapesDebugText.text = string.Join("\n", active);
    // }
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
        neutralMPValues = new Dictionary<string, float>(lastRawMPBlendshapes);
        isNeutralCalibrated = true;
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
            isMinMaxCalibrated = true;
            // assumes that once debug mode is disabled, min/max calibration is done
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
    public string getAvatarBlendshapeName(List<string> dModeAvatarBlendshapesToInspect)
    {
        return string.Join(", ", dModeAvatarBlendshapesToInspect); // Joins with ", "
    }
    public void getDictBlendshapeName()
    {
        Debug.Log("Blendshape names in the dictionary:");
        Debug.Log(string.Join(", ", neutralMPValues));
        Debug.Log("Last blendshapes received:");
        Debug.Log(string.Join(", ", lastBlendshapes)); // Returns the blendshape name as is
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
    // Helper method for forward calibration (MediaPipe raw value to Unity avatar blendshape weight)
    private float CalibrateMPValueToAvatarWeight(string mpBlendshapeName, float rawMPValue)
    {
        float processedValue = rawMPValue;

        // Apply Neutral Calibration (offset)
        if (isNeutralCalibrated && neutralMPValues.ContainsKey(mpBlendshapeName))
        {
            processedValue -= neutralMPValues[mpBlendshapeName];
        }

        // Apply Min/Max Remapping (scaling)
        if (isMinMaxCalibrated && minMPValues.ContainsKey(mpBlendshapeName) && maxMPValues.ContainsKey(mpBlendshapeName))
        {
            float min = minMPValues[mpBlendshapeName];
            float max = maxMPValues[mpBlendshapeName];

            // Avoid division by zero if min and max are the same
            if (Mathf.Approximately(min, max))
            {
                // If the range is zero, assume it's either fully on (if min > 0) or fully off (if min <= 0)
                processedValue = (min > 0) ? 100f : 0f;
            }
            else
            {
                // Remap from [min, max] to [0, 100]
                processedValue = Mathf.InverseLerp(min, max, processedValue) * 100f;
            }
        }
        else
        {
            // If no min/max calibration, simply scale to 0-100 (assuming raw MediaPipe values are typically 0-1)
            processedValue *= 100f;
        }

        // Final Clamping to ensure it's within Unity's blendshape range (0-100)
        return Mathf.Clamp(processedValue, 0f, 100f);
    }

    // Helper method for INVERSE calibration (Desired Avatar Weight to Required MediaPipe Raw Value)
    // This calculates what raw MediaPipe input would theoretically produce a given avatar blendshape weight
    private float InverseCalibrateAvatarWeightToMPValue(string mpBlendshapeName, float desiredAvatarWeight)
    {
        // Clamp desiredAvatarWeight to the Unity range for safety
        desiredAvatarWeight = Mathf.Clamp(desiredAvatarWeight, 0f, 100f);

        float requiredMPValue = desiredAvatarWeight; // Start with the desired output value

        // 1. Inverse Remap from [0, 100] (Avatar Output) to [min, max] (MediaPipe's Range)
        if (isMinMaxCalibrated && minMPValues.ContainsKey(mpBlendshapeName) && maxMPValues.ContainsKey(mpBlendshapeName))
        {
            float min = minMPValues[mpBlendshapeName];
            float max = maxMPValues[mpBlendshapeName];

            if (Mathf.Approximately(min, max))
            {
                // If min/max are the same, we can't reliably inverse map a range.
                // Best guess: if desired is 100, return max; if desired is 0, return min.
                requiredMPValue = (desiredAvatarWeight >= 50f) ? max : min;
            }
            else
            {
                // InverseLerp's mathematical inverse is Lerp: Lerp(a, b, t) where t is (desiredValue / 100)
                requiredMPValue = Mathf.Lerp(min, max, desiredAvatarWeight / 100f);
            }
        }
        else
        {
            // If no min/max calibration was used, just inverse scale from 0-100 to 0-1
            requiredMPValue /= 100f;
        }

        // 2. Inverse Apply Neutral Calibration (add the offset back)
        if (isNeutralCalibrated && neutralMPValues.ContainsKey(mpBlendshapeName))
        {
            requiredMPValue += neutralMPValues[mpBlendshapeName];
        }

        // Optional: Clamp the final raw MP value to MediaPipe's typical input range (e.g., 0-1)
        return Mathf.Clamp01(requiredMPValue); // MediaPipe values are typically 0 to 1
    }
    public void RecordMin()
    {
        string mpBlendshapeKey = mediapipeToAvatarMapping.Keys[getCurrentBlendshapeIndex()];
        if (lastRawMPBlendshapes.TryGetValue(mpBlendshapeKey, out float currentValue))
        {
            minMPValues[mpBlendshapeKey] = currentValue; // Add or update
            Debug.Log($"Recorded MIN value for MP blendshape '{mpBlendshapeKey}': {currentValue}");
        }
        else
        {
            Debug.LogWarning($"Cannot record MIN: Raw MediaPipe blendshape '{mpBlendshapeKey}' not found.");
        }
    }

    public void RecordMax()
    {
        string mpBlendshapeKey = mediapipeToAvatarMapping.Keys[getCurrentBlendshapeIndex()];
        if (lastRawMPBlendshapes.TryGetValue(mpBlendshapeKey, out float currentValue))
        {
            maxMPValues[mpBlendshapeKey] = currentValue; // Add or update
            Debug.Log($"Recorded MAX value for MP blendshape '{mpBlendshapeKey}': {currentValue}");
        }
        else
        {
            Debug.LogWarning($"Cannot record MAX: Raw MediaPipe blendshape '{mpBlendshapeKey}' not found.");
        }
    }

    public void ToggleBlendshapeExtremeVisually()
    {
        isManualOverrideActive = true;
        // Get the MediaPipe blendshape key that is currently selected for inspection
        // This assumes getCurrentBlendshapeIndex() correctly identifies the MP blendshape to work with.
        string mpBlendshapeKey = mediapipeToAvatarMapping.Keys[getCurrentBlendshapeIndex()];

        // Get the current target visual extreme for this blendshape, default to 0 (off)
        _toggledAvatarExtremeStates.TryGetValue(mpBlendshapeKey, out float currentTargetExtreme);

        // Determine the new target extreme: toggle between 0% and 100%
        float newTargetExtreme = (currentTargetExtreme >= 50f) ? 0f : 100f;

        // Store the new state
        _toggledAvatarExtremeStates[mpBlendshapeKey] = newTargetExtreme;

        // Apply the visual extreme to the avatar for all corresponding Unity blendshapes
        if (mediapipeToAvatarMapping.TryGetValue(mpBlendshapeKey, out List<string> avatarBlendshapeNames))
        {
            foreach (string avatarName in avatarBlendshapeNames)
            {
                int index = faceRenderer.sharedMesh.GetBlendShapeIndex(avatarName);
                if (index >= 0)
                {
                    faceRenderer.SetBlendShapeWeight(index, newTargetExtreme);
                }
                else
                {
                    Debug.LogWarning($"Avatar blendshape '{avatarName}' not found on the mesh renderer.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"MediaPipe blendshape '{mpBlendshapeKey}' not found in mapping dictionary.");
            return; // Exit if the MP key isn't in the mapping
        }


        // Debug display what raw MediaPipe value would theoretically produce this extreme
        float requiredMPValue = InverseCalibrateAvatarWeightToMPValue(mpBlendshapeKey, newTargetExtreme);

        Debug.Log($"Toggled '{mpBlendshapeKey}' to Avatar Visual Extreme: {newTargetExtreme}%. " +
                $"Estimated Raw MediaPipe value needed: {requiredMPValue:F4}");

        // Optional: You might want to update your debug UI panels here if they display these values
        // UpdateDebugPanel(); // If your debug panel shows current blendshape weights
    }
    
    public void DeactivateManualOverride()
    {
        if (!isManualOverrideActive)
        {
            Debug.Log("Manual override is already inactive. No action needed.");
            return;
        }

        isManualOverrideActive = false; // Deactivate manual override
        _toggledAvatarExtremeStates.Clear(); // Clear any stored manual extreme states

        Debug.Log("Manual override deactivated. Returning control to live OSC input.");

        // Optional: Immediately apply the last known live OSC values to the avatar
        // This ensures a smooth transition back without a "jump" to a default pose.
        foreach (var entry in lastBlendshapes) // lastBlendshapes holds the last CALIBRATED values
        {
            string avatarName = entry.Key;
            float value = entry.Value;
            int index = faceRenderer.sharedMesh.GetBlendShapeIndex(avatarName);
            if (index >= 0)
            {
                faceRenderer.SetBlendShapeWeight(index, value);
            }
        }

        // IMPORTANT: Reset hasPrinted if it's used for one-time debug logs in dMode
        // This allows the debug mode log to appear again if you re-enable dMode and select a blendshape.
        hasPrinted = false; // Or manage this flag more specifically based on your needs

        // Ensure debug panel updates to reflect live values
        UpdateDebugPanel();
        UpdateRawDebugPanel();
        // ShowCertainBlendshapes();
    }
}
