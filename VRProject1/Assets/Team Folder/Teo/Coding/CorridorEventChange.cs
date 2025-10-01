using UnityEngine;
using System.Collections;

public class CorridorEventChange : MonoBehaviour
{
    [SerializeField] private Renderer[] walls;
    [SerializeField] private Material creepyMaterial;
    [SerializeField] private Light newLight;
    private Material[] originalMaterials;
    private bool hasTriggered = false;

    void Start()
    {
        originalMaterials = new Material[walls.Length];
        for (int i = 0; i < walls.Length; i++)
            originalMaterials[i] = walls[i].material;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        Debug.Log("PlayerTriggeredEvent");
        hasTriggered = true;
        StartCoroutine(FlashWalls());
    }

    private IEnumerator FlashWalls()
    {
        foreach (var w in walls)
            w.material = creepyMaterial;
        newLight.enabled = false;
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < walls.Length; i++)
            walls[i].material = originalMaterials[i];
        newLight.enabled = true;
    }
}