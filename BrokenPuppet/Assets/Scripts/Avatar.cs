using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avatar : MonoBehaviour
{

    private PipeServer server;
    public Animator animator;

    public Camera cam;

    private Dictionary<HumanBodyBones, CalibrationData> parentCalibrationData = 
        new Dictionary<HumanBodyBones, CalibrationData>();


    // Start is called before the first frame update
    void Start()
    {
        server = FindObjectOfType<PipeServer>();
        if (server == null)
            Debug.LogError("Could not find a PipeServer in the scene");

        Calibrate();

    }

    // Update is called once per frame
    void Update()
    {
        /* Moves the model */
       foreach(var i in parentCalibrationData)
        {
            Quaternion deltaRotTracked = Quaternion.FromToRotation(
                i.Value.initialDirection, getCurrentDirection(i.Value.tchild, i.Value.tparent));
            animator.GetBoneTransform(i.Value.parent).rotation = (deltaRotTracked * i.Value.initialRotation);
        }

       // The tracking of the camera.
        if (cam)
        {
            Quaternion q = Quaternion.LookRotation((animator.GetBoneTransform(HumanBodyBones.Chest).transform.position - cam.transform.position).normalized, Vector3.up);
            cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, q, Time.deltaTime * 3f);
        }
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
                trackParent, trackChild);
        data.initialDirection = getInitialDirection(child, parent);
        data.initialRotation = getInitialRotation(parent);

        parentCalibrationData.Add(parent, data);
    }

    /* Calculates the first vector between the two target bones */
    private Vector3 getInitialDirection(HumanBodyBones child, HumanBodyBones parent) {
        return (animator.GetBoneTransform(child).position - 
            animator.GetBoneTransform(parent).position).normalized;
    }

    private Quaternion getInitialRotation(HumanBodyBones parent) {
        Quaternion rotation = animator.GetBoneTransform(parent).rotation;
        return rotation;
    }


    private Vector3 getCurrentDirection(Landmark child, Landmark parent) {
        return (server.getLandmark(child) - server.getLandmark(parent)).normalized;
    }


    /* Will store the mapping between a Bone and the Transform Data from PipeServer */
    class CalibrationData {
        public HumanBodyBones parent, child;
        public Landmark tparent, tchild;

        public Quaternion initialRotation;
        public Vector3 initialDirection;

        public CalibrationData(HumanBodyBones fparent, HumanBodyBones fchild, 
            Landmark tparent, Landmark tchild) {       
            this.parent = fparent;
            this.child = fchild;
            this.tparent = tparent;
            this.tchild = tchild;
        }
    }
}
