using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerOrbitReceiver : MonoBehaviour
{

    [SerializeField]
    private PlayerController _playerController;

    [Header("Orbit")]
    public bool orbitEnabled = true;
    [Min(0f)] public float orbitSpeed = 6f;
    [Min(0f)] public float orbitRadius = 0f;
    public bool clockwise = true;
    [Min(0f)] public float radialCorrectionGain = 3f;
    [Min(0f)] public float maxAcceleration = 40f;
    public bool requireOrbitFieldTrigger = true;

    private Rigidbody2D _rb;

    private OrbitField2D _activeOrbitField;
    private float _lockedOrbitRadius;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!orbitEnabled) return;
        if (!(_playerController.GetMagnetState() == MagnetState.Neutral)) return;
        if (requireOrbitFieldTrigger && _activeOrbitField == null) return;

        // Orbit center position
        Vector2 center = (_activeOrbitField != null)
            ? (Vector2)_activeOrbitField.transform.position
            : Vector2.zero;

        // Radial points from the center to the player
        Vector2 pos = _rb.position;
        Vector2 radial = pos - center;

        // Distance from center to player
        float dist = radial.magnitude;
        if (dist < 0.001f) return;

        // Radial direction
        Vector2 radialDir = radial / dist;
        
        //Tangent direction is perpendicular to radial direction, which is the direction that produces circular motion
        Vector2 tangentDir = new Vector2(-radialDir.y, radialDir.x);
        if (clockwise) tangentDir = -tangentDir;

        //Target orbit radius
        float targetRadius = (_lockedOrbitRadius > 0f) ? _lockedOrbitRadius : dist;

        // positive means too far from center, negative means too near the center
        float radiusError = dist - targetRadius;

        // Desired radial velocity that push player toward target radius
        Vector2 desiredRadialVelocity = (-radialDir) * (radiusError * radialCorrectionGain);

        // Calculate the desired overall orbit velocity
        Vector2 desiredVelocity = tangentDir * orbitSpeed + desiredRadialVelocity;

        // Accelerate toward desired velocity
        Vector2 currentVelocity = _rb.linearVelocity;
        Vector2 desiredAcceleration = (desiredVelocity - currentVelocity) / Time.fixedDeltaTime;

        //prevent the speed to be too fast
        float accelMag = desiredAcceleration.magnitude;
        if (accelMag > maxAcceleration && accelMag > 0.0001f)
        {
            desiredAcceleration = desiredAcceleration / accelMag * maxAcceleration;
        }

        _rb.AddForce(desiredAcceleration * _rb.mass, ForceMode2D.Force);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out OrbitField2D field)) return;

        _activeOrbitField = field;

        orbitSpeed = field.orbitSpeed;
        clockwise = field.clockwise;

        Vector2 center = (Vector2)field.transform.position;
        float captureDistance = Vector2.Distance(_rb.position, center);

        _lockedOrbitRadius = (field.orbitRadius > 0f) ? field.orbitRadius : captureDistance;
        if (orbitRadius > 0f) _lockedOrbitRadius = orbitRadius;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_activeOrbitField == null) return;
        if (!other.TryGetComponent(out OrbitField2D field)) return;
        if (field != _activeOrbitField) return;

        _activeOrbitField = null;
    }
}
