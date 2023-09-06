using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LSystem
{
    public string lSystemString;
    public Dictionary<char,string> replacementStrings = new Dictionary<char,string>();

    public void Iterate()
    {
        string newString = "";
        foreach(char c in lSystemString)
        {
            newString += replacementStrings.GetValueOrDefault(c,c.ToString());
        }
        lSystemString = newString;
    }

    public void Iterate(int iterations)
    {
        for(int i = 0; i < iterations; i++)
        {
            Iterate();
        }
    }
}
