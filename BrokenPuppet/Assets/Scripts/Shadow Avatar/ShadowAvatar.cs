using System;
using System.Linq;
using UnityEngine;

public class ShadowAvatar : MonoBehaviour
{

    public  Animator shadowBody;

    public static Transform[] boneTransforms;
    public static Quaternion[] minRotations;
    public static Quaternion[] maxRotations;
    public static Difficulty GameDifficulty;
    float MaxMin;


    static int AllBones = 51;
    static int[] LeftLeg = { 36, 38, 40, 42 };
    static int[] RightLeg = { 37, 39, 41, 43 };
    static int[] randomBonesSelected = new int[AllBones];

    // Start is called before the first frame update
    void Start()
    {
        GameDifficulty = Randomiser.GameDifficulty;

    }

    // Update is called once per frame
    void Update()
    {
        /* Copy the movement of the avatar */

    }

    static void SelectRandomBones()
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
        // Populate the array with random bone indexes
        int i = 0;
        while (i < NumberOfBones)
        {
            // Randomly select an index to assign the transform
            int r = rnd.Next(AllBones);
            print(r + "\n");
            // Populate array with bone indexes
            // if the index has already been selected
            //if (randomBonesSelected[r] == 1)
            //    continue;
            // if the right leg has already been selected and transformed, dont do the left leg
            //if (LeftLeg.Contains(r) && (randomBonesSelected[37] == 1 || randomBonesSelected[39] == 1 || randomBonesSelected[41] == 1 || randomBonesSelected[43] == 1))
            //    continue;
            //if the left leg has already been selected and transformed, dont do the right leg
            //if (RightLeg.Contains(r) && (randomBonesSelected[36] == 1 || randomBonesSelected[38] == 1 || randomBonesSelected[40] == 1 || randomBonesSelected[42] == 1))
            //    continue;
            randomBonesSelected[r] = 1;
            i++;
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
            if (randomBonesSelected[i] == 0)
            {
                //print("Staying in T-Pose " + i + "Bone Transforms " + boneTransforms[i] + "Random Bones " + randomBonesSelected[i]);
                continue; // Skip this bone if it's null or not selected
            }

            // Get the rotation of the bone from boneTransforms array
            Quaternion newRotation = shadowBody.GetBoneTransform((HumanBodyBones)i).rotation;

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
                    if (newRotation.eulerAngles != Vector3.zero)
                    {
                        currentRotation.SetLookRotation(newRotation.eulerAngles);// *= newRotation;
                        shadowBoneTransform.rotation = currentRotation;
                    }
                }
            }
        }
    }




    //54 bone
    void InitializeRotationLimits()
    {
        minRotations = new Quaternion[AllBones];
        maxRotations = new Quaternion[AllBones];

        int i = 0;
        foreach (var item in Avatar.parentCalibrationData)
        {
            // Check if calibration data exists for the bone
            if (Avatar.parentCalibrationData[item.Key] != null)
            {
                (float, float)[] limits = Avatar.parentCalibrationData[item.Key].getLimit();
                float x1 = limits[0].Item1;
                float x2 = limits[0].Item2;
                float y1 = limits[1].Item1;
                float y2 = limits[1].Item2;

                // Set default Euler angles to 0 if calibration data is not available
                if (x1 == 0 && x2 == 0 && y1 == 0 && y2 == 0)
                {
                    minRotations[i] = Quaternion.identity;
                    maxRotations[i] = Quaternion.identity;
                }
                else
                {
                    // Convert Euler angles to quaternions
                    minRotations[i] = Quaternion.Euler(x1, y1, 0);
                    maxRotations[i] = Quaternion.Euler(x2, y2, 0);
                }
            }
            else
            {
                // Set default quaternions if calibration data is null
                minRotations[i] = Quaternion.identity;
                maxRotations[i] = Quaternion.identity;
            }

            i++;
        }
    }



    void GenerateRandomPose()
    {
        // Loop through all bones
        for (int i = 0; i < AllBones; i++)
        {
            if (minRotations[i] == Quaternion.identity || maxRotations[i] == Quaternion.identity)
                continue;

            // Check if the bone is selected
            if (randomBonesSelected[i] == 1)
            {
                // Randomize rotation for the selected bone
                Quaternion randomRotation = Quaternion.Lerp(minRotations[i].normalized, maxRotations[i].normalized, UnityEngine.Random.value);
                // Apply the random rotation to the bone
                shadowBody.GetBoneTransform((HumanBodyBones)i).rotation = randomRotation;
            }
        }
    }

    public void UpdateShadow()
    {

        SelectRandomBones();
        InitializeRotationLimits();
        GenerateRandomPose();
        MoveModel();
    }

}
