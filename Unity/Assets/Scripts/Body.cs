using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 543 Landmarks
 * 33 - pose
 * 468 - face
 * 21 left hand
 * 21 right hand
 */

public class Body
{
    private const int LANDMARK_COUNT = 543;

    public Transform parent;

    Transform[] bones;

    public Body(Transform parent) {

        /* This should be the parent of all the bones */
        this.parent = parent;

        /* Gets all child objects from the parent */
        bones = parent.GetComponentsInChildren<Transform>();
        

        foreach (Transform bone in bones) { 
           Debug.Log(bone.ToString());
        }

        Debug.Log("Body Created");
    }

    public void update() {
        foreach(Transform bone in bones) { 
                bone.transform.Rotate(new Vector3(0.001f,0.0f,0.0f));
        }
    }

    public void updateLandmark(int ind, string position) { 
       // Debug.Log("Updating Landmark "+ind);
    }

}
