using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalPart : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource mainAudio;   
    [SerializeField] private AudioSource finalAudio;  

    [Header("Objects to enable/disable")]
    [SerializeField] private List<GameObject> objectsToEnable = new();

    [Header("Timings")]
    [SerializeField] private float interval = 4f;      
    [SerializeField] private float delayBetweenClips = 1.5f; 

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(FinaleRoutine());
    }

    private IEnumerator FinaleRoutine()
    {
        if (mainAudio == null || mainAudio.clip == null)
        {
            Debug.LogWarning("Main audio missing or no clip assigned.");
            yield break;
        }

        mainAudio.Play();
        float clipLength = mainAudio.clip.length;

        int index = 0;
        float elapsed = 0f;

        
        while (elapsed < clipLength)
        {
            yield return new WaitForSeconds(interval);
            elapsed += interval;

            if (index < objectsToEnable.Count)
            {
                objectsToEnable[index].SetActive(true);
                index++;
            }
        }

        
        yield return new WaitWhile(() => mainAudio.isPlaying);

        
        yield return new WaitForSeconds(delayBetweenClips);

        
        if (finalAudio != null)
            finalAudio.Play();

        
        if (finalAudio != null)
            yield return new WaitWhile(() => finalAudio.isPlaying);

        
        foreach (var obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
