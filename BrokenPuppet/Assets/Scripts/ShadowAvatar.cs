using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShadowAvatar : MonoBehaviour { 

    public GameObject shadow;
    public Animator shadowBody;

    public Transform[] boneTransforms;
    public Quaternion[] minRotations;
    public Quaternion[] maxRotations;
    public Difficulty GameDifficulty;
    float MaxMin;


    static int AllBones = 50;

    static int[] randomBonesSelected = new int[AllBones];

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
            NumberOfBones = 4;
        else if ((int)GameDifficulty == 2)
            NumberOfBones = 8;
        else
            NumberOfBones = 12;
        print(NumberOfBones + " " + boneTransforms.Length);
        // Populate the array with random bone indexes
        for (int i = 0; i < NumberOfBones; i++)
        {
            // Randomly select an index to assign the transform
            int r = rnd.Next(boneTransforms.Length);
            print(r + "\n");
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
        for (int i = 0; i < randomBonesSelected.Length; i++)
        {
            // Check if the boneTransforms array is null or if the bone is not selected
            if (boneTransforms[i] == null || randomBonesSelected[i] == 0)
            {
                print("Staying in T-Pose " + i);
                continue; // Skip this bone if it's null or not selected
            }

            // Get the rotation of the bone from boneTransforms array
            Quaternion newRotation = boneTransforms[i].localRotation;

            // Apply the new rotation to the bone transform in the boneTransforms array
            //boneTransforms[i].rotation = newRotation;

            // Convert the integer index to a HumanBodyBones enum value
            HumanBodyBones boneType = (HumanBodyBones)i;

            // Update the rotation of the corresponding bone in shadowBody if it exists
            if (shadowBody != null)
            {
                Transform shadowBoneTransform = shadowBody.GetBoneTransform(boneType);
                if (shadowBoneTransform != null)
                {
                    //Quaternion.RotateTowards(boneTransforms[i].rotation, newRotation, MaxMin);

                    Quaternion currentRotation = shadowBoneTransform.localRotation;
                    currentRotation.SetLookRotation(newRotation.eulerAngles);// *= newRotation;
                    shadowBoneTransform.rotation = currentRotation;
                }
            }
        }
    }




    //54 bone
    void InitializeRotationLimits()
    {

        Limitations.initializeXArray();
        Limitations.initializeYArray();

        minRotations = new Quaternion[AllBones];
        maxRotations = new Quaternion[AllBones];
        if ((int)GameDifficulty == 1)
            MaxMin = 10;
        else if ((int)GameDifficulty == 2)
            MaxMin = 20;
        else
            MaxMin = 30;
        // Define min and max rotations for each selected bone
        for (int i = 0; i < randomBonesSelected.Length; i++)
        {
            if (randomBonesSelected[i] == 0 && boneTransforms[i] == null) // Check if the bone is not selected or if it's null
                continue;
            (float, float) x = Limitations.getXIndex(i);
            (float, float) y = Limitations.getYIndex(i);
            float x1 = x.Item1;
            float x2 = x.Item2; 
            float y1 = y.Item1;
            float y2 = y.Item2;
            if (x1 == 0)
                x1 = Quaternion.identity.x;
            if(x2 == 0)
                x2 = Quaternion.identity.x;
            if (y1 == 0)
                y1 = Quaternion.identity.y;
            if (y2 == 0)
                y2 = Quaternion.identity.y;
            
            minRotations[i] = Quaternion.Euler(x1, y1, -MaxMin);
            maxRotations[i] = Quaternion.Euler(x2, y2, MaxMin);
        }
    }


    void GenerateRandomPose()
    {
        // Loop through all bones
        for (int i = 0; i < boneTransforms.Length; i++)
        {
            // Check if the bone is selected and not null
            if (randomBonesSelected[i] == 1 && boneTransforms[i] != null)
            {
                // Randomize rotation for the selected bone
                Quaternion randomRotation = Quaternion.Lerp(minRotations[i], maxRotations[i], UnityEngine.Random.value);
                // Apply the random rotation to the bone
                boneTransforms[i].localRotation = randomRotation;
            }
        }
    }

}
