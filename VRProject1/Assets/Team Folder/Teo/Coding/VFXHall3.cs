using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VFXHall3 : MonoBehaviour
{
    public Volume volume;
    public Transform player;        // XR center-eye
    public Transform guiltPoint;    // hotspot
    [Min(0.1f)] public float reactRadius = 7f;

    [Header("Heartbeat (luminance-safe)")]
    [Range(30f, 120f)] public float bpm = 54f;
    [Range(0f, 1f)] public float exposureDip = 0.12f;   // was 0.18
    [Range(-1f, 1f)] public float baseExposure = 0.00f;  // was -0.2 (raise base)
    [Range(-1f, 1f)] public float minExposure = -0.15f;  // clamp floor

    [Header("Vignette")]
    [Range(0f, 1f)] public float baseVignette = 0.42f;  // was 0.50
    [Range(0f, 1f)] public float vignetteAdd = 0.06f;  // was 0.10
    [Range(0f, 1f)] public float maxVignette = 0.55f;   // cap

    [Header("Fog (global RenderSettings)")]
    [Range(0f, 0.02f)] public float fogDip = 0.0015f;      // was 0.0025
    [Range(0.5f, 1f)] public float minFogFactor = 0.8f;   // never go below 80% of base

    [Header("Color Grade")]
    [Range(-100f, 0f)] public float baseSat = -40f;        // was -60
    [Range(-100f, 0f)] public float maxDesat = -50f;       // clamp most extreme
    [Range(0f, 1f)] public float filterStrength = 0.15f;// 0..1 blend to redBias
    public Color redBias = new Color(0.95f, 0.7f, 0.7f, 1f); // light tint (not dark)

    // Components
    ColorAdjustments colorAdj;
    Vignette vignette;
    ShadowsMidtonesHighlights smh;

    // Internals
    float baseFogDensity;
    bool hadFog;

    void Awake()
    {
        if (!volume) volume = GetComponent<Volume>();
        volume.profile.TryGet(out colorAdj);
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out smh);

        hadFog = RenderSettings.fog;
        baseFogDensity = hadFog ? RenderSettings.fogDensity : 0f;

        // Safety caps
        maxVignette = Mathf.Clamp01(maxVignette);
        baseVignette = Mathf.Min(baseVignette, maxVignette);
        if (maxDesat > baseSat) maxDesat = baseSat; // ensure target <= base
    }

    void OnDisable()
    {
        if (hadFog) RenderSettings.fogDensity = baseFogDensity;
    }

    void Update()
    {
        if (!player || !guiltPoint) return;

        // Proximity 0..1
        float dist = Vector3.Distance(player.position, guiltPoint.position);
        float prox = Mathf.Clamp01(1f - dist / Mathf.Max(reactRadius, 0.001f));

        // Heartbeat (double tap)
        float pulse = HeartPulse(bpm, Time.time);

        // --- Color Adjustments ---
        if (colorAdj)
        {
            // Exposure with floor clamp
            float expTarget = baseExposure - exposureDip * pulse * (0.6f + 0.4f * prox);
            colorAdj.postExposure.Override(Mathf.Max(minExposure, expTarget));

            // Desat ramps but not past maxDesat
            float sat = Mathf.Lerp(baseSat, maxDesat, prox);
            colorAdj.saturation.Override(sat);

            // Gentle tint (blend from white to light red)
            Color tint = Color.Lerp(Color.white, redBias, prox * filterStrength);
            colorAdj.colorFilter.Override(tint);
        }

        // --- Vignette ---
        if (vignette)
        {
            float v = baseVignette + vignetteAdd * pulse * (0.5f + 0.5f * prox);
            vignette.intensity.Override(Mathf.Min(maxVignette, Mathf.Clamp01(v)));
        }

        // --- Fog ---
        if (hadFog)
        {
            float minAllowed = baseFogDensity * minFogFactor;
            float target = Mathf.Max(minAllowed, baseFogDensity - fogDip * pulse * (0.6f + 0.4f * prox));
            RenderSettings.fogDensity = target;
        }

        // --- Midtone red bias (kept subtle; doesn’t tank luminance) ---
        if (smh != null)
        {
            var mids = smh.midtones.value;
            mids.x = Mathf.Lerp(0.05f, 0.09f, prox);
            mids.y = Mathf.Lerp(-0.02f, -0.035f, prox);
            mids.z = Mathf.Lerp(0.02f, 0.03f, prox);
            smh.midtones.Override(mids);
        }
    }

    float HeartPulse(float bpmVal, float time)
    {
        float f = bpmVal / 60f;
        float a = Mathf.Abs(Mathf.Sin(time * f * Mathf.PI * 2f));
        float b = Mathf.Abs(Mathf.Sin((time + 0.2f) * f * Mathf.PI * 2f));
        return Mathf.Clamp01(a * 0.7f + b * 0.3f);
    }
}
