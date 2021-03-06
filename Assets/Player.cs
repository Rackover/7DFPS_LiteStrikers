using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public bool IsLocal { get; set; } = false;

    public bool IsSpawned { get; private set; } = false;

    public bool WasSpawnedOnce { private set; get; } = false;

    public bool IsRadarVisible => IsSpawned && (movement.IsBoosting || movement.SpeedAmount > 0.5f);

    public AudioSource source;
    public new Camera camera;
    public AudioClip[] meows;
    public Weapon weapon;
    public GameObject ignoreCollision { get; set; }

    public PlayerMovement movement;
    public int id = 0;
    public float catchUpSpeed = 4f;

    public bool isInScreen = false;
    public float localDistanceMeters = 0f;
    public Vector3 screenPosition;

    public int lastLocalKiller;

    [SerializeField]
    private ParticleSystem eliminationParticleSystem;

    [SerializeField]
    private TrailRenderer trailRenderer;

    [SerializeField]
    private AudioClip launchClip;

    [SerializeField]
    private AudioClip dieClip;

    public event Action<int> OnKilledSomeone;

    NetControllers.DeserializedPlayerMove previousMovement;
    NetControllers.DeserializedPlayerMove targetMovement;

    float timer = 0f;
    float deathTime = 0f;

    private void Awake() {
    }

    // Start is called before the first frame update
    void Start() {
        if (!IsLocal)
        {
            trailRenderer.widthMultiplier = 5f;
            Destroy(GetComponent<Rigidbody>());
            source.spatialBlend = 1f;
        }
    }

    public void Meow() {
        source.PlayOneShot(meows[id % meows.Length]);
    }

    public void Spawn()
    {
        if (IsSpawned)
        {
            Debug.LogError("?? Spawned me twice?");

            return;
        }

        source.PlayOneShot(launchClip, 0.5f);

        WasSpawnedOnce = true;
        IsSpawned = true;

    }

    public void Eliminate()
    {
        if (IsSpawned)
        {
            eliminationParticleSystem?.Play();
            deathTime = Time.time;

            source.PlayOneShot(dieClip);
        }

        IsSpawned = false;
    }

    public void AcknowledgeKill(int killedId)
    {
        OnKilledSomeone?.Invoke(killedId);
    }

    public void SetLoadout(Weapon.ELoadout loadout)
    {
        weapon.Loadout = loadout;
    }

    private void Update() {
        name = $"{(IsSpawned ? "[ACTIVE]" : "[WAITING]")} {(IsLocal ? "LOCAL_" : "")}{id}_{weapon.Loadout}";

        if (IsLocal)
        {
            if (WasSpawnedOnce && !IsSpawned && !Game.i.IsPressing && deathTime < Time.time - 6f )
            {
                Menu.ResetMenu();
                WasSpawnedOnce = false;
            }

            return;
        }


        // ONLINE ONLY (Remote client)
        timer += Time.deltaTime * catchUpSpeed;

        if (targetMovement != null) {
            float normalizedTimer = Mathf.Clamp01(timer);
            transform.position = Vector3.Slerp(previousMovement?.position ?? transform.position, targetMovement.position, normalizedTimer);
            transform.rotation = Quaternion.Slerp(previousMovement?.rotation ?? transform.rotation, targetMovement.rotation, normalizedTimer);
        }
    }

    private void FixedUpdate() {
        if (IsLocal && IsSpawned) 
        {
            Game.i.SendMyPosition(movement.transform.position, movement.transform.rotation, movement);
        }
    }

    // Remote stuff

    public void UpdatePosition(NetControllers.DeserializedPlayerMove movement) {

        ////previousMovement = targetMovement;
        previousMovement = new NetControllers.DeserializedPlayerMove() {
            position = transform.position,
            rotation = transform.rotation
        };

        targetMovement = movement;

        timer = 0f;
    }

}
