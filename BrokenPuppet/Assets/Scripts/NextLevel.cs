using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevelTest : MonoBehaviour
{
    public ShadowAvatar shadow;

    public GameObject AvatarFactory;
    private AvatarFactory AvatarManager;

    public static int level = 0;

    // Start is called before the first frame update
    void Start()
    {
        AvatarManager = AvatarFactory.GetComponent<AvatarFactory>();
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
        AvatarManager.resetPlayerOne();
        AvatarManager.resetPlayerTwo();
    }
}
