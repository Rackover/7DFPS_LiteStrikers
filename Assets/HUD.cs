using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class HUD : MonoBehaviour
{

    [SerializeField]
    MaskableGraphic[] crosshair;

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

    Vector2 outerAimBestSize;
    Vector2 innerAimBestSize;


    // Start is called before the first frame update
    void Start()
    {
        innerAimBestSize = innerAim.rectTransform.sizeDelta;
        outerAimBestSize = outerAim.rectTransform.sizeDelta;
    }

    // Update is called once per frame
    void Update()
    {
        if (Game.i.LocalPlayer == null) return;

        group.alpha = Game.i.LocalPlayer.movement.SpeedAmount;

        var delta = Mathf.Sin(Game.i.LocalPlayer.movement.SpeedAmount * Mathf.PI / 2f);

        outerAim.rectTransform.sizeDelta = Vector2.Lerp(Vector2.one * Screen.height, outerAimBestSize, delta);
        innerAim.rectTransform.sizeDelta = Vector2.Lerp(Vector2.one * Screen.height, innerAimBestSize, delta);

        var mousePosition = Game.i.MousePosition;

        foreach (var hair in crosshair)
        {
            hair.transform.position = mousePosition;
        }

        outerAim.transform.position = Vector3.Lerp(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f), mousePosition, 0.95f);

        strategicInfo.text = $"Designation: {Game.i.GetNameForId(Game.i.LocalPlayer.id)}\nSpeed: {Mathf.FloorToInt(Game.i.LocalPlayer.movement.VelocityMagnitude * 8f)} knots".ToUpper();

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < Game.i.Players.Count; i++)
        {
            var player = Game.i.Players[i];
            sb.AppendLine($"{Game.i.GetNameForId(player.id)}: {Game.i.GetScore(player.id)}");
        }

        scoreInfo.text = sb.ToString();
    }
}
