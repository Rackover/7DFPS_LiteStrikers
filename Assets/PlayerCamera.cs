using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PlayerCamera : MonoBehaviour {
    [SerializeField] Player playerScript;

    [SerializeField] private float fov = 80f;
    [SerializeField] private float boostFov = 110f;

    [SerializeField] private float shakeForce = 0.1f;

    [SerializeField] private MotionBlur blur;

    [SerializeField] private ColorCorrectionCurves colorCorrection;

    public PlayerMovement player;
    public Weapon weapon;

    private AudioListener listener;

    new Camera camera;
    Vector3 originalLocalPosition;

    private void Awake()
    {
        listener = GetComponent<AudioListener>();
    }

    // Start is called before the first frame update
    void Start() {
        if (!playerScript.IsLocal)
        {
            Destroy(this.gameObject);
        }

        camera = GetComponent<Camera>();
        originalLocalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update() 
    {
        listener.enabled = player.IsSpawned || player.WasSpawnedOnce;
        camera.enabled = player.IsSpawned || player.WasSpawnedOnce;

        if (player.IsSpawned)
        {
            if (/*player.SpeedAmount > 0.5f &&*/ player.IsBoosting)
            {
                transform.localPosition = originalLocalPosition + new Vector3(Random.value, Random.value, Random.value) * Mathf.Sin(player.SpeedAmount * Mathf.PI) * Mathf.Sign(Random.value - 0.5f) * shakeForce;
            }
            else
            {
                transform.localPosition = originalLocalPosition;
            }

            var delta = Mathf.Sin(player.SpeedAmount * Mathf.PI / 2f);

            camera.fieldOfView = Mathf.Lerp(fov, boostFov, delta);
            blur.blurAmount = Mathf.Lerp(1f, 0.3f, delta);

            colorCorrection.saturation = Mathf.Lerp(0.2f, 1f, delta);
        }
        else if (player.WasSpawnedOnce)
        {
            // Deadcam
            blur.blurAmount = 1f;
            colorCorrection.saturation = 0f;
        }
    }
}
