using System.Diagnostics;
using UnityEngine;

public class Boss_RunState : BossStateBase
{
    public override void Enter()
    {
        boss.PlayAnimation("Run");
        boss.navMeshAgent.enabled = true;
        boss.navMeshAgent.speed = boss.runSpeed;
    }
    public override void Update()
    {
        float distance = Vector3.Distance(boss.transform.position, boss.targetPlayer.transform.position);
        if (distance <= boss.walkRange)
        {
            boss.ChangeState(BossState.Walk);
        }
        else
        {
            boss.navMeshAgent.SetDestination(boss.targetPlayer.transform.position);
        }
    }

    public override void Exit()
    {
        boss.navMeshAgent.enabled = false;
    }
}