using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSimilarityChecker : MonoBehaviour
{

    public GameObject BrokenPuppet;
    public GameObject GhostAvatar;
    // Start is called before the first frame update
    void Start()
    {
        BrokenPuppet = GameObject.Find("BrokenPuppetModel_2");
        GhostAvatar = GameObject.Find("");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }
}
