using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DifficultyCollision : MonoBehaviour
{
    public static int difficultyLevel;
    public TextMeshProUGUI counterText;
    Coroutine countdownCoroutine;

    private void OnCollisionEnter2D(Collision2D CollisionObject)
    {
        Debug.Log("Collision detected!");

        if (CollisionObject.gameObject.name == "Easy")
            difficultyLevel = 0;
        else if (CollisionObject.gameObject.name == "Medium")
            difficultyLevel = 1;
        else if (CollisionObject.gameObject.name == "Hard")
            difficultyLevel = 2;
        Debug.Log(CollisionObject.gameObject.name);
        GameObject movementObject = GameObject.Find("Cursor");

        if (movementObject != null)
        {
            // Get the movement script component
            Movement movementScript = movementObject.GetComponent<Movement>();

            // Set the ClosePort property using the movement script instance
            if (movementScript != null)
            {
                Movement.closePort = 1; // Example value
                Debug.Log("ClosePort set to 1\n");
            }
        }

        // Start the countdown coroutine to load scene after 3 seconds
        countdownCoroutine = StartCoroutine(CountdownAndLoadScene(3.0f, CollisionObject.gameObject.name));
    }

    private IEnumerator CountdownAndLoadScene(float countdownDuration, string name)
    {
        float timeElapsed = 0f;
        while (timeElapsed < countdownDuration)
        {
            float remainingTime = countdownDuration - timeElapsed;
            counterText.text = "Loading in " + name + ": " + Mathf.CeilToInt(remainingTime) + " seconds";
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        // Load the scene after the countdown
        SceneManager.LoadScene("GameSceneWithUI");
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("COLLISION EXITED\n");
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine); // Stop the countdown if collision ends prematurely
        counterText.text = ""; // Clear counter text
    }
}
