using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Drives a character from keyboard/gamepad input.
/// Reads Unity 6's project-wide "Move" (Vector2) and "Sprint" (button) actions,
/// computes a speed value, writes it to the Animator's "Speed" parameter,
/// and moves the character through space.
///
/// Control scheme:
///  W / S  — move forward / backward along the character's facing direction.
///  A / D  — rotate (yaw) the character left / right.
///  This lets you complete a full 360° turn on the spot or while walking.
///
/// REQUIRES:
///  - An Animator on the same GameObject with a Float parameter called "Speed".
///  - Unity 6's default project-wide actions, which contain "Move" and "Sprint".
///
/// Attach to: the Character GameObject in the Lab04_Corridor scene.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    // ----- Inspector fields -------------------------------------------------

    [Header("Locomotion")]
    [SerializeField, Tooltip("World-space movement speed when walking, in units per second.")]
    private float walkSpeed = 2f;

    [SerializeField, Tooltip("World-space movement speed when sprinting, in units per second.")]
    private float runSpeed = 5f;

    [SerializeField, Tooltip("How fast `currentSpeed` ramps toward target. Higher = snappier, lower = floatier.")]
    private float acceleration = 8f;

    [SerializeField, Tooltip("Rotation speed in degrees per second when pressing A or D.")]
    private float turnSpeed = 180f;

    [Header("Animation Sync")]
    [SerializeField, Tooltip("The world-units-per-second the walk clip was authored for. Used to scale clip playback speed so the legs match the body's velocity. Ask your Blender lecturer if unsure — 2 is a common default.")]
    private float walkClipBaselineSpeed = 2f;

    // ----- Cached references -----------------------------------------------

    private Animator animator;

    // The two project-wide actions we'll read every frame.
    // Cached in Awake() so we look them up once, not every frame.
    private InputAction moveAction;
    private InputAction sprintAction;

    // ----- Runtime state ---------------------------------------------------

    // The smoothed speed value we feed the Animator each frame.
    // Smoothed (rather than raw) so the character ramps up/down naturally
    // instead of snapping between speeds.
    private float currentSpeed;

    // ----- Unity lifecycle methods -----------------------------------------

    private void Awake()
    {
        // Cache the Animator reference once. Calling GetComponent every frame is wasteful.
        animator = GetComponent<Animator>();

        // Look up the project-wide actions by name.
        // InputSystem.actions is auto-populated from the project-wide Action Asset
        // (the default in Unity 6 — no Inspector field needed, no Enable/Disable lifecycle).
        // FindAction returns the InputAction we can read every frame.
        moveAction = InputSystem.actions.FindAction("Move");
        sprintAction = InputSystem.actions.FindAction("Sprint");
    }

    private void Update()
    {
        // 1. Read inputs.
        //    moveInput.x = A/D (rotation), moveInput.y = W/S (forward/back).
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool wantsToSprint = sprintAction.IsPressed();

        float horizontal = moveInput.x; // A/D — yaw rotation
        float vertical = moveInput.y; // W/S — forward/back translation

        // 2. Compute the *target* speed for this frame.
        //    Speed is based only on forward/back input so that pressing A or D
        //    on the spot rotates the character without triggering the Walk animation.
        float targetSpeed = Mathf.Abs(vertical) * (wantsToSprint ? runSpeed : walkSpeed);

        // 3. Smoothly ramp currentSpeed toward targetSpeed.
        //    Mathf.Lerp here gives natural acceleration instead of a snap from 0 to full speed.
        //    Multiplying by Time.deltaTime makes the ramp frame-rate independent.
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        // 4. Push the speed into the Animator. The Lab 2 transitions will respond:
        //    - Speed > 0.1 → Walk
        //    - Speed < 0.1 → Idle
        animator.SetFloat("Speed", currentSpeed);

        // 5. Scale the walk clip's playback speed so the legs match the body's velocity.
        //    Without this, the character "skates" — feet stationary while the body slides.
        //    We treat walkClipBaselineSpeed as "the world-speed the clip was authored for"
        //    and scale linearly from there. The Mathf.Max prevents the clip from playing
        //    too slowly at low speeds (which looks worse than playing at normal speed).
        if (currentSpeed > 0.1f)
        {
            animator.speed = Mathf.Max(0.5f, currentSpeed / walkClipBaselineSpeed);
        }
        else
        {
            animator.speed = 1f;
        }

        // 6. Rotate: A/D yaw the character around its up axis.
        //    turnSpeed is in degrees per second, so a full 360° turn takes
        //    (360 / turnSpeed) seconds — about 2 seconds at the default 180.
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            transform.Rotate(Vector3.up, horizontal * turnSpeed * Time.deltaTime);
        }

        // 7. Translate: W/S move the character along its *current* facing direction.
        //    Using transform.forward (not world +Z) means the character always moves
        //    in the direction it is facing, regardless of how much it has rotated.
        if (Mathf.Abs(vertical) > 0.01f)
        {
            Vector3 moveDir = transform.forward * Mathf.Sign(vertical);
            transform.position += moveDir * currentSpeed * Time.deltaTime;
        }
    }
}