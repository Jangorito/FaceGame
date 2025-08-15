using UnityEngine;
using extOSC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.Rendering;

public class FaceBlendshapeReceiver : MonoBehaviour
{
    public int oscPort = 9000;
    public string oscAddress = "/FaceBlendshapes"; // The OSC address to listen for blendshape messages
    public float debugScrollValue; // Value for the debug scroll bar
    public float lastInputFieldText; // Store the last text input from the input field
    public SkinnedMeshRenderer faceRenderer; // The SkinnedMeshRenderer component that contains Unity's blendshapes
    public TextMeshProUGUI debugText; // Avatar Blendshapes debug text
    public TextMeshProUGUI rawDebugText; // MediaPipe Blendshapes debug text
    public TextMeshProUGUI certainRawBlendshapesDebugText; // Debug text for certain raw blendshapes
    public TextMeshProUGUI debuggingBSName; // The blendshape name currently being debugged
    private OSCReceiver receiver; // The OSCReceiver component to handle incoming OSC messages
    public Dictionary<string, float> lastBlendshapes = new Dictionary<string, float>(); // Store the last received value for each blendshape
    private Dictionary<string, float> neutralMPValues = new Dictionary<string, float>(); // Store neutral blendshape values for calibration
                                                                                         // This will be used to determine the neutral position of each blendshape
    private Dictionary<string, float> minMPValues = new Dictionary<string, float>(); // Store neutral blendshape values for calibration
    private Dictionary<string, float> maxMPValues = new Dictionary<string, float>(); // Store neutral blendshape values for calibration
    private Dictionary<string, float> _toggledAvatarExtremeStates = new Dictionary<string, float>(); // Stores 0f or 100f for toggled extreme states of avatar blendshapes
    private bool isMinMaxCalibrated = false; // Flag to check if min/max calibration has been done
    private bool isNeutralCalibrated = false; // Flag to check if neutral calibration has been done
    public bool isDebugMode = false; // Toggle for debug mode, can be set in Inspector
    public bool dModeLevels = false; // Toggle if you only want to debug/calibrate the objective blendshapes
    private Dictionary<string, float> lastLongFormRawBlendshapes = new Dictionary<string, float>(); // Store the last received raw blendshape values
    private Dictionary<string, float> lastRawMPBlendshapes = new Dictionary<string, float>(); // Store the last received raw MediaPipe blendshape values
    public SortedList<string, List<string>> mediapipeToAvatarMapping = new SortedList<string, List<string>>(); // Mapping from MediaPipe blendshape names to Unity blendshape names
    private List<string> objectiveNames = new List<string> { "Smile", "Frown", "Surprise" }; // List of objective names for blendshapes
    private int currentObjectiveIndex = 0; // Index of the current objective
    private int currentLevelBlendshapeIndex = 0; // Index of the current Levels DebugMode blendshape in debug mode
    public List<string> SmileObjective = new List<string>
    {
        "mouthSmileLeft",
        "mouthSmileRight",
    }; // List of blendshapes for the Smile objective
    public List<string> FrownObjective = new List<string>
    {
        "browDownLeft",
        "browDownRight",
    }; // List of blendshapes for the Frown objective
    public List<string> SurpriseObjective = new List<string>
    {
        "mouthLowerDownLeft",
        "mouthLowerDownRight",
        "browInnerUp",
    }; // List of blendshapes for the Surprise objective
    public Dictionary<string, List<string>> ObjectiveList = new Dictionary<string, List<string>>(); // Dictionary to hold the objectives and their blendshapes
    public List<string> dModeAvatarBlendshapesToInspect; // List of Avatar blendshapes to inspect in debug mode
    public List<string> BlendshapesToCheck = new List<string>(); // List of Avatar blendshapes to fine tune
    public int blendshapeIndex = 0; // Index of the blendshape to be processed if in debug mode
    public string dModeMPKeyToInspect = ""; // Key for debug mode, used to access the current blendshape in debug mode
    public bool hasPrinted = false;
    private bool isManualOverrideActive = false; // Flag to check if manual override is active
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

            if (isDebugModeLevelsEnabled())
            {
                dModeMPKeyToInspect = getCurrentObjectiveBSName();
            } // If in levels debug mode, use the current objective blendshape name

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
        ShowRawCertainBlendshapes();
        // update the debug panels
    }

    void OnBlendshapeMessage(OSCMessage message)
    {
        // Debug.Log("Received OSC message on /10sfBlendshapes");
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
        ShowRawCertainBlendshapes();
    }

    private void UpdateRawDebugPanel()
    {
        if (rawDebugText == null)
        {
            rawDebugText.text = "...";
            return;
        }

        if (!dModeLevels)
        {
            var active = lastRawMPBlendshapes
                .OrderByDescending(kv => Mathf.Abs(kv.Value))
                .Take(8)
                .Select(kv => $"{kv.Key}: {kv.Value * 100f:F1}");
            rawDebugText.text = string.Join("\n", active);
        } // If not in debug mode levels, show the top 8 blendshapes, sorted by value descending

        else
        {
            var activeObjectiveBlendshapes = GetActiveObjectiveBlendshapes().ToHashSet();

            var active = lastRawMPBlendshapes
                .Where(kv => activeObjectiveBlendshapes.Contains(kv.Key))
                .Select(kv => $"{kv.Key}: {kv.Value * 1000:F4}");

            rawDebugText.text = string.Join("\n", active);
        } // If in debug mode levels, show only the active objective blendshapes
    }
    private void ShowRawCertainBlendshapes()
    {
        if (isDebugModeLevelsEnabled())
        {

            if (certainRawBlendshapesDebugText == null) return;
            var active = lastLongFormRawBlendshapes
                .Where(kv => kv.Key == dModeMPKeyToInspect)
                .Select(kv => $"{kv.Key}: {kv.Value * 1000:F4}");

            string output = string.Join("\n", active);
            certainRawBlendshapesDebugText.text = output;
        } // If in debug mode levels, show only the current objective blendshape

        else
        {
            if (certainRawBlendshapesDebugText == null) return;
            var active = lastLongFormRawBlendshapes
                .Where(kv => kv.Key == getCurrentObjectiveBSName())
                .Select(kv => $"{kv.Key}: {kv.Value * 1000:F4}");

            string output = string.Join("\n", active);
            certainRawBlendshapesDebugText.text = output;
        } // If not in debug mode levels, show the current blendshape
    }
    private void UpdateDebugPanel()
    {
        if (debugText == null) return;

        if (!dModeLevels)
        {
            var active = lastBlendshapes
                .Where(kv => Mathf.Abs(kv.Value) > 1f)
                .OrderByDescending(kv => Mathf.Abs(kv.Value))
                .Take(8)
                .Select(kv => $"{kv.Key}: {kv.Value:F1}");

            debugText.text = string.Join("\n", active);
        } // If not in debug mode levels, show the top 8 blendshapes with value > 1, sorted by value descending

        else
        {
            var activeObjectiveMPBlendshapes = GetActiveObjectiveBlendshapes();
            var avatarBlendshapeNames = activeObjectiveMPBlendshapes
                .SelectMany(mp => mediapipeToAvatarMapping.ContainsKey(mp) ? mediapipeToAvatarMapping[mp] : new List<string>())
                .Distinct();

            var active = avatarBlendshapeNames
                .Where(name => lastBlendshapes.ContainsKey(name))
                .Select(name => $"{name}: {lastBlendshapes[name] * 1000:F4}");

            debugText.text = string.Join("\n", active);
        } // If in debug mode levels, show only the active objective blendshapes
    }
    public void CalibrateNeutral()
    { 
        neutralMPValues = new Dictionary<string, float>(lastRawMPBlendshapes);
        isNeutralCalibrated = true;
        Debug.Log("Neutral face calibrated.");
    } // Calibrate the neutral position of each blendshape based on the last received raw MediaPipe values
    public void dModeLevelsToggle()
    {
        if (!isDebugMode)
        {
            Debug.LogWarning("Debug mode must be enabled to toggle debug mode levels.");
            return; // Exit if debug mode is not enabled
        }
        dModeLevels = !dModeLevels;
        if (dModeLevels)
        {
            debuggingBSName.text = getCurrentBlendshapeName();
            Debug.Log("Debug mode levels enabled. Only objective blendshapes will be displayed.");
        }
        else
        {
            debuggingBSName.text = getBlendshapeName();
            Debug.Log("Debug mode levels disabled. All blendshapes will be displayed.");
        }
    } // Toggle debug mode levels, which allows you to focus on objective blendshapes only
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
            dModeLevels = false;
            isMinMaxCalibrated = true;
            // assumes that once debug mode is disabled, min/max calibration is done
            Debug.Log("Debug mode disabled.");
        }
    } // Toggle debug mode, which allows you to inspect and manipulate blendshapes in real-time
    public void addBlendshape() // DEBUG FUNCTION
    { 
        if (BlendshapesToCheck.Contains(getBlendshapeName()))
        {
            Debug.Log($"Blendshape '{getBlendshapeName()}' is already in the list.");
            return; // Exit if the blendshape is already in the list
        }
        Debug.Log($"Adding blendshape '{getBlendshapeName()}' to the list.");
        BlendshapesToCheck.Add(getBlendshapeName());
    } // Adds the current blendshape to the list of blendshapes to check, if not already present
    public void printBlendshapesToCheck() // DEBUG FUNCTION
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
    public bool isDebugModeLevelsEnabled()
    {
        return dModeLevels;
    }
    public void printRawMPs() // DEBUG FUNCTION
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
            int index = faceRenderer.sharedMesh.GetBlendShapeIndex(blendshape.Key);
            if (index >= 0)
            {
                faceRenderer.SetBlendShapeWeight(index, 0f);
            }
        }
    } // Resets all previously set blendshapes to 0, clearing the avatar's facial expression
    public void incrementBlendshapeIndex()
    {
        if (isDebugModeLevelsEnabled() == false)
        {
            resetPreviousBlendshapes(); // Reset all blendshapes to 0 before incrementing
            blendshapeIndex++;
            if (blendshapeIndex >= 48) 
            {
                blendshapeIndex = 0; // Wrap around to the first index
            }
            Debug.Log($"Current blendshape index: {blendshapeIndex}");
            debuggingBSName.text = getBlendshapeName(); 
            hasPrinted = false; 
        } // If not in debug mode levels, increment the blendshape index normally

        else
        {
            resetPreviousBlendshapes(); // Reset all blendshapes to 0 before incrementing
            NextBlendshape();
            Debug.Log($"Current objective: {objectiveNames[currentObjectiveIndex]}");
            debuggingBSName.text = getCurrentBlendshapeName();
        } // If in debug mode levels, increment the blendshape index based on the current objective
    } 
    public void decrementBlendshapeIndex()
    {
        if (isDebugModeLevelsEnabled() == false)
        {
            resetPreviousBlendshapes(); 
            blendshapeIndex--;
            if (blendshapeIndex < 0)
            {
                blendshapeIndex = 48; // Wrap around to the last index
            }
            Debug.Log($"Current blendshape index: {blendshapeIndex}");
            debuggingBSName.text = getBlendshapeName(); 
            hasPrinted = false;
        }
        else
        {
            resetPreviousBlendshapes(); 
            PreviousBlendshape();
            Debug.Log($"Current objective: {objectiveNames[currentObjectiveIndex]}");
            debuggingBSName.text = getCurrentObjectiveBSName();
        }
    }
    public int getCurrentBlendshapeIndex()
    {
        return blendshapeIndex;
    }
    public string getAvatarBlendshapeName(List<string> dModeAvatarBlendshapesToInspect)
    {
        return string.Join(", ", dModeAvatarBlendshapesToInspect); // Joins with ", "
    } // Returns a string of the avatar blendshape names to inspect in debug mode, separated by commas
    public void getDictBlendshapeName() // DEBUG FUNCTION
    {
        Debug.Log("Blendshape names in the dictionary:");
        Debug.Log(string.Join(", ", neutralMPValues));
        Debug.Log("Last blendshapes received:");
        Debug.Log(string.Join(", ", lastBlendshapes)); 
    } 
    public void initialiseDictionary()
    {
        // Initialize the mapping dictionary for MediaPipe blendshapes to Unity blendshapes
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

        // Initialize the ObjectiveList with predefined objectives
        ObjectiveList = new Dictionary<string, List<string>>
        {
            { "Smile", SmileObjective },
            { "Frown", FrownObjective },
            { "Surprise", SurpriseObjective }
        };
    } // Initializes the mapping dictionary for MediaPipe blendshapes to Unity blendshapes and sets up the objectives
    public string getBlendshapeName()
    {
        return mediapipeToAvatarMapping.Keys[blendshapeIndex].ToString();
    }
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
    } // Helper method for forward calibration (MediaPipe raw value to Unity avatar blendshape weight)
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
                // InverseLerp's mathematical inverse is Lerp: Lerp(a, b, t) where t is (desiredValue / 100) because Lerp scales from 0 to 1.
                // So we can use Mathf.Lerp to find the required MediaPipe value.
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

        // Helper method for INVERSE calibration (Desired Avatar Weight to Required MediaPipe Raw Value)
        // This calculates what raw MediaPipe input would theoretically produce a given avatar blendshape weight
        return Mathf.Clamp01(requiredMPValue); // MediaPipe values are typically 0 to 1
    }
    public void RecordMin()
    {
        if (!isDebugModeLevelsEnabled())
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

        } // If not in debug mode levels, record the min value for the current blendshape

        else
        {
            string MPBblendshape = getCurrentObjectiveBSName();
            if (lastRawMPBlendshapes.TryGetValue(MPBblendshape, out float currentValue))
            {
                minMPValues[MPBblendshape] = currentValue; // Add or update
                Debug.Log($"Recorded MIN value for MP blendshape '{MPBblendshape}': {currentValue}");
            }
            else
            {
                Debug.LogWarning($"Cannot record MIN: Raw MediaPipe blendshape '{MPBblendshape}' not found.");
            }
        } // If in debug mode levels, record the min value for the current objective blendshape
    }
    public void RecordMax()
    {
        if (!isDebugModeLevelsEnabled())
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
        
        else
        {
            string MPBblendshape = getCurrentObjectiveBSName();
            if (lastRawMPBlendshapes.TryGetValue(MPBblendshape, out float currentValue))
            {
                maxMPValues[MPBblendshape] = currentValue; // Add or update
                Debug.Log($"Recorded MAX value for MP blendshape '{MPBblendshape}': {currentValue}");
            }
            else
            {
                Debug.LogWarning($"Cannot record MAX: Raw MediaPipe blendshape '{MPBblendshape}' not found.");
            }
        }

    }
    public void ToggleBlendshapeExtremeVisually()
    {
        isManualOverrideActive = true;
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
    } // Toggles the visual extreme state of the current blendshape, applying it to the avatar and logging the estimated MediaPipe value needed
    public string getCurrentBlendshapeName()
    {
        return objectiveNames[currentObjectiveIndex];
    }
    public string getCurrentObjectiveBSName()
    {
        return ObjectiveList[getCurrentBlendshapeName()][currentLevelBlendshapeIndex];
    }
    private List<string> GetActiveObjectiveBlendshapes() 
    {
        string currentObjective = getCurrentBlendshapeName();

        return currentObjective switch
        {
            "Smile" => SmileObjective,
            "Frown" => FrownObjective,
            "Surprise" => SurpriseObjective,
            _ => new List<string>()
        };
    } // Returns the blendshapes for the current objective
    public void NextBlendshape()
    {
        var activeObjectiveBlendshapes = GetActiveObjectiveBlendshapes();
        int listLength = activeObjectiveBlendshapes.Count;
        currentLevelBlendshapeIndex = (currentLevelBlendshapeIndex + 1) % listLength; // Cycle through the list
    } // Cycles to the next blendshape in the current objective's list
    public void PreviousBlendshape()
    {
        var activeObjectiveBlendshapes = GetActiveObjectiveBlendshapes();
        int listLength = activeObjectiveBlendshapes.Count;
        currentLevelBlendshapeIndex = (currentLevelBlendshapeIndex - 1 + listLength) % listLength; // Cycle through the list
    }
    public void NextObjective()
    {
        if (isDebugModeLevelsEnabled())
        {
            resetPreviousBlendshapes();
            currentObjectiveIndex = (currentObjectiveIndex + 1) % objectiveNames.Count;
            debuggingBSName.text = getCurrentBlendshapeName();
            Debug.Log($"Next objective: {objectiveNames[currentObjectiveIndex]} because currentObjectiveIndex is {currentObjectiveIndex}");
        } // If in debug mode levels, increment the objective index and update the blendshape name

        else
        {
            Debug.LogWarning("NextObjective called, but not in debug mode levels. No action taken.");
            return;
        } // If not in debug mode levels, do nothing
    }
    public void PreviousObjective()
    {
        if (isDebugModeLevelsEnabled())
        {
            resetPreviousBlendshapes();
            currentObjectiveIndex = (currentObjectiveIndex - 1 + objectiveNames.Count) % objectiveNames.Count;
            debuggingBSName.text = getCurrentBlendshapeName();
            Debug.Log($"Previous objective: {objectiveNames[currentObjectiveIndex]} because currentObjectiveIndex is {currentObjectiveIndex}");
        }
        else
        {
            Debug.LogWarning("PreviousObjective called, but not in debug mode levels. No action taken.");
            return;
        } // If not in debug mode levels, do nothing        
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

        // smooth transition back without a "jump" to a default pose.
        foreach (var entry in lastBlendshapes) 
        {
            string avatarName = entry.Key;
            float value = entry.Value;
            int index = faceRenderer.sharedMesh.GetBlendShapeIndex(avatarName);
            if (index >= 0)
            {
                faceRenderer.SetBlendShapeWeight(index, value);
            }
        }

        // Flag assignment allows the debug mode log to appear again if you re-enable dMode and select a blendshape.
        hasPrinted = false; 

        // Ensure debug panels update to reflect live values
        UpdateDebugPanel();
        UpdateRawDebugPanel();
        ShowRawCertainBlendshapes();
    }
}
