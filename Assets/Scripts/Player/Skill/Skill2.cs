using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill2 : SkillObjectBase
{
    public AudioSource audioSource;
    public override void Init(List<string> enemeyTagList, Action<IHurt, Vector3> onHitAction)
    {
        base.Init(enemeyTagList, onHitAction);
        Destroy(gameObject, 4f);
        Invoke(nameof(StartSkillHit), 0.5f);
        Invoke(nameof(PlayAudio), 0.6f);
    }
    private void PlayAudio()
    {
        audioSource.enabled = true;
    }
}
