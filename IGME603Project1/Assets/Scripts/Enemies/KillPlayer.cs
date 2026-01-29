using UnityEngine;

[RequireComponent(typeof(Collider2D))]

public class KillPlayer : MonoBehaviour
{
    private float requiredForce = Mathf.Infinity;

    private void Start()
    {
        BreakableBlock breakable = GetComponent<BreakableBlock>();
        if (breakable != null)
        {
            requiredForce = breakable.requireForce;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            float speed = collision.gameObject.GetComponent<Rigidbody2D>().linearVelocity.magnitude;
            if (speed < requiredForce) //if this entity would NOT be broken by the force
            {
                PlayerRespawn respawner = collision.gameObject.GetComponent<PlayerRespawn>();
                respawner.Respawn(); //Respawn the player
            }
        }
    }
}
