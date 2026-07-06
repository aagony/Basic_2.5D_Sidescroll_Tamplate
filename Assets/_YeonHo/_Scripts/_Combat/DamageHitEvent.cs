using UnityEngine;

public readonly struct DamageHitEvent
{
    public readonly GameObject TargetObject;
    public readonly GameObject AttackerObject;
    public readonly Collider HitCollider;
    public readonly Vector3 HitPoint;
    public readonly Vector3 HitDirection;
    public readonly float DamageAmount;

    // 데미지가 실제로 적용된 피격 정보를 저장합니다
    public DamageHitEvent(
        GameObject targetObject,
        GameObject attackerObject,
        Collider hitCollider,
        Vector3 hitPoint,
        Vector3 hitDirection,
        float damageAmount)
    {
        TargetObject = targetObject;
        AttackerObject = attackerObject;
        HitCollider = hitCollider;
        HitPoint = hitPoint;
        HitDirection = hitDirection;
        DamageAmount = damageAmount;
    }
}
