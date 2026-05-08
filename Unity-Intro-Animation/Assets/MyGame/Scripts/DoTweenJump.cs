using UnityEngine;
using DG.Tweening;  // DOTween namespace. "DG" stands for Demigiant (the developer).

/// <summary>
/// Makes a GameObject hop back and forth using DOTween's DOJump method.
/// Demonstrates how a tweening library compresses ~20 lines of timing code into
/// one chained call.
/// Attach to: PistonC in the Lab03_LockingMechanism scene.
/// </summary>
public class DoTweenJump : MonoBehaviour
{
    [SerializeField, Tooltip("How high the jump arcs.")]
    private float jumpPower = 2f;

    [SerializeField, Tooltip("How long each jump takes, in seconds.")]
    private float duration = 1f;

    [SerializeField, Tooltip("How far to jump sideways from the starting position.")]
    private float horizontalDistance = 2f;

    [SerializeField, Tooltip("The easing function — controls the 'personality' of the motion. Try OutBounce, InBack, Linear.")]
    private Ease easeType = Ease.OutBounce;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;

        // Kick off the loop. JumpRight chains JumpLeft via OnComplete, and
        // JumpLeft chains back to JumpRight, producing an infinite back-and-forth.
        JumpRight();
    }

    /// <summary>
    /// Jumps to the right, then chains JumpLeft() when complete.
    /// </summary>
    private void JumpRight()
    {
        Vector3 target = startPos + Vector3.right * horizontalDistance;

        // .DOJump animates the transform along an arc to `target` in `duration` seconds,
        //   completing `1` jump along the way.
        // .SetEase chooses the easing curve — OutBounce gives the satisfying "land" feel.
        // .OnComplete schedules JumpLeft when the tween finishes.
        // The chained-call style is DOTween's idiomatic Fluent Builder pattern.
        transform.DOJump(target, jumpPower, 1, duration)
                 .SetEase(easeType)
                 .OnComplete(JumpLeft);
    }

    /// <summary>
    /// Jumps back to the start position, then loops by calling JumpRight() again.
    /// </summary>
    private void JumpLeft()
    {
        transform.DOJump(startPos, jumpPower, 1, duration)
                 .SetEase(easeType)
                 .OnComplete(JumpRight);
    }

    /// <summary>
    /// IMPORTANT: kill all tweens on this transform when the GameObject is disabled
    /// or destroyed. DOTween tweens persist by default — leaving them alive on a
    /// destroyed object causes errors. Always kill in OnDisable for objects with a
    /// finite lifetime.
    /// </summary>
    private void OnDisable()
    {
        transform.DOKill();
    }
}