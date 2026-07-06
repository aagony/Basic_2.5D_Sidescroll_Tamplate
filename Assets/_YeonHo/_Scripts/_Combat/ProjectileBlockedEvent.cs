using UnityEngine;

public readonly struct ProjectileBlockedEvent
{
    public readonly GameObject ProjectileObject;
    public readonly GameObject OwnerObject;
    public readonly Collider BlockCollider;
    public readonly Vector3 HitPoint;
    public readonly Vector3 HitDirection;

    // 투사체가 방어 또는 지형에 막힌 정보를 저장합니다
    public ProjectileBlockedEvent(
        GameObject projectileObject,
        GameObject ownerObject,
        Collider blockCollider,
        Vector3 hitPoint,
        Vector3 hitDirection)
    {
        ProjectileObject = projectileObject;
        OwnerObject = ownerObject;
        BlockCollider = blockCollider;
        HitPoint = hitPoint;
        HitDirection = hitDirection;
    }
}
