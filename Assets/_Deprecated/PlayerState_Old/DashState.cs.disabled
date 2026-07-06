using UnityEngine;

// 플레이어가 짧은 시간 동안 고속 이동하는 대쉬 상태입니다.
// 대쉬 중에는 공격, 재대쉬, 패링을 막고 정해진 시간 동안 한 방향으로 이동합니다.
public class DashState : PlayerBaseState
{
    // 대쉬가 남은 시간을 저장합니다.
    private float dashTimer;

    // 대쉬 시작 시점에 확정된 이동 방향입니다.
    private Vector3 dashDirection;

    // 대쉬 중에는 공격 입력을 처리하지 않습니다.
    public override bool CanAttack => false;

    // 대쉬 중에는 추가 대쉬와 바라보는 방향 갱신을 막습니다.
    public override bool CanDash => false;

    public override bool CanUpdateFacingDirection => false;

    // 대쉬 중에는 패링을 막습니다.
    public override bool CanParry => false;

    // 대쉬 중에는 회복 아이템 사용을 시작할 수 없습니다.
    public override bool CanUseHealItem => false;

    // 대시 중에는 회피 판정으로 피해를 받지 않습니다.
    public override bool CanTakeDamage => false;

    public DashState(PlayerController controller) : base(controller) { }

    public override void EnterState()
    {
        Debug.Log("Dash Enter");

        // 대쉬 시간과 방향은 진입 시점에 고정합니다.
        dashTimer = controller.Movement.DashDuration;
        dashDirection = GetDashDirection();

        // 대쉬 쿨타임과 공중 대쉬 사용 여부를 기록합니다.
        controller.Movement.ConsumeDash();

        // 대쉬 중 위/아래 속도가 섞이지 않도록 수직 속도를 초기화합니다.
        controller.Movement.ResetVerticalVelocity();
    }

    public override void UpdateState()
    {
        // 대쉬 시간은 프레임마다 감소합니다.
        dashTimer -= Time.deltaTime;

        // 정해진 방향과 속도로 CharacterController를 이동시킵니다.
        Vector3 dashVelocity = dashDirection * controller.Movement.DashSpeed;
        controller.Movement.MoveByVelocity(dashVelocity);

        // 아직 시간이 남아 있으면 상태 전환 없이 대쉬를 계속합니다.
        if (dashTimer > 0f) return;

        // 대쉬가 끝난 뒤 지상에 있다면 입력 여부에 따라 이동/정지 상태로 돌아갑니다.
        if (controller.Movement.IsGrounded)
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
        else
        {
            // 공중에서 대쉬가 끝났다면 점프 상태로 넘겨 중력과 공중 이동을 계속 처리합니다.
            controller.TransitionTo(controller.FallState);
        }
    }

    private Vector3 GetDashDirection()
    {
        // 현재 입력 방향이 있으면 입력을 우선해서 대쉬 방향을 정합니다.
        if (controller.MoveInput.x > 0f) return Vector3.left;
         
        if (controller.MoveInput.x < 0f) return Vector3.right;

        // 입력이 없다면 마지막으로 바라보던 방향으로 대쉬합니다.
        return controller.IsFacingRight ? Vector3.left : Vector3.right;
    }

    public override void ExitState()
    {
        Debug.Log("Dash Exit");
    }

    public override void FixedUpdateState()
    {
    }
}
