using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Landmarks;

/* Controls the movement of the character */
public class Avatar : MonoBehaviour
{

    public int iClientID;

    // The reference to the animator controlling the Avatar transforms
    public Animator animator;

    /** mappings for Bones and its the landmarks it is following */
    public Dictionary<HumanBodyBones, CalibrationData> parentCalibrationData =  new();
    
    private AvatarBody m_AvatarBody;



    // The Avatar's initial rotation
    private Quaternion initialRotation;

    // The Avatar's initial position
    private Vector3 initialPosition;

    // The target rotation for the hips
    private Quaternion targetRot;
    private CalibrationData spineUpDown, hipsTwist, chest, head;

    // Flags if the avatar has been calibrated
    private bool bIsCalibrated = false;

    // Flags if the avatar is calibrated
    private bool bIsCalibrating = false;

    // frequency at which the avatar checks for data on the server
    private const float WAIT_FOR = 2.0f;

    // Flags if script should output debugging info
    public bool shouldDebug = false;

    private Quaternion[] initialRotations;

    // Used to display debugging info
    Logger logger;

    // TODO: separate the shadow implementation from Avatar
    public ShadowAvatar shadow;

    private void Awake()
    {
        logger = new(shouldDebug);

        m_AvatarBody = new AvatarBody(getClient());

        // Initialises all of the joint limitations
        Limitations.initializeXArray();
        Limitations.initializeYArray();

        // Sets the initial rotation and position of the Avatar
        initialRotation = transform.rotation;
        initialPosition = transform.position;
    }

    private void Start()
    {
        initialRotations = new Quaternion[(int)HumanBodyBones.LastBone];
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++) {
            if (animator.GetBoneTransform((HumanBodyBones)i) != null) {
                initialRotations[i] = animator.GetBoneTransform((HumanBodyBones)i).localRotation;
            }
            else {
                initialRotations[i] = Quaternion.identity;
            }
        }
    }

    public Transform getBoneTransform(HumanBodyBones bone) {
        return animator.GetBoneTransform(bone);
    }

    private int getClient() { return iClientID; }

    public void change_calibrations(Dictionary<HumanBodyBones, CalibrationData> movement) {
        parentCalibrationData = movement;
    }

    public Dictionary<HumanBodyBones, CalibrationData> getCalibrations() {
        return parentCalibrationData;
    }

    // Moves the bone
    public void updateBoneTransform(HumanBodyBones bone, CalibrationData calibration) {
        // Get rotation of bone
        Quaternion deltaRotation = calibration.getRotation();

        /** 
         If the rotation is the same assume no input detected:
            Do not move bone - should move with parent
         */
        if (deltaRotation == animator.GetBoneTransform(bone).rotation)
            return;
            

        // Apply calculated rotation
        animator.GetBoneTransform(bone).rotation = deltaRotation;
    }

    public AvatarBody getAvatarBody() { return m_AvatarBody; }

    public Boolean StopAvatarMoving = false;

    void Update()
    {
        m_AvatarBody.updateBody();

        // Do nothing if not connected
        if (!getAvatarBody().IsConnected())
            return;

        // Do nothing if currently calibrating
        if (IsCalibrating())
            return;

        // Calibrate if not calibrated
        if (!IsCalibrated()) {
            StartCoroutine(Calibrate());
            return;
        }

        // Allows player to re-calibrate the avatar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Move Avatar back into T-pose
            resetAvatar();

            SetIsCalibrated(false);

            // Calibrate the Avatar
            StartCoroutine(Calibrate());
        }

        // Moves each joint in the Calibration Data
        foreach (var i in parentCalibrationData)
        {
           updateBoneTransform(i.Key, i.Value);
        }

        // Moves the head, hips, and spine to match movement in body

        /* calculate new rotations */
        Quaternion headr = Quaternion.FromToRotation(head.initialDirection, head.getCurrentDirection());
        Quaternion twist = Quaternion.FromToRotation(hipsTwist.initialDirection,
            Vector3.Slerp(hipsTwist.initialDirection, hipsTwist.getCurrentDirection(), .25f));
        Quaternion updown = Quaternion.FromToRotation(spineUpDown.initialDirection,
            Vector3.Slerp(spineUpDown.initialDirection, spineUpDown.getCurrentDirection(), .25f));

        // Compute the final rotations.
        Quaternion h = updown * updown * updown * twist * twist;
        Quaternion s = h * twist * updown;
        Quaternion c = s * twist * twist;
        float speed = 15f;
        hipsTwist.Tick(h * hipsTwist.initialRotation, speed);
        spineUpDown.Tick(s * spineUpDown.initialRotation, speed);
        chest.Tick(c * chest.initialRotation, speed);
        head.Tick(updown * twist * headr * head.initialRotation, speed);

        // For additional responsiveness, we rotate the entire transform slightly based on the hips.
        Vector3 d = Vector3.Slerp(hipsTwist.initialDirection, hipsTwist.getCurrentDirection(), .25f);
        d.y *= 0.5f;
        Quaternion deltaRotTracked = Quaternion.FromToRotation(hipsTwist.initialDirection, d);
        targetRot = deltaRotTracked * initialRotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * speed);
        if (StopAvatarMoving)
        {
            return;
        }
    }

    /* returns the avatar to the base pose */
    public void resetAvatar()
    {
        logger.LogMsg("Avatar::resetAvatar | resetting avatar");
        transform.rotation = initialRotation;
        hipsTwist.reset(ref animator);
        spineUpDown.reset(ref animator);
        chest.reset(ref animator);
        head.reset(ref animator);
        foreach (var i in parentCalibrationData)
        {
            i.Value.reset(ref animator);
        }
    }

    /* Sets up Mappings between Unity Bones and The Landmarks */
    public IEnumerator Calibrate()
    {
        SetIsCalibrating(true);

        // Gives some time to allow for player to assume T-Pose
        int t = 5;
        while (t > 0)
        {
            logger.LogMsg("Avatar::Calibrate | Calibrating in: " + t.ToString());
            t--;
            yield return new WaitForSeconds(1f);
        }

        logger.LogMsg("Avatar::Calibrate | Starting Calibration");

        // Empty all Calibrations from the previous Calibration Data
        parentCalibrationData.Clear();


        // ==== CALIBRATIONS ====

        AddPoseCalibrations();
        //AddLeftHandCalibrations();
        //AddRightHandCalibrations();
        

        /* Manually define neck and hip connections */
        spineUpDown = new CalibrationData(
            HumanBodyBones.Spine, HumanBodyBones.Neck,
            m_AvatarBody.GetVirtualHip(), m_AvatarBody.GetVirtualNeck()
          , ref animator, ref m_AvatarBody);
        hipsTwist = new CalibrationData( 
            HumanBodyBones.Hips, HumanBodyBones.Hips,
            Landmark.RIGHT_HIP, Landmark.LEFT_HIP, ref animator, ref m_AvatarBody);
        chest = new CalibrationData(
            HumanBodyBones.Chest, HumanBodyBones.Chest,
            Landmark.RIGHT_HIP, Landmark.LEFT_HIP, ref animator, ref m_AvatarBody);
        head = new CalibrationData(
            HumanBodyBones.Neck, HumanBodyBones.Head,
            m_AvatarBody.GetVirtualNeck(), m_AvatarBody.GetVirtualHip(), ref animator, ref m_AvatarBody);

        logger.LogMsg("Avatar::Calibrate | Finished Calibration");
        SetIsCalibrated(true);
        SetIsCalibrating(false);
    }

    private void SetIsCalibrated(bool val) { bIsCalibrated = val; }
    public bool IsCalibrated() { return bIsCalibrated; }

    private void SetIsCalibrating(bool val) { bIsCalibrating = val; }
    private bool IsCalibrating() { return bIsCalibrating; } 

    private void AddCalibration(HumanBodyBones parent, HumanBodyBones child,
        Landmark trackParent, Landmark trackChild)
    {
        CalibrationData data = new(parent, child,
                trackParent, trackChild, ref animator, ref m_AvatarBody);
        parentCalibrationData.Add(parent, data);
    }

    private void AddPoseCalibrations()
    {
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
    }

    private void AddLeftHandCalibrations()
    {

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

    private void AddRightHandCalibrations()
    {

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
