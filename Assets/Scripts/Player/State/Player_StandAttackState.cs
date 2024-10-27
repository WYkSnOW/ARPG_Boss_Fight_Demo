﻿using UnityEngine;

public class Player_StandAttackState : PlayerStateBase
{
    // 当前时第几次攻击
    private int currentAttackIndex;

    private int CurrentAttackIndex
    {
        get => currentAttackIndex;
        set
        {
            if (value >= player.standAttackConfigs.Length) currentAttackIndex = 0;
            else currentAttackIndex = value;
        }
    }

    public override void Enter()
    {
        // 注册根运动
        player.Model.SetRooMotionAction(OnRootMotion);
        CurrentAttackIndex = -1;
        // 播放技能
        StandAttack();
    }

    public override void Exit()
    {
        player.Model.ClearRootMotionAction();
        player.OnSkillOver();
    }

    private void StandAttack()
    {
        CurrentAttackIndex += 1;
        player.StartAttack(player.standAttackConfigs[CurrentAttackIndex]);
    }

    public override void Update()
    {
        // 待机检测
        if (CheckAnimatorStateName(player.standAttackConfigs[CurrentAttackIndex].AnimationName, out float aniamtionTime) && aniamtionTime >= 1)
        {
            // 回到待机
            player.ChangeState(PlayerState.Idle);
            return;
        }

        // 检测技能
        if (player.CheckAndEnterSkillState())
        {
            return;
        }

        // 攻击检测
        if (CheckStandAttack())
        {
            StandAttack();
            return;
        }



        // 旋转逻辑
        if (player.CurrentSkillConfig.ReleaseData.CanRotate)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (h != 0 || v != 0)
            {
                // 处理旋转的问题
                Vector3 input = new Vector3(h, 0, v);
                // 获取相机的旋转值 y
                float y = Camera.main.transform.rotation.eulerAngles.y;
                // 让四元数和向量相乘：表示让这个向量按照这个四元数所表达的角度进行旋转后得到新的向量
                Vector3 targetDir = Quaternion.Euler(0, y, 0) * input;
                player.Model.transform.rotation = Quaternion.Slerp(player.Model.transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * player.rotateSpeedForAttack);
            }
        }

        if (player.CanSwitchSkill)
        {
            // 检测跳跃
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 切换到移动状态
                moveStatePower = 0;
                player.ChangeState(PlayerState.Jump);
                return;
            }
            // 检测翻滚
            if (Input.GetKeyDown(KeyCode.C))
            {
                moveStatePower = 0;
                player.ChangeState(PlayerState.Roll);
                return;
            }
        }
    }

    public bool CheckStandAttack()
    {
        return Input.GetMouseButtonDown(0) && player.CanSwitchSkill;
    }

    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        deltaPosition.y = player.gravity * Time.deltaTime;
        player.CharacterController.Move(deltaPosition);
    }
}