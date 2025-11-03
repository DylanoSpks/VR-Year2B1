using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VFXHall3 : MonoBehaviour
{
    public Volume volume;
    public Transform player;
    public Transform guiltPoint;
    public float reactRadius = 7f;

    [Header("Heartbeat")]
    public float bpm = 54f;
    public float exposureDip = 0.18f;
    public float vignetteAdd = 0.10f;
    public float fogDip = 0.0025f;      // global fog delta on beat (tiny)
    public float minFog = 0.0f;         // clamp

    [Header("Proximity")]
    public float maxDesat = -85f;
    public Color redBias = new Color(0.55f, 0.05f, 0.05f, 1f);

    ColorAdjustments colorAdj;
    Vignette vignette;
    ShadowsMidtonesHighlights smh;

    float baseExposure = -0.2f;
    float baseVignette = 0.5f;
    float baseSat = -60f;
    Color baseFilter = new Color(0.47f, 0.08f, 0.08f, 1f);

    float baseFogDensity;
    float t;

    void Awake()
    {
        if (!volume) volume = GetComponent<Volume>();
        volume.profile.TryGet(out colorAdj);
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out smh);
        baseFogDensity = RenderSettings.fog ? RenderSettings.fogDensity : 0f;
    }

    void OnDisable()
    {
        // restore fog density
        if (RenderSettings.fog) RenderSettings.fogDensity = baseFogDensity;
    }

    void Update()
    {
        if (!player || !guiltPoint) return;

        float dist = Vector3.Distance(player.position, guiltPoint.position);
        float prox = Mathf.Clamp01(1f - dist / reactRadius);

        // proximity grading
        if (colorAdj)
        {
            colorAdj.saturation.Override(Mathf.Lerp(baseSat, maxDesat, prox));
            colorAdj.colorFilter.Override(Color.Lerp(baseFilter, redBias, prox * 0.8f));
        }

        // heartbeat
        t += Time.deltaTime;
        float pulse = HeartPulse(bpm, t); // 0..1 two-tap shape

        if (colorAdj)
            colorAdj.postExposure.Override(baseExposure - exposureDip * pulse * (0.6f + 0.4f * prox));

        if (vignette)
            vignette.intensity.Override(Mathf.Clamp01(baseVignette + vignetteAdd * pulse * (0.5f + 0.5f * prox)));

        if (RenderSettings.fog)
        {
            float d = Mathf.Max(minFog, baseFogDensity - fogDip * pulse * (0.6f + 0.4f * prox));
            RenderSettings.fogDensity = d;
        }

        // subtle midtone push with proximity
        if (smh != null)
        {
            var mids = smh.midtones.value;
            mids.x = 0.05f + prox * 0.04f;
            mids.y = -0.02f - prox * 0.01f;
            mids.z = 0.02f + prox * 0.01f;
            smh.midtones.Override(mids);
        }
    }

    float HeartPulse(float bpm, float time)
    {
        float f = bpm / 60f;
        // double beat: quick pulse + aftershock
        float a = Mathf.Abs(Mathf.Sin(time * f * Mathf.PI * 2f));
        float b = Mathf.Abs(Mathf.Sin((time + 0.2f) * f * Mathf.PI * 2f));
        return Mathf.Clamp01(a * 0.7f + b * 0.3f);
    }
}
