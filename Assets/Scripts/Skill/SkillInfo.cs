using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SkillInfo
{
    public KeyCode keyCode;
    public SkillConfig skillConfig;
    public float cdTime;
    [NonSerialized] public float currentTime;
    public Image cdmMaskImage;
}
