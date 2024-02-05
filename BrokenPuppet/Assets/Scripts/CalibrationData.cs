using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationData 
{
    /* stores what parts of the mesh this data is linked to */
    public HumanBodyBones parent, child;

    /* stores what landmark is being tracked */
    Landmark tchild, tparent;

    public Vector3 initialDirection;
    public Quaternion initialRotation;
    
    public CalibrationData(HumanBodyBones fparent, HumanBodyBones fchild, 
        Landmark tparent, Landmark target, ref Animator animator, ref PipeServer server) {
        this.parent  = fparent;
        this.child = fchild;
        this.tchild = target;
        this.tparent = tparent;
        
        this.initialDirection = getInitialDirection(ref server);
        this.initialRotation = getInitialRotation(ref animator);
    }

    /* The current vector between the   */
    public Vector3 getCurrentDirection(ref PipeServer server) {
        return (server.getLandmark(tchild) - server.getLandmark(tparent)).normalized;
    }

    /* calculates the initial vector */
    private Vector3 getInitialDirection(ref PipeServer server) {
        return (server.getLandmark(tchild) - 
            server.getLandmark(tparent)).normalized;
    }

    private Quaternion getInitialRotation(ref Animator animator) {
        return animator.GetBoneTransform(parent).rotation;
    }

    public void reset(ref Animator animator) {
        initialDirection = Vector3.zero;
    }
}