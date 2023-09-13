using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class LSystem
{
    public string lSystemString;
    public Dictionary<char,string> replacementStrings = new Dictionary<char,string>();

    public bool Iterate(ref int iterations, float timeOutInSeconds = -1.0f)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        for(int i = 0; i < iterations; i++)
        {
            if(timeOutInSeconds >= 0.0f)
            {
                float timeRemaining = Mathf.Clamp(timeOutInSeconds - (float)stopwatch.Elapsed.TotalSeconds,0.0f,Mathf.Infinity);
                if(!Iterate(timeRemaining))
                {
                    iterations = i;
                    return false;
                }
            }
            else
            {
                Iterate(timeOutInSeconds);
            }
        }

        return true;
    }

    private bool Iterate(float timeOutInSeconds = -1.0f)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        string newString = "";
        foreach(char c in lSystemString)
        {
            newString += replacementStrings.GetValueOrDefault(c,c.ToString());

            if(timeOutInSeconds >= 0 && stopwatch.Elapsed.TotalSeconds > timeOutInSeconds)
            { 
                return false;
            }
        }
        lSystemString = newString;
        return true;
    }
}
