
using System;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.IO.Pipelines;
using System.Reflection.Metadata;
using Microsoft.VisualBasic;

class Program
{
    //Write a C# Sharp program to create a string
    // made of every other character starting with 
    // the first in a given string.
    static void Main(string[] args)
    {
       Console.WriteLine(Tata("Python"));
       Console.WriteLine(Tata("PHP"));
       Console.WriteLine(Tata("JS"));
       
    }

     static public string Tata(string str)
    {
        string result="";

        for(int i=0; i<str.Length; i++)
        {
           if (i % 2 == 0)
            {
              result += str[i]; 
            }
        }
            return result;
    
    }}



    
