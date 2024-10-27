using UnityEngine;
using System.Collections;

public class Player_DefenseState : PlayerStateBase
{
    private enum DefenseChildState
    {
        Enter,
        Hold,
        WaitCounterattack,
        Counterattack,
        Exit,
    }

    private DefenseChildState childState;

    private DefenseChildState ChildState
    {
        get => childState;
        set
        {
            childState = value;
            switch (childState)
            {
                case DefenseChildState.Enter:
                    player.PlayAnimation("EnterDefence");
                    break;
                case DefenseChildState.Hold:
                    break;
                case DefenseChildState.WaitCounterattack:
                    waitCounterattackTimerCoroutine = MonoManager.Instance.StartCoroutine(WaitCounterattackTimer());
                    break;
                case DefenseChildState.Counterattack:
                    player.StartAttack(player.counterattackSkillConfig);
                    break;
                case DefenseChildState.Exit:
                    player.PlayAnimation("ExitDefence");
                    break;
            }
        }
    }

    public void Hurt()
    {
        if (childState == DefenseChildState.Hold)
        {
            ChildState = DefenseChildState.WaitCounterattack;
        }
    }

    public override void Enter()
    {
        // 注册根运动
        player.Model.SetRooMotionAction(OnRootMotion);
        ChildState = DefenseChildState.Enter;
    }

    public override void Update()
    {
        switch (childState)
        {
            case DefenseChildState.Enter:
                if (CheckAnimatorStateName("EnterDefence", out float aniamtionTime) && aniamtionTime >= 1)
                {
                    ChildState = DefenseChildState.Hold;
                    return;
                }
                break;
            case DefenseChildState.Hold:
                if (Input.GetKeyUp(KeyCode.F)) // 如果松开按键，则切换到退出
                {
                    ChildState = DefenseChildState.Exit;
                }
                break;
            case DefenseChildState.WaitCounterattack:
                // 反击检测
                if (Input.GetMouseButtonDown(0))
                {
                    MonoManager.Instance.StopCoroutine(waitCounterattackTimerCoroutine);
                    waitCounterattackTimerCoroutine = null;
                    ChildState = DefenseChildState.Counterattack;
                }
                // 退出状态检测
                else if (Input.GetKeyUp(KeyCode.F)) // 如果松开按键，则切换到退出
                {
                    ChildState = DefenseChildState.Exit;
                    MonoManager.Instance.StopCoroutine(waitCounterattackTimerCoroutine);
                    waitCounterattackTimerCoroutine = null;
                }
                break;
            case DefenseChildState.Counterattack:
                // 动画播放完毕检测
                if (CheckAnimatorStateName(player.counterattackSkillConfig.AnimationName, out float attackAniamtionTime) && attackAniamtionTime >= 1)
                {
                    // 回到待机
                    player.ChangeState(PlayerState.Idle);
                }
                else if (player.CanSwitchSkill && Input.GetMouseButton(0))
                {
                    player.ChangeState(PlayerState.StandAttack);
                }
                break;
            case DefenseChildState.Exit:
                if (CheckAnimatorStateName("ExitDefence", out float exitAniamtionTime) && exitAniamtionTime >= 1)
                {
                    // 回到待机
                    player.ChangeState(PlayerState.Idle);
                }
                break;
        }
    }

    private Coroutine waitCounterattackTimerCoroutine;
    private IEnumerator WaitCounterattackTimer()
    {
        yield return new WaitForSeconds(player.waitCounterattackTime);
        ChildState = DefenseChildState.Hold;
        waitCounterattackTimerCoroutine = null;
    }

    public override void Exit()
    {
        player.Model.ClearRootMotionAction();
    }

    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        deltaPosition.y = player.gravity * Time.deltaTime;
        player.CharacterController.Move(deltaPosition);
    }
}