using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<LevelSO> levels;
    public int currentLevelIndex = 0;
    public LevelManager levelManager; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void playButtonClicked()
    {
        Debug.Log("Play Button Clicked!");
        StartLevel(currentLevelIndex);
    }
    
    public void StartLevel(int levelIndex)
    {
        if (levelIndex < levels.Count)
        {
            Debug.Log($"Starting Level: {levels[levelIndex].levelName}");
            levelManager.LoadLevel(levels[levelIndex]);
        }
        else
        {
            Debug.Log("YOU WIN THE GAME!");
        }
    }

    public void LevelCompleted()
    {
        Debug.Log("Level Success!");
        currentLevelIndex++;
        Debug.Log($"Loading Next Level: {currentLevelIndex}");
        StartLevel(currentLevelIndex);
    }

    public void LevelFailed()
    {
        Debug.Log("Level Failed!");

        Debug.Log($"Restarting Level: {currentLevelIndex}");
        StartLevel(currentLevelIndex);
    }
}