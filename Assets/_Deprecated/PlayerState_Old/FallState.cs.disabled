using UnityEngine;

// 플레이어가 공중에 떠 있거나 낙하 중일 때의 상태입니다.
// JumpState는 점프 시작만 맡고, FallState는 중력, 공중 이동, 추가 점프, 공중 대쉬, 착지를 처리합니다.
public class FallState : PlayerBaseState
{
    public FallState(PlayerController controller) : base(controller) { }

    public override void EnterState()
    {
        Debug.Log("Fall Enter");
    }

    public override void UpdateState()
    {
        controller.Movement.ApplyGravity();
        controller.Movement.Move(controller.MoveInput);

        if (controller.DashTriggered && controller.Movement.CanDash())
        {
            controller.TransitionTo(controller.DashState); 
            return;
        }

        if (controller.JumpTriggered && controller.Movement.CanJump())
        {
            controller.TransitionTo(controller.JumpState); 
            return;                        
        }

        if (controller.Movement.IsGrounded && controller.Movement.IsFalling)
        {
            if (controller.MoveInput != Vector2.zero)
            {
                controller.TransitionTo(controller.MoveState);
            }
            else
            {
                controller.TransitionTo(controller.IdleState);
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log("Fall Exit");
    }

    public override void FixedUpdateState()
    {
    }
}
