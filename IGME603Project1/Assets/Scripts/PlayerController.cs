using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Tooltip("How far the right stick needs to be pushed up or down to activate the magnet.\nShould be a value between 0 (stick just barely pushed up or down) and 1 (stick pushed all the way up or down).")]
    [SerializeField]
    private float MAGNET_JOYSTICK_THRESHOLD = 0.1f;

    [Tooltip("Speed of the player when crawling along a magnetic surface.")]
    [SerializeField]
    private float CRAWL_SPEED = 100.0f;

    [SerializeField]
    private MagnetState _magnetState = MagnetState.Neutral;

    private Vector2 _movementInput = Vector2.zero;

    private Rigidbody2D _rigidbody;
    private PlayerForceReceiver _playerForceReceiver;


    [SerializeField] private float WALL_GRAVITY_SCALE = 0f;
    [SerializeField] private float NORMAL_GRAVITY_SCALE = 0.1f;

    [Header("Player Sprite")]
    public Sprite playerPositive;
    public Sprite PlayerNeutral;
    public Sprite playerNegative;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerForceReceiver = GetComponent<PlayerForceReceiver>();
    }

    //Gets the player's current magnet state (Neutral / Positive / Negative)
    public MagnetState GetMagnetState() {
        return _magnetState;
    }

    //Sets the player's current magnet state (Neutral / Positive / Negative)
    public void SetMagnetState(MagnetState ms)
    {
        _magnetState = ms;

        //Change the player's color based on the new state
        switch(ms)
        {
            case MagnetState.Positive:
                gameObject.GetComponent<SpriteRenderer>().sprite = playerPositive;
                break;
            case MagnetState.Negative:
                gameObject.GetComponent<SpriteRenderer>().sprite = playerNegative;
                break;
            default:
                gameObject.GetComponent<SpriteRenderer>().sprite = PlayerNeutral;
                break;
        }
    }

    private void FixedUpdate()
    {
        if(_magnetState == MagnetState.Neutral)
            _rigidbody.gravityScale = NORMAL_GRAVITY_SCALE;

        //Handle player movement input along a wall they are being pushed or pulled into.
        if (_movementInput != Vector2.zero && PlayerForceReceiver.Instance.GetIsTouchingSurface())
        {
            // Get the force on the player by magnets
            Vector2 totalForce = _playerForceReceiver.GetForce();
            bool hasForceDir = totalForce.sqrMagnitude >= 0.0001f;
            Vector2 forceDir = hasForceDir ? totalForce.normalized : Vector2.zero;


            ////Set up the filter to only track walls the character is being forced into
            //ContactFilter2D filter = new ContactFilter2D();
            //float forceAngle = Mathf.Atan2(totalForce.y, totalForce.x) * 180 / Mathf.PI;
            //float filterAngle = forceAngle + 180.0f; //the normals are opposite to the force
            //filter.SetNormalAngle(filterAngle - 90.0f, filterAngle + 90.0f); //check for normals within 90 degrees

            //Gather the player character's contact points matching the above filter
            List<ContactPoint2D> contacts = new List<ContactPoint2D>();
            _rigidbody.GetContacts(contacts);

            //Vector2 fastestMotion = Vector2.zero;
            int bestIndex = -1;
            float bestScore = -Mathf.Infinity;

            // Track whether we are simultaneously attached to a wall and a floor/ceiling 
            bool hasWall = false;
            bool hasFloorOrCeiling = false;

            // Combined surface motion when multiple valid surfaces exist
            Vector2 combinedMotion = Vector2.zero;

            // Only count contacts whose normals oppose the force direction
            const float attachThreshold = 0.35f;

            // Track the best wall contact and the best floor/ceiling contact separately
            int bestWallIndex = -1;
            float bestWallScore = -Mathf.Infinity;
            int bestFloorIndex = -1;
            float bestFloorScore = -Mathf.Infinity;

            for (int i = 0; i < contacts.Count; i++)
            {
                if (!contacts[i].collider.CompareTag("Surface")) continue;

                // higher = better match
                float score = hasForceDir ? Vector2.Dot(contacts[i].normal, -forceDir) : 1f;
                if (hasForceDir && score < attachThreshold) continue;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }

                //Calculate the surface using dot product to determine whether player is on wall or floor
                Vector2 n_i = contacts[i].normal.normalized;
                float upDot_i = Mathf.Abs(Vector2.Dot(n_i, Vector2.up));
                bool isFloorOrCeiling_i = upDot_i > 0.6f;
                bool isWall_i = !isFloorOrCeiling_i;

                hasWall |= isWall_i;
                hasFloorOrCeiling |= isFloorOrCeiling_i;

                // For corner movement, still get the tangent and a floor tangent even if one of them is not perfectly aligned with the force.
                if (score >= attachThreshold)
                {
                    if (isWall_i && score > bestWallScore)
                    {
                        bestWallScore = score;
                        bestWallIndex = i;
                    }
                    else if (isFloorOrCeiling_i && score > bestFloorScore)
                    {
                        bestFloorScore = score;
                        bestFloorIndex = i;
                    }
                }
                else
                {
                    // prevents losing left/right control at wall+ceiling intersections
                    if (isWall_i && bestWallIndex < 0)
                        bestWallIndex = i;
                    if (isFloorOrCeiling_i && bestFloorIndex < 0)
                        bestFloorIndex = i;
                }
            }

            //if (fastestMotion != Vector2.zero)
            //{
            //    _rigidbody.totalForce = Vector2.zero;
            //    _rigidbody.AddForce(fastestMotion * CRAWL_SPEED);
            //}

            if (bestIndex < 0) return;
            Vector2 n = contacts[bestIndex].normal.normalized;

            //Calculate the surface using dot product to determine whether player is on wall or floor
            float upDot = Mathf.Abs(Vector2.Dot(n, Vector2.up));
            bool isFloorOrCeiling = upDot > 0.6f;
            bool isWall = !isFloorOrCeiling;

            Vector2 tangent = Vector2.Perpendicular(n).normalized;

            // Get the axis input based on the surface type
            float axisInput = isWall ? _movementInput.y : _movementInput.x;

            Vector2 desiredWorldDir = isWall ? Vector2.up : Vector2.right;
            if (Vector2.Dot(tangent, desiredWorldDir) < 0f)
                tangent = -tangent;

            // Calculate and apply the crawl force to the player
            Vector2 crawlForce = tangent * axisInput * CRAWL_SPEED;

            // If there is both a wall and a floor/ceiling contact (corner), allow BOTH axes explicitly.
            if (hasWall && hasFloorOrCeiling)
            {
                Vector2 wallTangent = Vector2.zero;
                Vector2 floorTangent = Vector2.zero;

                // Find one wall tangent and one floor tangent from contacts
                for (int i = 0; i < contacts.Count; i++)
                {
                    if (!contacts[i].collider.CompareTag("Surface")) continue;

                    Vector2 n_i = contacts[i].normal.normalized;
                    float upDot_i = Mathf.Abs(Vector2.Dot(n_i, Vector2.up));
                    bool isFloorOrCeiling_i = upDot_i > 0.6f;
                    bool isWall_i = !isFloorOrCeiling_i;

                    if (isWall_i && wallTangent == Vector2.zero)
                    {
                        wallTangent = Vector2.Perpendicular(n_i).normalized;
                        if (Vector2.Dot(wallTangent, Vector2.up) < 0f) wallTangent = -wallTangent;
                    }

                    if (isFloorOrCeiling_i && floorTangent == Vector2.zero)
                    {
                        floorTangent = Vector2.Perpendicular(n_i).normalized;
                        if (Vector2.Dot(floorTangent, Vector2.right) < 0f) floorTangent = -floorTangent;
                    }

                    if (wallTangent != Vector2.zero && floorTangent != Vector2.zero)
                        break;
                }

                // Build motion from axes directly
                Vector2 cornerMotion = Vector2.zero;
                if (floorTangent != Vector2.zero) cornerMotion += floorTangent * _movementInput.x;
                if (wallTangent != Vector2.zero) cornerMotion += wallTangent * _movementInput.y;

                crawlForce = cornerMotion * CRAWL_SPEED;
            }

            _rigidbody.AddForce(crawlForce, ForceMode2D.Force);

            _rigidbody.gravityScale = (isWall && _magnetState != MagnetState.Neutral) ? WALL_GRAVITY_SCALE : NORMAL_GRAVITY_SCALE;

        }
    }

    public void UpdateMovement(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            _movementInput = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            _movementInput = Vector2.zero;
        }
    }

    public void UpdateMagnet(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            float magnetism = context.ReadValue<float>();
            if (magnetism > MAGNET_JOYSTICK_THRESHOLD)
            {
                SetMagnetState(MagnetState.Positive);
            }
            else if (magnetism < -MAGNET_JOYSTICK_THRESHOLD)
            {
                SetMagnetState(MagnetState.Negative);
            }
            else
            {
                SetMagnetState(MagnetState.Neutral);
            }
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            SetMagnetState(MagnetState.Neutral);
        }
    }
}
