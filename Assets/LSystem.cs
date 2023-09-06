using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class LSystem
{
    public string lSystemString;
    public Dictionary<char,string> replacementStrings = new Dictionary<char,string>();

    public void Iterate(float timeOutInSeconds = -1.0f)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string newString = "";
        foreach(char c in lSystemString)
        {
            newString += replacementStrings.GetValueOrDefault(c,c.ToString());
            if(timeOutInSeconds > 0 && stopwatch.Elapsed.TotalSeconds > timeOutInSeconds)
            { 
                return;
            }
        }
        lSystemString = newString;
    }

    public void Iterate(int iterations, float timeOutInSeconds = -1.0f)
    {
        for(int i = 0; i < iterations; i++)
        {
            Iterate(timeOutInSeconds);
        }
    }
}
