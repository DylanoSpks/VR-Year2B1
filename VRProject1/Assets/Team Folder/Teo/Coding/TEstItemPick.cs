using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class PickupAutoWireHardened : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string itemId = "item";
    [TextArea][SerializeField] private string dialogueText = "Picked up!";
    [SerializeField] private bool verbose = true;

    private readonly HashSet<object> _subs = new();
    private readonly List<GrabInteractable> _grabs = new();
    private readonly List<HandGrabInteractable> _handGrabs = new();

    string Key => $"pickup_seen_{itemId}";
    Coroutine _scan;

    void Awake()
    {
        if (string.IsNullOrEmpty(itemId)) itemId = gameObject.name;
        ScanAndSubscribe();
    }

    void OnEnable()
    {
        if (_scan == null)
            _scan = StartCoroutine(RescanLoop());
    }

    void OnDisable()
    {
        if (_scan != null)
        {
            StopCoroutine(_scan);
            _scan = null;
        }
        UnsubAll();
    }

    IEnumerator RescanLoop()
    {
        var wait = new WaitForSeconds(0.5f);
        while (true)
        {
            ScanAndSubscribe();
            yield return wait;
        }
    }

    void ScanAndSubscribe()
    {
        _grabs.Clear();
        _handGrabs.Clear();
        GetComponentsInChildren(true, _grabs);
        GetComponentsInChildren(true, _handGrabs);

        if (verbose)
            Debug.Log($"[PickupAutoWire] {name}: grabs={_grabs.Count}, handGrabs={_handGrabs.Count}");

        foreach (var g in _grabs) TrySub(g);
        foreach (var hg in _handGrabs) TrySub(hg);
    }

    void TrySub(GrabInteractable g)
    {
        if (!g || _subs.Contains(g)) return;
        g.WhenPointerEventRaised += OnEvt;
        _subs.Add(g);
        if (verbose) Debug.Log($"[PickupAutoWire] Subscribed Grab on '{g.gameObject.name}'");
    }

    void TrySub(HandGrabInteractable hg)
    {
        if (!hg || _subs.Contains(hg)) return;
        hg.WhenPointerEventRaised += OnEvt;
        _subs.Add(hg);
        if (verbose) Debug.Log($"[PickupAutoWire] Subscribed HandGrab on '{hg.gameObject.name}'");
    }

    void UnsubAll()
    {
        foreach (var s in _subs)
        {
            if (s is GrabInteractable g) g.WhenPointerEventRaised -= OnEvt;
            if (s is HandGrabInteractable hg) hg.WhenPointerEventRaised -= OnEvt;
        }
        _subs.Clear();
    }

    void OnEvt(PointerEvent e)
    {
        if (verbose) Debug.Log($"[PickupAutoWire] EVENT {e.Type}");
        if (e.Type != PointerEventType.Select) return;

        // TEMP: disable one-time gate while debugging
        if (FirstTimePickupUIManager.Instance == null)
        {
            Debug.LogWarning("[PickupAutoWire] No UI manager found!");
            return;
        }

        FirstTimePickupUIManager.Instance.ShowDialogue(
            string.IsNullOrEmpty(dialogueText) ? "TEST" : dialogueText);

        // Uncomment these lines once it works to restore one-time behavior:
        // if (PlayerPrefs.GetInt(Key, 0) == 1) return;
        // PlayerPrefs.SetInt(Key, 1);
        // PlayerPrefs.Save();
        // FirstTimePickupUIManager.Instance.ShowDialogue(dialogueText);
    }
}
