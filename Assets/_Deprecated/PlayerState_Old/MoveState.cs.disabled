using UnityEngine;

// 플레이어가 지상에서 좌우 이동 중인 상태입니다.
// 이동 입력이 사라지면 Idle, 점프 입력이면 Jump, 대쉬 입력이면 Dash로 전환합니다.
public class MoveState : PlayerBaseState
{
    public MoveState(PlayerController controller) : base(controller) { }

    public override void EnterState()
    {
        Debug.Log("Move Enter");
    }

    public override void UpdateState()
    {
        // 이동 중에도 중력을 적용해 경사, 단차, 착지 판정이 자연스럽게 갱신되도록 합니다.
        controller.Movement.ApplyGravity();

        // 현재 입력 방향으로 수평 이동하고, 누적된 수직 속도도 함께 적용합니다.
        controller.Movement.Move(controller.MoveInput);

        if (!controller.Movement.IsGrounded && controller.Movement.IsFalling)
        {
            controller.TransitionTo(controller.FallState);
            return;
        }

        // 대쉬가 가능한 상태라면 이동보다 대쉬를 우선 처리합니다.
        if (controller.DashTriggered && controller.Movement.CanDash())
        {
            controller.TransitionTo(controller.DashState);
            return;
        }

        // 이동 입력이 완전히 사라지면 정지 상태로 돌아갑니다.
        if (controller.MoveInput == Vector2.zero)
        {
            controller.TransitionTo(controller.IdleState);
            return;
        }

        // 점프 입력을 받으면 점프 상태로 전환합니다.
        if (controller.JumpTriggered)
        {
            controller.TransitionTo(controller.JumpState);
            return;
        }
    }

    public override void ExitState()
    {
        Debug.Log("Move Exit");
    }

    public override void FixedUpdateState()
    {
    }
}
