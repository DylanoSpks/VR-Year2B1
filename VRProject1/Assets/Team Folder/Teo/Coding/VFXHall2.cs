using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

// Drop this on the Local Volume object for Hallway #2
public class VFXHall2 : MonoBehaviour
{
    [Header("Refs")]
    public Volume volume;           // assign local volume
    public Transform player;        // XR/Main camera
    public Transform reactPoint;    // center of weirdness

    [Header("Proximity")]
    public float reactRadius = 6f;  // meters

    [Header("Subtle Noise (always on, non-dizzy)")]
    public float vignetteNoise = 0.04f;      // max extra
    public float aberrationNoise = 0.02f;    // max extra
    public float exposureNoise = 0.06f;      // +/- range
    public float noiseSpeed = 0.35f;         // Perlin time scale

    [Header("Glitch Bursts (short, irregular)")]
    public Vector2 burstInterval = new Vector2(2.5f, 5.5f); // seconds
    public float burstDuration = 0.18f;
    public float burstAberration = 0.18f;    // spike
    public float burstVignette = 0.22f;      // add on top
    public float burstExposureDip = 0.25f;   // darker on burst
    public float lensKick = -0.22f;          // quick squeeze (negative = barrel)

    [Header("Depth of Field micro-breathing")]
    public float dofMin = 7.0f;
    public float dofMax = 8.2f;
    public float dofSpeed = 0.12f;           // slow

    // Internals
    ColorAdjustments colorAdj;
    ChromaticAberration ca;
    Vignette vignette;
    LensDistortion lens;
    DepthOfField dof;

    float baseVignette = 0.28f;
    float baseAberration = 0.03f;
    float baseExposure = 0f;
    float baseLens = -0.08f;

    float burstT = 0f;
    bool inBurst = false;
    float nextBurstAt = 0f;
    float tSeed;

    void Awake()
    {
        if (!volume) volume = GetComponent<Volume>();
        volume.profile.TryGet(out colorAdj);
        volume.profile.TryGet(out ca);
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out lens);
        volume.profile.TryGet(out dof);
        tSeed = Random.value * 1000f;
        ScheduleNextBurst();
    }

    void Update()
    {
        if (!player || !reactPoint) return;
        float dt = Time.deltaTime;

        // Proximity 0..1
        float dist = Vector3.Distance(player.position, reactPoint.position);
        float prox = Mathf.Clamp01(1f - dist / reactRadius);

        // Non-periodic noise (Perlin) — tiny, avoids “drunk” sway
        float t = tSeed + Time.time * noiseSpeed;
        float n1 = Mathf.PerlinNoise(t, 0.123f) * 2f - 1f;  // [-1..1]
        float n2 = Mathf.PerlinNoise(0.317f, t) * 2f - 1f;
        float n3 = Mathf.PerlinNoise(t * 0.71f, 0.777f) * 2f - 1f;

        // Base + small noise, scaled by proximity (closer = slightly stronger)
        float vNoise = baseVignette + Mathf.Clamp01(vignetteNoise * n1 * (0.4f + 0.6f * prox));
        float aNoise = baseAberration + Mathf.Clamp01(aberrationNoise * Mathf.Abs(n2) * (0.5f + 0.5f * prox));
        float eNoise = baseExposure + (exposureNoise * n3 * (0.3f + 0.7f * prox));

        // Apply subtle noise when not in burst
        if (!inBurst)
        {
            if (vignette) vignette.intensity.Override(Mathf.Clamp01(vNoise));
            if (ca) ca.intensity.Override(Mathf.Clamp01(aNoise));
            if (colorAdj) colorAdj.postExposure.Override(eNoise);
            if (lens) lens.intensity.Override(baseLens); // steady lens; no sway
        }

        // Handle burst timing (only when reasonably close so bursts don’t feel random)
        if (!inBurst && prox > 0.25f)
        {
            nextBurstAt -= dt;
            if (nextBurstAt <= 0f)
                StartCoroutine(Burst(prox));
        }

        // Micro DOF breathing — very slow, non-nausea
        if (dof)
        {
            float s = Mathf.PerlinNoise(0.91f, Time.time * dofSpeed); // [0..1]
            float f = Mathf.Lerp(dofMin, dofMax, s);
            // stronger when close, but still tiny
            f = Mathf.Lerp(f, Mathf.Lerp(dofMin, dofMax, 0.7f), prox * 0.3f);
            dof.focusDistance.Override(f);
        }
    }

    IEnumerator Burst(float prox)
    {
        inBurst = true;
        float t0 = 0f;

        // Precompute targets scaled by proximity (closer = harsher)
        float targetCA = Mathf.Clamp01(baseAberration + burstAberration * (0.5f + 0.5f * prox));
        float targetVig = Mathf.Clamp01(baseVignette + burstVignette * (0.5f + 0.5f * prox));
        float targetExp = baseExposure - burstExposureDip * (0.4f + 0.6f * prox);
        float targetLens = Mathf.Lerp(baseLens, lensKick, 0.6f + 0.4f * prox);

        // Quick attack, quicker release
        float attack = burstDuration * 0.35f;
        float release = burstDuration * 0.65f;

        // Attack
        while (t0 < attack)
        {
            t0 += Time.deltaTime;
            float k = t0 / attack;
            if (ca) ca.intensity.Override(Mathf.Lerp(baseAberration, targetCA, k));
            if (vignette) vignette.intensity.Override(Mathf.Lerp(baseVignette, targetVig, k));
            if (colorAdj) colorAdj.postExposure.Override(Mathf.Lerp(baseExposure, targetExp, k));
            if (lens) lens.intensity.Override(Mathf.Lerp(baseLens, targetLens, k));
            yield return null;
        }

        // Release
        float t1 = 0f;
        while (t1 < release)
        {
            t1 += Time.deltaTime;
            float k = t1 / release;
            if (ca) ca.intensity.Override(Mathf.Lerp(targetCA, baseAberration, k));
            if (vignette) vignette.intensity.Override(Mathf.Lerp(targetVig, baseVignette, k));
            if (colorAdj) colorAdj.postExposure.Override(Mathf.Lerp(targetExp, baseExposure, k));
            if (lens) lens.intensity.Override(Mathf.Lerp(targetLens, baseLens, k));
            yield return null;
        }

        inBurst = false;
        ScheduleNextBurst();
    }

    void ScheduleNextBurst()
    {
        nextBurstAt = Random.Range(burstInterval.x, burstInterval.y);
    }
}
