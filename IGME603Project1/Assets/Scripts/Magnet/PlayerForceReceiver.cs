using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerForceReceiver : MonoBehaviour
{
    [SerializeField]
    private PlayerController _playerController;

    [Min(0f)] public float maxTotalForce = 100f;

    private Rigidbody2D _rb;

    private readonly List<MagnetField2D> _activeFields = new();

    private Vector2 _force = Vector2.zero;

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
        _force = totalForce;
    }

    public void SetPlayerState(MagnetState newState)
    {
        _playerController.SetMagnetState(newState);
    }

    public Vector2 GetForce()
    {
        return _force;
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
