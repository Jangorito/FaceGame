using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    private GameObject hand;
    // Start is called before the first frame update
    void Start()
    {
        hand = GameObject.Find("spine/pelvis.L");
    }

    // Update is called once per frame
    void Update()
    {
        hand.transform.Translate(new Vector3(1,1,1));
    }
}
