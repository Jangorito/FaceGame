using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShadowAvatar : MonoBehaviour { 

    public GameObject shadow;
    public Animator shadowBody;

    public Transform[] boneTransforms = new Transform[AllBones]; // Assign the bones of your avatar
    public Quaternion[] minRotations;
    public Quaternion[] maxRotations;
    public Difficulty GameDifficulty;

    static int AllBones = 50;

    static public int[] randomBonesSelected = new int[AllBones];

    // Start is called before the first frame update
    void Start()
    {
        GameDifficulty = Randomiser.GameDifficulty;
        SelectRandomBones();
        InitializeRotationLimits();
        GenerateRandomPose();
        MoveModel();
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
        System.Random rnd = new System.Random();

        // Determine the number of bones based on the game difficulty
        int NumberOfBones;
        if ((int)GameDifficulty == 1)
            NumberOfBones = 8;
        else if ((int)GameDifficulty == 2)
            NumberOfBones = 16;
        else
            NumberOfBones = 24;

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
        for (int i = 0; i < boneTransforms.Length; i++)
        {
            // Check if boneTransforms[i] is null
            if (boneTransforms[i] == null)
            {
                Debug.LogError("boneTransforms[" + i + "] is null.");
                continue;
            }

            // Get the rotation of the bone from boneTransforms array
            Quaternion newRotation = boneTransforms[i].localRotation;

            // Apply the new rotation to the bone transform in the boneTransforms array
            boneTransforms[i].rotation = newRotation;

            // Convert the integer index to a HumanBodyBones enum value
            HumanBodyBones boneType = (HumanBodyBones)i;

            // Update the rotation of the corresponding bone in shadowBody
            Transform shadowBoneTransform = shadowBody.GetBoneTransform(boneType);
            if (shadowBoneTransform != null)
            {
                shadowBoneTransform.rotation = newRotation;
            }
        }
    }



    //54 bone
    void InitializeRotationLimits()
    {
        minRotations = new Quaternion[AllBones];
        maxRotations = new Quaternion[AllBones];
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

        for (int i = 0; i < randomBonesSelected.Length; i++)
        {
            if (randomBonesSelected[i] != 1)
            {
                // Set rotation to default for non-selected bones
                boneTransforms[i].localRotation = Quaternion.identity;
                continue;
            }
            // Randomize rotation for each selected bone
            Quaternion randomRotation = Quaternion.Lerp(minRotations[i], maxRotations[i], UnityEngine.Random.value);

            // Apply the random rotation to the bone
            boneTransforms[i].localRotation = randomRotation;
        }
    }
}
