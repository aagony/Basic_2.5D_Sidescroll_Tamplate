using UnityEngine;

// 몬스터가 실제 데미지를 받은 뒤 발행되는 이벤트입니다.
// 피격 이펙트, 사운드, 디버그 표시처럼 후처리 시스템이 구독합니다.
public readonly struct MonsterDamagedEvent
{
    // 실제로 데미지를 받은 몬스터 오브젝트입니다.
    public readonly GameObject MonsterObject;

    // 이번 피격에 사용된 데미지 정보입니다.
    public readonly DamageInfo DamageInfo;

    // 피해 적용 후 현재 체력입니다.
    public readonly float CurrentHP;

    // 몬스터의 최대 체력입니다.
    public readonly float MaxHP;

    public MonsterDamagedEvent(
        GameObject monsterObject,
        DamageInfo damageInfo,
        float currentHP,
        float maxHP)
    {
        MonsterObject = monsterObject;
        DamageInfo = damageInfo;
        CurrentHP = currentHP;
        MaxHP = maxHP;
    }
}

// 몬스터 체력이 바뀌었을 때 UI나 디버그 표시가 구독할 수 있는 이벤트입니다.
public readonly struct MonsterHealthChangedEvent
{
    public readonly GameObject MonsterObject;
    public readonly float CurrentHP;
    public readonly float MaxHP;

    public MonsterHealthChangedEvent(GameObject monsterObject, float currentHP, float maxHP)
    {
        MonsterObject = monsterObject;
        CurrentHP = currentHP;
        MaxHP = maxHP;
    }
}

// 몬스터 체력이 0 이하가 되었을 때 발행하는 이벤트입니다.
public readonly struct MonsterDeadEvent
{
    public readonly GameObject MonsterObject;

    public MonsterDeadEvent(GameObject monsterObject)
    {
        MonsterObject = monsterObject;
    }
}
