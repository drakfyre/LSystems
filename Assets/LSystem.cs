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
            bool didReplacement = false;
            foreach(KeyValuePair<char,string> pair in replacementStrings)
            {
                if(c == pair.Key)
                {
                    newString += pair.Value;
                    didReplacement = true;
                    break;
                }
            }
            if(!didReplacement)
            {
                newString += c;
            }
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
