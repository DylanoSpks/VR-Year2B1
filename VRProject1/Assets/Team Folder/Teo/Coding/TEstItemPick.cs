using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
// If you also use HandGrabInteractable, uncomment the next line and mirror the subscription.
using Oculus.Interaction.HandGrab;

public class FirstPickupAutoWire : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string itemId = "item";
    [TextArea][SerializeField] private string dialogueText = "Picked up!";
    [SerializeField] private bool verbose = false;

    private readonly List<GrabInteractable> grabs = new();
    private readonly List<HandGrabInteractable> handGrabs = new();

    private string Key => $"pickup_seen_{itemId}";

    private void Awake()
    {
        if (string.IsNullOrEmpty(itemId)) itemId = gameObject.name;

        GetComponentsInChildren(true, grabs);
        if (verbose) Debug.Log($"[AutoWire] Found {grabs.Count} GrabInteractable(s) under '{name}'");

        // If using hand grab too:
        GetComponentsInChildren(true, handGrabs);
        if (verbose) Debug.Log($"[AutoWire] Found {handGrabs.Count} HandGrabInteractable(s) under '{name}'");
    }

    private void OnEnable()
    {
        foreach (var g in grabs)
        {
            g.WhenPointerEventRaised += OnEvt;
            if (verbose) Debug.Log($"[AutoWire] Subscribed to '{g.gameObject.name}'");
        }
        foreach (var hg in handGrabs)
         {
             hg.WhenPointerEventRaised += OnEvt;
             if (verbose) Debug.Log($"[AutoWire] Subscribed (hand) to '{hg.gameObject.name}'");
         }
    }

    private void OnDisable()
    {
        foreach (var g in grabs) g.WhenPointerEventRaised -= OnEvt;
         foreach (var hg in handGrabs) hg.WhenPointerEventRaised -= OnEvt;
    }

    private void OnEvt(PointerEvent e)
    {
        if (e.Type != PointerEventType.Select) return;

#if !UNITY_EDITOR
        // Device: force-show (ignore one-time while debugging)
        FirstTimePickupUIManager.Instance?.ShowDialogue(string.IsNullOrEmpty(dialogueText) ? "DEVICE TEST" : dialogueText);
        if (verbose) Debug.Log("[AutoWire] DEVICE Select → ShowDialogue()");
        return;
#endif

        // Editor: one-time gate
        if (PlayerPrefs.GetInt(Key, 0) == 1) return;
        PlayerPrefs.SetInt(Key, 1); PlayerPrefs.Save();
        FirstTimePickupUIManager.Instance?.ShowDialogue(dialogueText);
        if (verbose) Debug.Log($"[AutoWire] EDITOR first pickup → {itemId}");
    }
}
