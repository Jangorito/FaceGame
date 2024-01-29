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


    void Start()
    {
        server = getServer();
        Calibrate();
    }

    void Update()
    {
        /* Moves the model */
       foreach(var i in parentCalibrationData)
        {
            i.Value.update(ref animator, ref server);
        }
    }

    /* attemps to find the active PipeServer to gain access to data */
    private PipeServer getServer() {
        PipeServer server = FindObjectOfType<PipeServer>();
        if (server == null)
                Debug.LogError("Could not find a PipeServer in the scene");
        return server;
        
    }


    /* Sets up Mappings between Unity Bones and The Landmarks */
    public void Calibrate() {

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
    }

    private void AddCalibration(HumanBodyBones parent, HumanBodyBones child, 
        Landmark trackParent, Landmark trackChild) {
        CalibrationData data = new CalibrationData(parent, child, 
                trackParent, trackChild, ref animator);
        parentCalibrationData.Add(parent, data);
    }
}
