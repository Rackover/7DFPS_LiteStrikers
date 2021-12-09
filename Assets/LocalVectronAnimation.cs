using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalVectronAnimation : MonoBehaviour
{
    [SerializeField]
    Player player;

    [SerializeField]
    PlayerMovement playerMovement;

    [SerializeField]
    float lerpSpeed = 5f;

    [SerializeField]
    GameObject visualToHide;

    [SerializeField]
    Dictionary<Weapon.ELoadout, MeshRenderer> loadoutsWeaps = new Dictionary<Weapon.ELoadout, MeshRenderer>();

    // Start is called before the first frame update
    void Start()
    {
        if (player.IsLocal)
        {
            transform.parent = null;
            transform.position = player.transform.position;
        }
        else 
        { 
            Destroy(this); // script
        }
    }

    private void Update()
    {
        if (player.IsSpawned && !visualToHide.activeSelf)
        {
            UpdateLoadout();

            visualToHide.SetActive(true);
        }
        else if (!player.IsSpawned && visualToHide.activeSelf)
        {
            visualToHide.SetActive(false);
        }
    }

    void UpdateLoadout()
    {
        foreach(var loadout in loadoutsWeaps.Keys)
        {
            if(loadout == player.weapon.Loadout)
            {
                if (!loadoutsWeaps[loadout].gameObject.activeSelf)
                {
                    loadoutsWeaps[loadout].gameObject.SetActive(true);
                }
            }
            else
            {
                if (loadoutsWeaps[loadout].gameObject.activeSelf)
                {
                    loadoutsWeaps[loadout].gameObject.SetActive(false);
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player.IsSpawned)
        {
            var localPos = player.transform.InverseTransformPoint(transform.position);
            localPos.z = 0f;
            transform.position = player.transform.TransformPoint(localPos);


            transform.position = Vector3.Lerp(transform.position, player.transform.position, lerpSpeed * Time.deltaTime);

            if (playerMovement.SpeedAmount < 0.5f && !playerMovement.IsBoosting)
            {
                transform.eulerAngles += (transform.forward + transform.up + Vector3.left) * 60f * Time.deltaTime * (1f - playerMovement.SpeedAmount);
            }
            else
            {
                transform.forward =
                   Vector3.Lerp(
                       transform.forward,
                        player.transform.TransformDirection(new Vector3(playerMovement.VirtualJoystick.x, playerMovement.VirtualJoystick.y, 1f).normalized),
                        playerMovement.SpeedAmount
                    );
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 60f * (-playerMovement.VirtualJoystick.x));
            }
        }
    }
}
