using System;
using System.Collections;
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
    }
    //Coroutine will run until game ends
    IEnumerator Coroutine()
    {
        yield return new WaitForSeconds(2);
        BrokenPuppet = getPuppetAvatar();
        GhostAvatar = getShadowAvatar();
        //Gets the puppets bones
        BrokenPuppetBones = BrokenPuppet.GetComponentInChildren<SkinnedMeshRenderer>().bones;
        //print(BrokenPuppetBones.Length);
        //Gets the shadows bones
        ShadowCharacterBones = GhostAvatar.GetComponentInChildren<SkinnedMeshRenderer>().bones;
        //print(ShadowCharacterBones.Length);
        GetVectors();
        bool Successful = IsModelNear(PuppetVectors, GhostVectors);
        if (Successful)
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
        StartCoroutine(Coroutine());
        if(!GameEnd)
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

    public Boolean IsModelNear(Vector3[] Puppet, Vector3[] Ghost)
    {
        int count = 0;
        for(int i = 0; i < Puppet.Length; i++)
        {
            //Takes the distance between the puppet and ghost in terms of vectors
            decimal distance = Math.Round((decimal)Vector3.Distance(Puppet[i], Ghost[i]), 2);
            print("Puppet vector: " + Puppet[i] + "Ghost vector: " + Ghost[i] + "Distance: " + distance);
            //checks if every bone is <0.25 units away from the corresponding ghost one
            if ((float) distance <= 0.25)
            { 
                //Incremeents the count so we know if all the bones match
                count++;
            }
        }
        //If all bones match, returns true
        if (count == 65)
            return true;
        else
            return false;
    }
}
