using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementBreaker : MonoBehaviour
{
    private Dictionary<HumanBodyBones, CalibrationData>[] originalMovement;

    private Dictionary<HumanBodyBones, CalibrationData>[] brokenMovement;

    // The minmum number of joints that can be changed
    public int MIN = 1;

    // The Maximum number of joints that can be changed
    public int MAX = 5;

    public GameObject PlayerAvatars;
    private AvatarFactory avatarManager;

    private Avatar P1;
    private Avatar P2;

    // Store index of players calibrations data
    private const int p1 = 0;
    private const int p2 = 1;

    // Allows for filtering of debugging info 
    private Logger logger;
    public bool shouldDebug = false;

    bool m_bPlayer1Calibrated = false;
    bool m_bPlayer2Calibrated = false;
    bool m_bIsBroken = false;

    System.Random random;

    private void Awake()
    {
        avatarManager = PlayerAvatars.GetComponent<AvatarFactory>();
    }

    void Start()
    {
        P1 = avatarManager.getPlayerOne().GetComponent<Avatar>();
        P2 = avatarManager.getPlayerTwo().GetComponent<Avatar>();

        logger = new(shouldDebug);
        logger.LogMsg("MovementBreaker::Start");
        random = new();

        // Get backup of each player avatars movement
        originalMovement = new Dictionary<HumanBodyBones, CalibrationData>[2];
        // Initialise broken movement of each Avatar
        brokenMovement = new Dictionary<HumanBodyBones, CalibrationData>[2];
    }

    void Update()
    {
        if (!m_bPlayer1Calibrated) {
            m_bPlayer1Calibrated = P1.IsCalibrated();
            return;
        }
        if (!m_bPlayer2Calibrated) {
            m_bPlayer2Calibrated = P2.IsCalibrated();
            return;
        }
        if (!m_bIsBroken) {
            m_bIsBroken = true;
            originalMovement[p1] = P1.getCalibrations();
            originalMovement[p2] = P2.getCalibrations();
            brokenMovement[p1] = P1.getCalibrations();
            brokenMovement[p2] = P2.getCalibrations();
            breakMovements();
            return;
        }
    }

    public void breakMovements() {
        generate_random_calibrations();
        apply_random_calibrations();
    }

    void generate_random_calibrations() {
        logger.LogMsg("Generating random movement");

        // Generate a random number of joints to change
        int iJointsToChange = UnityEngine.Random.Range(MIN, MAX);

        // Choose a set of landmarks
        HumanBodyBones[] iFromLandmarks = getRandomLandmarks(iJointsToChange);

        // Choose another set of landmarks
        HumanBodyBones[] iToLandmarks = getRandomLandmarks(iJointsToChange);

        // Swap those landmarks 
        for (int i = 0; i < 2; i++) {
            for (int j = 0; j < iJointsToChange; j++) {
                Transform newParent = originalMovement[i][iToLandmarks[j]].tparent;
                Transform newChild = originalMovement[i][iToLandmarks[j]].tchild;

                brokenMovement[i][iFromLandmarks[j]].change_calibrations(newParent, newChild);
            }
        }
    }

    void apply_random_calibrations() {
        logger.LogMsg("Applying Random Movement");

        P1.change_calibrations(brokenMovement[p1]);
        P2.change_calibrations(brokenMovement[p2]);
    }

    private HumanBodyBones[] getRandomLandmarks(int number) {

        HumanBodyBones[] landmarks = new HumanBodyBones[number];

        for (int i = 0; i < number; i++) {
            // Get Random Landmark
            landmarks[i] = P1.parentCalibrationData.ElementAt(
                random.Next(0, P1.parentCalibrationData.Count())).Value.parentBone;
        }

        return landmarks;
    }
}
