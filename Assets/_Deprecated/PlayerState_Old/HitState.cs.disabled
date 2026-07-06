using UnityEngine;

// 플레이어가 피격된 직후 짧은 경직과 넉백을 처리하는 상태입니다.
// 추가 무적 시간은 PlayerStatus의 타이머가 관리합니다.
public class HitState : PlayerBaseState
{
    // 경직이 끝날 때까지 남은 시간입니다.
    private float stunTimer;

    // 경직 시간 동안 적용할 넉백 속도입니다.
    private Vector3 knockbackVelocity;

    // 피격 상태에서는 공격, 대쉬, 패링, 방향 전환을 모두 막습니다.
    public override bool CanAttack => false;
    public override bool CanDash => false;
    public override bool CanParry => false;
    public override bool CanUseHealItem => false;
    public override bool CanUpdateFacingDirection => false;
    public override bool CanTakeDamage => false;

    public HitState(PlayerController controller) : base(controller) { }

    public void SetHit(DamageInfo damageInfo)
    {
        // 피격 시작 시점에 경직 시간과 넉백 방향을 확정합니다.
        stunTimer = PlayerStatus.HitStunDuration;
        knockbackVelocity = GetKnockbackDirection(damageInfo) *
            (PlayerStatus.HitKnockbackDistance / PlayerStatus.HitStunDuration);
    }

    public override void EnterState()
    {
        Debug.Log("Hit Enter");

        // 기존 점프/낙하 속도가 넉백 체감에 섞이지 않도록 약하게 초기화합니다.
        controller.Movement.ResetVerticalVelocity();
    }

    public override void UpdateState()
    {
        stunTimer -= Time.deltaTime;

        // 경직 중에도 중력은 적용해 공중에서 피격될 때 위치가 멈추지 않게 합니다.
        controller.Movement.ApplyGravity();
        controller.Movement.MoveByVelocity(knockbackVelocity);
        controller.Movement.MoveVerticalVelocity();

        if (stunTimer > 0f) return;

        // 경직이 끝나면 현재 지상 여부와 입력에 맞는 기본 상태로 복귀합니다.
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
        Debug.Log("Hit Exit");
    }

    private Vector3 GetKnockbackDirection(DamageInfo damageInfo)
    {
        // 공격 방향이 있으면 그 방향으로 밀려나는 것을 기본으로 사용합니다.
        Vector3 direction = new Vector3(damageInfo.HitDirection.x, 0f, 0f);
        if (direction.sqrMagnitude > Mathf.Epsilon)
            return direction.normalized;

        // 공격 주체가 있으면 공격자 반대 방향으로 밀려납니다.
        if (damageInfo.AttackerObject != null)
        {
            Vector3 fromAttacker = controller.transform.position - damageInfo.AttackerObject.transform.position;
            fromAttacker.y = 0f;

            if (fromAttacker.sqrMagnitude > Mathf.Epsilon) return fromAttacker.normalized;
        }

        // 정보가 부족하면 현재 바라보는 방향의 반대로 밀려나는 기본값을 사용합니다.
        return controller.IsFacingRight ? Vector3.right : Vector3.left;
    }
}
