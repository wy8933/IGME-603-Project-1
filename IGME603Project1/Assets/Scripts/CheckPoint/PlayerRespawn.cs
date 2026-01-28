using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    // Store the current position to respawn at
    private Vector3 currentRespawnPoint;

    void Start()
    {
        // Set the initial spawn point to the player's starting position
        currentRespawnPoint = transform.position;
    }

    void Update()
    {
        // Check if the 'R' key is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
        }
    }

    // Public method for Checkpoints to call to update the spawn point
    public void SetRespawnPoint(Vector3 newPoint)
    {
        currentRespawnPoint = newPoint;
        Debug.Log("Respawn point updated!");
    }

    // Logic to move player back to the checkpoint
    void Respawn()
    {
        transform.position = currentRespawnPoint;

        // Reset velocity to zero to prevent momentum from carrying over after respawn
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}