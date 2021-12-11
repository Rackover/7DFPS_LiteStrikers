using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardMissile : MonoBehaviour
{
    [SerializeField]
    private float speed = 20f;

    [SerializeField]
    private float lifespan = 1f;

    [SerializeField]
    private ParticleSystem explosionShuriken;

    [SerializeField]
    private Renderer toHideWhenDetonating;

    [SerializeField]
    private float detonateDistance = 50f;

    private Vector3 velocity;

    public Player Owner;

    private List<Player> affectedPlayers = new List<Player>();

    // Start is called before the first frame update
    void Start()
    {
        velocity = transform.forward * speed;
        StartCoroutine(LiveAndDie());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += velocity * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == Owner.gameObject)
        {
            return;
        }

        Detonate();
    }

    IEnumerator LiveAndDie()
    {
        var wait = new WaitForEndOfFrame();
        var time = Time.time;
        var sqr = detonateDistance * detonateDistance;

        while(time + lifespan > Time.time)
        {
            for (int i = 0; i < Game.i.Players.Count; i++)
            {
                var player = Game.i.Players[i];
                if (player == Owner) continue;

                if (Vector3.SqrMagnitude(player.transform.position - transform.position) < sqr)
                {
                    affectedPlayers.Add(player);
                }

                yield return wait;
            }

            yield return wait;

            if (affectedPlayers.Count > 0)
            {
                break;
            }
        }

        Detonate();
    }

    private void Detonate()
    {
        explosionShuriken.Play();
        explosionShuriken.transform.parent = null;
#pragma warning disable CS0618 // Type or member is obsolete
        Destroy(explosionShuriken.gameObject, explosionShuriken.startLifetime);
#pragma warning restore CS0618 // Type or member is obsolete
        velocity = Vector3.zero;
        toHideWhenDetonating.enabled = false;
        Destroy(gameObject);

        foreach(var player in affectedPlayers)
        {
            if (player == Game.i.LocalPlayer)
            {
                Game.i.EliminateMyself();
            }
        }
    }
}
