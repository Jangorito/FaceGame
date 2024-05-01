using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;



public class DifficultyCollision : MonoBehaviour
{

    public static int difficultyLevel;

    public int getDifficulty()
    {
        return difficultyLevel;
    }

    private void Start()
    {
    }

    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("GameSceneWithUI");
    }

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
                movementScript.ClosePort = 1; // Example value
                Debug.Log("ClosePort set to 1\n");
            }
        }

        StartCoroutine(LoadSceneAfterDelay(1.0f)); // Load scene after 1 second
    }
}
