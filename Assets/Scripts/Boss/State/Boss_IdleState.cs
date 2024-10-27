using System.Diagnostics;
using UnityEngine;

public class Boss_IdleState : BossStateBase
{
    public override void Enter()
    {
        boss.PlayAnimation("Idle");
    }
    public override void Update()
    {
        boss.CharacterController.Move(new Vector3(0, boss.gravity * Time.deltaTime, 0));

        float distance = Vector3.Distance(boss.transform.position, boss.targetPlayer.transform.position);
        if (distance < boss.walkRange)
        {
            boss.ChangeState(BossState.Walk);
            return;
        }
        else
        {
            boss.ChangeState(BossState.Run);
            return;
        }
    }
}