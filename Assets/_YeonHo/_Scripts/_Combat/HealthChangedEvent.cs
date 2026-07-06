using UnityEngine;

public readonly struct HealthChangedEvent
{
    public readonly GameObject TargetObject;
    public readonly float CurrentHp;
    public readonly float MaxHp;

    // 체력 변경 이벤트에 필요한 정보를 저장합니다
    public HealthChangedEvent(GameObject targetObject, float currentHp, float maxHp)
    {
        TargetObject = targetObject;
        CurrentHp = currentHp;
        MaxHp = maxHp;
    }
}
