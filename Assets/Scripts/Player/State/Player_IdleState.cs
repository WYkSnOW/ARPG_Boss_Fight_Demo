using UnityEngine;

public class Player_IdleState : PlayerStateBase
{
    public override void Enter()
    {
        // 播放角色待机动画
        player.PlayAnimation("Idle");
    }

    public override void Update()
    {
        // 检测技能
        if (player.CheckAndEnterSkillState())
        {
            return;
        }

        // 检测攻击
        if (Input.GetMouseButtonDown(0))
        {
            player.ChangeState(PlayerState.StandAttack);
            return;
        }

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

        // 检测玩家移动
        player.CharacterController.Move(new Vector3(0, player.gravity * Time.deltaTime, 0));
        // 检测下落
        if (player.CharacterController.isGrounded == false)
        {
            player.ChangeState(PlayerState.AirDown);
            return;
        }

        // 检测格挡
        if (Input.GetKeyDown(KeyCode.F))
        {
            player.ChangeState(PlayerState.DefenseState);
            return;
        }


        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (h != 0 || v != 0)
        {
            // 切换到移动状态
            player.ChangeState(PlayerState.Move);
            return;
        }
    }
}