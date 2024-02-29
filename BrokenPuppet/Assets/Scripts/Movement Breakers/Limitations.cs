using System.Collections.Generic;

public class Limitations
{

    static Dictionary<int, (float, float)> x_hashmap = new();
    static Dictionary<int, (float, float)> y_hashmap = new();


    public static void initializeXArray()
    {
        x_hashmap.Clear();
        //X Limitations -- Min, Max
        //Misc
        x_hashmap.Add(1, (0, 0));
        x_hashmap.Add(2, (0, 0));
        //neck and hip
        x_hashmap.Add(0, (-70, 70));
        x_hashmap.Add(3, (-70, 70));
        //wrists
        x_hashmap.Add(4, (-15, 15));
        x_hashmap.Add(20, (-15, 15));

        //Left Hand
        x_hashmap.Add(5, (0, 0));
        x_hashmap.Add(6, (0, 0));
        x_hashmap.Add(7, (0, 0));
        x_hashmap.Add(8, (0, 0));
        x_hashmap.Add(9, (0, 0));
        x_hashmap.Add(10, (0, 0));
        x_hashmap.Add(11, (0, 0));
        x_hashmap.Add(12, (0, 0));
        x_hashmap.Add(13, (0, 0));
        x_hashmap.Add(14, (0, 0));
        x_hashmap.Add(15, (0, 0));
        x_hashmap.Add(16, (0, 0));
        x_hashmap.Add(17, (0, 0));
        x_hashmap.Add(18, (0, 0));
        x_hashmap.Add(19, (0, 0));

        //Right hand
        x_hashmap.Add(21, (0, 0));
        x_hashmap.Add(22, (0, 0));
        x_hashmap.Add(23, (0, 0));
        x_hashmap.Add(24, (0, 0));
        x_hashmap.Add(25, (0, 0));
        x_hashmap.Add(26, (0, 0));
        x_hashmap.Add(27, (0, 0));
        x_hashmap.Add(28, (0, 0));
        x_hashmap.Add(29, (0, 0));
        x_hashmap.Add(30, (0, 0));
        x_hashmap.Add(31, (0, 0));
        x_hashmap.Add(32, (0, 0));
        x_hashmap.Add(33, (0, 0));
        x_hashmap.Add(34, (0, 0));
        x_hashmap.Add(35, (0, 0));

        // Feet -- Toes
        x_hashmap.Add(36, (0, 0));
        x_hashmap.Add(37, (0, 0));

        //Ankles
        x_hashmap.Add(38, (-20, 20));
        x_hashmap.Add(39, (-20, 20));

        //Knees
        x_hashmap.Add(40, (0, 0));
        x_hashmap.Add(41, (0, 0));

        //Hip Joints
        x_hashmap.Add(42, (0, 40));
        x_hashmap.Add(43, (0, 40));

        // elbows
        x_hashmap.Add(44, (-20, 20));
        x_hashmap.Add(45, (-20, 20));

        //Upper arm???
        x_hashmap.Add(46, (-90, 90));
        x_hashmap.Add(47, (-90, 90));

        //Shoulders
        x_hashmap.Add(48, (-90, 90));
        x_hashmap.Add(49, (-90, 90));

    }

    public static void initializeYArray()
    {
        y_hashmap.Clear();
        //X Limitations -- Min, Max
        //Misc
        y_hashmap.Add(1, (0, 0));
        y_hashmap.Add(2, (0, 0));
        //neck and hip

        y_hashmap.Add(0, (-50, 50));
        y_hashmap.Add(3, (-30, 40));
        //wrists
        y_hashmap.Add(4, (-15, 15));
        y_hashmap.Add(20, (-15, 15));

        //Left Hand
        y_hashmap.Add(5, (0, 20));
        y_hashmap.Add(6, (0, 20));
        y_hashmap.Add(7, (0, 20));
        y_hashmap.Add(8, (0, 20));
        y_hashmap.Add(9, (0, 20));
        y_hashmap.Add(10, (0, 20));
        y_hashmap.Add(11, (0, 20));
        y_hashmap.Add(12, (0, 20));
        y_hashmap.Add(13, (0, 20));
        y_hashmap.Add(14, (0, 20));
        y_hashmap.Add(15, (0, 20));
        y_hashmap.Add(16, (0, 20));
        y_hashmap.Add(17, (0, 20));
        y_hashmap.Add(18, (0, 20));
        y_hashmap.Add(19, (0, 20));

        //Right hand
        y_hashmap.Add(21, (0, 20));
        y_hashmap.Add(22, (0, 20));
        y_hashmap.Add(23, (0, 20));
        y_hashmap.Add(24, (0, 20));
        y_hashmap.Add(25, (0, 20));
        y_hashmap.Add(26, (0, 20));
        y_hashmap.Add(27, (0, 20));
        y_hashmap.Add(28, (0, 20));
        y_hashmap.Add(29, (0, 20));
        y_hashmap.Add(30, (0, 20));
        y_hashmap.Add(31, (0, 20));
        y_hashmap.Add(32, (0, 20));
        y_hashmap.Add(33, (0, 20));
        y_hashmap.Add(34, (0, 20));
        y_hashmap.Add(35, (0, 20));

        // Feet -- Toes
        y_hashmap.Add(36, (0, 20));
        y_hashmap.Add(37, (0, 20));

        //Ankles
        y_hashmap.Add(38, (-80, 60));
        y_hashmap.Add(39, (-80, 60));

        //Knees
        y_hashmap.Add(40, (0, 90));
        y_hashmap.Add(41, (0, 90));

        //Hip Joints
        y_hashmap.Add(42, (0, 90));
        y_hashmap.Add(43, (0, 90));

        // elbows
        y_hashmap.Add(44, (0, 110));
        y_hashmap.Add(45, (0, 110));

        //Upper arm???
        y_hashmap.Add(46, (0, 180));
        y_hashmap.Add(47, (0, 180));

        //Shoulders
        y_hashmap.Add(48, (0, 180));
        y_hashmap.Add(49, (0, 180));

    }

    public static (float, float) getXIndex(int index)
    {   //x, y
        (float, float) MapIndex = x_hashmap[index];

        return MapIndex;
    }

    public static (float, float) getYIndex(int index)
    {   //x, y
        (float, float) MapIndex = y_hashmap[index];

        return MapIndex;
    }

}
