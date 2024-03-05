using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Limitations
{

    public static Dictionary<HumanBodyBones, (float, float)> x_hashmap = new();
    public static Dictionary<HumanBodyBones, (float, float)> y_hashmap = new();


    public static void initializeXArray()
    {
        x_hashmap.Clear();
        //X Limitations -- Min, Max
        //Misc
        x_hashmap.Add(HumanBodyBones.LeftEye, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightEye, (0, 0));
        x_hashmap.Add(HumanBodyBones.Head, (0, 0));
        x_hashmap.Add(HumanBodyBones.Jaw, (0, 0));
        x_hashmap.Add(HumanBodyBones.Chest, (0, 0));

        //neck and hip and spine
        x_hashmap.Add(HumanBodyBones.Neck, (-70, 70));
        x_hashmap.Add(HumanBodyBones.Hips, (-70, 70));
        x_hashmap.Add(HumanBodyBones.Spine, (-10, 50));
        //wrists
        x_hashmap.Add(HumanBodyBones.LeftHand, (-15, 15));
        x_hashmap.Add(HumanBodyBones.RightHand, (-15, 15));

        //Left Hand
        x_hashmap.Add(HumanBodyBones.LeftThumbProximal, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftThumbIntermediate, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftThumbDistal, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftIndexProximal, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftIndexIntermediate, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftIndexDistal, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftMiddleProximal, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftMiddleIntermediate, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftMiddleDistal, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftRingProximal, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftRingIntermediate, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftRingDistal, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftLittleProximal, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftLittleIntermediate, (0, 0));
        x_hashmap.Add(HumanBodyBones.LeftLittleDistal, (0, 0));

        //Right hand
        x_hashmap.Add(HumanBodyBones.RightThumbProximal, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightThumbIntermediate, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightThumbDistal, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightIndexProximal, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightIndexIntermediate, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightIndexDistal, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightMiddleProximal, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightMiddleIntermediate, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightMiddleDistal, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightRingProximal, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightRingIntermediate, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightRingDistal, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightLittleProximal, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightLittleIntermediate, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightLittleDistal, (0, 0));

        // Feet -- Ankles
        x_hashmap.Add(HumanBodyBones.LeftFoot, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightFoot, (0, 0));

        //Knees
        x_hashmap.Add(HumanBodyBones.LeftLowerLeg, (0, 0));
        x_hashmap.Add(HumanBodyBones.RightLowerLeg, (0, 0));

        //Hip Joints
        x_hashmap.Add(HumanBodyBones.LeftUpperLeg, (0, 40));
        x_hashmap.Add(HumanBodyBones.RightUpperLeg, (0, 40));

        // elbows
        x_hashmap.Add(HumanBodyBones.LeftLowerArm, (-20, 20));
        x_hashmap.Add(HumanBodyBones.RightLowerArm, (-20, 20));

        //Upper arm???
        x_hashmap.Add(HumanBodyBones.LeftUpperArm, (-90, 90));
        x_hashmap.Add(HumanBodyBones.RightUpperArm, (-90, 90));

        //Shoulders
        x_hashmap.Add(HumanBodyBones.LeftShoulder, (-90, 90));
        x_hashmap.Add(HumanBodyBones.RightShoulder, (-90, 90));

    }

    public static void initializeYArray()
    {

        y_hashmap.Clear();
        //X Limitations -- Min, Max
        //Misc
        y_hashmap.Add(HumanBodyBones.LeftEye, (0, 0));
        y_hashmap.Add(HumanBodyBones.RightEye, (0, 0));
        y_hashmap.Add(HumanBodyBones.Head, (0, 0));
        y_hashmap.Add(HumanBodyBones.Jaw, (0, 0));
        y_hashmap.Add(HumanBodyBones.Chest, (0, 0));
        //neck and hip and spine
        y_hashmap.Add(HumanBodyBones.Neck, (-50, 50));
        y_hashmap.Add(HumanBodyBones.Hips, (-30, 40));
        y_hashmap.Add(HumanBodyBones.Spine, (-10, 50));
        //wrists
        y_hashmap.Add(HumanBodyBones.LeftHand, (-15, 15));
        y_hashmap.Add(HumanBodyBones.RightHand, (-15, 15));

        //Left Hand
        y_hashmap.Add(HumanBodyBones.LeftThumbProximal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftThumbIntermediate, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftThumbDistal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftIndexProximal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftIndexIntermediate, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftIndexDistal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftMiddleProximal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftMiddleIntermediate, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftMiddleDistal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftRingProximal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftRingIntermediate, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftRingDistal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftLittleProximal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftLittleIntermediate, (-30, 0));
        y_hashmap.Add(HumanBodyBones.LeftLittleDistal, (-30, 0));

        //Right hand
        y_hashmap.Add(HumanBodyBones.RightThumbProximal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightThumbIntermediate, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightThumbDistal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightIndexProximal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightIndexIntermediate, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightIndexDistal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightMiddleProximal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightMiddleIntermediate, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightMiddleDistal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightRingProximal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightRingIntermediate, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightRingDistal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightLittleProximal, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightLittleIntermediate, (-30, 0));
        y_hashmap.Add(HumanBodyBones.RightLittleDistal, (-30, 0));

        // Feet -- Ankles
        y_hashmap.Add(HumanBodyBones.LeftFoot, (-80, 60));
        y_hashmap.Add(HumanBodyBones.RightFoot, (-80, 60));

        //Knees
        y_hashmap.Add(HumanBodyBones.LeftLowerLeg, (-90, 0));
        y_hashmap.Add(HumanBodyBones.RightLowerLeg, (-90, 0));

        //Hip Joints
        y_hashmap.Add(HumanBodyBones.LeftUpperLeg, (-90, 90));
        y_hashmap.Add(HumanBodyBones.RightUpperLeg, (-90, 90));

        // elbows
        y_hashmap.Add(HumanBodyBones.LeftLowerArm, (0, 110));
        y_hashmap.Add(HumanBodyBones.RightLowerArm, (0, 110));

        //Upper arm???
        y_hashmap.Add(HumanBodyBones.LeftUpperArm, (0, 180));
        y_hashmap.Add(HumanBodyBones.RightUpperArm, (0, 180));

        //Shoulders
        y_hashmap.Add(HumanBodyBones.LeftShoulder, (0, 180));
        y_hashmap.Add(HumanBodyBones.RightShoulder, (0, 180));

    }

    /* returns x and y limit values in an array for given bone */
    public static (float, float) getXLimit(HumanBodyBones bone) { 
        if (!x_hashmap.ContainsKey(bone))
            return (0,0);

        return x_hashmap[bone]; 
    }

    public static (float, float) getYLimit(HumanBodyBones bone) { 
        if (!y_hashmap.ContainsKey(bone))
            return (0,0);
        return y_hashmap[bone]; 
    }
}
