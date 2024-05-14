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
        originalMovement = new Dictionary<HumanBodyBones, CalibrationData>[2];
        brokenMovement = new Dictionary<HumanBodyBones, CalibrationData>[2];

        logger = new(shouldDebug);
        random = new();
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
            originalMovement[p1] = new Dictionary<HumanBodyBones, CalibrationData>(P1.getCalibrations());
            originalMovement[p2] = new Dictionary<HumanBodyBones, CalibrationData>(P2.getCalibrations());
            brokenMovement[p1] = new Dictionary<HumanBodyBones, CalibrationData>(P1.getCalibrations());
            brokenMovement[p2] = new Dictionary<HumanBodyBones, CalibrationData>(P2.getCalibrations());
            breakMovements();
            return;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
           breakMovements();
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

        // Swap those landmarks 
        for (int i = 0; i < 2; i++) {
            for (int j = 0; j < iJointsToChange; j++) {

                // Get Two random joints
                HumanBodyBones joint1 = P1.parentCalibrationData.ElementAt(
                    random.Next(0, P1.parentCalibrationData.Count())).Value.parentBone;
                HumanBodyBones joint2 = P1.parentCalibrationData.ElementAt(
                    random.Next(0, P1.parentCalibrationData.Count())).Value.parentBone;

                // allow same joint choice to add hidden way of easier breaking
                if (joint1 == joint2)
                    continue;

                // Get the transforms at each joint
                Transform j1_parent = originalMovement[i][joint1].tparent;
                Transform j1_child = originalMovement[i][joint1].tchild;
                Transform j2_parent = originalMovement[i][joint2].tparent;
                Transform j2_child = originalMovement[i][joint2].tchild;

                // Swap transforms
                brokenMovement[i][joint1].change_calibrations(j2_parent, j2_child);
                brokenMovement[i][joint2].change_calibrations(j1_parent, j1_child);
            }
        }
    }

    void apply_random_calibrations() {
        logger.LogMsg("Applying Random Movement");

        P1.change_calibrations(brokenMovement[p1]);
        P2.change_calibrations(brokenMovement[p2]);
    }
}
