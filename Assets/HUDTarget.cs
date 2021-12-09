using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDTarget : MonoBehaviour
{
    public Player Player { get; set; }

    [SerializeField] private CanvasGroup group;
    [SerializeField] private new TextMeshProUGUI tag;
    [SerializeField] private Image targetImage;
    [SerializeField] private Image diagonalBarImage;
    [SerializeField] private Image horizontalBarImage;



    // Update is called once per frame
    void Update()
    {
        if (Player)
        {

            if (Player.isInScreen && Player.IsSpawned)
            {
                group.alpha = 1f;

                transform.position = Player.screenPosition;
                //transform.Translate(Player.screenPosition - transform.position);
                tag.text = $"{Game.i.GetNameForId(Player.id).ToUpper()}\n{Mathf.FloorToInt(Player.localDistanceMeters)}";

                //// Normal
                //if (Screen.width/2f > Player.screenPosition.x)
                //{
                //    diagonalBarImage.rectTransform.localEulerAngles = Vector3.forward * -45f;
                //    diagonalBarImage.rectTransform.localEulerAngles = Vector3.forward * 45f;
                //    tag.rectTransform.anchoredPosition = Vector2.right * 12f;
                //    tag.horizontalAlignment = HorizontalAlignmentOptions.Left;
                //}
                //// Inverted
                //else
                //{
                //    diagonalBarImage.rectTransform.localEulerAngles = Vector3.forward * -135f;
                //    diagonalBarImage.rectTransform.localEulerAngles = Vector3.forward * -45f;
                //    tag.rectTransform.anchoredPosition = Vector2.right *( 12f + tag.rectTransform.sizeDelta.x);
                //    tag.horizontalAlignment = HorizontalAlignmentOptions.Right;
                //}
            }
            else
            {
                group.alpha = 0f;
            }
        }
    }
}
