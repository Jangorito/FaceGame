//Medium randomisation
//Body part x moves body part y by z degrees (reduced range)
//Not too sure about this but I remember talking about a medium mode.

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
        
        //Angles from 10 to 50 degrees
        //We can modify values as appropriate
        //Especially with medium mode
        int deck3 = random.Next(10, 50);
        
        //Example: [9, 31, 50] translates to 
        //"Landmark 9 (mouth right) rotates landmark 31 (right foot) by 50 degrees"
        Console.WriteLine($"[{deck1}, {deck2}, {deck3}]");
        //Return deck1, deck2, deck3 to calling function
        //And then we can figure out how to get this data represented visually
    }
}
