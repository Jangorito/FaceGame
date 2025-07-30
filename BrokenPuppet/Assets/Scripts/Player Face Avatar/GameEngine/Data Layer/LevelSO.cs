using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Level", menuName = "Emotion Game/Level")]
public class LevelSO : ScriptableObject
{
    public string levelName;
    public float timeLimit = 30f;
    // review this timelimit, it might be too short or too long depending on the level design

    public List<ObjectiveSO> objectives; // A list of all objectives for this level
}