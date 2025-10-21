using System.Collections;
using UnityEngine;

namespace Coding
{
    public class UIAppear : MonoBehaviour

    {
        [SerializeField] private GameObject offObject;
        [SerializeField] private GameObject uiTrigger;
        [SerializeField] private GameObject player;
        [SerializeField] float remainingTime;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && remainingTime > 0)
                StartCoroutine(StartTimer());

        }
    
        //Starts a timer that makes the hit box onenter and the UI disappear after a set amount of time
        IEnumerator StartTimer()
        {
            uiTrigger.SetActive(true);
            yield return new WaitForSeconds(remainingTime);
            uiTrigger.SetActive(false);
            offObject.SetActive(false);
        }
    }
}
