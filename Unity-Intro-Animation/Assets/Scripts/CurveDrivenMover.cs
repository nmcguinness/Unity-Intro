using UnityEngine;

/// <summary>
/// Moves a GameObject up and down following the shape of an AnimationCurve.
/// Same kind of curve as you authored in Lab 1, but evaluated from code.
/// This is the bridge between authored animation and procedural animation.
/// Attach to: PistonB in the Lab03_LockingMechanism scene.
/// </summary>
public class CurveDrivenMover : MonoBehaviour
{
    [SerializeField, Tooltip("Author this curve in the Inspector. X axis = normalised time (0 to 1), Y axis = output value (multiplied by amplitude).")]
    // EaseInOut(0,0,1,1) gives us a sensible default smooth ramp from 0 to 1.
    // Click the curve field in the Inspector to edit it visually.
    private AnimationCurve heightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField, Tooltip("How long one full cycle takes, in seconds.")]
    private float duration = 2f;

    [SerializeField, Tooltip("Multiplier applied to the curve's output value.")]
    private float amplitude = 2f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Compute a normalised time `t` that loops from 0 to 1 over `duration` seconds.
        // The `%` (modulo) operator wraps the value back to 0 each time it hits `duration`,
        // and dividing by duration normalises the result into [0, 1) for curve evaluation.
        float t = (Time.time % duration) / duration;

        // Evaluate the curve at `t`. The curve decides what value comes out — a step,
        // a smooth ease, a wobble, anything you can draw in the curve editor.
        float curveValue = heightCurve.Evaluate(t);

        // Apply that value as a Y offset from the starting position.
        transform.position = startPos + Vector3.up * curveValue * amplitude;

    }
}