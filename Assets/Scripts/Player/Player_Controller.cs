using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
public class Player_Controller : CharacterBase
{
    [SerializeField] private CinemachineImpulseSource impulseSource;

    #region 配置类型的信息
    [Header("配置")]
    public float rotateSpeed = 5;
    public float rotateSpeedForAttack = 4;
    public float walk2RunTransition = 1;
    public float walkSpeed = 1;
    public float runSpeed = 1;
    public float jumpPower = 1;
    public float moveSpeedForJump;
    public float moveSpeedForAirDown;

    public float waitCounterattackTime;
    public SkillConfig counterattackSkillConfig;
    public SkillConfig jumpAttackSkillConfig;
    #endregion

    public bool isDefense { get => currState == PlayerState.DefenseState; }

    private void Start()
    {
        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Init();
        ChangeState(PlayerState.Idle); // 默认进入待机状态
    }
    private void Update()
    {
        UpdateSkillCDTime();
    }
    private PlayerState currState;
    public void ChangeState(PlayerState playerState, bool reCurrstate = false)
    {
        currState = playerState;
        switch (playerState)
        {
            case PlayerState.Idle:
                stateMachine.ChangeState<Player_IdleState>(reCurrstate);
                break;
            case PlayerState.Move:
                stateMachine.ChangeState<Player_MoveState>(reCurrstate);
                break;
            case PlayerState.Jump:
                stateMachine.ChangeState<Player_JumpState>(reCurrstate);
                break;
            case PlayerState.AirDown:
                stateMachine.ChangeState<Player_AirDownState>(reCurrstate);
                break;
            case PlayerState.Roll:
                stateMachine.ChangeState<Player_RollState>(reCurrstate);
                break;
            case PlayerState.Hurt:
                stateMachine.ChangeState<Player_HurtState>(reCurrstate);
                break;
            case PlayerState.StandAttack:
                stateMachine.ChangeState<Player_StandAttackState>(reCurrstate);
                break;
            case PlayerState.SkillAttack:
                stateMachine.ChangeState<Player_SkillAttackState>(reCurrstate);
                break;
            case PlayerState.DefenseState:
                stateMachine.ChangeState<Player_DefenseState>(reCurrstate);
                break;
        }
    }

    public void ScreenImpulse(float force)
    {
        impulseSource.GenerateImpulse(force * 2); // 默认2倍
    }

    public override void OnHit(IHurt target, Vector3 hitPostion)
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
            if (attackData.ScreenImpulseValue != 0) ScreenImpulse(attackData.ScreenImpulseValue);
            if (attackData.ChromaticAberrationValue != 0) PostProcessManager.Instance.ChromaticAberrationEF(attackData.ChromaticAberrationValue);
            StartFreezeFrame(attackData.FreezeFrameTime);
            StartFreezeTime(attackData.FreezeGameTime);
        }
        else
        {
            // 生成基于命中配置的效果
            StartCoroutine(DoSkillHitEF(attackData.SkillHitEFConfig.FailSpawnObject, hitPostion));
        }
    }

    // True:成功命中
    // False:格挡住了
    public override bool Hurt(Skill_HitData hitData, ISkillOwner hurtSource)
    {
        SetHurtData(hitData, hurtSource);
        bool isDefence = currState == PlayerState.DefenseState;
        if (isDefence && hitData.Break) // 虽然玩家处于防御状态，但是这个技能破防，所以不需要考虑方向之类的问题
        {
            isDefence = false;
        }

        if (isDefence) // 玩家有可能背对着敌人进行防御，此时是无效的
        {
            Transform enemyTransform = ((CharacterBase)hurtSource).ModelTransform;
            Vector3 enemyToPlayerDir = (ModelTransform.position - enemyTransform.position).normalized;
            float dot = Vector3.Dot(ModelTransform.forward, enemyToPlayerDir);
            if (dot > 0)
            {
                isDefence = false;
            }
            else
            {
                // 通知防御状态
                Player_DefenseState defenseState = (Player_DefenseState)stateMachine.CurrentState;
                defenseState.Hurt();
            }
        }
        if (!isDefence)
        {
            UpdateHP(hitData);
            ChangeState(PlayerState.Hurt, true);
        }
        return !isDefence;
    }

    /// <summary>
    /// 检查并且进入技能状态
    /// </summary>
    /// <returns></returns>
    public bool CheckAndEnterSkillState()
    {
        if (!CanSwitchSkill) return false;

        // 检测所有技能有没有CD，并且玩家按了对应的键位
        for (int i = 0; i < skillInfoList.Count; i++)
        {
            if (skillInfoList[i].currentTime == 0 && Input.GetKeyDown(skillInfoList[i].keyCode))
            {
                // 释放技能
                ChangeState(PlayerState.SkillAttack, true);
                Player_SkillAttackState skillAttackState = (Player_SkillAttackState)stateMachine.CurrentState;
                skillAttackState.InitData(skillInfoList[i].skillConfig);
                // 让技能CD
                skillInfoList[i].currentTime = skillInfoList[i].cdTime;
                return true;
            }
        }
        return false;
    }

    private void UpdateSkillCDTime()
    {
        for (int i = 0; i < skillInfoList.Count; i++)
        {
            skillInfoList[i].currentTime = Mathf.Clamp(skillInfoList[i].currentTime - Time.deltaTime, 0, skillInfoList[i].cdTime);
            skillInfoList[i].cdmMaskImage.fillAmount = skillInfoList[i].currentTime / skillInfoList[i].cdTime;
        }
    }
}
