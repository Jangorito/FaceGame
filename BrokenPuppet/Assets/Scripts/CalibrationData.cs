using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationData 
{
    public HumanBodyBones parentBone, childBone;
    public Landmark parentLandmark, childLandmark;

    private Vector3 oInitialDirection;
    private Quaternion oInitialRotation;

    public Transform parent, child, tparent, tchild;

    public Vector3 initialDirection;
    public Quaternion initialRotation;
    
    public CalibrationData(HumanBodyBones parentBone, HumanBodyBones childBone, 
        Landmark parentLandmark, Landmark childLandmark, ref Animator animator, ref PipeServer server) {

        this.parentBone = parentBone;
        this.childBone = childBone;
        this.parentLandmark = parentLandmark;
        this.childLandmark = childLandmark;

        parent  = animator.GetBoneTransform(parentBone);
        child = animator.GetBoneTransform(childBone);
        tchild = server.getLandmark(parentLandmark);
        tparent = server.getLandmark(childLandmark);
        
        initialDirection = getCurrentDirection();
        initialRotation = getInitialRotation();
        oInitialDirection = initialDirection;
        oInitialRotation = initialRotation;
    }

    public CalibrationData(Transform fparent, Transform fchild, Transform tparent, Transform tchild, ref PipeServer server) {
        parent = fparent;
        child = fchild;
        this.tparent = tparent;
        this.tchild = tchild;

        initialRotation = getInitialRotation();
        initialDirection = getCurrentDirection();
    }

    public Quaternion targetRotation;
    public void Tick(Quaternion newTarget, float speed) {
        parent.rotation = newTarget;
        parent.rotation = Quaternion.Lerp(parent.rotation, targetRotation, Time.deltaTime * speed);
    }

    private Quaternion getInitialRotation() {
        return parent.rotation;
    }

    /* get the vector between the parent and child bones  */
    public Vector3 getCurrentDirection() {
        return (tchild.position - tparent.position).normalized;
    }

    public void reset() {
        initialDirection = oInitialDirection;
        initialRotation = oInitialRotation;
    }
}