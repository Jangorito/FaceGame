using System.Timers;
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ModelSimilarityChecker : MonoBehaviour
{
    public Avatar BrokenPuppet;
    public ShadowAvatar GhostAvatar;
    private Transform[] ShadowCharacterBones;
    private Transform[] BrokenPuppetBones;
    private Vector3[] PuppetVectors = new Vector3[65];
    private Vector3[] GhostVectors = new Vector3[65];
    private static bool GameEnd = false;
    bool Successful;
    public static PlayModeStateChange state;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI PercentageMatchText;
    public int points = 0;

    // Int to tell what Model that instance is
    private static int instances = 0;
    private int instance;

    // The Offset between the unmoved BrokenPuppet and the GhostAvatar
    Vector3 modelOffset;

    // Will Output Debugging info
    Logger logger;

    // Flags if should output debugging info
    public bool bShouldDebug;

    // Start is called before the first frame update
    private void Start()
    {
        logger = new Logger(bShouldDebug);

        // Player Number
        instance = ++instances;

        BrokenPuppet = getPuppetAvatar();
        GhostAvatar = getShadowAvatar();
        Successful = false;
        //StartCoroutine(Coroutine());
        StartTimer();
        logger.LogMsg("ModelSimilarityChecker::Start | Is Successful + " + Successful.ToString());

        // Calculate the initial offset that will be matched against
        modelOffset = getOffset(GhostAvatar.transform.position, BrokenPuppet.transform.position);
    }

    private void StartTimer()
    {
        Timer initialTimer = new()
        {
            Interval = 8000,
            AutoReset = false,
        };
        initialTimer.Elapsed += (sender, args) =>
        {
            // Call CheckModels after 8 seconds
            //CheckModels();

            // Dispose of the timer after it's used
            initialTimer.Stop();
            initialTimer.Dispose();
        };

        logger.LogMsg("ModelSimilarityChecker::StartTimer | Initial 8 seconds started");
        initialTimer.Start();
    }

    private void NextLevelTimer()
    {
        Timer initialTimer = new()
        {
            Interval = 30000,
            AutoReset = false,
        };
        initialTimer.Elapsed += (sender, args) =>
        {
            // Call CheckModels after 8 seconds
            //CheckModels();

            // Dispose of the timer after it's used
            initialTimer.Stop();
            initialTimer.Dispose();
            SceneManager.LoadScene("NextLevelMenu");
        };

        logger.LogMsg("ModelSimilarityChecker::StartTimer | Initial 8 seconds started");
        initialTimer.Start();
    }


    private void Update()
    {

        // Will stop the Avatar from checking before Shadow Avatar has moved
        if (!GhostAvatar.IsShadowAvatarReady()) {
            return;
        }

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
        else // Calculate percentage match if successful
        {
            float totalDifference = 0f;
            GetVectors();
            int numVectors = PuppetVectors.Length; // Assuming PuppetVectors and GhostVectors have the same length

            for (int i = 0; i < numVectors; i++)
            {
                Vector3 puppetVector = PuppetVectors[i];
                Vector3 ghostVector = GhostVectors[i];
                totalDifference += Vector3.Distance(puppetVector, ghostVector);
            }

            // Normalize the result
            float maxPossibleDistance = Vector3.Distance(Vector3.zero, Vector3.one) * numVectors;
            float normalizedDifference = totalDifference / maxPossibleDistance;

            // Calculate percentage match
            float percentageMatch = Mathf.Clamp01(1f - normalizedDifference) * 100f;

            PercentageMatchText.text = percentageMatch.ToString() + "Player " + instance + ": " + percentageMatch + "% Match\n";

            logger.LogMsg("ModelSimilarityChecker::Update | Percentage Match for " + instance + ": " + percentageMatch + "%");
            BrokenPuppet.StopAvatarMoving = true;
            NextLevelTimer(); // Starts timer for next level
            return;
        }
    }

    public bool getSuccessful()
    {
        return Successful;
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
            logger.LogError("ModelSimilarityChecker::getShadowAvatar | Could not find an Avatar in the scene");
        return avatar;
    }

    //Gets the puppet avatar the user is manipulating
    private Avatar getPuppetAvatar()
    {
        Avatar avatar = FindObjectOfType<Avatar>();
        if (avatar == null)
            logger.LogError("ModelSimilarityChecker::getPuppetAvatar | Could not find an Avatar in the scene");
        return avatar;
    }

    Vector3 getOffset(Vector3 from, Vector3 to) {
        return to - from;
    }

    public bool IsModelNear(Vector3[] Puppet, Vector3[] Ghost, Vector3 puppetPosition, Vector3 ghostPosition)
    {
        for (int i = 0; i < Puppet.Length; i++)
        {
            // Calculate the Offset from the Ghost to the Puppet
            Vector3 offset = getOffset(Ghost[i], Puppet[i]);

            // Adjust the positions by adding the offsets
            Vector3 adjustedPuppetPosition = Puppet[i] - modelOffset;
            Vector3 adjustedGhostPosition = Ghost[i];

            // Takes the distance between the puppet and ghost in terms of vectors
            float distance = Vector3.Distance(adjustedPuppetPosition, adjustedGhostPosition);

            // Checks if every bone is <0.1 units away from the corresponding ghost one
            if (distance > 2.6)
                return false;
        }
        points += 5;
        pointsText.text = $"Points: {(int)points}";
        return true;
    }
}