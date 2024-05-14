using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarFactory : MonoBehaviour
{
    public GameObject PlayerOne;
    public GameObject PlayerTwo;

    private GameObject p1;
    private GameObject p2;

    // Start is called before the first frame update
    void Start()
    {
        p1 = Instantiate(PlayerOne);
        p2 = Instantiate(PlayerTwo);
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

