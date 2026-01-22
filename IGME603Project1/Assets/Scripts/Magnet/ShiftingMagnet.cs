using UnityEngine;

public class ShiftingMagnet : MagnetField2D
{

    public float shiftingCD = 3;
    private float currentTimer = 0;

    public SpriteRenderer sprite;

    public void Update()
    {
        currentTimer += Time.deltaTime;

        if (currentTimer > shiftingCD) 
        {
            currentTimer -= shiftingCD;

            if (fieldState == MagnetState.Positive)
            {
                fieldState = MagnetState.Negative;
                sprite.color = Color.blue;
            }
            else 
            {
                fieldState = MagnetState.Positive;
                sprite.color = Color.red;
            }
        }
    }
}
