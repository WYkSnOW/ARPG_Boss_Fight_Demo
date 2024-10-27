using UnityEngine;
using System.Collections;

public class Player_HurtState : PlayerStateBase
{
    private Skill_HitData hitData => player.hitData;
    private ISkillOwner source => player.hurtSource;

    private Coroutine repelCoroutine;

    enum HurtChildState
    {
        // 普通受伤
        NoramlHurt,
        // 击倒-Down
        Down,
        // 击倒-Rise
        Rise
    }

    private HurtChildState hurtState;
    private HurtChildState HurtState
    {
        get => hurtState;
        set
        {
            hurtState = value;
            switch (hurtState)
            {
                case HurtChildState.NoramlHurt:
                    player.OnFootStep();
                    player.PlayAnimation("Hurt");
                    break;
                case HurtChildState.Down:
                    player.OnFootStep();
                    Vector3 pos = source.ModelTransform.position;
                    player.ModelTransform.LookAt(new Vector3(pos.x, player.transform.position.y, pos.z));
                    player.PlayAnimation("Down");
                    break;
                case HurtChildState.Rise:
                    player.OnFootStep();
                    player.PlayAnimation("Rise");
                    break;
            }
        }
    }

    public override void Enter()
    {
        currHardTime = 0;
        if (hitData.Down) HurtState = HurtChildState.Down;
        else HurtState = HurtChildState.NoramlHurt;

        // 击退击飞
        if (hitData.RepelVelocity != Vector3.zero)
        {
            repelCoroutine = MonoManager.Instance.StartCoroutine(DoRepel(hitData.RepelTime, hitData.RepelVelocity));
        }
    }

    private float currHardTime = 0;
    public override void Update()
    {
        // 没有在击飞击退模拟中，就可以有重力
        if (repelCoroutine == null)
        {
            player.CharacterController.Move(new Vector3(0, player.gravity * Time.deltaTime, 0));
        }

        currHardTime += Time.deltaTime;
        switch (HurtState)
        {
            case HurtChildState.NoramlHurt:
                // 硬直时间到了 && 击飞击退效果也结束了 
                if (currHardTime >= hitData.HardTime && repelCoroutine == null)
                {
                    player.ChangeState(PlayerState.Idle);
                }
                break;
            case HurtChildState.Down:
                // 硬直时间到了 && 击飞击退效果也结束了 
                if (currHardTime >= hitData.HardTime && repelCoroutine == null)
                {
                    HurtState = HurtChildState.Rise;
                }
                break;
            case HurtChildState.Rise:

                // 检测起身动画播放完毕
                if (CheckAnimatorStateName("Rise", out float time) && time >= 0.99f)
                {
                    player.ChangeState(PlayerState.Idle);
                }
                break;
        }
    }

    private IEnumerator DoRepel(float time, Vector3 velocity)
    {
        float currTime = 0;
        // 避免分母为0
        time = time == 0 ? 0.0001f : time;
        // 将位移方向修改为针对对手的方向
        // Vector3 veloFromSource = source.ModelTransform.TransformDirection(velocity);
        Vector3 targetPosition = source.ModelTransform.TransformPoint(velocity);
        Vector3 dir = targetPosition - player.ModelTransform.position;
        while (currTime < time)
        {
            player.CharacterController.Move(dir / time * Time.deltaTime);
            currTime += Time.deltaTime;
            yield return null;
        }
        repelCoroutine = null;
    }

    public override void Exit()
    {
        if (repelCoroutine != null)
        {
            MonoManager.Instance.StopCoroutine(repelCoroutine);
            repelCoroutine = null;
        }
    }

}
