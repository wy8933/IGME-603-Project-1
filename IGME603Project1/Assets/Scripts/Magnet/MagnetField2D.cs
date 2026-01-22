using UnityEngine;


[RequireComponent(typeof(CircleCollider2D))]
public class MagnetField2D : MonoBehaviour
{
    public MagnetState fieldState = MagnetState.Positive;

    [Range(-180f, 180f)]
    public float pushDirectionAngleDeg = 0f;

    [Header("Force Settings")]
    [Min(0.01f)] public float effectRadius = 3f;
    [Min(0f)] public float baseStrength = 20f;
    public bool strongerNearCenter = true;


    public bool autoSyncTriggerRadius = true;
    private CircleCollider2D _trigger;

    private void Awake()
    {
        _trigger = GetComponent<CircleCollider2D>();
        _trigger.isTrigger = true;
        SyncRadius();
    }

    private void SyncRadius()
    {
        if (!autoSyncTriggerRadius || _trigger == null) return;
        _trigger.radius = effectRadius;
    }


    public Vector2 ComputeForce(Vector2 playerPos, MagnetState playerState)
    {
        if (playerState == MagnetState.Neutral || fieldState == MagnetState.Neutral)
            return Vector2.zero;

        Vector2 fieldPos = (Vector2)transform.position;
        Vector2 toPlayer = playerPos - fieldPos;
        float dist = toPlayer.magnitude;

        if (dist > effectRadius)
            return Vector2.zero;

        const float minDist = 0.001f;
        if (dist < minDist) dist = minDist;

        bool sameSign = (playerState == fieldState);

        Vector2 dir = Vector2.zero;

        if (!sameSign)
        {
            if (!PlayerForceReceiver.Instance.GetIsTouchingSurface()) 
            {
                dir = (fieldPos - playerPos).normalized;
            }
        }
        else
        {
            dir = -(fieldPos - playerPos).normalized;
        }

        float t = 1f - (dist / effectRadius);
        if (strongerNearCenter) t *= t;

        float mag = baseStrength * t;
        return dir * mag;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, effectRadius);

        // Visualize push direction arrow
        float rad = pushDirectionAngleDeg * Mathf.Deg2Rad;
        Vector3 arrowDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + arrowDir * Mathf.Min(effectRadius, 1.5f));
    }
}