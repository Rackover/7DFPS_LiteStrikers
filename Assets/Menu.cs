using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Menu : MonoBehaviour
{
    [SerializeField] Canvas canvas;

    [SerializeField] TextMeshProUGUI holdClickText;

    [SerializeField] Image imageToFill;

    [SerializeField]
    float acceleration = 5f;

    float holdAccumulator = 0f;

    float maxHold = 3f;

    bool isWaitingOnServer = false;
    
    
    void Start()
    {
        if (Game.i.IsMobile)
        {
            holdClickText.text = @"TOUCH THE CENTER - HOLD IT FIRMLY - GOOD LUCK";
        }
        else
        {
            holdClickText.text = @"MOUSE TO THE CENTER - CLICK AND HOLD - DON'T LET GO";
        }
    }

    void Update()
    {
        if (Game.i.LocalPlayer == null) return;

        canvas.enabled = !Game.i.LocalPlayer.IsSpawned;

        if (canvas.enabled == false) return;

        if (isWaitingOnServer) { return; }

        Cursor.visible = true;

        holdAccumulator = Mathf.Clamp(holdAccumulator +( Game.i.IsPressing ? Time.deltaTime : -Time.deltaTime), 0f, maxHold);

        if (holdAccumulator >= maxHold)
        {
            Game.i.RequestSpawn();
            isWaitingOnServer = true;
        }


        holdClickText.alpha = Mathf.Floor((Time.time * (1F + holdAccumulator / maxHold * acceleration))  % 2f);
        imageToFill.fillAmount = holdAccumulator / maxHold;
        imageToFill.color = new Color(1f, 1f, 1f, holdAccumulator / maxHold);
    }
}
