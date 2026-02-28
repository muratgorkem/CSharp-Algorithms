
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.IO.Pipelines;
using System.Reflection.Metadata;
using Microsoft.VisualBasic;

class Program
{
    /**
   Write a C# Sharp program to check
 if one of the first 4 elements in an array of 
 integers is equal to a given element.

Sample Input:
{1,2,9,3}, 3
{1,2,3,4,5,6}, 2
{1,2,2,3}, 9
Expected Output:

True
True
False
*/

    static void Maiin(string[] args)
    {
       Console.WriteLine(Tata(new[] {1,2,9,3}, 3));
       Console.WriteLine(Tata(new[] {1,2,3,4,5,6}, 2));
       Console.WriteLine(Tata(new[] {1,2,2,3}, 9));
          
    }

     static public bool Tata(int[] numbers,int x)
    {
        return numbers.Length<4? numbers.Contains(x): numbers.Take(4).Contains(x);
    
    }
}



    
