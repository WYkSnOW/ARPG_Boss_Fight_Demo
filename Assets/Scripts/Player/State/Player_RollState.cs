using UnityEngine;
using System.Collections;

public class Player_RollState : PlayerStateBase
{
    private Coroutine rotateCoroutine;
    private bool isRotate = false;
    public override void Enter()
    {
        player.Model.Animator.speed = 1;
        // 检测玩家的输入方向
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h != 0 || v != 0)
        {
            Vector3 inputDir = new Vector3(h, 0, v).normalized;
            rotateCoroutine = MonoManager.Instance.StartCoroutine(DoRotate(inputDir));
        }
        else
        {
            player.PlayAnimation("Roll");
            player.Model.SetRooMotionAction(OnRootMotion);
        }
    }

    private IEnumerator DoRotate(Vector3 dir)
    {
        isRotate = true;
        float y = Camera.main.transform.rotation.eulerAngles.y;
        Vector3 targetDir = Quaternion.Euler(0, y, 0) * dir;
        Quaternion targetRotation = Quaternion.LookRotation(targetDir);
        float rate = 0;
        while (rate < 1)
        {
            rate += Time.deltaTime * 10; // 10倍速旋转
            player.Model.transform.rotation = Quaternion.Slerp(player.Model.transform.rotation, targetRotation, rate);
            yield return null;
        }
        isRotate = false;
        player.PlayAnimation("Roll");
        player.Model.SetRooMotionAction(OnRootMotion);
    }

    public override void Update()
    {
        if (isRotate) return;
        if (CheckAnimatorStateName("Roll", out float animationTime))
        {
            if (animationTime > 0.8f)
            {
                player.ChangeState(PlayerState.Idle);
            }
        }
    }

    public override void Exit()
    {
        moveStatePower = 0;
        player.Model.ClearRootMotionAction();

        if (rotateCoroutine != null) MonoManager.Instance.StopCoroutine(rotateCoroutine);
    }
    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        deltaPosition *= Mathf.Clamp(moveStatePower, 1, 1.5f);
        deltaPosition.y = player.gravity * Time.deltaTime;
        player.CharacterController.Move(deltaPosition);
    }
}