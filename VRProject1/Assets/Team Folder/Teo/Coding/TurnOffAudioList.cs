using System.Collections.Generic;
using UnityEngine;

public class TurnOffAudioList : MonoBehaviour
{
    [SerializeField] private List<GameObject> audioOff = new();
  
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (GameObject obj in audioOff) 
            { 
            
            obj.SetActive(false);
            
            }


        }

    }
}
