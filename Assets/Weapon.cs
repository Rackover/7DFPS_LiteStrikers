using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum ELoadout { LMG = 1 , TRIPLE = 2, HOMING = 3}


    [SerializeField] StandardMissile missilePrefab;
    [SerializeField] float delayBetweenTripleShoot = 0.1f;
    [SerializeField] Transform parentVectron;

    public ELoadout Loadout { get; set; } = ELoadout.TRIPLE;

    bool isFiring = false;

    float lastShot = 0f;
    Player player;

    private void Start() 
    {
        player = GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        if (player.IsLocal && player.IsSpawned && player.movement.IsBoosting)
        {
            if (Input.GetMouseButton(1))
            {
                 FireTriple();
            }
        }
    }

    public StandardMissile SpawnMissile()
    {
        var missile = Instantiate(missilePrefab, parentVectron.position + parentVectron.forward * 2f, parentVectron.rotation);
        missile.transform.parent = null;
        missile.Owner = player;

        return missile.GetComponent<StandardMissile>();
    }

    public void FireTriple() 
    {
        if (isFiring && player.IsLocal)
        {
            return;
        }

        StartCoroutine(FireTripleRoutine());

        //if (isBursting)
        //{
        //    if (lastShot < Time.time - delayBetweenShoot)
        //    {
        //        var missile = Instantiate(missilePrefab, parentVectron.position + parentVectron.forward * 2f, parentVectron.rotation);
        //        missile.transform.parent = null;

        //        var mousePosition = Game.i.MousePosition;

        //        missile.transform.forward = Camera.main.ScreenPointToRay(mousePosition).direction;
        //        missile.Owner = player;
        //        lastShot = Time.time;
        //    }
        //}
    }

    IEnumerator FireTripleRoutine()
    {
        isFiring = true;
        var wait = new WaitForSeconds(delayBetweenTripleShoot);
        for (int i = 0; i < 3; i++)
        {
            var missile = SpawnMissile();

            var mousePosition = Game.i.MousePosition;
            missile.transform.forward = player.camera.ScreenPointToRay(mousePosition + new Vector2(Random.value*2f-1f, Random.value*2f-1f) * 10f).direction;

            Game.i.SendShootState(new MissileRequest(0, new NetControllers.Position()
            {
                x = missile.transform.position.x,
                y = missile.transform.position.y,
                z = missile.transform.position.z

            }, new float[] { missile.transform.rotation.x, missile.transform.rotation.y, missile.transform.rotation.z, missile.transform.rotation.w }));

            yield return wait;
        }

        yield return new WaitForSeconds(1f);

        isFiring = false;
    }
}
