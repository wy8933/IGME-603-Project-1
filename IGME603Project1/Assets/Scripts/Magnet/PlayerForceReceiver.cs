using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerForceReceiver : MonoBehaviour
{
    public static PlayerForceReceiver Instance { get; private set; }

    [SerializeField]
    private PlayerController _playerController;

    [Min(0f)] public float maxTotalForce = 100f;

    private Rigidbody2D _rb;

    private readonly List<MagnetField2D> _activeFields = new();

    private Vector2 _force = Vector2.zero;

    // Track all Surface colliders currently touching the player.
    // This avoids false negatives when the player touches multiple colliders (tiles/edges/corners).
    private readonly HashSet<Collider2D> _touchingSurfaces = new();

    // Backing bool kept for compatibility with existing calls.
    private bool _isTouchingSurface = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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

        // Store the force for other systems (crawl/orbit logic) to reference this frame.
        // Use assignment so GetForce() represents the current combined field force.
        _force = totalForce;

        // Remove destroyed colliders so _isTouchingSurface doesn't get stuck true.
        _touchingSurfaces.RemoveWhere(c => c == null);

        // Derived state: touching any Surface collider means we are touching a surface.
        _isTouchingSurface = _touchingSurfaces.Count > 0;
    }

    public bool GetIsTouchingSurface()
    {
        return _isTouchingSurface;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Use the specific collider from the collision rather than the whole GameObject.
        // This is important when multiple colliders exist under one object.
        if (collision.collider != null && collision.collider.CompareTag("Surface"))
        {
            _touchingSurfaces.Add(collision.collider);
            _isTouchingSurface = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Ensures we remain "touching" even if Enter/Exit events happen at seams.
        if (collision.collider != null && collision.collider.CompareTag("Surface"))
        {
            _touchingSurfaces.Add(collision.collider);
            _isTouchingSurface = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.CompareTag("Surface"))
        {
            _touchingSurfaces.Remove(collision.collider);
            _isTouchingSurface = _touchingSurfaces.Count > 0;
        }
    }
}
