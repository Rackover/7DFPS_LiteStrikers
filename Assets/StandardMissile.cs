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

    private Vector3 velocity;

    public Player Owner;

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
        yield return new WaitForSeconds(lifespan);
        Destroy(gameObject);
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
        Destroy(gameObject, 0f);
    }
}
