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

    // Update is called once per frame
    void FixedUpdate()
    {
        var localPos = player.transform.InverseTransformPoint(transform.position);
        localPos.z = 0f;
        transform.position = player.transform.TransformPoint(localPos);
        

        transform.position = Vector3.Lerp(transform.position, player.transform.position, lerpSpeed * Time.deltaTime);

        if (playerMovement.SpeedAmount < 0.5f && !playerMovement.IsBoosting)
        {
            transform.eulerAngles += (transform.forward + transform.right) * 60f * Time.deltaTime * (1f - playerMovement.SpeedAmount);
        }
        else
        {
            transform.forward =
               Vector3.Lerp(
                   transform.forward,
                    player.transform.TransformDirection(new Vector3(playerMovement.VirtualJoystick.x, playerMovement.VirtualJoystick.y, 1f).normalized),
                    playerMovement.SpeedAmount
                );
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 60f * playerMovement.VirtualJoystick.x);
        }
    }
}
