using UnityEngine;

public class Boss_AttackState : BossStateBase
{
    // 当前时第几次攻击
    private int currentAttackIndex;

    private int CurrentAttackIndex
    {
        get => currentAttackIndex;
        set
        {
            if (value >= boss.standAttackConfigs.Length) currentAttackIndex = 0;
            else currentAttackIndex = value;
        }
    }

    private float currentAttackTime = 0;
    private SkillConfig currentSkillConfig;
    public override void Enter()
    {
        boss.Model.SetRooMotionAction(OnRootMotion);
        CurrentAttackIndex = -1;
        currentAttackTime = 0;
        // 播放技能
        StandAttack();
    }

    public override void Exit()
    {
        boss.Model.ClearRootMotionAction();
        boss.OnSkillOver();
        currentSkillConfig = null;
    }

    private void StandAttack()
    {
        CurrentAttackIndex += 1;
        // 注册根运动
        Vector3 pos = boss.targetPlayer.transform.position;
        boss.transform.LookAt(new Vector3(pos.x, boss.transform.position.y, pos.z));
        boss.StartAttack(boss.standAttackConfigs[CurrentAttackIndex]);
        currentSkillConfig = boss.standAttackConfigs[CurrentAttackIndex];
    }

    public override void Update()
    {
        currentAttackTime += Time.deltaTime;

        if (boss.CanSwitchSkill)// 后摇可以取消了
        {
            float distance = Vector3.Distance(boss.transform.position, boss.targetPlayer.transform.position);
            if (distance <= boss.attackRange && currentAttackTime < boss.attackTime)
            {
                // 先检测有没有破防技能
                if (boss.targetPlayer.isDefense)
                {
                    for (int i = 0; i < boss.skillInfoList.Count; i++)
                    {
                        if (boss.skillInfoList[i].currentTime == 0 && boss.skillInfoList[i].skillConfig.AttackData.Length > 0)
                        {
                            if (boss.skillInfoList[i].skillConfig.AttackData[0].HitData.Break)
                            {
                                StartSkill(i);
                                return;
                            }
                        }
                    }
                }
                // 检测其他技能
                for (int i = 0; i < boss.skillInfoList.Count; i++)
                {
                    if (boss.skillInfoList[i].currentTime == 0)
                    {
                        StartSkill(i);
                        return;
                    }
                }

                // 普通攻击
                StandAttack();
            }
            else
            {
                boss.ChangeState(BossState.Walk);
            }
        }
        else if (currentSkillConfig != null && CheckAnimatorStateName(currentSkillConfig.AnimationName, out float aniamtionTime) && aniamtionTime >= 1)
        {
            // 回到待机
            boss.ChangeState(BossState.Walk);
            return;
        }
    }

    private void StartSkill(int index)
    {
        Vector3 pos = boss.targetPlayer.transform.position;
        boss.transform.LookAt(new Vector3(pos.x, boss.transform.position.y, pos.z));
        currentSkillConfig = boss.skillInfoList[index].skillConfig;
        boss.StartSkill(index);
    }

    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        deltaPosition.y = boss.gravity * Time.deltaTime;
        boss.CharacterController.Move(deltaPosition);
    }
}