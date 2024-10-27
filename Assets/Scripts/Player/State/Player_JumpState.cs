using UnityEngine;

public class Player_JumpState : PlayerStateBase
{
    public override void Enter()
    {
        player.PlayAnimation("JumpStart");
        // 注册根运动
        player.Model.SetRooMotionAction(OnRootMotion);
    }

    public override void Update()
    {

        if (CheckAnimatorStateName("JumpStart", out float animationTime) && animationTime >= 0.9f)
        {
            player.ChangeState(PlayerState.AirDown);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // 释放技能
            player.ChangeState(PlayerState.SkillAttack, true);
            Player_SkillAttackState skillAttackState = (Player_SkillAttackState)player.stateMachine.CurrentState;
            skillAttackState.InitData(player.jumpAttackSkillConfig);
        }
    }

    public override void Exit()
    {
        player.Model.ClearRootMotionAction();
    }
    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        deltaPosition.y *= player.jumpPower;
        Vector3 offset = moveStatePower * Time.deltaTime * player.moveSpeedForJump * player.Model.transform.forward;
        player.CharacterController.Move(deltaPosition + offset);
    }
}
