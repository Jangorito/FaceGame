using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Objective", menuName = "Emotion Game/Objective")]
public class ObjectiveSO : ScriptableObject
{
    public string objectiveName; // e.g., "Big Smile"

    // List of custom serializable class
    public List<BlendshapeEntry> blendshapesToCheckList; 
    

    // PRIVATE actual Dictionary object.
    private Dictionary<string, int> _blendshapesToCheckDictionary; // <-- CHANGE: Added '_' prefix for clarity

    // PUBLIC property that provides controlled access to the dictionary.
    public Dictionary<string, int> BlendshapesToCheckDictionary 
    {
        get 
        {
            if (_blendshapesToCheckDictionary == null || _blendshapesToCheckDictionary.Count != blendshapesToCheckList.Count)
            {
                PopulateDictionaryFromList();
            }
            // Always return the PRIVATE backing field.
            return _blendshapesToCheckDictionary;
        }
    }

    private void PopulateDictionaryFromList()
    {
        // Initialize the PRIVATE backing field here.
        _blendshapesToCheckDictionary = new Dictionary<string, int>(); // <-- CHANGE: Assign to _blendshapesToCheckDictionary

        foreach (BlendshapeEntry entry in blendshapesToCheckList)
        {
            // Add a null check for safety in case an entry is somehow empty in the list
            if (entry != null && !string.IsNullOrEmpty(entry.blendshapeName))
            {
                // Add to the PRIVATE backing field.
                if (!_blendshapesToCheckDictionary.ContainsKey(entry.blendshapeName)) // <-- CHANGE: Use _blendshapesToCheckDictionary
                {
                    _blendshapesToCheckDictionary.Add(entry.blendshapeName, entry.threshold); // <-- CHANGE: Use _blendshapesToCheckDictionary
                }
                else
                {
                    Debug.LogWarning($"Duplicate blendshape name '{entry.blendshapeName}' found in objective '{objectiveName}'. Only the first instance will be used.");
                }
            }
        }
        
        Debug.Log($"Objective '{objectiveName}' blendshapes dictionary populated with {_blendshapesToCheckDictionary.Count} entries.");
    }

    // Optional: Call this from OnEnable if you want the dictionary to be ready as soon as the object is enabled
    private void OnEnable()
    {
        // Directly call the method to populate the internal dictionary.
        PopulateDictionaryFromList();
    }

    // Optional: Call this from OnValidate in the editor to keep the dictionary updated when changes are made
    private void OnValidate()
    {
        // Directly call the method to populate the internal dictionary.
        PopulateDictionaryFromList();
    }
}