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
            i.Value.update(ref animator, ref server);
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
}
