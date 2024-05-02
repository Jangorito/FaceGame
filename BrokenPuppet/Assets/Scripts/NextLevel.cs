using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevelTest : MonoBehaviour
{
    public ShadowAvatar shadow;
    public GameObject Player1;
    public GameObject Player2;
    public static int level = 0;

    Avatar player1;
    Avatar player2;

    // Start is called before the first frame update
    void Start()
    {
        player1 = Player1.GetComponent<Avatar>();

        player2 = Player2.GetComponent<Avatar>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void nextLevel()
    {
        newShadow();
        resetBreaking();
        level++;
    }

    public int getLevel()
    {
        return level;
    }

    public static void incrementLevel()
    {
        level = level++;
    }

    void newShadow()
    {
        shadow.UpdateShadow();
    }

    void resetBreaking()
    {
        player1.resetAvatar();
        player2.resetAvatar();
    }
}
