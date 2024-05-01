using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;



public class Collision : MonoBehaviour
{

    private void Start()
    {


    }

    private void OnCollisionEnter2D(Collision2D CollisionObject)
    {
        Debug.Log("Collision detected!");

        SceneManager.LoadScene("SettingsMenu");


    }
}
