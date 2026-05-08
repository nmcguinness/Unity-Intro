using UnityEngine;

/// <summary>
/// Bobs a GameObject up and down using a sine wave.
/// This is the simplest possible procedural animation — pure maths over time.
/// Attach to: PistonA in the Lab03_LockingMechanism scene.
/// </summary>
public class BobUpDown : MonoBehaviour
{
    // [SerializeField] makes a private field visible and editable in the Inspector.
    // Tinker with these values at runtime to feel how each parameter changes the motion.

    [SerializeField, Tooltip("How fast the cube bobs (oscillations per second).")]
    private float speed = 2f;

    [SerializeField, Tooltip("How high/low the cube travels from its starting position, in world units.")]
    private float amplitude = 1f;

    // We cache the starting position once in Start().
    // If we computed motion relative to transform.position every frame, the cube
    // would drift because we'd be adding offsets on top of an already-moved position.
    private Vector3 startPos;

    // Start() is called once before the first frame. Perfect place to cache references.
    private void Start()
    {
        startPos = transform.position;
    }

    // Update() runs every frame. We compute a new Y offset from a sine wave.
    private void Update()
    {
        // Time.time = seconds since the game started.
        // Mathf.Sin returns a smooth oscillation between -1 and +1.
        // Multiplying time by `speed` controls how fast the wave oscillates.
        // Multiplying the result by `amplitude` controls how far up/down it travels.
        float yOffset = Mathf.Sin(Time.time * speed) * amplitude;

        // Set the cube's position to its starting position plus the Y offset.
        // Vector3.up is shorthand for new Vector3(0, 1, 0).
        transform.position = startPos + Vector3.up * yOffset;
    }
}