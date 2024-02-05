//Hard randomisation
//Body part x moves body part y by z degrees

using System;

class Program
{
    static void Main()
    {
        Random random = new Random();
        
        //Body parts range from -2 to 32
        int deck1 = random.Next(-2, 32);
        int deck2 = random.Next(-2, 32);

        //Incase we need to exclude values.
        // while (deck1 == 7 || deck1 == 8 || deck2 == 7 || deck2 == 8){
        //     deck1 = random.Next(-2, 32);
        //     deck2 = random.Next(-2, 32);
        // }

        
        //Angles from 10 to 90 degrees
        int deck3 = random.Next(10, 90);
        
        //Example: [30, 28, 40] translates to 
        //"Landmark 30 (left heel) rotates landmark 28 (left ankle) by 40 degrees"
        Console.WriteLine($"[{deck1}, {deck2}, {deck3}]");
        //Return deck1, deck2, deck3 to calling function
    }
}
