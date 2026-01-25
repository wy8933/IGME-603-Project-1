using UnityEngine;
using UnityEngine.SceneManagement; // Required for changing scenes

public class FinishPoint : MonoBehaviour
{
    // Type the exact name of the next scene in the Inspector
    [Tooltip("The name of the scene to load")]
    public string nextSceneName;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object is the Player
        if (collision.CompareTag("Player"))
        {
            // Load the specified scene
            SceneManager.LoadScene(nextSceneName);
        }
    }
}