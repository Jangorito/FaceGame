using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{

    Logger logger;

    private void Awake()
    {
        logger = new(true);
        logger.LogMsg("test::Awake");
    }

    private void OnEnable()
    {
        logger.LogMsg("test::OnEnable");   
    }

    private void OnDisable()
    {
        logger.LogMsg("test::OnDisable");
    }

    // Start is called before the first frame update
    void Start()
    {
        logger.LogMsg("test::Start");
    }

    // Update is called once per frame
    void Update()
    {
        logger.LogMsg("test::Update");   
    }
}
