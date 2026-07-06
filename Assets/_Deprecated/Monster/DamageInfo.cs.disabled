using UnityEngine;

// 실제 데미지 처리에 필요한 피격 정보를 한 번에 넘기기 위한 값 타입입니다.
// 이벤트 요청용이 아니라 Status가 직접 TakeDamage를 받을 때 사용하는 데이터입니다.
public readonly struct DamageInfo
{
    // 데미지를 받을 대상 오브젝트입니다.
    public readonly GameObject TargetObject;

    // 실제로 맞은 콜라이더입니다.
    public readonly Collider HitCollider;

    // 공격을 만든 주체입니다.
    public readonly GameObject AttackerObject;

    // 공격이 닿은 월드 좌표입니다.
    public readonly Vector3 HitPoint;

    // 공격이 들어온 방향입니다.
    public readonly Vector3 HitDirection;

    // 요청된 피해량입니다. 실제 적용 전 Status에서 보정합니다.
    public readonly float Damage;

    public DamageInfo(
        GameObject targetObject,
        Collider hitCollider,
        GameObject attackerObject,
        Vector3 hitPoint,
        Vector3 hitDirection,
        float damage)
    {
        TargetObject = targetObject;
        HitCollider = hitCollider;
        AttackerObject = attackerObject;
        HitPoint = hitPoint;
        HitDirection = hitDirection;
        Damage = damage;
    }
}
