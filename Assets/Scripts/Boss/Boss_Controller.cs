using UnityEngine;
using UnityEngine.AI;

public class Boss_Controller : CharacterBase
{
    public Player_Controller targetPlayer;
    public NavMeshAgent navMeshAgent;
    public float walkRange = 8;
    public float walkSpeed;
    public float runSpeed;
    public float attackRange;

    public float vigilantTime = 10;
    public float vigilantRange = 6;
    public float vigilantSpeed = 2.5f;

    public float attackTime = 5;
    public bool anger;

    private void Start()
    {
        currentSkillCDTimer = 0;
        Init();
        ChangeState(BossState.Idle);
    }

    private void Update()
    {
        UpdateSkillCDTime();
    }
    public void ChangeState(BossState bossState, bool reCurrstate = false)
    {
        switch (bossState)
        {
            case BossState.Idle:
                stateMachine.ChangeState<Boss_IdleState>(reCurrstate);
                break;
            case BossState.Walk:
                stateMachine.ChangeState<Boss_WalkState>(reCurrstate);
                break;
            case BossState.Run:
                stateMachine.ChangeState<Boss_RunState>(reCurrstate);
                break;
            case BossState.Hurt:
                anger = true;
                stateMachine.ChangeState<Boss_HurtState>(reCurrstate);
                break;
            case BossState.Attack:
                anger = false;
                stateMachine.ChangeState<Boss_AttackState>(reCurrstate);
                break;
        }
    }

    public float currentSkillCDTimer;
    public void StartSkill(int index)
    {
        currentSkillCDTimer = 2;
        skillInfoList[index].currentTime = skillInfoList[index].cdTime;
        StartAttack(skillInfoList[index].skillConfig);
    }
    private void UpdateSkillCDTime()
    {
        for (int i = 0; i < skillInfoList.Count; i++)
        {
            skillInfoList[i].currentTime = Mathf.Clamp(skillInfoList[i].currentTime - Time.deltaTime, 0, skillInfoList[i].cdTime);
        }
        if (currentSkillCDTimer > 0)
        {
            currentSkillCDTimer -= Time.deltaTime;
        }
    }
    public override bool Hurt(Skill_HitData hitData, ISkillOwner hurtSource)
    {
        SetHurtData(hitData, hurtSource);
        ChangeState(BossState.Hurt, true);
        UpdateHP(hitData);
        return true;
    }

    #region UnityEditor

#if UNITY_EDITOR
    [ContextMenu("SetHurtCollider")]
    private void SetHurtCollider()
    {
        // 设置所有的碰撞体为HurtColldier
        Collider[] colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            // 排除武器
            if (colliders[i].GetComponent<Weapon_Controller>() == null)
            {
                colliders[i].gameObject.layer = LayerMask.NameToLayer("HurtCollider");
            }
        }
        // 标记场景修改
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
#endif
    #endregion
}
