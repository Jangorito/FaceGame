using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;



public class Collision : MonoBehaviour
{
    public TextMeshProUGUI counterText;
    bool isColliding = false; // Flag to track if collision is ongoing
    Coroutine countdownCoroutine; // Coroutine reference for countdown

    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("Collision detected!");
        if (!isColliding)
        {
            isColliding = true;
            countdownCoroutine = StartCoroutine(CountdownAndLoadScene(3.0f)); // Start the countdown coroutine
        }
    }

    private IEnumerator CountdownAndLoadScene(float countdownDuration)
    {
        float timeElapsed = 0f;
        while (timeElapsed < countdownDuration)
        {
            float remainingTime = countdownDuration - timeElapsed;
            counterText.text = "Starting in: " + Mathf.CeilToInt(remainingTime);
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        // Load the scene after the countdown
        NextLevelTest.level = 0;
        SceneManager.LoadScene("SettingsMenu");

        // Reset flags and UI
        isColliding = false;
        counterText.text = "";
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("COLLISION EXITED\n");
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine); // Stop the countdown if collision ends prematurely
        isColliding = false;
        counterText.text = ""; // Clear counter text
    }
}
