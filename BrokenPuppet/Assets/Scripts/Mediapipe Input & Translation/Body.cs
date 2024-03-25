using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body
{
    public Transform parent;
    public AccumulatedBuffer[] bPositions;
    public GameObject[] instances;

    public Body(Transform parent) {
        bPositions = new AccumulatedBuffer[(int)LenLandmark.Total];
        instances = new GameObject[(int)LenLandmark.Total];

        for (int i =0; i < (int)LenLandmark.Total; i++) {
            instances[i] = new GameObject(((Landmark)i).ToString());
            instances[i].transform.parent = parent;
            instances[i].transform.localScale = Vector3.one;
            bPositions[i] = new AccumulatedBuffer();
        }
    }

    public void addValue(int index, Vector3 value) {
        if (index < bPositions.Length)
            bPositions[index].addValue(value);
    }
}
