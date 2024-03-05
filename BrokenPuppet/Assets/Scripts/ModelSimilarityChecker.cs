using System;
using System.Collections;
using System.Timers;
using UnityEditor;
using UnityEngine;

public class ModelSimilarityChecker : MonoBehaviour
{
    public Avatar BrokenPuppet;
    public ShadowAvatar GhostAvatar;
    private Transform[] ShadowCharacterBones;
    private Transform[] BrokenPuppetBones;
    private Vector3[] PuppetVectors = new Vector3[65];
    private Vector3[] GhostVectors = new Vector3[65];
    private static bool GameEnd = false;
    static bool Successful;

    // Start is called before the first frame update
    private void Start()
    {
        BrokenPuppet = getPuppetAvatar();
        GhostAvatar = getShadowAvatar();
        Successful = false;
        //StartCoroutine(Coroutine());
        StartTimer();
    }

    private void StartTimer()
    {
        Timer initialTimer = new Timer();
        initialTimer.Interval = 8000;
        initialTimer.Elapsed += InitialEvent;
        Debug.Log("Initial 8 Seconds started");
        initialTimer.Start();
    }
    private void InitialEvent(object source, ElapsedEventArgs e)
    {
        Timer timer = new Timer();
        timer.Interval = 2000; // 2 seconds
        timer.Elapsed += OnTimedEvent;
        timer.Enabled = true;
        timer.Start();

        Debug.Log("Timer started");

        ((Timer)source).Stop();
        ((Timer)source).Dispose();
        Debug.Log("Initial Timer Finished");
        return;
    }

    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        //if (IsUnityPaused())
        //{
        //    Debug.Log("Unity is paused. Stopping the timer.");
        //    ((Timer)source).Stop(); // Stop the timer
        //    return;
        //}
        if (Successful)
        {
            // Stop the timer
            ((Timer)source).Stop();
            ((Timer)source).Dispose(); // Dispose the timer to release resources
            Debug.Log("Timer stopped.");
            return;
        }
        Debug.Log("Checking Models");

        // Perform actions every 2 seconds
        BrokenPuppet = getPuppetAvatar();
        GhostAvatar = getShadowAvatar();
        //Gets the puppets bones
        BrokenPuppetBones = BrokenPuppet.GetComponentInChildren<SkinnedMeshRenderer>().bones;
        //Gets the shadows bones
        ShadowCharacterBones = GhostAvatar.GetComponentInChildren<SkinnedMeshRenderer>().bones;
        GetVectors();
        Successful = IsModelNear(PuppetVectors, GhostVectors);  
    }

    private bool IsUnityPaused()
    {
        return !Application.isPlaying || Time.timeScale == 0;
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
    private void Update()
    {       
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
        for (int i = 0; i < Puppet.Length; i++)
        {
            //Takes the distance between the puppet and ghost in terms of vectors
            float distance = Vector3.Distance(Puppet[i], Ghost[i]);
            //print($"Puppet vector: {Puppet[i]} Ghost vector: {Ghost[i]} Distance: {distance}");
            //checks if every bone is <0.1 units away from the corresponding ghost one
            if (distance > 0.1)
                return false;
        }
        return true;
    }
}