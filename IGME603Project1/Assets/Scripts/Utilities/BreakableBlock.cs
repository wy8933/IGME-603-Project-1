using UnityEngine;


[RequireComponent(typeof(Collision2D))]
public class BreakableBlock : MonoBehaviour
{
    public float requireForce = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) 
        {
            float velocity = collision.relativeVelocity.magnitude;

            if (velocity >= requireForce) 
            {
                Destroy(gameObject);
            }
        }
    }

}
