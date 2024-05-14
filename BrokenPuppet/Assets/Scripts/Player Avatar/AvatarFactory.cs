using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarFactory : MonoBehaviour
{
    public GameObject PlayerOne;
    public GameObject PlayerTwo;

    private GameObject p1;
    private GameObject p2;

    private Vector3 p1_Position = new(-1.5f, 0.03f, 0f);
    private Vector3 p2_Position = new(1.5f, 0.03f, 0f);



    // Start is called before the first frame update
    void Start()
    {

        p1 = Instantiate(PlayerOne, p1_Position, transform.rotation);
        p2 = Instantiate(PlayerTwo, p2_Position, transform.rotation);
    }

    public void resetPlayerOne() {
        Destroy(p1);
        p1 = Instantiate(PlayerOne);
    }

    public void resetPlayerTwo() {
        Destroy(p2);
        p2 = Instantiate(PlayerTwo);
    }
}

