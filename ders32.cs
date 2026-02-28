using System;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.IO.Pipelines;
using System.Reflection.Metadata;
using Microsoft.VisualBasic;

class ders32
{
    //Write a C# Sharp program to create a string
    // like "aababcabcd" from a given string 
    // "abcd".
    /*
    Sample Input:
"abcd"
"abc"
"a"
Expected Output:

aababcabcd
aababc
a*/

    static void Main(string[] args)
    {
       Console.WriteLine(Tata("abcdsab"));
       Console.WriteLine(Tata("abcdabab"));
       Console.WriteLine(Tata("abcabdabab"));
       Console.WriteLine(Tata("abcabd"));
       
    }

     static public int Tata(string str)
    {
        string result="";
        int countt=0;
        result += str.Substring(str.Length-2,2); 
        // stringlerin içindeki son 2 harfi bul.

        // string içinde gezinme
        for(int i=0; i<str.Length-1; i++)
        {
            string lastTwo=str.Substring(i,2);

            if (lastTwo == result)
            {
                countt++;
            }
        }

        
        return countt-1;
    
    }
    }