using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationData 
{
    public HumanBodyBones parentBone, childBone;
    public Landmark parentLandmark, childLandmark;

    private Vector3 oInitialDirection;
    private Quaternion oInitialRotation;

    private (float, float) x_limit;
    private (float, float) y_limit;

    public Transform parent, child, tparent, tchild;

    public Vector3 initialDirection;
    public Quaternion initialRotation;
    
    public CalibrationData(HumanBodyBones parentBone, HumanBodyBones childBone, 
        Landmark parentLandmark, Landmark childLandmark, ref Animator animator, ref OSCServer server) {

        this.parentBone = parentBone;
        this.childBone = childBone;
        this.parentLandmark = parentLandmark;
        this.childLandmark = childLandmark;

        getBoneTransforms(ref animator);
        tchild = server.getLandmark(parentLandmark);
        tparent = server.getLandmark(childLandmark);

        setInitialRotAndDir();
    }

    public CalibrationData(HumanBodyBones parentBone, HumanBodyBones childBone, Transform tparent, 
        Transform tchild, ref Animator animator, ref OSCServer server) {
        this.parentBone = parentBone;
        this.childBone = childBone;

        getBoneTransforms(ref animator);
        this.tparent = tparent;
        this.tchild = tchild;

        setInitialRotAndDir();
    }

    private void setLimits()
    {
        x_limit = Limitations.getXLimit(this.parentBone);
        y_limit = Limitations.getYLimit(this.parentBone);
    }

    public (float, float)[] getLimit()
    {
        return new (float, float)[] { x_limit, y_limit };
    }


    private void getBoneTransforms(ref Animator animator)
    {
        parent = animator.GetBoneTransform(parentBone);
        child = animator.GetBoneTransform(childBone);
    }

    private void setInitialRotAndDir()
    {
        initialRotation = getInitialRotation();
        initialDirection = getCurrentDirection();
        oInitialDirection = initialDirection;
        oInitialRotation = initialRotation;
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