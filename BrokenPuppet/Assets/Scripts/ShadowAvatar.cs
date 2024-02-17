using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShadowAvatar : MonoBehaviour { 

    public GameObject shadow;
    public Animator shadowBody;

    public Transform[] boneTransforms; // Assign the bones of your avatar
    public Quaternion[] minRotations;
    public Quaternion[] maxRotations;
    public Difficulty GameDifficulty;
    public int[] randomBonesSelected;

    // Start is called before the first frame update
    void Start()
    {
        InitializeRotationLimits();
        GameDifficulty = Randomiser.GameDifficulty;
        GenerateRandomPose();
    }

    // Update is called once per frame
    void Update()
    {
        /* Copy the movement of the avatar */
        
        
    }

    void SelectRandomBones()
    {
        // Clear the array
        Array.Clear(randomBonesSelected, 0, randomBonesSelected.Length);
        // Create a new Random object
        int AllBones = 54;
        System.Random rnd = new System.Random();

        // Determine the number of bones based on the game difficulty
        int NumberOfBones;
        if ((int)GameDifficulty == 1)
            NumberOfBones = 13;
        else if ((int)GameDifficulty == 2)
            NumberOfBones = 27;
        else
            NumberOfBones = 54;

        // Populate the array with random Transform objects
        for (int i = 0; i < NumberOfBones; i++)
        {
            // Randomly select an index to assign the transform
            int r = rnd.Next(AllBones);

            // Populate array with bone indexes
            randomBonesSelected[r] = 1;
        }
    }

    /* Moves the model */
    //foreach(var i in parentCalibrationData)
    // {
    ///     Quaternion deltaRotation = Quaternion.FromToRotation(i.Value.initialDirection,
    //    i.Value.getCurrentDirection());

    //     animator.GetBoneTransform(i.Key).rotation = deltaRotation* i.Value.initialRotation;
    // }

    void MoveModel()
    {
        Avatar instance = new Avatar();
        Dictionary<HumanBodyBones, CalibrationData> calibrationData = instance.ParentCalibrationData;

        foreach (var i in calibrationData)
        { Quaternion deltaRotation = Quaternion.FromToRotation(i.Value.initialDirection, i.Value.getCurrentDirection());

        shadowBody.GetBoneTransform(i.Key).rotation = i.Value.initialRotation * deltaRotation;
        }
    }

//54 bones
    void InitializeRotationLimits()
    {
        minRotations = new Quaternion[boneTransforms.Length];
        maxRotations = new Quaternion[boneTransforms.Length]
            ;
        float MaxMin;
        if ((int)GameDifficulty == 1)
            MaxMin = 45;
        else if ((int)GameDifficulty == 2)
            MaxMin = 90;
        else
            MaxMin = 180;
        // Define min and max rotations for each bone

        for (int i = 0; i < randomBonesSelected.Length; i++)
        {
            if (randomBonesSelected[i] != 1)
                continue;
            minRotations[i] = Quaternion.Euler(-MaxMin, -MaxMin, -MaxMin);
            maxRotations[i] = Quaternion.Euler(MaxMin, MaxMin, MaxMin);

        }
    }

    /* Will use procedural generation to generate the random pose */
    void GenerateRandomPose() {

        for (int i = 0; i < boneTransforms.Length; i++)
        {
            // Randomize rotation for each bone
            Quaternion randomRotation = Quaternion.Lerp(minRotations[i], maxRotations[i], UnityEngine.Random.value);

            // Apply the random rotation to the bone
            boneTransforms[i].localRotation = randomRotation;
        }
    }
}
