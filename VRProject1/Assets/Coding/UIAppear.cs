using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class UIAppear : MonoBehaviour


{
    [SerializeField] private GameObject UITrigger;
    [SerializeField] private GameObject player;
    [SerializeField] float remainingTime;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && remainingTime > 0)
            StartCoroutine(StartTimer());

    }
    
    IEnumerator StartTimer()
    {
        UITrigger.SetActive(true);
        yield return new WaitForSeconds(remainingTime);
        UITrigger.SetActive(false);
    }
}
