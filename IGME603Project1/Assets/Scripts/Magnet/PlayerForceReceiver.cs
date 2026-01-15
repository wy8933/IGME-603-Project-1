using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerForceReceiver : MonoBehaviour
{
    [SerializeField]
    private PlayerController _playerController;

    [Min(0f)] public float maxTotalForce = 100f;

    [Tooltip("Extra damping applied to velocity each FixedUpdate. 0 = none.")]
    public float velocityDamping = 0f;

    private Rigidbody2D _rb;

    private readonly List<MagnetField2D> _activeFields = new();

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 playerPos = _rb.position;

        Vector2 totalForce = Vector2.zero;

        for (int i = _activeFields.Count - 1; i >= 0; i--)
        {
            if (_activeFields[i] == null)
            {
                _activeFields.RemoveAt(i);
                continue;
            }

            totalForce += _activeFields[i].ComputeForce(playerPos, _playerController.GetMagnetState());
        }

        float mag = totalForce.magnitude;
        if (mag > maxTotalForce && mag > 0.0001f)
        {
            totalForce = totalForce / mag * maxTotalForce;
        }

        _rb.AddForce(totalForce, ForceMode2D.Force);

        if (velocityDamping > 0f)
        {
            float factor = Mathf.Clamp01(1f - velocityDamping * Time.fixedDeltaTime);
            _rb.linearVelocity *= factor;
        }
    }

    public void SetPlayerState(MagnetState newState)
    {
        _playerController.SetMagnetState(newState);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out MagnetField2D field))
        {
            if (!_activeFields.Contains(field))
                _activeFields.Add(field);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out MagnetField2D field))
        {
            _activeFields.Remove(field);
        }
    }
}
