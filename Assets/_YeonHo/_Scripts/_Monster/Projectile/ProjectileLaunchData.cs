using UnityEngine;

public readonly struct ProjectileLaunchData
{
    public readonly Vector3 Position;
    public readonly Vector3 Direction;
    public readonly float Speed;
    public readonly float Damage;
    public readonly GameObject Owner;
    public readonly int PenetrationCount;
    public readonly float LifeTime;

    // 투사체 발사에 필요한 실행 시점 데이터를 저장합니다
    public ProjectileLaunchData(
        Vector3 position,
        Vector3 direction,
        float speed,
        float damage,
        GameObject owner = null,
        int penetrationCount = 0,
        float lifeTime = -1f)
    {
        Position = position;
        Direction = direction;
        Speed = speed;
        Damage = damage;
        Owner = owner;
        PenetrationCount = penetrationCount;
        LifeTime = lifeTime;
    }
}
