using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public Player playerScript;

    public Vector2 VirtualJoystick => virtualStick;

    [SerializeField] private float gravity = 20f;
    [SerializeField] private SphereCollider worldCollider;
    [SerializeField] private new PlayerCamera camera;
    [SerializeField] private float killZ = -50f;
    [SerializeField] private float maxBoost = 40f;
    [SerializeField] private float boostAcceleration = 5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] TrailRenderer trail;

    public float SpeedAmount => currentBoost / maxBoost;

    public bool IsBoosting => isBoosting;

    public bool IsLocal => playerScript.IsLocal;

    Vector3 velocity = Vector3.zero;

    float currentBoost = 0f;

    bool isBoosting;

    Vector2 virtualStick;

    // Start is called before the first frame update
    void Start() {
        if (IsLocal)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    // Update is called once per frame
    void Update() {
        if (IsLocal)
        {
            Cursor.visible = false;
            //var inputMouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            //transform.Rotate(transform.up * inputMouse.x * Time.deltaTime * mouseSensitivity * 1.5f);

            var mousePosition = Input.mousePosition;
            isBoosting = Input.GetMouseButton(0);

            if (Game.i.IsMobile)
            {
                if (Input.touchCount > 0)
                {
                    var touch = Input.GetTouch(0);
                    mousePosition = touch.position;

                    isBoosting = true;
                }
                else
                {
                    isBoosting = false;
                }
            }

            virtualStick = new Vector2(mousePosition.x / Screen.width, mousePosition.y / Screen.height) * 2 - Vector2.one;

            currentBoost = Mathf.Clamp(currentBoost + boostAcceleration * maxBoost * Time.deltaTime * (isBoosting ? 1f : -1f), 0f, maxBoost);

            //if (Input.GetButtonDown("Meow")) {
            //    Game.i.SendMeow();
            //}

            trail.time = SpeedAmount + 0.5f;
            trail.material.SetColor("_Color", new Color(1f, SpeedAmount, SpeedAmount, SpeedAmount));
        }
    }


    private void FixedUpdate() {
        if (IsLocal)
        {
            if (transform.position.y < killZ)
            {
                Debug.Log($"Kill Z was hit, resetting position!");
                transform.position = new Vector3(5f, 2f, 5F);
                velocity = Vector3.zero;
            }
            else
            {
                velocity += transform.forward * currentBoost * Time.deltaTime;

                // Clamping XRot to avoid full up/down
                var localRot = transform.eulerAngles;
                if (localRot.x < 260f)
                {
                    // We're going down
                    if (localRot.x > 85f)
                    {
                        virtualStick.y = Mathf.Max(virtualStick.y, 0f);
                    }
                }
                else if (localRot.x > 270f)
                {
                    if (localRot.x < 275f)
                    {
                        virtualStick.y = Mathf.Min(virtualStick.y, 0f);
                    }
                }

                transform.forward = Vector3.Lerp(transform.forward, transform.TransformDirection(virtualStick), rotationSpeed * Time.deltaTime * (currentBoost / maxBoost));


                velocity *= 0.95f; // Friction

                velocity += Vector3.down * gravity * Mathf.Clamp01(1f - SpeedAmount * 0.75f) * Time.deltaTime;
            }

            ApplyMovementVector();
        }
    }


    void ApplyMovementVector() {

        transform.position += velocity;
    }
}
