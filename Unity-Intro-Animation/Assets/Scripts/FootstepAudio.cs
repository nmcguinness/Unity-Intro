using UnityEngine;

/// <summary>
/// Plays a random clip from a pool each time OnFootstep() is called.
/// OnFootstep is invoked by an Animation Event placed on the walk clip —
/// Unity calls it at the exact frame you choose, so audio stays in sync
/// with the foot striking the ground without any manual timing code.
///
/// REQUIRES: an AudioSource on the same GameObject (auto-added via RequireComponent).
/// Attach to: the Character GameObject alongside PlayerController.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    [SerializeField, Tooltip("Pool of footstep clips to pick from at random. Add 2–4 variations to avoid a robotic repeating sound.")]
    private AudioClip[] footstepClips;

    [SerializeField, Range(0f, 1f), Tooltip("Playback volume for each footstep hit.")]
    private float volume = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Called by the Animation Event on the walk clip at each footfall frame.
    // Must be public so the Animator can find it by name at runtime.
    public void OnFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0)
            return;

        // Pick a random clip so consecutive steps don't sound identical.
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

        // PlayOneShot lets overlapping calls play simultaneously — important when
        // the character walks quickly or sprints and events fire close together.
        audioSource.PlayOneShot(clip, volume);
    }
}