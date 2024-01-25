using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationData 
{
    /* stores what parts of the mesh this data is linked to */
    HumanBodyBones parent, child;

    /* stores what landmark is being tracked */
    Landmark target, tparent;

    Vector3 initialDirection;
    Quaternion initialRotation;
    
    public CalibrationData(HumanBodyBones fparent, HumanBodyBones fchild, 
        Landmark tparent, Landmark target, ref Animator animator) {
        this.parent  = fparent;
        this.child = fchild;
        this.target = target;
        this.tparent = tparent;
        
        this.initialDirection = getInitialDirection(ref animator);
        this.initialRotation = getInitialRotation(ref animator);
    }

    /* returns the current vector between target and  */
    private Vector3 getCurrentDirection(ref PipeServer server) {
        return (server.getLandmark(target) - server.getLandmark(tparent)).normalized;
    }

    /* calculates the initial vector between  */
    private Vector3 getInitialDirection(ref Animator animator) {
        return (animator.GetBoneTransform(child).position - animator.GetBoneTransform(parent).position).normalized;
    }

    private Quaternion getInitialRotation(ref Animator animator) {
        return animator.GetBoneTransform(parent).rotation;
    }

    public void update(ref Animator animator, ref PipeServer server) {
        Quaternion deltaRotation = Quaternion.FromToRotation(initialDirection, getCurrentDirection(ref server));
        animator.GetBoneTransform(parent).rotation = deltaRotation * initialRotation;
    }
}