using UnityEngine;

public class Hover : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] private float amplitude = 0.5f; // how far up and down
    [SerializeField] private float frequency = 1f;   // speed of the hover

    [Header("Scale Settings")]
    [SerializeField] private float scaleAmplitude = 0.1f; // how much it grows/shrinks
    [SerializeField] private float scaleFrequency = 1f;   // speed of scaling

    private Vector3 startPos;
    private Vector3 startScale;

    void Start()
    {
        // Store the starting position and scale
        startPos = transform.position;
        startScale = transform.localScale;
    }

    void Update()
    {
        // Hover movement
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        // Breathing scale
        float scaleFactor = 1 + Mathf.Sin(Time.time * scaleFrequency) * scaleAmplitude;
        transform.localScale = startScale * scaleFactor;
    }
}
