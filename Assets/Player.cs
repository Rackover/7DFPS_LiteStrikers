﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public bool IsLocal { get; set; } = false;

    public AudioSource source;

    public AudioClip[] meows;
    public Texture[] furTextures;
    public Renderer bodyRenderer;

    public PlayerMovement movement;
    public int id = 0;
    public float catchUpSpeed = 4f;
    public TextMesh textMesh;

    public bool isInScreen = false;
    public float localDistanceMeters = 0f;
    public Vector3 screenPosition;

    NetControllers.DeserializedPlayerMove previousMovement;
    NetControllers.DeserializedPlayerMove targetMovement;

    float timer = 0f;

    private void Awake() {
    }

    // Start is called before the first frame update
    void Start() {
        bodyRenderer.material.mainTexture = furTextures.Length == 0 ? new Texture2D(1, 1) : furTextures[id % furTextures.Length];
    }

    private void OnDestroy() {
        if (bodyRenderer)
        {
            Destroy(bodyRenderer.material);
        }
    }

    public void Meow() {
        source.PlayOneShot(meows[id % meows.Length]);
    }

    private void Update() {
        if (IsLocal) return;

        // ONLINE ONLY (Remote client)
        timer += Time.deltaTime * catchUpSpeed;

        if (targetMovement != null) {
            float normalizedTimer = Mathf.Clamp01(timer);
            transform.position = Vector3.Slerp(previousMovement?.position ?? transform.position, targetMovement.position, normalizedTimer);
            transform.rotation = Quaternion.Slerp(previousMovement?.rotation ?? transform.rotation, targetMovement.rotation, normalizedTimer);
        }
    }

    private void FixedUpdate() {
        if (IsLocal) 
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
