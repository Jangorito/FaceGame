using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Controls the movement of the character */
public class Avatar : MonoBehaviour
{

    private PipeServer server;
    public Animator animator;

    public Camera cam;

    private Dictionary<HumanBodyBones, CalibrationData> parentCalibrationData = 
        new Dictionary<HumanBodyBones, CalibrationData>();

    private CalibrationData spineUpDown, hipsTwist, chest, head;

    void Start()
    {
        server = getServer();
        StartCoroutine(Calibrate());
    }

    void Update()
    {
        /* Allow Player to re-calibrate */
        if (Input.GetKeyDown("space")) {
            Debug.Log("Re-Calibrating...");
            StartCoroutine(Calibrate());
        }

        /* Moves the model */
       foreach(var i in parentCalibrationData)
        {
            Quaternion deltaRotation = Quaternion.FromToRotation(i.Value.initialDirection,
            i.Value.getCurrentDirection(ref server));

            animator.GetBoneTransform(i.Key).rotation = deltaRotation * i.Value.initialRotation;
        }

        /* Apply motion to neck and hips */
    }



    /* attemps to find the active PipeServer to gain access to data */
    private PipeServer getServer() {
        PipeServer server = FindObjectOfType<PipeServer>();
        if (server == null)
                Debug.LogError("Could not find a PipeServer in the scene");
        return server;
        
    }

    /* returns the avatar to the base pose */
    void resetAvatar() {
        foreach (var i in parentCalibrationData) {
            i.Value.reset(ref animator);
        }
    }

    /* Changes the movement between the parent and child bone to match the movement between the newParent and newChild landmark data */
    void changeCalibration(HumanBodyBones parent, Landmark newParent, Landmark newChild) {
        HumanBodyBones child = parentCalibrationData[parent].child;
        parentCalibrationData.Remove(parent);
        AddCalibration(parent, child, newParent, newChild);
    }

    /* Sets up Mappings between Unity Bones and The Landmarks */
    public IEnumerator Calibrate() {

        resetAvatar();

        /* waits t seconds */
        int t = 5;

        while (t > 0) {
            Debug.Log("Calibrating in: "+t);
            t--;
            yield return new WaitForSeconds(1f);
        }


        /* resets the calibration data */
        parentCalibrationData.Clear();

        addLeftHandCalibrations();
        addRightHandCalibrations();

        AddCalibration(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm,
            Landmark.RIGHT_SHOULDER, Landmark.RIGHT_ELBOW);

        AddCalibration(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm,
            Landmark.LEFT_SHOULDER, Landmark.LEFT_ELBOW);

        AddCalibration(HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
            Landmark.RIGHT_ELBOW, Landmark.RIGHT_WRIST);

        AddCalibration(HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
            Landmark.LEFT_ELBOW, Landmark.LEFT_WRIST);

        AddCalibration(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg,
            Landmark.LEFT_HIP, Landmark.LEFT_KNEE);

        AddCalibration(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg,
            Landmark.RIGHT_HIP, Landmark.RIGHT_KNEE);

        AddCalibration(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
            Landmark.LEFT_KNEE, Landmark.LEFT_ANKLE);

        AddCalibration(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot,
            Landmark.RIGHT_KNEE, Landmark.RIGHT_ANKLE);

        /* Manually define neck and hip connections */
        spineUpDown = new CalibrationData(HumanBodyBones.Spine, HumanBodyBones.Neck,
            Landmark.VHIP,  Landmark.VNECK, ref animator, ref server);
        hipsTwist = new CalibrationData(HumanBodyBones.Hips, HumanBodyBones.Hips,
            Landmark.RIGHT_HIP, Landmark.LEFT_HIP, ref animator, ref server);
        chest = new CalibrationData(HumanBodyBones.Chest, HumanBodyBones.Chest,
            Landmark.RIGHT_HIP, Landmark.LEFT_HIP, ref animator, ref server);
        head = new CalibrationData(HumanBodyBones.Neck, HumanBodyBones.Head,
            Landmark.VNECK, Landmark.NOSE, ref animator, ref server);

        Debug.Log("Calibrated");
    }

    public Transform getBoneTransform(HumanBodyBones bone) {
        return animator.GetBoneTransform(bone);
    }

    private void AddCalibration(HumanBodyBones parent, HumanBodyBones child, 
        Landmark trackParent, Landmark trackChild) {
        CalibrationData data = new CalibrationData(parent, child, 
                trackParent, trackChild, ref animator, ref server);
        parentCalibrationData.Add(parent, data);
    }

    private void addLeftHandCalibrations() {

        /* Thumb */
        AddCalibration(HumanBodyBones.LeftHand, HumanBodyBones.LeftThumbProximal,
            Landmark.LEFT_WRIST_ALT, Landmark.LEFT_THUMB_1);

        AddCalibration(HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate,
            Landmark.LEFT_THUMB_1, Landmark.LEFT_THUMB_2);

        AddCalibration(HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal,
            Landmark.LEFT_THUMB_2, Landmark.LEFT_THUMB_4);

        /* Index */
        //AddCalibration(HumanBodyBones.LeftHand, HumanBodyBones.LeftIndexProximal,
        //    Landmark.LEFT_WRIST_ALT, Landmark.LEFT_INDEX_1);

        AddCalibration(HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate,
            Landmark.LEFT_INDEX_1, Landmark.LEFT_INDEX_2);

        AddCalibration(HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal,
            Landmark.LEFT_INDEX_2, Landmark.LEFT_INDEX_4);

        /* Middle */
       // AddCalibration(HumanBodyBones.LeftHand, HumanBodyBones.LeftMiddleProximal,
           // Landmark.LEFT_WRIST_ALT, Landmark.LEFT_MIDDLE_1);

        AddCalibration(HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate,
            Landmark.LEFT_MIDDLE_1, Landmark.LEFT_MIDDLE_2);

        AddCalibration(HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal,
            Landmark.LEFT_MIDDLE_2, Landmark.LEFT_MIDDLE_4);

        /* Little */
       // AddCalibration(HumanBodyBones.LeftHand, HumanBodyBones.LeftLittleProximal,
            //Landmark.LEFT_WRIST_ALT, Landmark.LEFT_LITTLE_1);

        AddCalibration(HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate,
            Landmark.LEFT_LITTLE_1, Landmark.LEFT_LITTLE_2);

        AddCalibration(HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal,
            Landmark.LEFT_LITTLE_2, Landmark.LEFT_LITTLE_4);


    }

    private void addRightHandCalibrations() {

        /* Thumb */
        AddCalibration(HumanBodyBones.RightHand, HumanBodyBones.RightThumbProximal,
            Landmark.RIGHT_WRIST_ALT, Landmark.RIGHT_THUMB_1);

        AddCalibration(HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate,
            Landmark.RIGHT_THUMB_1, Landmark.RIGHT_THUMB_2);

        AddCalibration(HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal,
            Landmark.RIGHT_THUMB_2, Landmark.RIGHT_THUMB_4);

        /* Index */
      //  AddCalibration(HumanBodyBones.LeftHand, HumanBodyBones.RightIndexProximal,
        //    Landmark.RIGHT_WRIST_ALT, Landmark.RIGHT_INDEX_1);

        AddCalibration(HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate,
            Landmark.RIGHT_INDEX_1, Landmark.RIGHT_INDEX_2);

        AddCalibration(HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal,
            Landmark.RIGHT_INDEX_2, Landmark.RIGHT_INDEX_4);

        /* Middle */
      //  AddCalibration(HumanBodyBones.RightHand, HumanBodyBones.RightMiddleProximal,
       //     Landmark.RIGHT_WRIST_ALT, Landmark.RIGHT_MIDDLE_1);

        AddCalibration(HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate,
            Landmark.RIGHT_MIDDLE_1, Landmark.RIGHT_MIDDLE_2);

        AddCalibration(HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal,
            Landmark.RIGHT_MIDDLE_2, Landmark.RIGHT_MIDDLE_4);

        /* Little */
      //  AddCalibration(HumanBodyBones.RightHand, HumanBodyBones.RightLittleProximal,
       //     Landmark.RIGHT_WRIST_ALT, Landmark.RIGHT_LITTLE_1);

        AddCalibration(HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate,
            Landmark.RIGHT_LITTLE_1, Landmark.RIGHT_LITTLE_2);

        AddCalibration(HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal,
            Landmark.RIGHT_LITTLE_2, Landmark.RIGHT_LITTLE_4);

    }

}
