using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallbackCamera : MonoBehaviour
{

    [SerializeField] private float height = 200f;
    
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
        bool active = Game.i.LocalPlayer && !Game.i.LocalPlayer.IsSpawned;

        listener.enabled = active;
        camera.enabled = active;

        if (active)
        {
            transform.position = new Vector3(Mathf.Sin(Time.time * 0.1f) * 400f, height, Mathf.Cos(Time.time * 0.1f) * 400f);


            transform.LookAt(Vector3.zero);
        }
    }
}
