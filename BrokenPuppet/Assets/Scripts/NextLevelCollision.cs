using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;



public class NextLevelCollision : MonoBehaviour
{

    private void Start()
    {


    }

    private void OnCollisionEnter2D(Collision2D CollisionObject)
    {
        Debug.Log("BEFORE: " + NextLevelTest.level);
        Debug.Log("Collision detected!");

        NextLevelTest.level++;
        //RESET OTHER THINGS HERE;


        Debug.Log("AFTER: " + NextLevelTest.level);
        SceneManager.LoadScene("GameSceneWithUI");


    }
}
