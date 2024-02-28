using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class ModelSimilarityChecker : MonoBehaviour
{

    public Avatar BrokenPuppet;
    public ShadowAvatar GhostAvatar;
    Transform[] ShadowCharacterBones;
    Transform[] BrokenPuppetBones;
    Vector3[] PuppetVectors = new Vector3[65];
    Vector3[] GhostVectors = new Vector3[65];
    // Start is called before the first frame update
    void Start()
    {
        BrokenPuppet = getPuppetAvatar();
        GhostAvatar = getShadowAvatar();
    }

    IEnumerator Coroutine()
    {
        yield return new WaitForSeconds(2);
        BrokenPuppet = getPuppetAvatar();
        GhostAvatar = getShadowAvatar();

        BrokenPuppetBones = BrokenPuppet.GetComponentInChildren<SkinnedMeshRenderer>().bones;
        //print(BrokenPuppetBones.Length);
        ShadowCharacterBones = GhostAvatar.GetComponentInChildren<SkinnedMeshRenderer>().bones;
        //print(ShadowCharacterBones.Length);
        GetVectors();
    }


    public void GetVectors()
    {
        int i = 0;
        foreach (Transform bone in BrokenPuppetBones)
        {
            PuppetVectors[i] = bone.position;
            i++;
        }
        i = 0;
        foreach (Transform bone in ShadowCharacterBones)
        {
            GhostVectors[i] = bone.position;
            i++;
        }
        bool Successful = IsModelNear(PuppetVectors, GhostVectors);
        print(Successful);
    }
    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Coroutine());

    }

    private ShadowAvatar getShadowAvatar()
    {
        ShadowAvatar avatar = FindObjectOfType<ShadowAvatar>();
        if (avatar == null)
            Debug.LogError("Could not find an Avatar in the scene");
        return avatar;

    }
    private Avatar getPuppetAvatar()
    {
        Avatar avatar = FindObjectOfType<Avatar>();
        if (avatar == null)
            Debug.LogError("Could not find an Avatar in the scene");
        return avatar;
    }

    public Boolean IsModelNear(Vector3[] Puppet, Vector3[] Ghost)
    {
        int count = 0;
        for(int i = 0; i < Puppet.Length; i++)
        {
            decimal distance = Math.Round((decimal)Vector3.Distance(Puppet[i], Ghost[i]), 2);
            print("Puppet vector: " + Puppet[i] + "Ghost vector: " + Ghost[i] + "Distance: " + distance);

            if ((float) distance <= 0.25)
            { 
                count++;
            }
        }
        if (count == 65)
            return true;
        else
            return false;
    }
}
