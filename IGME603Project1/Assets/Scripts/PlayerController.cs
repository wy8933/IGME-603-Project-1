using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Tooltip("How far the right stick needs to be pushed up or down to activate the magnet.\nShould be a value between 0 (stick just barely pushed up or down) and 1 (stick pushed all the way up or down).")]
    [SerializeField] private const float _magnetJoystickThreshold = 0.1f;

    [SerializeField]
    private MagnetState _magnetState = MagnetState.Neutral;

    private Rigidbody2D _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
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

    public void Move(InputAction.CallbackContext context)
    {
        //currently unused, will eventually handle crawling
    }

    public void UpdateMagnet(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            float magnetism = context.ReadValue<float>();
            if (magnetism > _magnetJoystickThreshold)
            {
                SetMagnetState(MagnetState.Positive);
            }
            else if (magnetism < -_magnetJoystickThreshold)
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
