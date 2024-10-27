using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessManager : SingletonMono<PostProcessManager>
{
    public PostProcessVolume volume;
    private ChromaticAberration chromaticAberration;
    [SerializeField]private float speed;
    void Start()
    {
        chromaticAberration = volume.profile.GetSetting<ChromaticAberration>();
    }

    /// <summary>
    /// 色差效果
    /// </summary>
    public void ChromaticAberrationEF(float value)
    {
        StopAllCoroutines(); // 防止多次触发
        StartCoroutine(StartChromaticAberrationEF(value));
    }

    IEnumerator StartChromaticAberrationEF(float value)
    {
        // 递增到value
        while (chromaticAberration.intensity<value)
        {
            yield return null;
            chromaticAberration.intensity.value += Time.deltaTime * speed;
        }
        // 递减到0
        while (chromaticAberration.intensity > 0)
        {
            yield return null;
            chromaticAberration.intensity.value -= Time.deltaTime * speed;
        }
    }



}
