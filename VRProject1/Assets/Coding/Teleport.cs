using UnityEngine;

namespace Coding
{
    public class Teleport : MonoBehaviour


    {
        [SerializeField] private GameObject player;
        [SerializeField] private GameObject teleportPos;
    

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                player.transform.position = teleportPos.transform.position;
            }
        }
    }
}
