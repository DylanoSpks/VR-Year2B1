using System.Collections;
using UnityEngine;

namespace Coding
{
    public class UIAppear : MonoBehaviour

    {
        [SerializeField] private GameObject offObject;
        [SerializeField] private GameObject uiTrigger;
        [SerializeField] private GameObject uiTrigger2;
        [SerializeField] private GameObject player;
        [SerializeField] float remainingTime;
        [SerializeField] float remainingTime2;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && remainingTime > 0)
                Debug.Log("Fuck my chungus life");
                StartCoroutine(StartTimer());
        }
    
        //Starts a timer that makes the hit box onenter and the UI disappear after a set amount of time
        IEnumerator StartTimer()
        {
            uiTrigger.SetActive(true);
            yield return new WaitForSeconds(remainingTime);
            uiTrigger.SetActive(false);
            //After the first UI is done the 2nd start with its own timer that can be set within unity
            uiTrigger2.SetActive(true);
            yield return new WaitForSeconds(remainingTime2);
            uiTrigger2.SetActive(false);
            offObject.SetActive(false);
        }
        
    }
}
