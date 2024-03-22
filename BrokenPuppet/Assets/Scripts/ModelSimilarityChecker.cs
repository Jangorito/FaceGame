using System;
using System.Collections;
using System.Timers;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

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
    public static PlayModeStateChange state;
    public TextMeshProUGUI pointsText;
    public int points =0;

    // Start is called before the first frame update
    private void Start()
    {
        //BrokenPuppet = getPuppetAvatar();
        //GhostAvatar = getShadowAvatar();
        Successful = false;
        //StartCoroutine(Coroutine());
        StartTimer();
        Debug.Log(Successful);
    }

    private void StartTimer()
    {
        Timer initialTimer = new Timer();
        initialTimer.Interval = 8000;
        initialTimer.AutoReset = false; // Set AutoReset to false to ensure it only triggers once
        initialTimer.Elapsed += (sender, args) =>
        {
            // Call CheckModels after 8 seconds
            //CheckModels();

            // Dispose of the timer after it's used
            initialTimer.Stop();
            initialTimer.Dispose();
        };

        Debug.Log("Initial 8 Seconds started");
        initialTimer.Start();
    }

    private void Update()
    {
        if (!Successful) // Only check models if the round is not successful
        {
            Vector3 puppetPosition = BrokenPuppet.transform.position;
            Vector3 ghostPosition = GhostAvatar.transform.position;
            //Debug.Log("********" + puppetPosition + " " + ghostPosition);

            //Gets the puppets bones
            BrokenPuppetBones = BrokenPuppet.GetComponentInChildren<SkinnedMeshRenderer>().bones;
            //Gets the shadows bones
            ShadowCharacterBones = GhostAvatar.GetComponentInChildren<SkinnedMeshRenderer>().bones;
            GetVectors();
            Successful = IsModelNear(PuppetVectors, GhostVectors, puppetPosition, ghostPosition);
        }
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

    public bool IsModelNear(Vector3[] Puppet, Vector3[] Ghost, Vector3 puppetPosition, Vector3 ghostPosition)
    {
        for (int i = 0; i < Puppet.Length; i++)
        {
            // Calculate the Offset between the Puppet and the Ghost
            Vector3 offset = Puppet[i] - Ghost[i];

            // Adjust the positions by adding the offsets
            Vector3 adjustedPuppetPosition = Puppet[i] - offset;
            Vector3 adjustedGhostPosition = Ghost[i];

            // Takes the distance between the puppet and ghost in terms of vectors
            float distance = Vector3.Distance(adjustedPuppetPosition, adjustedGhostPosition);

            // Checks if every bone is <0.1 units away from the corresponding ghost one
            if (distance > 0.1)
                return false;
        }
        points += 5;
        pointsText.text = $"Points: {points}";
        return true;
    }
}