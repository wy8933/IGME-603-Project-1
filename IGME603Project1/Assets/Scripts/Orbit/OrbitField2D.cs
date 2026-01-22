using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public sealed class OrbitField2D : MonoBehaviour
{
    [Min(0.01f)] public float captureRadius = 3f;
    public bool autoSyncTriggerRadius = true;

    [Header("Orbit")]
    [Min(0f)] public float orbitRadius = 0f;
    [Min(0f)] public float orbitSpeed = 6f;
    public bool clockwise = true;
    [Min(0f)] public float radialCorrectionStrength = 25f;

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
        _trigger.radius = captureRadius;
    }

    public Vector2 GetCenterPosition()
    {
        return (Vector2)transform.position;
    }
}
