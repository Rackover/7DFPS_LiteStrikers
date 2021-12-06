using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HUD : MonoBehaviour
{

    [SerializeField]
    MaskableGraphic[] crosshair;

    [SerializeField]
    Image aim;

    [SerializeField]
    CanvasGroup group;

    Vector2 aimBestSize;


    // Start is called before the first frame update
    void Start()
    {
        aimBestSize = aim.rectTransform.sizeDelta;
    }

    // Update is called once per frame
    void Update()
    {
        if (Game.i.LocalPlayer == null) return;

        group.alpha = Game.i.LocalPlayer.movement.SpeedAmount;

        var delta = Mathf.Sin(Game.i.LocalPlayer.movement.SpeedAmount * Mathf.PI / 2f);

        aim.rectTransform.sizeDelta = Vector2.Lerp(Vector2.one * Screen.height, aimBestSize, delta);

        Vector2 mousePosition = Input.mousePosition;
        if (Game.i.IsMobile)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                mousePosition = touch.position;
            }
        }

        foreach (var hair in crosshair)
        {
            hair.transform.position = mousePosition;
        }
    }
}
