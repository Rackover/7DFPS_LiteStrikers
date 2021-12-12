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

    [SerializeField] AudioClip validateClip;

    [SerializeField] AudioSource music;

    [SerializeField] AudioSource beep;

    [SerializeField] float volume;

    float holdAccumulator = 0f;

    float maxHold = 3f;

    bool isAtInitialStep = true;

    bool isAnimating = false;

    bool hasSpawnedOnce => Game.i.LocalPlayer && Game.i.LocalPlayer.WasSpawnedOnce;

    bool isWaitingOnServer = false;

    private Vector2 titleBasePosition;

    public static event System.Action OnReset;

    public static void ResetMenu()
    {
        OnReset?.Invoke();
    }

    private void OnDestroy()
    {
        Debug.Log($"OnDestroy " + name);
        OnReset -= Menu_OnReset;
    }

    void Start()
    {
        titleBasePosition = title.position;

        Menu_OnReset();

        OnReset += Menu_OnReset;
    }

    private void Menu_OnReset()
    {
        beep.volume = 0f;
        holdAccumulator = 0f;
        isAtInitialStep = true;
        title.position = titleBasePosition;
        mouseCursor.color = new Color(1f, 1f, 1f, 0f);
        accumulatorRectangle.color = new Color(1f, 1f, 1f, 0f);
    }

    void Update()
    {

        canvas.enabled = !Game.i.IsConnected || !Game.i.LocalPlayer.IsSpawned;

        music.volume = Mathf.Clamp(music.volume + (canvas.enabled ? 1f : -1f) * Time.deltaTime, 0f, volume);

        if (hasSpawnedOnce && Game.i.LocalPlayer.IsSpawned && isWaitingOnServer)
        {
            beep.volume = 0f;
            holdAccumulator = 0f;
            isWaitingOnServer = false;
        }


        if (canvas.enabled == false) return;

        if (isAnimating)
        {
            holdClickText.color = Color.yellow;
            holdClickText.text = "WAIT...";
        }
        else if (!Game.i.IsConnected)
        {
            holdClickText.color = Color.yellow;
            holdClickText.text = "CONNECTING TO SERVER...";
        }
        else
        {
            holdClickText.color = Color.white;
        }

        holdClickText.alpha = Mathf.Clamp01(Mathf.Floor((Time.time * (isAnimating ? 4f : (isAtInitialStep ? 1f : 8f))) % 2f));

        if (isWaitingOnServer) { return; }

        Cursor.visible = true;

        if (!Game.i.IsConnected) { return; }

        if (isAnimating) return;

        if (isAtInitialStep)
        {
            beep.volume = 0f;

            if (Game.i.IsMobile)
            {
                holdClickText.text = @"TOUCH ANYWHERE TO BEGIN";
            }
            else
            {
                holdClickText.text = @"CLICK ANYWHERE TO BEGIN";
            }

            if (Game.i.IsPressing)
            {
                isAtInitialStep = false;
                isAnimating = true;
                title.DOMoveX(-title.sizeDelta.x, 1.5f)
                    .SetEase(Ease.InOutCubic)
                    .OnComplete(() => { isAnimating = false; })
                    .Play();

                Game.i.AudioSource.PlayOneShot(validateClip);
            }
        }
        else
        {
            bool isValid = Game.i.IsPressing;

            accumulatorRectangle.rectTransform.sizeDelta = Vector2.one * 400f * (1f - holdAccumulator / maxHold) + (Game.i.IsPressing ? Vector2.zero : Vector2.one * 10f * Mathf.Sin(Time.time * 10f));
            accumulatorRectangle.rectTransform.localEulerAngles = Vector3.forward * 180f * (holdAccumulator / maxHold);

            beep.volume = beep.volume + (isValid ? 1f : -1f) * Time.deltaTime * 3f;
            beep.pitch = holdAccumulator / maxHold + 0.5f;

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
                holdClickText.text = (Game.i.LocalPlayer.lastLocalKiller == Game.i.LocalPlayer.id ? "SELF-DESTRUCT! " : $"ELIMINATED BY {Game.i.GetNameForId(Game.i.LocalPlayer.lastLocalKiller).ToUpper()} - ") + $"{(Game.i.IsMobile ? "TOUCH" : "CLICK")} CENTER AND HOLD FIRMLY";
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
