using UnityEngine;

public class HealItemUseState : PlayerBaseState
{
    private float useTimer;

    public override bool CanAttack => false;
    public override bool CanDash => false;
    public override bool CanUpdateFacingDirection => false;
    public override bool CanParry => false;
    public override bool CanUseHealItem => false;
    public override bool CanTakeDamage => true;

    public HealItemUseState(PlayerController controller) : base(controller) { }

    public override void EnterState()
    {
        Debug.Log("Heal Item Use Enter");

        // 사용 중에는 완전히 멈춘 상태가 되도록 기존 수직 속도를 제거합니다.
        useTimer = controller.HealItemInventory.UseDuration;
        controller.Movement.ResetVerticalVelocity();
    }

    public override void UpdateState()
    {
        useTimer -= Time.deltaTime;
        if (useTimer > 0f) return;

        // 1초 사용이 끝난 시점에만 아이템을 소모하고 체력을 회복합니다.
        controller.HealItemInventory.TryCompleteUse();

        if (!controller.Movement.IsGrounded)
        {
            controller.TransitionTo(controller.FallState);
            return;
        }

        if (controller.MoveInput != Vector2.zero)
        {
            controller.TransitionTo(controller.MoveState);
            return;
        }


        controller.TransitionTo(controller.IdleState);
    }

    public override void FixedUpdateState()
    {
    }

    public override void ExitState()
    {
        Debug.Log("Heal Item Use Exit");
    }
}
