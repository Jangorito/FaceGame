//Easy randomisation
//Body part x moves body part x by y degrees
//(Scales funnily)

using System;

class Program
{
    static void Main()
    {
        Random random = new Random();
        
        //Body parts range from -2 to 32
        int deck1 = random.Next(-2, 32);
        //Body part x moves body part y
        int deck2 = deck1;

        //Incase we need to exclude values.
        // while (deck1 == 7 || deck1 == 8){
        //     deck1 = random.Next(-2, 32);
        //     int deck2 = deck1;
        // }
        
        //Angles from 10 to 90 degrees
        //We can modify values as appropriate
        int deck3 = random.Next(10, 90);
        
        //Example: [15, 15, 26] translates to 
        //"Landmark 15 (right wrist) rotates landmark 15 (right wrist) by 26 degrees"
        Console.WriteLine($"[{deck1}, {deck2}, {deck3}]");
        //Return deck1, deck2, deck3 to calling function
    }
}
