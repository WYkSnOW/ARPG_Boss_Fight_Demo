using UnityEngine;

[CreateAssetMenu(menuName = "Config/SkillHitEFConfig")]
public class SkillHitEFConfig : ScriptableObject
{
    // 产生的粒子物体-生成
    public Skill_SpawnObj SpawnObject;

    // 通用的，无论是否对方格挡住都会播放的音效
    public AudioClip AudioClip;

    // 产生的粒子物体-失败
    public Skill_SpawnObj FailSpawnObject;
}
