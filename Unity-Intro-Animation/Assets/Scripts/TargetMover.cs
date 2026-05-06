using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Moves a target GameObject through the scene from keyboard input.
/// Reads the project-wide "Look" action (Vector2) from Unity 6's default input actions
/// and translates the target on the world XZ plane (with optional vertical movement).
///
/// REQUIRES:
///  - Unity 6's default project-wide actions (specifically the "Look" action).
///
/// Attach to: the LookTarget GameObject in the Lab06_Chamber scene.
/// </summary>
public class TargetMover : MonoBehaviour
{
    [Header("Input")]
    [SerializeField, Tooltip("Which input action to read. 'Look' is the default project-wide action bound to mouse delta and gamepad right stick. You can change this to 'Move' if you want WASD-style target control.")]
    private string actionName = "Look";

    [Header("Movement")]
    [SerializeField, Tooltip("How fast the target moves in world units per second.")]
    private float moveSpeed = 3f;

    [SerializeField, Tooltip("Multiplier on the input. Mouse delta values are typically much smaller than gamepad stick values, so you may need to tune this.")]
    private float inputSensitivity = 0.05f;

    [Header("Vertical Control")]
    [SerializeField, Tooltip("Hold this key to raise the target vertically. Default: E.")]
    private Key raiseKey = Key.E;

    [SerializeField, Tooltip("Hold this key to lower the target vertically. Default: Q.")]
    private Key lowerKey = Key.Q;

    [SerializeField, Tooltip("Vertical movement speed when Raise/Lower is held.")]
    private float verticalSpeed = 2f;

    [Header("Bounds")]
    [SerializeField, Tooltip("Optional: limit how far from origin the target can travel. Set to 0 to disable.")]
    private float maxDistanceFromOrigin = 10f;

    [SerializeField, Tooltip("Optional: minimum vertical position. Prevents the target dropping below the floor.")]
    private float minHeight = 0.2f;

    // Cached reference to the input action.
    private InputAction lookAction;

    private void Awake()
    {
        // Find the named action in Unity 6's project-wide input actions.
        lookAction = InputSystem.actions.FindAction(actionName);
    }

    private void Update()
    {
        // Read the input. "Look" returns mouse delta (small frame-by-frame values)
        // or gamepad right-stick position (-1 to +1). We treat both the same way.
        Vector2 input = lookAction.ReadValue<Vector2>();

        // Compute horizontal movement (XZ plane).
        // Multiplied by inputSensitivity to normalise mouse-vs-gamepad scales,
        // then by moveSpeed and Time.deltaTime for frame-rate-independent motion.
        Vector3 horizontalDelta = new Vector3(
            input.x * inputSensitivity,
            0f,
            input.y * inputSensitivity
        ) * moveSpeed * Time.deltaTime;

        // Compute vertical movement from the raise/lower keys.
        float verticalDelta = 0f;
        if (Keyboard.current[raiseKey].isPressed)
        {
            verticalDelta += verticalSpeed * Time.deltaTime;
        }
        if (Keyboard.current[lowerKey].isPressed)
        {
            verticalDelta -= verticalSpeed * Time.deltaTime;
        }

        // Apply movement to the target.
        Vector3 newPosition = transform.position + horizontalDelta + Vector3.up * verticalDelta;

        // Enforce bounds: clamp distance from origin (XZ only) and minimum height.
        if (maxDistanceFromOrigin > 0f)
        {
            Vector2 horizontal = new Vector2(newPosition.x, newPosition.z);
            if (horizontal.magnitude > maxDistanceFromOrigin)
            {
                horizontal = horizontal.normalized * maxDistanceFromOrigin;
                newPosition.x = horizontal.x;
                newPosition.z = horizontal.y;
            }
        }
        newPosition.y = Mathf.Max(newPosition.y, minHeight);

        transform.position = newPosition;
    }
}