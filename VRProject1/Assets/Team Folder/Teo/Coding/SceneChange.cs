using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            int nextScene = currentScene == 0 ? 1 : 0;
            SceneManager.LoadScene(nextScene);
        }
    }
}

