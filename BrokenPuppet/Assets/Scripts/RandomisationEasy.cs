using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;


public enum Difficulty
{
    Easy = 1,
    Medium = 2,
    Hard = 3
}

public class Randomiser
{
    public static GameObject gameObject;
    static List<int>[] LandmarkSections; // Declare LandmarkSections array in a broader scope
    public static Difficulty GameDifficulty = Difficulty.Easy;

    //Selects the landmark list, a landmark from the list, then again for another landmark
    static (int, int, int, int, Transform) SelectLandmarks()
    {
        (int, int, int, int) SelectedLandmarks;
        (int, int, int, int, Transform) DataToReturn;

        InitialiseLandmarks(); // Call InitialiseLandmarks to populate LandmarkSections
        SelectedLandmarks = RandomLandmarkGenerator();
        Transform Degree = MovementDegreeGenerator(SelectedLandmarks.Item2, SelectedLandmarks.Item4);

        //Debugging
        Console.WriteLine("Selected Landmark List 1: " + SelectedLandmarks.Item2 + " Selected Landmark List 2: " + SelectedLandmarks.Item4);
        Console.WriteLine("First Selected Landmark: " + SelectedLandmarks.Item1 + " Second Selected Landmark: " + SelectedLandmarks.Item3);
        Console.WriteLine("Selected Degree = " + Degree);

        DataToReturn = (SelectedLandmarks.Item2, SelectedLandmarks.Item1, SelectedLandmarks.Item4, SelectedLandmarks.Item3, Degree);

        return DataToReturn;
    }

    //Selects randomo landmarks
    static (int, int, int, int) RandomLandmarkGenerator()
    {
        //Create a random object
        System.Random Random = new System.Random();
        //Select a random integer in the given range
        int LandmarkList = Random.Next(0, LandmarkSections.Length);
        List<int> SelectedList = LandmarkSections[LandmarkList]; // Selects a random list from pose, face and hands

        int SelectedLandmark = Random.Next(0, SelectedList.Count); //Selects a random landmark from the selected list
        int SecondSelectedLandmark;
        int SecondLandmarkList;
        // if difficulty is easy --> select second landmark from the same list
        if(GameDifficulty == Difficulty.Easy)
        {
            SecondSelectedLandmark = Random.Next(0, SelectedList.Count);
            SecondLandmarkList = LandmarkList;
        }
        //if game difficulty is not easy -> select from any list
        else
        {
            SecondLandmarkList = Random.Next(0, LandmarkSections.Length);
            List<int> SecondSelectedList = LandmarkSections[SecondLandmarkList];
            SecondSelectedLandmark = Random.Next(0, SecondSelectedList.Count);
        }
        //Collate all data into a tuple with 4 items
        (int, int, int, int) SelectedLandmarks = (SelectedLandmark, LandmarkList, SecondSelectedLandmark, SecondLandmarkList);
        
        return SelectedLandmarks;
    }
    //Function to generate random degrees
    static Transform MovementDegreeGenerator(int LandmarkIndex, int SecondLandmarkIndex)
    {
        //Convert the difficulty type to an integer
        int Difficulty = (int)GameDifficulty;
        //Initial movement limit ranges
        int[] MovementLimits = {0, 25, 50, 75};
        //need to implement constraints n such here
        
        Transform transform = gameObject.transform;
        //Create a vector object to hold the current angles of rotations
        Vector3 rot = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
        //Clamp the rotations to the movement limit selected
        rot.x = Mathf.Clamp(rot.x, -MovementLimits[Difficulty-1], MovementLimits[Difficulty]);
        rot.y = Mathf.Clamp(rot.y, -MovementLimits[Difficulty - 1], MovementLimits[Difficulty]);


        transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);

        return transform;
    }

    static void InitialiseLandmarks()
    {
        List<int> LeftHandLandmarks = new List<int>();
        List<int> RightHandLandmarks = new List<int>();

        int[] FaceLandmarksList = { 0, 384, 386, 388, 132, 133, 263, 389, 390, 10, 269, 14, 397, 400, 145, 402, 13,
                        17, 405, 150, 276, 152, 282, 154, 284, 157, 285, 159, 33, 161, 291, 163, 162, 39, 297, 172, 300, 46,
                        176, 178, 52, 308, 54, 55, 311, 181, 61, 67, 454, 70, 78, 334, 336, 81, 105, 234, 107, 361, 362, 374, 379, 381 };

        int[] PoseLandmarksList = { 11, 12, 13, 14, 15, 16, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33 };

        Array.Sort(FaceLandmarksList); // Fix the sorting of FaceLandmarksList
        List<int> FaceLandmarks = FaceLandmarksList.ToList();
        List<int> PoseLandmarks = PoseLandmarksList.ToList();
        LeftHandLandmarks.AddRange(Enumerable.Range(0, 21));
        RightHandLandmarks.AddRange(Enumerable.Range(0, 21));

        LandmarkSections = new List<int>[] { FaceLandmarks, PoseLandmarks, RightHandLandmarks, LeftHandLandmarks }; // Assign LandmarkSections
    }
}
