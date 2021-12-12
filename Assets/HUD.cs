using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class HUD : MonoBehaviour
{
    [SerializeField]
    float horizonVerticalAmplitude = 19f;

    [SerializeField]
    float horizonMaxSize = 128;

    [SerializeField]
    TextMeshProUGUI killFeed;

    [SerializeField]
    MaskableGraphic[] crosshair;

    [SerializeField]
    Image horizon;

    [SerializeField]
    Image outerAim;

    [SerializeField]
    Image innerAim;

    [SerializeField]
    TextMeshProUGUI strategicInfo;

    [SerializeField]
    TextMeshProUGUI scoreInfo;

    [SerializeField]
    CanvasGroup group;

    [SerializeField]
    Canvas canvas;

    Vector2 outerAimBestSize;
    Vector2 innerAimBestSize;
    float horizonBestSize;

    bool hasRegisteredEvent = false;

    // Start is called before the first frame update
    void Start()
    {
        innerAimBestSize = innerAim.rectTransform.sizeDelta;
        outerAimBestSize = outerAim.rectTransform.sizeDelta;
        horizonBestSize = horizon.rectTransform.sizeDelta.y;

        Cursor.lockState = CursorLockMode.Confined;

        killFeed.alpha = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        canvas.enabled = Game.i.IsConnected && Game.i.LocalPlayer && Game.i.LocalPlayer.IsSpawned;

        if (canvas.enabled == false) return;

        if (!hasRegisteredEvent)
        {
            Game.i.LocalPlayer.OnKilledSomeone += LocalPlayer_OnKilledSomeone;
        }
        
        Cursor.visible = false;
        group.alpha = Game.i.LocalPlayer.movement.SpeedAmount;
        killFeed.alpha = Mathf.Clamp01(killFeed.alpha - Time.deltaTime * 0.33f);

        var delta = Mathf.Sin(Game.i.LocalPlayer.movement.SpeedAmount * Mathf.PI / 2f);

        //outerAim.rectTransform.sizeDelta = Vector2.Lerp(Vector2.one * Screen.height, outerAimBestSize, delta);
        innerAim.rectTransform.sizeDelta = Vector2.Lerp(Vector2.one * Screen.height, innerAimBestSize, delta);

        var mousePosition = Game.i.MousePosition;

        foreach (var hair in crosshair)
        {
            hair.transform.position = mousePosition;
        }

        horizon.rectTransform.anchoredPosition = Vector2.up * horizonVerticalAmplitude * Game.i.LocalPlayer.movement.Pitch101;
        horizon.rectTransform.sizeDelta = new Vector2(50f, Mathf.Lerp(horizonMaxSize, horizonBestSize, Game.i.LocalPlayer.movement.SpeedAmount));

        //outerAim.transform.position = Vector3.Lerp(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f), mousePosition, 0.95f);

        strategicInfo.text = $"Designation: {Game.i.GetNameForId(Game.i.LocalPlayer.id)}\nSpeed: {Mathf.FloorToInt(Game.i.LocalPlayer.movement.VelocityMagnitude * 8f)} knots".ToUpper();

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < Game.i.Players.Count; i++)
        {
            var player = Game.i.Players[i];
            sb.AppendLine($"{Game.i.GetNameForId(player.id)}: {Game.i.GetScore(player.id)}");
        }

        scoreInfo.text = sb.ToString();
    }

    private void LocalPlayer_OnKilledSomeone(int obj)
    {
        killFeed.alpha = 2f;
        killFeed.text = $"ELIMINATED {Game.i.GetNameForId(obj).ToUpper()}";
    }
}
