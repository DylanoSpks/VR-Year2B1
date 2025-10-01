using UnityEngine;
using System.Collections;

public class CorridorEventDoor : MonoBehaviour
{

    [SerializeField] private GameObject door;             // door to hide
    [SerializeField] private GameObject wallReplacement;  // wall to show
    [SerializeField] private Transform playerHead;        // VR camera (the HMD)

    private bool hasSwapped = false;

    void Start()
    {
        if (wallReplacement) wallReplacement.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasSwapped) return;
        if (!other.CompareTag("Player")) return;

        StartCoroutine(CheckAndSwap());
    }

    private IEnumerator CheckAndSwap()
    {
        // wait until the player is looking away
        while (IsFacingDoor())
            yield return null;

        // swap
        hasSwapped = true;
        if (door) door.SetActive(false);
        if (wallReplacement) wallReplacement.SetActive(true);
    }

    private bool IsFacingDoor()
    {
        if (!playerHead || !door) return false;

        Vector3 toDoor = (door.transform.position - playerHead.position).normalized;
        float dot = Vector3.Dot(playerHead.forward, toDoor);

        // dot > 0 = looking toward the door
        return dot > 0f;
    }
}