using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ModelSimilarityChecker : MonoBehaviour
{

    public Avatar BrokenPuppet;
    public ShadowAvatar GhostAvatar;
    Transform[] ShadowCharacterBones;
    Transform[] BrokenPuppetBones;
    Vector3[] PuppetVectors = new Vector3[65];
    Vector3[] GhostVectors = new Vector3[65];
    bool GameEnd = false;
    // Start is called before the first frame update
    void Start()
    {
        BrokenPuppet = getPuppetAvatar();
        GhostAvatar = getShadowAvatar();
        StartCoroutine(Coroutine());
    }
    //Coroutine will run until game ends
    IEnumerator Coroutine()
    {
        while (!GameEnd)
        {
            yield return new WaitForSeconds(2);
            BrokenPuppet = getPuppetAvatar();
            GhostAvatar = getShadowAvatar();
            //Gets the puppets bones
            BrokenPuppetBones = BrokenPuppet.GetComponentInChildren<SkinnedMeshRenderer>().bones;
            //Gets the shadows bones
            ShadowCharacterBones = GhostAvatar.GetComponentInChildren<SkinnedMeshRenderer>().bones;
            GetVectors();
            bool Successful = IsModelNear(PuppetVectors, GhostVectors);
            if (Successful)
                break;
        }
        GameEnd = true;
    }

    //Functions to gathers vectors into an array
    public void GetVectors()
    {
        int i = 0;
        foreach (Transform bone in BrokenPuppetBones)
        {
            //Takes all positions of bones in terms of vectors, and puts them in the array
            PuppetVectors[i] = bone.position;
            i++;
        }
        i = 0;
        foreach (Transform bone in ShadowCharacterBones)
        {
            //Takes all positions of bones in terms of vectors, and puts them in the array

            GhostVectors[i] = bone.position;
            i++;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(GameEnd)
            StopCoroutine(Coroutine());

    }
    //Gets the shadow "ghost" avatar the user has to match
    private ShadowAvatar getShadowAvatar()
    {
        ShadowAvatar avatar = FindObjectOfType<ShadowAvatar>();
        if (avatar == null)
            Debug.LogError("Could not find an Avatar in the scene");
        return avatar;

    }
    //Gets the puppet avatar the user is manipulating
    private Avatar getPuppetAvatar()
    {
        Avatar avatar = FindObjectOfType<Avatar>();
        if (avatar == null)
            Debug.LogError("Could not find an Avatar in the scene");
        return avatar;
    }

    public bool IsModelNear(Vector3[] Puppet, Vector3[] Ghost)
    {
        for(int i = 0; i < Puppet.Length; i++)
        {
            //Takes the distance between the puppet and ghost in terms of vectors
            float distance = Vector3.Distance(Puppet[i], Ghost[i]);
            print($"Puppet vector: {Puppet[i]} Ghost vector: {Ghost[i]} Distance: {distance}");
            //checks if every bone is <0.25 units away from the corresponding ghost one
            if (distance >= 0.25)
                return false;
        }
        return true;
    }
}
