using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;


public class ItemPickUp : MonoBehaviour
{
    [SerializeField] private string itemId;
    [TextArea][SerializeField] private string dialogueText;
    private GrabInteractable _grab;

    private string Key => $"pickup_seen_{(string.IsNullOrEmpty(itemId) ? name : itemId)}";

    private void Awake()
    {
        _grab = GetComponent<GrabInteractable>();
        if (string.IsNullOrEmpty(itemId)) itemId = gameObject.name;
    }

    private void OnEnable() => _grab.WhenPointerEventRaised += OnPointerEvent;
    private void OnDisable() => _grab.WhenPointerEventRaised -= OnPointerEvent;

    private void OnPointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select && PlayerPrefs.GetInt(Key, 0) == 0)
        {
            PlayerPrefs.SetInt(Key, 1);
            PlayerPrefs.Save();
            DialogueManager.Instance?.ShowDialogue(dialogueText);
        }
    }
}