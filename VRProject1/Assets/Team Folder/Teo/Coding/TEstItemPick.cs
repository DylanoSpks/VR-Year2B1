using System.Collections;
using UnityEngine;

public class PickupActions : MonoBehaviour
{
    [Header("UI")]
    [TextArea] [SerializeField] private string dialogueText = "Picked up!";
    [SerializeField] private float uiSeconds = 6f; 

    [Header("Other Object")]
    public GameObject objectToDisable;      

    [Header("Picked Object (this one)")]
    public GameObject pickedObject;        

    public void OnPickup()
    {
        
        var mgr = FirstTimePickupUIManager.Instance;
        if (mgr != null)
        {
            mgr.ShowDialogue(string.IsNullOrEmpty(dialogueText) ? "Picked up." : dialogueText);
        }
        else
        {
            Debug.LogWarning("[PickupActions] No FirstTimePickupUIManager found.");
        }

      
       

        
        GameObject target = pickedObject != null ? pickedObject : gameObject;
        StartCoroutine(DestroyAfterDelay(target, Mathf.Max(0.01f, uiSeconds)));
    }

    private IEnumerator DestroyAfterDelay(GameObject item, float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return new WaitForEndOfFrame(); 
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }
        if (item != null) Destroy(item);
    }
}
