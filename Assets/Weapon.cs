using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum ELoadout { LMG = 1 , TRIPLE = 2, HOMING = 3}


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
        if (player.IsLocal)
        {
            if (Input.GetMouseButton(1))
            {
                Fire();
            }
        }
    }

    public void Fire() {
        if (lastShot < Time.time - delayBetweenShoot) {
            var missile = Instantiate(missilePrefab, parentVectron.position + parentVectron.forward * 2f, parentVectron.rotation);
            missile.transform.parent = null;

            var mousePosition = Game.i.MousePosition;

            missile.transform.forward = Camera.main.ScreenPointToRay(mousePosition).direction;
            missile.Owner = player;
            lastShot = Time.time;
        }
    }
}
