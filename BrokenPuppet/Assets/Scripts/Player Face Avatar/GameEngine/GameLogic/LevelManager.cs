using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;

public class LevelManager : MonoBehaviour
{
    public BlendshapeReader blendshapeReader; 
    private LevelSO currentLevel;
    private float currentTime;
    private List<ObjectiveSO> activeObjectives;
    public bool isLevelLoaded; 
    public UIManager uiManager;
    public bool isPaused = false;

    public void Awake()
    {
        isLevelLoaded = false; // Initialize the flag
    }

    public void LoadLevel(LevelSO level)
    {
        isLevelLoaded = true; // Set the flag to true when a level is loaded
        currentLevel = level;
        currentTime = 10;
        currentTime = level.timeLimit;
        activeObjectives = new List<ObjectiveSO>(level.objectives);

        if (uiManager != null)
        {
            uiManager.SetupLevelUI(level, activeObjectives);
        }

        Debug.Log($"Loading Level: {level.levelName} with {activeObjectives.Count} objectives.");

        if (activeObjectives.Count > 0)
        {
            Debug.Log("--- Objectives ---");
            foreach (ObjectiveSO obj in activeObjectives)
            {
                // Accessing objectiveName directly because it's a public field
                Debug.Log($"Objective Name: {obj.objectiveName}");

                if (obj.BlendshapesToCheckDictionary != null && obj.BlendshapesToCheckDictionary.Count > 0)
                {
                    Debug.Log("  Required Blendshapes:");
                    foreach (KeyValuePair<string, int> blendshapeEntry in obj.BlendshapesToCheckDictionary)
                    {
                        Debug.Log($"    - {blendshapeEntry.Key}: {blendshapeEntry.Value}");
                    }
                } // If there are no blendshapes defined, we can still log that
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
    }
    public void TogglePause()
    {
        isPaused = !isPaused;
    }
    void Update()
    {
        if (isPaused) return;
        
        if (currentLevel == null && isLevelLoaded)
        {
            Debug.LogWarning("No level loaded. Please load a level before updating.");
            return; // Do nothing if no level is loaded  
        } 
        
        if (currentLevel == null)
        {
            return; // No level loaded, nothing to update
        }

        // 1. Update Timer
        currentTime -= Time.deltaTime;
        if (uiManager != null)
        {
            uiManager.UpdateTimer(currentTime); // Update the timer display
        }

        if (currentTime <= 0)
        {
            Debug.Log("Time's Up! Level Failed.");
            GameManager.Instance.LevelFailed();
            currentLevel = null; // Stop processing this level
            return;
        }

        // 2. Check Objectives
        for (int i = activeObjectives.Count - 1; i >= 0; i--)
        {
            ObjectiveSO objective = activeObjectives[i];
            
            if (IsObjectiveComplete(objective))
            {
                Debug.Log($"Objective '{objective.objectiveName}' Complete!");
                uiManager?.MarkObjectiveComplete(objective);   
                activeObjectives.RemoveAt(i);
            }
        }

        // 3. Check for Level Win
        if (!activeObjectives.Any()) // If the list is empty
        {
            currentLevel = null; // Stop processing
            GameManager.Instance.LevelCompleted();
        }
    }

    private bool IsObjectiveComplete(ObjectiveSO objective)
    {
        // Check if ALL blendshapes for this objective are above the threshold
        foreach (string bsName in objective.BlendshapesToCheckDictionary.Keys)
        {

            if (blendshapeReader.GetBlendshapeValue(bsName) < objective.BlendshapesToCheckDictionary[bsName])
            {
                return false; // If any one is not active, the objective is not met
            }
        }
        
        return true; // All were active
    }
}