using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.ParticleSystem;

public class TurtleGraphics : MonoBehaviour
{
    public float moveDistance = 1.0f;
    public float moveSpeed = 1.0f;
    public float rotationAmount = 90.0f;
    public int iterations = 5;

    public bool usePerlinHeight = false;
    public float perlinFrequency = 1.0f;
    public float perlinAmplitude = 1.0f;

    public TrailRenderer trailRenderer = null;

    public string lString;
    public string lSuffix;
    public float timeOutInSeconds = 100.0f;
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
    private State startingState = new State();
    private float startingY;
    private float cachedVertexDistance;

    void OnValidate()
    {
        if(trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        startingY = transform.position.y;

        cachedVertexDistance = trailRenderer.minVertexDistance;

        if(usePerlinHeight)
        {
            Vector3 newPosition = transform.position;
            newPosition.y = Mathf.PerlinNoise(transform.position.x * perlinFrequency, transform.position.z * perlinFrequency) * perlinAmplitude + startingY;
            transform.position = newPosition;
        }

        startingState.position = transform.position;
        startingState.rotation = transform.rotation;

        lSystem.lSystemString = lString;
        for(int i = 0; i < ruleCharacters.Count; i++)
        {
            lSystem.replacementStrings.Add(ruleCharacters[i],ruleStrings[i]);
        }

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        lSystem.Iterate(iterations,timeOutInSeconds);
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Generation took {0} seconds to complete", stopwatch.Elapsed.TotalSeconds));

        lSystem.lSystemString += lSuffix;
        //UnityEngine.Debug.Log(lSystem.lSystemString);

        StartCoroutine(Draw());
    }

    // Update is called once per frame
    IEnumerator Draw()
    {
        yield return null;  // So first update can occur for TrailRenderer
        while(true)
        {
            char c = lSystem.lSystemString[index];
            switch(c)
            {
                case 'F':
                    yield return PenDown();
                    Vector3 targetPosition = transform.position + transform.forward * moveDistance;

                    if(usePerlinHeight)
                    {
                        targetPosition.y = Mathf.PerlinNoise(targetPosition.x * perlinFrequency, targetPosition.z * perlinFrequency) * perlinAmplitude + startingY;
                    }

                    do
                    {
                        transform.position = Vector3.MoveTowards(transform.position,targetPosition, moveSpeed * Time.deltaTime);
                        yield return null;
                    }
                    while (transform.position != targetPosition);

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
                    SaveState();
                    break;
                case ']':
                    yield return PenUp();
                    RestoreState();
                    break;
                default:
                    break;
            }
            index++;
            if(index >= lSystem.lSystemString.Length)
            {
                index = 0;
                yield return PenUp();
                transform.position = startingState.position;
                transform.rotation = startingState.rotation;
            }
        }
    }

    IEnumerator PenUp()
    {
        if(trailRenderer.emitting == true)
        {
            trailRenderer.emitting = false;
            yield return null;
            trailRenderer.AddPosition(transform.position);
            cachedVertexDistance = trailRenderer.minVertexDistance;
            trailRenderer.minVertexDistance = 1000000.0f;
        }
    }

    IEnumerator PenDown()
    {
        if(trailRenderer.emitting == false)
        {
            trailRenderer.AddPosition(transform.position);
            yield return null;
            trailRenderer.minVertexDistance = cachedVertexDistance;
            trailRenderer.emitting = true;
        }
    }

    void SaveState()
    {
        State newState;
        newState.position = transform.position;
        newState.rotation = transform.rotation;
        stateStack.Push(newState);
    }

    void RestoreState()
    {
        State newState = stateStack.Pop();
        transform.position = newState.position;
        transform.rotation = newState.rotation;
    }

}