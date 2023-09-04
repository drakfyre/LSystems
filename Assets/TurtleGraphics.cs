using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleGraphics : MonoBehaviour
{
    public float moveDistance = 0.1f;
    public float rotationAmount = 90.0f;
    public int iterations = 5;

    public string lString;
    public List<char> ruleCharacters = new List<char>();
    public List<string> ruleStrings = new List<string>();

    private LSystem lSystem = new LSystem();
    private int index = 0;
    private struct State
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    private Stack<State> stateStack = new Stack<State>();

    // Start is called before the first frame update
    void Start()
    {
        //lSystem.lSystemString = "A";
        //lSystem.replacementStrings.Add('A',"+BF-AFA-FB+");
        //lSystem.replacementStrings.Add('B',"-AF+BFB+FA-");

        //lSystem.lSystemString = "A";
        //lSystem.replacementStrings.Add('A',"B-F+CFC+F-D&F^D-F+&&CFC+F+B//");
        //lSystem.replacementStrings.Add('B',"A&F^CFB^F^D^^-F-D^|F^B|FC^F^A//");
        //lSystem.replacementStrings.Add('C',"|D^|F^B-F+C^F^A&&FA&F^C+F+B^F^D//");
        //lSystem.replacementStrings.Add('D',"|CFB-F+B|FA&F^A&&FB-F+B|FC//");

        lSystem.lSystemString = lString;

        for(int i = 0; i < ruleCharacters.Count; i++)
        {
            lSystem.replacementStrings.Add(ruleCharacters[i],ruleStrings[i]);
        }

        lSystem.Iterate(iterations);

        Debug.Log(lSystem.lSystemString);

        StartCoroutine(Draw());
    }

    // Update is called once per frame
    IEnumerator Draw()
    {
        while(index < lSystem.lSystemString.Length)
        {
            char c = lSystem.lSystemString[index];
            switch(c)
            {
                case 'F':
                    transform.position += transform.forward * moveDistance;
                    yield return null;
                    break;
                case '+':
                    transform.Rotate(0.0f,rotationAmount,0.0f);
                    break;
                case '-':
                    transform.Rotate(0.0f,-rotationAmount,0.0f);
                    break;
                case '^':
                    transform.Rotate(rotationAmount,0.0f,0.0f);
                    break;
                case '&':
                    transform.Rotate(-rotationAmount,0.0f,0.0f);
                    break;
                case '<':
                    transform.Rotate(0.0f,0.0f,-rotationAmount);
                    break;
                case '>':
                    transform.Rotate(0.0f,0.0f,rotationAmount);
                    break;
                case '\\':
                    transform.Rotate(0.0f,0.0f,-rotationAmount);
                    break;
                case '/':
                    transform.Rotate(0.0f,0.0f,rotationAmount);
                    break;
                case '|':
                    transform.Rotate(0.0f,180.0f,0.0f);
                    break;
                case '[':
                {
                    State newState;
                    newState.position = transform.position;
                    newState.rotation = transform.rotation;
                    stateStack.Push(newState);
                    break;
                }
                case ']':
                {
                    State newState = stateStack.Pop();
                    transform.position = newState.position;
                    transform.rotation = newState.rotation;
                    break;
                }
                default:
                    break;
            }
            index++;
        }
    }
}
