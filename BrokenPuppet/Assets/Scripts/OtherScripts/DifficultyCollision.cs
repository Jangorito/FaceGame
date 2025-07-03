using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DifficultyCollision : MonoBehaviour
{
    public static int difficultyLevel;
    bool isColliding = false; // Flag to track if collision is ongoing
    public TextMeshProUGUI counterText;
    Coroutine countdownCoroutine;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only start countdown if not already colliding
        if (!isColliding)
        {
            isColliding = true;

            // Start the countdown coroutine
            Debug.Log(collision.gameObject.name);
            countdownCoroutine = StartCoroutine(CountdownAndLoadScene(3.0f, collision.gameObject.name));
        }
    }

    private IEnumerator CountdownAndLoadScene(float countdownDuration, string buttonName)
    {
        float timeElapsed = 0f;

        while (timeElapsed < countdownDuration && isColliding)
        {
            float remainingTime = countdownDuration - timeElapsed;
            counterText.text = "Loading in: " + Mathf.CeilToInt(remainingTime) + " seconds";
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        // If countdown completes and still colliding, load scene
        if (isColliding)
        {
            SetDifficultyLevel(buttonName);
            SceneManager.LoadScene("GameSceneWithUI");
        }

        // Reset flags and UI
        isColliding = false;
        counterText.text = "";
    }

    private void SetDifficultyLevel(string buttonName)
    {
        switch (buttonName)
        {
            case "Easy":
                difficultyLevel = 0;
                break;
            case "Medium":
                difficultyLevel = 1;
                break;
            case "Hard":
                difficultyLevel = 2;
                break;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // If colliding, stop the countdown coroutine
        if (isColliding)
        {
            StopCoroutine(countdownCoroutine);
            isColliding = false;
            counterText.text = ""; // Clear counter text
        }
    }
}
