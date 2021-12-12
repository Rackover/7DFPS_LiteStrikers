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

    [SerializeField]
    private AudioClip detonateClip;

    [SerializeField]
    private AudioSource generalSource;

    [SerializeField]
    private Rigidbody body;

    public AudioSource Source => generalSource;

    private Vector3 velocity;

    public Player Owner { get; set; }

    private List<Player> affectedPlayers = new List<Player>();

    // Start is called before the first frame update
    void Start()
    {
        velocity = transform.forward * speed;

        body.velocity = velocity;

        if (Owner == Game.i.LocalPlayer)
        {
            GetComponent<AudioSource>().volume = 0.3f;
        }

        StartCoroutine(LiveAndDie());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == Owner.ignoreCollision)
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
                if (player == null) continue;
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
        generalSource.transform.parent = null;
        generalSource.pitch = 0.9f + Random.value * 0.2f;
        generalSource.PlayOneShot(detonateClip, 0.3f);
        Destroy(generalSource.gameObject, 3f);

        explosionShuriken.Play();
        explosionShuriken.transform.parent = null;
        Destroy(explosionShuriken.gameObject, 5f);

        velocity = Vector3.zero;
        toHideWhenDetonating.enabled = false;
        Destroy(gameObject);

        foreach(var player in affectedPlayers)
        {
            if (player == Game.i.LocalPlayer)
            {
                Game.i.EliminateMyself(Owner);
                break;
            }
        }
    }
}
