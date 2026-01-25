using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private SpriteRenderer sr;

    // Define colors (Editable in Inspector)
    // Default is Purple (R:0.5, G:0, B:0.5)
    public Color inactiveColor = new Color(0.5f, 0f, 0.5f, 1f);
    public Color activeColor = Color.green;

    private bool isActivated = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        // Initialize color to Purple
        sr.color = inactiveColor;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object is the Player and checkpoint is not yet active
        if (collision.CompareTag("Player") && !isActivated)
        {
            // Get the PlayerRespawn script from the player
            PlayerRespawn playerScript = collision.GetComponent<PlayerRespawn>();

            if (playerScript != null)
            {
                // 1. Update the player's respawn position to this object's position
                playerScript.SetRespawnPoint(transform.position);

                // 2. Change the sprite color to Green
                sr.color = activeColor;

                // 3. Mark as activated so it doesn't trigger again
                isActivated = true;
            }
        }
    }
}