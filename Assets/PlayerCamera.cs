using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PlayerCamera : MonoBehaviour {
    [SerializeField] Player playerScript;

    [SerializeField] private float fov = 80f;
    [SerializeField] private float boostFov = 110f;
    [SerializeField] private float bobLevels = 4;

    [SerializeField] private MotionBlur blur;

    public PlayerMovement player;
    public Weapon weapon;

    new Camera camera;
    Vector3 originalLocalPosition;

    // Start is called before the first frame update
    void Start() {
        camera = GetComponent<Camera>();
        originalLocalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update() 
    {
        if (player.SpeedAmount > 0.9f)
        {
            transform.localPosition = originalLocalPosition + new Vector3(Random.value, Random.value, Random.value) * 0.1f * player.SpeedAmount * Mathf.Sign(Random.value-0.5f);
        }
        else
        {
            transform.localPosition = originalLocalPosition;
        }

        camera.fieldOfView = Mathf.Lerp(fov, boostFov, player.SpeedAmount);
        blur.blurAmount = Mathf.Lerp(1f, 0.3f, player.SpeedAmount);
    }
}
