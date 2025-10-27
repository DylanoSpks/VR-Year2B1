using UnityEngine;
using Oculus.Interaction; 

[RequireComponent(typeof(GrabInteractable))]
public class FirstTimePickupItem_Meta : MonoBehaviour
{
    [SerializeField] private string itemId;                   // unique per item
    [TextArea][SerializeField] private string dialogueText;  
    private GrabInteractable _grab;

    private string Key => $"pickup_seen_{(string.IsNullOrEmpty(itemId) ? name : itemId)}";
    private bool _subscribed;

    private void Awake()
    {
        _grab = GetComponent<GrabInteractable>();
        if (string.IsNullOrEmpty(itemId)) itemId = gameObject.name;
    }

    private void OnEnable()
    {
        if (_grab != null && !_subscribed)
        {
            _grab.WhenPointerEventRaised += OnPointerEvent;   // Meta Interaction SDK hook
            _subscribed = true;
        }
    }

    private void OnDisable()
    {
        if (_grab != null && _subscribed)
        {
            _grab.WhenPointerEventRaised -= OnPointerEvent;
            _subscribed = false;
        }
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        // Fire once, on first grab of this item
        if (evt.Type == PointerEventType.Select && PlayerPrefs.GetInt(Key, 0) == 0)
        {
            PlayerPrefs.SetInt(Key, 1);
            PlayerPrefs.Save();
            DialogueManager.Instance?.ShowDialogue(dialogueText);
        }
    }
}
