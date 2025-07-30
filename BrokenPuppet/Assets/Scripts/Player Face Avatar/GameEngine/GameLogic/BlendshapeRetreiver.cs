using UnityEngine;

public class BlendshapeReader : MonoBehaviour
{
    // Assign your OSC receiver script here in the Inspector
    public FaceBlendshapeReceiver oscReceiver; 

    public float GetBlendshapeValue(string blendshapeName)
    {
        // Access the 'lastBlendshapes' dictionary from your receiver script
        if (oscReceiver.lastBlendshapes.TryGetValue(blendshapeName, out float value))
        {
            return value;
        }
        return 0f; // Return 0 if not found
    }
}