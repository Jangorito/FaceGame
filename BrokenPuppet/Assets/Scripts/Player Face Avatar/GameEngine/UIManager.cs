using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Timer
    public TextMeshProUGUI timerText;
    
    // Level & Objective Display
    public TextMeshProUGUI levelNameText;
    public Transform objectiveListParent; // The parent container for objective UI elements
    public GameObject objectiveUIPrefab; // A prefab for each objective, e.g., a panel with text
    
    // A dictionary to keep track of the UI elements for each active objective.
    private Dictionary<ObjectiveSO, GameObject> objectiveUIElements = new Dictionary<ObjectiveSO, GameObject>();

    /// <summary>
    /// Initializes the UI for a new level.
    /// </summary>
    /// <param name="level">The LevelScriptableObject containing level data.</param>
    /// <param name="activeObjectives">The list of objectives for the current level.</param>
    public void SetupLevelUI(LevelSO level, List<ObjectiveSO> activeObjectives)
    {
        Debug.Log($"Setting up UI for Level: {level.levelName} with {activeObjectives.Count} objectives.");
        // Clear any previous objective UI elements
        ClearObjectiveUI();

        // Set the level name
        if (levelNameText != null)
        {
            levelNameText.text = level.levelName;
        }

        // Create and display a UI element for each objective
        if (objectiveUIPrefab != null && objectiveListParent != null)
        {
            foreach (ObjectiveSO obj in activeObjectives)
            {
                GameObject newObjectiveUI = Instantiate(objectiveUIPrefab, objectiveListParent);
                TextMeshProUGUI textComponent = newObjectiveUI.GetComponentInChildren<TextMeshProUGUI>();
                if(textComponent != null)
                {
                    textComponent.text = obj.objectiveName;
                }
                
                objectiveUIElements.Add(obj, newObjectiveUI);
            }
        }
    }

    /// <summary>
    /// Updates the timer display.
    /// </summary>
    /// <param name="timeRemaining">The current time remaining in the level.</param>
    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(timeRemaining)}";
        }
    }

    /// <summary>
    /// Visually marks an objective as complete.
    /// </summary>
    /// <param name="objective">The completed objective.</param>
    public void MarkObjectiveComplete(ObjectiveSO objective)
    {
        if (objectiveUIElements.ContainsKey(objective))
        {
            GameObject uiElement = objectiveUIElements[objective];
            if(uiElement != null)
            {
                TextMeshProUGUI textComponent = uiElement.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.color = Color.green;
                }
            }
        }
    }

    /// <summary>
    /// Clears all objective UI elements from the screen.
    /// </summary>
    private void ClearObjectiveUI()
    {
        foreach (var element in objectiveUIElements.Values)
        {
            Destroy(element);
        }
        objectiveUIElements.Clear();
    }
}