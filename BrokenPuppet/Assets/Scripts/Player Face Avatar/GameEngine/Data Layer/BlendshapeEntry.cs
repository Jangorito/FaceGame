using UnityEngine;
using System; // Required for System.Serializable

[Serializable]
public class BlendshapeEntry{
    public string blendshapeName; // This will be your key
    public int threshold;         // This will be your value
}