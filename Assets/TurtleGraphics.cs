using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurtleGraphics : MonoBehaviour
{
    public float moveDistance = 1.0f;
    public float moveSpeed = 1.0f;
    public float rotationAmount = 90.0f;
    public int iterations = 5;

    public bool usePerlinHeight = false;
    public float perlinFrequency = 1.0f;
    public float perlinAmplitude = 1.0f;

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
    private float startingY;

    // Start is called before the first frame update
    void Start()
    {
        startingY = transform.position.y;

        if(usePerlinHeight)
        {
            Vector3 newPosition = transform.position;
            newPosition.y = Mathf.PerlinNoise(transform.position.x * perlinFrequency, transform.position.z * perlinFrequency) * perlinAmplitude + startingY;
            transform.position = newPosition;
        }


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
                    Vector3 targetPosition = transform.position + transform.forward * moveDistance;
                    if(usePerlinHeight)
                    {
                        targetPosition.y = Mathf.PerlinNoise(targetPosition.x * perlinFrequency, targetPosition.z * perlinFrequency) * perlinAmplitude + startingY;
                    }
                    while (Vector3.Distance(transform.position, targetPosition) > 0.001f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position,targetPosition, moveSpeed * Time.deltaTime);
                        yield return null;
                    }
                    transform.position = targetPosition;
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
