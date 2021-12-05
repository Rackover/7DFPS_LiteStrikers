using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] StandardMissile missilePrefab;
    [SerializeField] float delayBetweenShoot = 1f;
    [SerializeField] Transform parentVectron;

    float lastShot = 0f;
    Player player;

    private void Start() 
    {
        player = GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            Fire();
        }
    }

    public void Fire() {
        if (lastShot < Time.time - delayBetweenShoot) {
            var missile = Instantiate(missilePrefab, parentVectron.position + parentVectron.forward * 2f, parentVectron.rotation);
            missile.transform.parent = null;
            missile.transform.forward = Camera.main.ScreenPointToRay(Input.mousePosition).direction;
            missile.Owner = player;
            lastShot = Time.time;
        }
    }
}
