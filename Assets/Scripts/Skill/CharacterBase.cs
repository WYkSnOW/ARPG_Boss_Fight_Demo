using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public abstract class CharacterBase : MonoBehaviour, IStateMachineOwner, ISkillOwner, IHurt
{
    [SerializeField] protected ModelBase model;
    public ModelBase Model { get => model; }
    public Transform ModelTransform => Model.transform;
    [SerializeField] protected CharacterController characterController;
    public CharacterController CharacterController { get => characterController; }
    [SerializeField] protected AudioSource audioSource;
    public StateMachine stateMachine { get; protected set; }

    public AudioClip[] footStepAudioClips;
    public List<string> enemeyTagList;
    public SkillConfig[] standAttackConfigs;
    public List<SkillInfo> skillInfoList = new List<SkillInfo>();
    public float gravity = -9.8f;

    public Image HPFillImage;
    [SerializeField] protected float maxHP;
    protected float currentHP;
    protected float CurrentHP
    {
        get => currentHP;
        set
        {
            currentHP = value;
            if (currentHP <= 0)
            {
                currentHP = 0;
                SceneManager.LoadScene(0);
            }
            else HPFillImage.fillAmount = currentHP / maxHP;
        }
    }
    public virtual void Init()
    {
        CurrentHP = maxHP;
        Model.Init(this, enemeyTagList);
        stateMachine = new StateMachine();
        stateMachine.Init(this);
        CanSwitchSkill = true;
    }

    #region 技能相关

    public Skill_HitData hitData { get; protected set; }
    public ISkillOwner hurtSource { get; protected set; }
    public SkillConfig CurrentSkillConfig { get; private set; }
    protected int currentHitIndex = 0;
    // 可以切换技能，主要用于判定前摇和后摇
    public bool CanSwitchSkill { get; private set; }

    public void StartAttack(SkillConfig skillConfig)
    {
        CanSwitchSkill = false; // 防止玩家立刻播放下一个技能
        CurrentSkillConfig = skillConfig;
        currentHitIndex = 0;
        // 播放技能动画
        PlayAnimation(CurrentSkillConfig.AnimationName);
        // 技能释放音效
        SpawnSkillObject(skillConfig.ReleaseData.SpawnObj);
        // 技能释放物体
        PlayAudio(CurrentSkillConfig.ReleaseData.AudioClip);
    }
    public void StartSkillHit(int weaponIndex)
    {
        // 技能释放音效
        SpawnSkillObject(CurrentSkillConfig.AttackData[currentHitIndex].SpawnObj);
        // 技能释放物体
        PlayAudio(CurrentSkillConfig.AttackData[currentHitIndex].AudioClip);
    }

    public void StopSkillHit(int weaponIndex)
    {
        currentHitIndex += 1;
    }

    public void SkillCanSwitch()
    {
        CanSwitchSkill = true;
    }

    private void SpawnSkillObject(Skill_SpawnObj spawnObj)
    {
        if (spawnObj != null && spawnObj.Prefab != null)
        {
            StartCoroutine(DoSpawnObject(spawnObj));
        }
    }

    protected IEnumerator DoSpawnObject(Skill_SpawnObj spawnObj)
    {
        // 延迟时间
        yield return new WaitForSeconds(spawnObj.Time);
        GameObject skillObj = GameObject.Instantiate(spawnObj.Prefab, null);
        // 一般特效的生成位置是相对于主角的
        skillObj.transform.position = Model.transform.position + Model.transform.TransformDirection(spawnObj.Position);
        skillObj.transform.localScale = spawnObj.Scale;
        skillObj.transform.eulerAngles = Model.transform.eulerAngles + spawnObj.Rotation;
        PlayAudio(spawnObj.AudioClip);

        // 查找是否有技能物体，如果有的话进行初始化
        if (skillObj.TryGetComponent<SkillObjectBase>(out SkillObjectBase skillObject))
        {
            skillObject.Init(enemeyTagList, OnHitForRealseData);
        }
    }

    public virtual void OnHitForRealseData(IHurt target, Vector3 hitPostion)
    {
        // 拿到这一段攻击的数据
        Skill_AttackData attackData = CurrentSkillConfig.ReleaseData.AttackData;
        PlayAudio(attackData.SkillHitEFConfig.AudioClip); // 命中通用音效
        // 传递伤害数据
        if (target.Hurt(attackData.HitData, this))
        {
            // 生成基于命中配置的效果
            StartCoroutine(DoSkillHitEF(attackData.SkillHitEFConfig.SpawnObject, hitPostion));
            // 播放效果类
            StartFreezeFrame(attackData.FreezeFrameTime);
            StartFreezeTime(attackData.FreezeGameTime);
        }
        else
        {
            // 生成基于命中配置的效果
            StartCoroutine(DoSkillHitEF(attackData.SkillHitEFConfig.FailSpawnObject, hitPostion));
        }
    }

    public virtual void OnHit(IHurt target, Vector3 hitPostion)
    {
        // 拿到这一段攻击的数据
        Skill_AttackData attackData = CurrentSkillConfig.AttackData[currentHitIndex];
        PlayAudio(attackData.SkillHitEFConfig.AudioClip); // 命中通用音效
        // 传递伤害数据
        if (target.Hurt(attackData.HitData, this))
        {
            // 生成基于命中配置的效果
            StartCoroutine(DoSkillHitEF(attackData.SkillHitEFConfig.SpawnObject, hitPostion));
            // 播放效果类
            StartFreezeFrame(attackData.FreezeFrameTime);
            StartFreezeTime(attackData.FreezeGameTime);
        }
        else
        {
            // 生成基于命中配置的效果
            StartCoroutine(DoSkillHitEF(attackData.SkillHitEFConfig.FailSpawnObject, hitPostion));
        }
    }

    protected void StartFreezeFrame(float time)
    {
        if (time > 0) StartCoroutine(DoFreezeFrame(time));
    }

    protected IEnumerator DoFreezeFrame(float time)
    {
        Model.Animator.speed = 0;
        yield return new WaitForSeconds(time);
        Model.Animator.speed = 1;
    }

    protected void StartFreezeTime(float time)
    {
        if (time > 0) StartCoroutine(DoFreezeTime(time));
    }

    protected IEnumerator DoFreezeTime(float time)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = 1;
    }


    protected IEnumerator DoSkillHitEF(Skill_SpawnObj spawnObj, Vector3 spawnPoint)
    {
        if (spawnObj == null) yield break;

        if (spawnObj != null && spawnObj.Prefab != null)
        {
            // 延迟时间
            yield return new WaitForSeconds(spawnObj.Time);
            GameObject temp = Instantiate(spawnObj.Prefab);
            temp.transform.position = spawnPoint + spawnObj.Position;
            temp.transform.LookAt(Camera.main.transform);
            temp.transform.eulerAngles += spawnObj.Rotation;
            temp.transform.localScale += spawnObj.Scale;
            PlayAudio(spawnObj.AudioClip);
        }
    }

    public void OnSkillOver()
    {
        CanSwitchSkill = true;
    }


    #endregion

    private string currentAnimationName;
    public void PlayAnimation(string animationName, bool reState = true, float fixedTransitionDuration = 0.25f)
    {
        if (currentAnimationName == animationName && !reState)
        {
            return;
        }
        currentAnimationName = animationName;
        model.Animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration);
    }

    public void OnFootStep()
    {
        audioSource.PlayOneShot(footStepAudioClips[Random.Range(0, footStepAudioClips.Length)]);
    }
    public void PlayAudio(AudioClip audioClip)
    {
        if (audioClip != null) audioSource.PlayOneShot(audioClip);
    }

    public virtual void SetHurtData(Skill_HitData hitData, ISkillOwner hurtSource)
    {
        this.hitData = hitData;
        this.hurtSource = hurtSource;
    }
    public abstract bool Hurt(Skill_HitData hitData, ISkillOwner hurtSource);

    public virtual void UpdateHP(Skill_HitData hitData)
    {
        CurrentHP -= hitData.DamgeValue;
    }
}
