using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccumulatedBuffer
{
    private Vector3 value;
    private int samples;
    private const int samplesForPose = 1;

    public AccumulatedBuffer() {
        value = Vector3.zero;
        samples = 0;
    }

    public void addValue(Vector3 vec3) {
        value += vec3;
        samples++;
    }

    public void clearBuffer() {
        value = new Vector3();
        samples = 0;
    }

    public int getSamples() {
        return samples;
    }

    public bool enoughSamplesRecorded() {
        return samples > samplesForPose;
    }
    
    public void resetSamples() {
        samples = 0;
    }

    public Vector3 getBuffer() {
        Vector3 position = value / samples;
        clearBuffer();
        return position;
    }
}
