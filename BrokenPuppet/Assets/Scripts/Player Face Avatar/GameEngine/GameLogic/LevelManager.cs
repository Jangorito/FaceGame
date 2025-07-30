using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public BlendshapeReader blendshapeReader; // Assign in Inspector
    // public UIManager uiManager; // Assign your UI Manager here
    // need to create a UIManager script to handle UI updates

    private LevelSO currentLevel;
    private float currentTime;
    private List<ObjectiveSO> activeObjectives;

    public void LoadLevel(LevelSO level)
    {
        currentLevel = level;
        currentTime = level.timeLimit;
        activeObjectives = new List<ObjectiveSO>(level.objectives);

        Debug.Log($"Loading Level: {level.levelName} with {activeObjectives.Count} objectives.");
        
        // --- NEW CODE START ---
        if (activeObjectives.Count > 0)
        {
            Debug.Log("--- Objectives ---");
            foreach (ObjectiveSO obj in activeObjectives)
            {
                // Accessing objectiveName directly because it's a public field
                Debug.Log($"Objective Name: {obj.objectiveName}");

                // Accessing the BlendshapesToCheckDictionary property
                // This will trigger the getter and ensure the internal dictionary is populated.
                if (obj.BlendshapesToCheckDictionary != null && obj.BlendshapesToCheckDictionary.Count > 0)
                {
                    Debug.Log("  Required Blendshapes:");
                    foreach (KeyValuePair<string, int> blendshapeEntry in obj.BlendshapesToCheckDictionary)
                    {
                        Debug.Log($"    - {blendshapeEntry.Key}: {blendshapeEntry.Value}");
                    }
                }
                else
                {
                    Debug.Log("  No blendshapes defined for this objective.");
                }
            }
            Debug.Log("------------------");
        }
        else
        {
            Debug.Log("No objectives found for this level.");
        }
        // Tell the UI to update
        // uiManager.SetupLevelUI(level);
    }

    void Update()
    {
        if (currentLevel == null) return; // Do nothing if no level is loaded

        // 1. Update Timer
        currentTime -= Time.deltaTime;
        // uiManager.UpdateTimer(currentTime);

        if (currentTime <= 0)
        {
            Debug.Log("Time's Up! Level Failed.");
            GameManager.Instance.LevelFailed();
            currentLevel = null; // Stop processing this level
            return;
        }

        // 2. Check Objectives
        // Go backwards so we can safely remove completed objectives
        for (int i = activeObjectives.Count - 1; i >= 0; i--)
        {
            ObjectiveSO objective = activeObjectives[i];
            // Debug.Log($"Objective '{objective.objectiveName}' is active with all blendshapes above threshold.");
            
            if (IsObjectiveComplete(objective))
            {
                Debug.Log($"Objective '{objective.objectiveName}' Complete!");
                // uiManager.MarkObjectiveComplete(objective);
                activeObjectives.RemoveAt(i);
            }
        }

        // 3. Check for Level Win
        if (!activeObjectives.Any()) // If the list is empty
        {
            GameManager.Instance.LevelCompleted();
            currentLevel = null; // Stop processing
        }
    }

    private bool IsObjectiveComplete(ObjectiveSO objective)
    {
        // Check if ALL blendshapes for this objective are above the threshold
        foreach (string bsName in objective.BlendshapesToCheckDictionary.Keys)
        {

            if (blendshapeReader.GetBlendshapeValue(bsName) < objective.BlendshapesToCheckDictionary[bsName])
            // TODO: change the if condition to a variable one that is dependent on the objective/level; e.g. wry smile vs ott smile
            {
                return false; // If any one is not active, the objective is not met
            }
        }
        
        return true; // All were active
    }
}