using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class ItemPickUp : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string itemId = "item";
    [TextArea][SerializeField] private string dialogueText = "Picked up!";
    [SerializeField] private bool alsoListenHandGrab = false;

    [Header("Debug")]
    [SerializeField] private bool verbose = false;

    private readonly List<GrabInteractable> _grabs = new();
    private readonly List<HandGrabInteractable> _handGrabs = new();

    string Key => $"pickup_seen_{itemId}";

    void Awake()
    {
        if (string.IsNullOrEmpty(itemId)) itemId = gameObject.name;

        GetComponentsInChildren(true, _grabs);
        if (verbose) Debug.Log($"[FirstPickupAutoWire] Found {_grabs.Count} GrabInteractable(s) under {name}");

         if (alsoListenHandGrab) {
            GetComponentsInChildren(true, _handGrabs);
            if (verbose) Debug.Log($"[FirstPickupAutoWire] Found {_handGrabs.Count} HandGrabInteractable(s) under {name}");
        }
    }

    void OnEnable()
    {
        foreach (var g in _grabs)
        {
            g.WhenPointerEventRaised += OnPointerEvent;
            if (verbose) Debug.Log($"[FirstPickupAutoWire] Subscribed to {g.gameObject.name}");
        }

         if (alsoListenHandGrab)
             foreach (var hg in _handGrabs)
             {
               hg.WhenPointerEventRaised += OnPointerEvent;
               if (verbose) Debug.Log($"[FirstPickupAutoWire] Subscribed (hand) to {hg.gameObject.name}");
             }
    }

    void OnDisable()
    {
        foreach (var g in _grabs) g.WhenPointerEventRaised -= OnPointerEvent;
         foreach (var hg in _handGrabs) hg.WhenPointerEventRaised -= OnPointerEvent;
    }

    void OnPointerEvent(PointerEvent e)
    {
        if (e.Type != PointerEventType.Select) return;
        if (PlayerPrefs.GetInt(Key, 0) == 1) return;

        PlayerPrefs.SetInt(Key, 1);
        PlayerPrefs.Save();

       DialogueManager.Instance?.ShowDialogue(dialogueText);
        if (verbose) Debug.Log($"[FirstPickupAutoWire] First pickup → {itemId} | '{dialogueText}'");
    }
}
