using UnityEngine;

// 플레이어가 사망한 뒤 부활 전까지 모든 조작과 피격을 잠그는 상태입니다.
// 사망 연출과 UI는 PlayerDeadEvent를 구독한 별도 시스템이 처리합니다.
public class DeathState : PlayerBaseState
{
    // 마지막 사망 정보를 보관해 이후 연출 확장에서 참조할 수 있게 합니다.
    private DeathInfo deathInfo;

    // 사망 상태에서는 모든 플레이어 행동과 추가 피격을 막습니다.
    public override bool CanAttack => false;
    public override bool CanDash => false;
    public override bool CanParry => false;
    public override bool CanUseHealItem => false;
    public override bool CanUpdateFacingDirection => false;
    public override bool CanTakeDamage => false;

    public DeathState(PlayerController controller) : base(controller) { }

    public void SetDeath(DeathInfo info)
    {
        // 상태 진입 전에 사망 정보를 확정해 둡니다.
        deathInfo = info;
    }

    public override void EnterState()
    {
        Debug.Log($"Death Enter: {deathInfo.Cause}");

        // 사망 순간에는 기존 점프, 낙하, 넉백 속도를 멈춰 현재 위치에 고정합니다.
        controller.Movement.ResetVerticalVelocity();
    }

    public override void UpdateState()
    {
        // 부활 코루틴이 끝날 때까지 현재 위치에서 멈춰 있습니다.
    }

    public override void FixedUpdateState()
    {
    }

    public override void ExitState()
    {
        Debug.Log("Death Exit");
    }
}
