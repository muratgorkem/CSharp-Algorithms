
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.IO.Pipelines;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;

class Program
{
    /**
   Write a C# Sharp program to check whether 
   the sequence of numbers 1, 2, 3 appears 
   in a given array of integers somewhere.

Sample Input:
{1,1,2,3,1}
{1,1,2,4,1}
{1,1,2,1,2,3}
Expected Output:

True
False
True
*/

    static void Main(string[] args)
    {
       Console.WriteLine(Tata(new[] {1,1,2,3,1}));
       Console.WriteLine(Tata(new[] {1,1,2,4,1}));
       Console.WriteLine(Tata(new[] {1,1,2,1,2,3}));
          
    }

     static public bool Tata(int[] numbers)
    {
       if (numbers.Length<8)
        {
            if (numbers.Contains(1))
            {
                if (numbers.Contains(2))
                {
                    if(numbers.Contains(3))
                    return true;
                }
                else return false;
            }
            else return false;
        }
        return false;
    }
}



    
