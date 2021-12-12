using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Menu : MonoBehaviour
{
    [SerializeField] Canvas canvas;

    [SerializeField] TextMeshProUGUI holdClickText;

    [SerializeField] MaskableGraphic accumulatorRectangle;

    [SerializeField] MaskableGraphic mouseCursor;

    [SerializeField] float acceleration = 5f;

    [SerializeField] RectTransform title;

    float holdAccumulator = 0f;

    float maxHold = 3f;

    bool isAtInitialStep = true;

    bool isAnimating = false;

    bool hasSpawnedOnce = false;

    bool isWaitingOnServer = false;
    
    
    void Start()
    {
        if (Game.i.IsMobile)
        {
            holdClickText.text = @"TOUCH ANYWHERE TO BEGIN";
        }
        else
        {
            holdClickText.text = @"CLICK ANYWHERE TO BEGIN";
        }

        mouseCursor.color = new Color(1f, 1f, 1f, 0f);
        accumulatorRectangle.color = new Color(1f, 1f, 1f, 0f);
    }

    void Update()
    {
        if (Game.i.LocalPlayer == null) return;

        if (Game.i.LocalPlayer.IsSpawned)
        {
            if (isWaitingOnServer)
            {
                hasSpawnedOnce = true;
            }

            holdAccumulator = 0f;
            isWaitingOnServer = false;
        }

        canvas.enabled = !Game.i.LocalPlayer.IsSpawned;

        if (canvas.enabled == false) return;

        holdClickText.alpha = Mathf.Clamp01(Mathf.Floor((Time.time * ( isAnimating ? 4f : (isAtInitialStep ? 1f : 8f))) % 2f));

        if (isAnimating)
        {
            holdClickText.text = "WAIT...";
        }

        if (isWaitingOnServer) { return; }

        Cursor.visible = true;

        if (isAnimating) return;

        if (isAtInitialStep)
        {
            if (Game.i.IsPressing)
            {
                isAtInitialStep = false;
                isAnimating = true;
                title.DOMoveX(-title.sizeDelta.x, 1.5f)
                    .SetEase(Ease.InOutCubic)
                    .OnComplete(() => { isAnimating = false; })
                    .Play();

            }
        }
        else
        {
            bool isValid = Game.i.IsPressing;

            accumulatorRectangle.rectTransform.sizeDelta = Vector2.one * 400f * (1f - holdAccumulator / maxHold);
            accumulatorRectangle.rectTransform.localEulerAngles = Vector3.forward * 180f * (holdAccumulator / maxHold);


            if (isValid)
            {
                isValid = Game.i.LocalPlayer.movement.VirtualJoystick.magnitude < 1f - holdAccumulator / maxHold + 0.1f;
            }

            holdAccumulator = Mathf.Clamp(holdAccumulator + (isValid ? Time.deltaTime : -Time.deltaTime), 0f, maxHold);

            mouseCursor.transform.position = Game.i.MousePosition;

            if (holdAccumulator >= maxHold)
            {
                Game.i.RequestSpawn();
                isWaitingOnServer = true;
            }

            mouseCursor.color = new Color(1f, 1f, 1f, Mathf.Clamp01(mouseCursor.color.a + Time.deltaTime));
            accumulatorRectangle.color = new Color(isValid && Game.i.IsPressing ? 0f : 1f, isValid || !Game.i.IsPressing ? 1f : 0f, Game.i.IsPressing ? 0f : 1f, Mathf.Clamp01(accumulatorRectangle.color.a + Time.deltaTime));

            if (hasSpawnedOnce)
            {
                holdClickText.text = (Game.i.LocalPlayer.lastLocalKiller == Game.i.LocalPlayer.id ? "SELF-DESTRUCT! " :  $"ELIMINATED BY {Game.i.GetNameForId(Game.i.LocalPlayer.lastLocalKiller).ToUpper()} ") + $"{(Game.i.IsMobile ? "TOUCH" : "CLICK")} CENTER AND HOLD FIRMLY";
            }
            else
            {
                if (Game.i.IsMobile)
                {
                    holdClickText.text = "TOUCH THE CENTER - HOLD FIRMLY - DON'T LET GO";
                }
                else
                {
                    holdClickText.text = "MOUSE TO THE CENTER - HOLD LEFT CLICK - DON'T LET GO";
                }
            }
        }
    }
}
