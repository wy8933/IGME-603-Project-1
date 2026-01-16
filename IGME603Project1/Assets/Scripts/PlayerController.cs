using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Tooltip("How far the right stick needs to be pushed up or down to activate the magnet.\nShould be a value between 0 (stick just barely pushed up or down) and 1 (stick pushed all the way up or down).")]
    [SerializeField]
    private const float MAGNET_JOYSTICK_THRESHOLD = 0.1f;

    [Tooltip("Speed of the player when crawling along a magnetic surface.")]
    [SerializeField]
    private const float CRAWL_SPEED = 2.0f;

    [SerializeField]
    private MagnetState _magnetState = MagnetState.Neutral;

    private Vector2 _movementInput = Vector2.zero;

    private Rigidbody2D _rigidbody;
    private PlayerForceReceiver _playerForceReceiver;

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
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                break;
            case MagnetState.Negative:
                gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
                break;
            default:
                gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                break;
        }
    }

    private void FixedUpdate()
    {
        //Handle player movement input along a wall they are being pushed or pulled into.
        if (_movementInput != Vector2.zero)
        {
            //Set up the filter to only track walls the character is being forced into
            Vector2 totalForce = _playerForceReceiver.GetForce();
            if (totalForce != Vector2.zero)
            {
                ContactFilter2D filter = new ContactFilter2D();
                float forceAngle = Mathf.Atan2(totalForce.y, totalForce.x) * 180 / Mathf.PI;
                float filterAngle = forceAngle + 180.0f; //the normals are opposite to the force
                filter.SetNormalAngle(filterAngle - 90.0f, filterAngle + 90.0f); //check for normals within 90 degrees

                //Gather the player character's contact points matching the above filter
                List<ContactPoint2D> contacts = new List<ContactPoint2D>();
                _rigidbody.GetContacts(filter, contacts);

                Vector2 fastestMotion = Vector2.zero;
                foreach (ContactPoint2D contact in contacts)
                {
                    //Get the vector perpendicular to the collision normal
                    //(this will be parallel to the wall)
                    Vector2 normalPerpendicular = Vector2.Perpendicular(contact.normal);

                    //Project the player's input onto that vector to get the crawling motion
                    Vector2 motion = normalPerpendicular * Vector2.Dot(_movementInput, normalPerpendicular);

                    //If there are multiple possible walls to crawl along, only crawl along one.
                    //Choose the one that would move the player character the fastest.
                    //This crawling direction should most closely match the player's input.
                    if (fastestMotion.magnitude < motion.magnitude)
                    {
                        fastestMotion = motion;
                    }
                }

                if (fastestMotion != Vector2.zero)
                {
                    _rigidbody.totalForce = Vector2.zero;
                    _rigidbody.AddForce(fastestMotion * CRAWL_SPEED);
                }
            }
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
