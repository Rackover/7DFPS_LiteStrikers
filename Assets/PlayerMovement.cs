using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Player playerScript;

    public Vector2 VirtualJoystick => virtualStick;

    [SerializeField] private float gravity = 20f;
    [SerializeField] private SphereCollider worldCollider;
    [SerializeField] private new PlayerCamera camera;
    private float killZ => Game.i.killZ;
    [SerializeField] private float maxBoost = 40f;
    [SerializeField] private float boostAcceleration = 5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] TrailRenderer trail;
    [SerializeField] private float frictionAmount = 0.1f;
    [SerializeField] private float velocityReductionPerSpeed = 1.2f;

    public float Pitch101 { private set; get; }

    public float VelocityMagnitude => velocity.magnitude;

    public float Speed => currentBoost;

    public float SpeedAmount => currentBoost / maxBoost;

    public bool IsBoosting => isBoosting;

    public bool IsSpawned => playerScript.IsSpawned;

    public bool IsLocal => playerScript.IsLocal;

    Vector3 velocity = Vector3.zero;

    float verticalGravityVelocity = 0f;
    
    float currentBoost = 0f;

    bool isBoosting;

    Vector2 virtualStick;

    // Update is called once per frame
    void Update()
    {
        if (IsLocal)
        {
            var mousePosition = Game.i.MousePosition;

            virtualStick = new Vector2(mousePosition.x / Screen.width, mousePosition.y / Screen.height) * 2 - Vector2.one;

            if (IsSpawned)
            {
                //var inputMouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                //transform.Rotate(transform.up * inputMouse.x * Time.deltaTime * mouseSensitivity * 1.5f);

                isBoosting = Game.i.IsPressing;

                currentBoost = Mathf.Clamp(currentBoost + boostAcceleration * maxBoost * Time.deltaTime * (isBoosting ? 1f : -0.33f), 0f, maxBoost);

                //if (Input.GetButtonDown("Meow")) {
                //    Game.i.SendMeow();
                //}

                trail.time = SpeedAmount + 0.5f;
                trail.material.SetColor("_Color", new Color(1f, SpeedAmount, SpeedAmount, SpeedAmount));
            }
        }
    }


    private void FixedUpdate()
    {
        if (IsLocal)
        {
            if (IsSpawned)
            {
                if (transform.position.y < killZ)
                {
                    Debug.Log($"Kill Z was hit, killing myself!");
                    Game.i.EliminateMyself(playerScript);
                    velocity = Vector3.zero;
                    verticalGravityVelocity = 0f;
                }
                else
                {
                    velocity += transform.forward * currentBoost;

                    // Clamping XRot to avoid full up/down
                    var localRot = Mathf.Repeat(transform.eulerAngles.x + 180f, 360) - 180f;
                    float yStick = virtualStick.y;
                    if (localRot > 85f)
                    {
                        // We're going down
                        yStick = Mathf.Max(virtualStick.y, 0f);
                    }
                    else if (localRot < -85f)
                    {
                        yStick = Mathf.Min(virtualStick.y, 0f);
                    }

                    Vector2 fixedStick = new Vector2(
                        virtualStick.x * (1f - Mathf.Abs(localRot) / 90f),
                        yStick
                    );

                    Pitch101 = localRot / 90f;

                    transform.forward = Vector3.Lerp(transform.forward, transform.TransformDirection(fixedStick), rotationSpeed * Time.deltaTime * (currentBoost / maxBoost));

                    // Friction
                    velocity -= velocity.normalized * Mathf.Pow(velocity.magnitude, velocityReductionPerSpeed) * frictionAmount * Time.deltaTime;

                    if (isBoosting)
                    {
                        velocity += Vector3.down * gravity
                            * Mathf.Clamp01(1f - SpeedAmount * 0.75f) // Reduce gravity from speed
                            * Mathf.Abs(Pitch101)  // Reduce gravity from pitch
                            * Time.deltaTime;

                        verticalGravityVelocity = Mathf.Max(0f, verticalGravityVelocity - 80f * Time.deltaTime);
                    }
                    else
                    {
                        verticalGravityVelocity += gravity * Time.deltaTime;
                    }

                }

                ApplyMovementVector();
            }
        }
    }


    void ApplyMovementVector()
    {
        transform.position += (velocity + Vector3.down * verticalGravityVelocity) * Time.deltaTime;
    }
}
