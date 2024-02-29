using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{

    private Avatar avatar;
    public int Range = 10;
    public void Start() {
        avatar = getAvatar();

    }

    /* requests the position of the hand */
    public Vector3 GetCursor() {
        return avatar.getBoneTransform(HumanBodyBones.LeftHand).position;
    }

    public Boolean isCursorNear(Vector3 cursor) {
        Vector2 cursorxy = new Vector2(cursor.x, cursor.y);
   
        /* NOTE: 10 is the random number I have chosen, will need fine tuning once Buttons are added */
        return (Vector2.Distance(cursorxy, GetCursor()) <= Range);
    }

    /* attemps to find the active Avatar to gain access to data */
    private Avatar getAvatar()
    {
        Avatar avatar = FindObjectOfType<Avatar>();
        if (avatar == null)
            Debug.LogError("Could not find an Avatar in the scene");
        return avatar;

    }

}
