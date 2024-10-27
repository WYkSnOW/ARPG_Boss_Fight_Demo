using UnityEngine;

public class Player_AirDownState : PlayerStateBase
{
    private enum AirDownChildState
    { 
        Loop,
        End
    }

    private float playEndAnimationHeight = 1.5f;   // 如果空中检测到距离地面有2米则启动翻滚
    private float endAnimationHeight = 1.5f;     // End动画播放需要的高度，从这个高度开始播放

    private LayerMask groundLayerMask = LayerMask.GetMask("Env");
    private bool needEndAnimation;
    private AirDownChildState airDownState;
    private AirDownChildState AirDownState
    {
        get => airDownState;
        set {
            airDownState = value;
            switch (airDownState)
            {
                case AirDownChildState.Loop:
                    player.PlayAnimation("JumpLoop");
                    break;
                case AirDownChildState.End:
                    player.PlayAnimation("JumpEnd");
                    break;
            }
        }
    }

    public override void Enter()
    {
        AirDownState = AirDownChildState.Loop;
        // 判断当前角色的高度是否有可能切换到End
        needEndAnimation = !Physics.Raycast(player.transform.position + new Vector3(0, 0.5f, 0), player.transform.up * -1, playEndAnimationHeight+0.5f, groundLayerMask);
    }

    public override void Update()
    {
        switch (airDownState)
        {
            case AirDownChildState.Loop:

                if (needEndAnimation)
                {
                    // 高度检测
                    if (Physics.Raycast(player.transform.position + new Vector3(0, 0.5f, 0), player.transform.up * -1, endAnimationHeight + 0.5f, groundLayerMask))
                    {
                        AirDownState = AirDownChildState.End;
                    }
                }
                else
                {
                    if (player.CharacterController.isGrounded)
                    {
                        player.OnFootStep();
                        player.ChangeState(PlayerState.Idle);
                        return;
                    }
                }
                AirControll();

                break;
            case AirDownChildState.End:
                if (CheckAnimatorStateName("JumpEnd",out float animationTime))
                {
                    if (animationTime>=0.8f)
                    {
                        // 此时依然踩空了，继续下坠
                        if (player.CharacterController.isGrounded == false)
                        {
                            AirDownState = AirDownChildState.Loop;
                        }
                        else
                        {
                            player.ChangeState(PlayerState.Idle);
                        }
                    }
                    else if(animationTime<0.6f)
                    {
                        AirControll();
                    }   
                }

                break;
        }
    }

    private void AirControll()
    {
        // 处理控制位移
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 motion = new Vector3(0, player.gravity * Time.deltaTime, 0);
        if (h != 0 || v != 0)
        {
            Vector3 input = new Vector3(h, 0, v);
            Vector3 dir = Camera.main.transform.TransformDirection(input);
            motion.x = player.moveSpeedForAirDown * Time.deltaTime * dir.x;
            motion.z = player.moveSpeedForAirDown * Time.deltaTime * dir.z;
            // 处理旋转
            // 获取相机的旋转值 y
            float y = Camera.main.transform.rotation.eulerAngles.y;
            // 让四元数和向量相乘：表示让这个向量按照这个四元数所表达的角度进行旋转后得到新的向量
            Vector3 targetDir = Quaternion.Euler(0, y, 0) * input;
            player.Model.transform.rotation = Quaternion.Slerp(player.Model.transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * player.rotateSpeed);
        }

        player.CharacterController.Move(motion);
    }
}
