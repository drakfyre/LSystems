using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.ParticleSystem;

public class TurtleGraphics : MonoBehaviour
{
    [Header("Turtle Movement")]
    public float moveDistance = 1.0f;       // Distance to the next node on a Forward command
    public float moveSpeed = 1.0f;          // Distance the turtle moves per second
    public float rotationAmount = 90.0f;    // Rotation amount on a rotation command

    [Header("Perlin Height")]
    public bool usePerlinHeight = false;    // Whether or not to offset the height based on Perlin noise
    public float perlinFrequency = 1.0f;    // Frequency adjustment of the Perlin noise (higher makes narrower mountains/valleys)
    public float perlinAmplitude = 1.0f;    // Amplitude adjustment of the Perlin noise (higher makes taller mountains and deeper valleys)

    [Header("L-System Path Generation")]
    public string lString;                  // Starting string for LSystem, also known as "Axiom"
    public string lSuffix;                  // Suffix appended to the end of a generated LSystem, for convenience
    public int iterations = 5;              // Number of times to iterate
    public float timeOutInSeconds = 100.0f; // Timeout per iteration; when timed out the previous iteration will be used
    public List<char> ruleCharacters = new List<char>();    // List of characters to be replaced
    public List<string> ruleStrings = new List<string>();   // List of strings to replace characters with

    [Header("Required Component References")]
    public TrailRenderer trailRenderer = null;  // The TrailRenderer that draws the path; should be attached to this object or a child

    // Internals
    // Our LSystem instance
    private LSystem lSystem = new LSystem();

    // The current index the turtle is at in the LSystem string
    private int index = 0;

    // A structure to store position and rotation
    private struct State
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    // Stack of State structs (First In, Last Out)
    private Stack<State> stateStack = new Stack<State>();

    // Single state to store starting position/rotation
    private State startingState = new State();

    // The y height the object started at
    private float startingY;

    // Stores the vertex distance for when we change it temporarily
    private float cachedVertexDistance;


    // This function is called only in editor and is simply used so that the TrailRenderer will be set if one exists on the object or it's children
    // This function has no cost at runtime
    void OnValidate()
    {
        if(trailRenderer == null)
        {
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Save Y position for Perlin height offset
        startingY = transform.position.y;

        // Save current minVertexDistance just-in-case (So that PenUp doesn't HAVE to be called first)
        cachedVertexDistance = trailRenderer.minVertexDistance;

        if(usePerlinHeight)
        {
            // Set initial height based on Perlin noise at current position
            Vector3 newPosition = transform.position;
            newPosition.y = Mathf.PerlinNoise(transform.position.x * perlinFrequency, transform.position.z * perlinFrequency) * perlinAmplitude + startingY;
            transform.position = newPosition;
        }

        // Set startingState for looping
        startingState.position = transform.position;
        startingState.rotation = transform.rotation;

        // Set our internal LSystem variables based on editor variables
        lSystem.lSystemString = lString;
        for(int i = 0; i < ruleCharacters.Count; i++)
        {
            lSystem.replacementStrings.Add(ruleCharacters[i],ruleStrings[i]);
        }

        // Time our generation
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        // Generate based on iterations
        lSystem.Iterate(iterations,timeOutInSeconds);

        // Stop generation timer
        stopwatch.Stop();

        // Display generation time
        UnityEngine.Debug.Log(string.Format("Generation took {0} seconds to complete", stopwatch.Elapsed.TotalSeconds));

        // Set final LSystem suffix
        lSystem.lSystemString += lSuffix;

        // Display final generated LSystem string
        UnityEngine.Debug.Log(lSystem.lSystemString);

        StartCoroutine(Draw());
    }

    // Update is called once per frame
    IEnumerator Draw()
    {
        // Wait one frame so first update can occur for TrailRenderer
        yield return null;

        // This coroutine will never end; that's okay because it yields
        while(true)
        {
            // Get the current character
            char c = lSystem.lSystemString[index];

            // Decide what to do based on the character
            switch(c)
            {
                case 'F': // Draw Forward
                    yield return DrawForward(); // must be called this way because it may yield a frame inside
                    break;
                case '+': // Yaw right
                    transform.Rotate(0.0f,rotationAmount,0.0f);
                    break;
                case '-': // Yaw left
                    transform.Rotate(0.0f,-rotationAmount,0.0f);
                    break;
                case '^': // Pitch up
                    transform.Rotate(rotationAmount,0.0f,0.0f);
                    break;
                case '&': // Pitch down
                    transform.Rotate(-rotationAmount,0.0f,0.0f);
                    break;
                case '<': // Roll left
                    transform.Rotate(0.0f,0.0f,-rotationAmount);
                    break;
                case '>': // Roll right
                    transform.Rotate(0.0f,0.0f,rotationAmount);
                    break;
                case '\\':// Roll left (Duplicate for convenience); must use \\ because \ denotes a special character
                    transform.Rotate(0.0f,0.0f,-rotationAmount);
                    break;
                case '/': // Roll right (Duplicate for convenience)
                    transform.Rotate(0.0f,0.0f,rotationAmount);
                    break;
                case '|': // Turn around
                    transform.Rotate(0.0f,180.0f,0.0f);
                    break;
                case '[': // Save state to stack
                    SaveState();
                    break;
                case ']': // Restore state from stack
                    yield return PenUp(); // must be called this way because it may yield a frame inside
                    RestoreState();
                    break;
                default:  // Ignore any other characters
                    break;
            }

            // Next index
            index++;

            // Check for end of string and loop
            if(index >= lSystem.lSystemString.Length)
            {
                // Reset index
                index = 0;

                // Stop drawing
                yield return PenUp(); // must be called this way because it may yield a frame inside

                // Reset position to startingState
                transform.position = startingState.position;
                transform.rotation = startingState.rotation;
            }
        }
    }

    // Stop Drawing
    IEnumerator PenUp()
    {
        // Don't tweak things unless we have to, as this is hacky, and will cost a frame to do nothing
        if(trailRenderer.emitting == true)
        {
            // Stop trailRenderer from emitting
            trailRenderer.emitting = false;

            // Wait one frame so trailRenderer can catch up
            yield return null;

            // Manually add a new position at our position
            trailRenderer.AddPosition(transform.position);

            // Cache the current minVertex distance
            cachedVertexDistance = trailRenderer.minVertexDistance;

            // Set the minVertex distance to a huge value so new verticies will not be generated automatically
            trailRenderer.minVertexDistance = 1000000.0f;
        }
    }

    // Start Drawing
    IEnumerator PenDown()
    {
        // Don't tweak things unless we have to, as this is hacky, and will cost a frame to do nothing
        if(trailRenderer.emitting == false)
        {
            // Manually add a new position at our position
            trailRenderer.AddPosition(transform.position);

            // Wait a frame
            yield return null;

            // Restore the minVertex distance from the cache
            trailRenderer.minVertexDistance = cachedVertexDistance;

            // Start emitting again
            trailRenderer.emitting = true;
        }
    }

    void SaveState()
    {
        // Create new state
        State newState;

        // Set the state info based on our position and rotation
        newState.position = transform.position;
        newState.rotation = transform.rotation;

        // Push the state onto the stack (put it on top of the stack)
        stateStack.Push(newState);
    }

    void RestoreState()
    {
        // Pop state from stack (take it off the top of the stack)
        State newState = stateStack.Pop();

        // Set our position and rotation based on the state info
        transform.position = newState.position;
        transform.rotation = newState.rotation;
    }

    IEnumerator DrawForward()
    {
        // Put pen down (start drawing)
        yield return PenDown(); // must be called this way because it may yield a frame inside

        // Calculate target position based on moveDistance
        Vector3 targetPosition = transform.position + transform.forward * moveDistance;
        if(usePerlinHeight)
        {
            // Adjust height based on Perlin noise at current position
            targetPosition.y = Mathf.PerlinNoise(targetPosition.x * perlinFrequency, targetPosition.z * perlinFrequency) * perlinAmplitude + startingY;
        }

        // Move towards the target till you are there
        do
        {
            transform.position = Vector3.MoveTowards(transform.position,targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        while (transform.position != targetPosition);
    }

}