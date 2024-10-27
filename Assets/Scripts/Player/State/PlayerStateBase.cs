using UnityEngine;

public class PlayerStateBase : StateBase
{
    protected Player_Controller player;
    protected static float moveStatePower;
    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        player = (Player_Controller)owner;
    }

    protected virtual bool CheckAnimatorStateName(string stateName,out float normalizedTime)
    {
        // 处于过于动画过渡阶段的考虑，优先判断下一个状态
        AnimatorStateInfo nextInfo = player.Model.Animator.GetNextAnimatorStateInfo(0);
        if (nextInfo.IsName(stateName))
        {
            normalizedTime = nextInfo.normalizedTime;
            return true;
        }

        AnimatorStateInfo currentInfo = player.Model.Animator.GetCurrentAnimatorStateInfo(0);
        normalizedTime = currentInfo.normalizedTime;
        return currentInfo.IsName(stateName);
    }
}
