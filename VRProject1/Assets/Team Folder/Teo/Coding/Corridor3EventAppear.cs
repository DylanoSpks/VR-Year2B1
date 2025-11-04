using UnityEngine;

public class Corridor3EventAppear : MonoBehaviour
{

    [SerializeField] GameObject hallway2;
    [SerializeField] GameObject hallway3;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            hallway2.SetActive(false);
            hallway3.SetActive(true);

        }
    }
}
