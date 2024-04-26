using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBreaker : MonoBehaviour
{
    private Dictionary<HumanBodyBones, CalibrationData>[] originalMovement;

    private Dictionary<HumanBodyBones, CalibrationData>[] brokenMovement;

    // The minmum number of joints that can be changed
    public int MIN = 1;

    // The Maximum number of joints that can be changed
    public int MAX = 5;

    // Reference to the player one avatar 
    public Avatar player_1;

    // Reference to the player two avatar
    public Avatar player_2;

    // Store index of players calibrations data
    private const int p1 = 0;
    private const int p2 = 1;

    // Allows for filtering of debugging info 
    private Logger logger;
    public bool shouldDebug = false;

    System.Random random;

    void Start()
    {
        logger = new(shouldDebug);

        logger.LogMsg("MovementBreaker::Start");

        random = new();


        // Get backup of each player avatars movement
        originalMovement = new Dictionary<HumanBodyBones, CalibrationData>[2];
        originalMovement[p1] = player_1.getCalibrations();
        originalMovement[p2] = player_2.getCalibrations();

        // Initialise broken movement of each Avatar
        brokenMovement = new Dictionary<HumanBodyBones, CalibrationData>[2];

        // create the first random set of calibrations
        generate_random_calibrations();

        // apply broken movements to each avatar
        apply_random_calibrations();
    }

    void Update()
    {
        
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

        player_1.change_calibrations(brokenMovement[p1]);
        player_2.change_calibrations(brokenMovement[p2]);

    }

    private IEnumerator wait() {
        yield return new WaitForSeconds(2);
    }

    private HumanBodyBones[] getRandomLandmarks(int number) {

        HumanBodyBones[] landmarks = new HumanBodyBones[number];

        for (int i = 0; i < number; i++) {
            // Get Random Landmark
            landmarks[i] = (HumanBodyBones) random.Next(Enum.GetNames(typeof(Landmark)).Length);
        }

        return landmarks;
    }
}
