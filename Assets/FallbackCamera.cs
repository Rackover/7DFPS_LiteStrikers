using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallbackCamera : MonoBehaviour
{

    [SerializeField] private float height = 200f;
    [SerializeField] private float lookAt = 500f;
    [SerializeField] private float range = 1000f;
    private AudioListener listener;
    private new Camera camera;


    private void Awake()
    {
        listener = GetComponent<AudioListener>();
        camera = GetComponent<Camera>();
    }

    private void Start()
    {
        PlaceCamera();
    }


    // Update is called once per frame
    void Update()
    {
        PlaceCamera();
    }

    void PlaceCamera()
    {
        bool active = !Game.i.IsConnected || !Game.i.LocalPlayer || (!Game.i.LocalPlayer.IsSpawned && !Game.i.LocalPlayer.WasSpawnedOnce);

        listener.enabled = active;
        camera.enabled = active;

        if (active)
        {
            transform.position = new Vector3(Mathf.Sin(Time.time * 0.1f) * range, height, Mathf.Cos(Time.time * 0.1f) * range);


            transform.LookAt(Vector3.up * lookAt);
        }
    }
}
