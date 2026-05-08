using UnityEngine;

/// <summary>
/// Pulses a Renderer's emission colour intensity over time, following an AnimationCurve.
/// Drives a *material property* using the same curve technique you used for transforms in Lab 3.
/// Attach to: the EnergyOrb GameObject in the Lab05_Chamber scene.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class EmissivePulse : MonoBehaviour
{
    // The Lit shader exposes its emission colour under the property name "_EmissionColor".
    // We cache the property ID for fast lookup (faster than re-hashing the string each frame).
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    [SerializeField, Tooltip("Author this curve so the pulse spikes at the moment of impact (around t=0.5 if the bounce loops over 1 second). X = normalised time (0–1), Y = brightness multiplier (0–1).")]
    private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField, Tooltip("How long one pulse cycle takes, in seconds. Match this to your bounce clip's length (typically 1s if your bounce uses 60 frames at 60 fps).")]
    private float duration = 1f;

    [SerializeField, Tooltip("Base hue of the emission. The curve will multiply this colour's intensity each frame.")]
    private Color baseEmission = Color.cyan;

    [SerializeField, Tooltip("Maximum intensity multiplier at the curve's peak. Pushed >1 for HDR bloom.")]
    private float maxIntensity = 4f;

    // We need a *unique* material instance for this object so we don't overwrite the asset
    // on disk and so multiple orbs could pulse independently.
    private Material materialInstance;

    // Time accumulator that loops within [0, duration].
    private float timer;

    private void Start()
    {
        // .material (singular) returns a per-instance copy. Editing it does not affect
        // the shared material asset. .sharedMaterial would, which is rarely what you want.
        materialInstance = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        // 1. Advance the timer and wrap it within [0, duration].
        timer = (timer + Time.deltaTime) % duration;

        // 2. Compute a normalised time t in [0, 1] for curve evaluation.
        float t = timer / duration;

        // 3. Evaluate the curve. The output is multiplied by maxIntensity to control glow strength.
        float intensity = pulseCurve.Evaluate(t) * maxIntensity;

        // 4. Push the new emission colour into the material instance.
        //    Multiplying a Color by a float scales each RGB channel by that amount.
        materialInstance.SetColor(EmissionColorID, baseEmission * intensity);
    }

    // Material instances created with .material are not garbage collected automatically.
    // We destroy our copy when this component is removed to avoid a memory leak across scene reloads.
    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}