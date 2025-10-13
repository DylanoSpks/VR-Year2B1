using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.Audio;

public class CorridorEventChange : MonoBehaviour
{
    [Header("Walls")]
    [SerializeField] private Renderer[] walls;    
    [SerializeField] private Material creepyMaterial;
    [SerializeField] private float flashDuration = 1.5f;

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpScare;

    [Header("Post Processing")]
    [SerializeField] private Volume creepyVolume; 
    [SerializeField] private float fadeSpeed = 2f;

    private Material[] originalMaterials;
    private bool hasTriggered;

    void Awake()
    {
        // Cache original wall materials
        if (walls == null || walls.Length == 0)
        {
            Debug.LogWarning("[CorridorEventChange] No walls assigned.");
            return;
        }

        originalMaterials = new Material[walls.Length];
        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i] == null) continue;
            originalMaterials[i] = walls[i].material;
        }

        if (creepyVolume != null)
            creepyVolume.weight = 0f; 
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        StartCoroutine(FlashSequence());
    }

    private IEnumerator FlashSequence()
    {

        if (audioSource != null && jumpScare != null)
            audioSource.PlayOneShot(jumpScare);

        foreach (var w in walls)
        {
            if (w != null)
                w.material = creepyMaterial;
        }

      
        if (creepyVolume != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * fadeSpeed;
                creepyVolume.weight = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
        }

        
        yield return new WaitForSeconds(flashDuration);

    
        if (creepyVolume != null)
        {
            float t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime * fadeSpeed;
                creepyVolume.weight = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            creepyVolume.weight = 0f;
        }

    
        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i] != null && originalMaterials[i] != null)
                walls[i].material = originalMaterials[i];
        }
    }
}
