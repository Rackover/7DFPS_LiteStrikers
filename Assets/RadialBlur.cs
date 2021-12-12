/************************************************* *******************
 FileName: RadialBlurEffect.cs
 Description: Radial blur effect
 Created: 2017/02/2-
 history: 16: 1: 2017 23:05 by puppet_master
************************************************** *******************/
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class RadialBlur : PostEffectsBase
{

    //The degree of blur cannot be too high
    [Range(0, 0.05f)]
    public float blurFactor = 1.0f;
    //Fuzzy center (0-1) screen space, the default is the center point
    public Vector2 blurCenter = new Vector2(0.5f, 0.5f);

    [SerializeField] Material _Material;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_Material)
        {
            _Material.SetFloat("_ BlurFactor", blurFactor);
            _Material.SetVector("_BlurCenter", blurCenter);
            Graphics.Blit(source, destination, _Material);
        }
        else
        {
            Graphics.Blit(source, destination);
        }

    }
}