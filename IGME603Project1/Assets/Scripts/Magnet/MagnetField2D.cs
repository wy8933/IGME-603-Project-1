using UnityEngine;


[RequireComponent(typeof(CircleCollider2D))]
public class MagnetField2D : MonoBehaviour
{
    public MagnetState fieldState = MagnetState.Positive;

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_trigger == null) _trigger = GetComponent<CircleCollider2D>();

        if (_trigger != null)
        {
            _trigger.isTrigger = true;

            if (autoSyncTriggerRadius)
                _trigger.radius = Mathf.Max(0.01f, effectRadius);
        }
    }
#endif

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
}