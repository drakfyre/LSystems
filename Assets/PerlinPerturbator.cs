using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinPerturbator : MonoBehaviour
{
    public float amplitude = 100.0f;
    public float frequency = 1.0f;

    void LateUpdate()
    {
        Vector3 newPosition = transform.position;
        newPosition.y = Mathf.PerlinNoise(transform.position.x * frequency + x, transform.position.z * frequency + y) * amplitude;
        transform.position = newPosition;
    }
}
