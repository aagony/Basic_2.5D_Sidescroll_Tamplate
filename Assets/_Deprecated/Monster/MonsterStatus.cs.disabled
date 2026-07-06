using System;
using UnityEngine;

[Serializable]
public class MonsterStatusData : LivingStatus
{
}

// 몬스터의 HP와 기본 스탯을 관리하는 컴포넌트입니다.
// 공격 판정이 직접 전달한 DamageInfo를 받아 실제 HP를 깎습니다.
[RequireComponent(typeof(HitFlashFeedback))]
public class MonsterStatus : MonoBehaviour
{
    [Header("Base Status")]
    // 테스트 몬스터의 기본 체력입니다.
    [SerializeField] private float baseMaxHP = 30f;

    // 이후 몬스터 공격력이 스탯 기반으로 필요할 때 사용할 기본 공격력입니다.
    [SerializeField] private float baseAttackPower = 10f;

    // 이동 속도 스탯입니다. 현재 ParryTestMonster의 돌진은 별도 값으로 움직입니다.
    [SerializeField] private float baseMoveSpeed = 3f;

    // 공격 속도 스탯입니다. 이후 공격 주기 보정에 사용할 수 있습니다.
    [SerializeField] private float baseAttackSpeed = 1f;

    // 쿨다운 스탯입니다. 이후 스킬 쿨타임 보정에 사용할 수 있습니다.
    [SerializeField] private float baseCooldown = 1f;

    // 실제 전투 중 사용되는 몬스터 스탯 데이터입니다.
    [SerializeField]
    private MonsterStatusData status = new MonsterStatusData();

    public MonsterStatusData Status => status;

    private void Awake()
    {
        // 피격 색상 피드백이 빠져 있으면 런타임에서 붙여 테스트가 막히지 않게 합니다.
        EnsureHitFlashFeedback();

        // 인스펙터 기본값을 Stat 객체에 반영하고 현재 체력을 채웁니다.
        SetupBaseStatus();
        Init();
    }

    private void SetupBaseStatus()
    {
        // 인스펙터에서 조절한 값을 LivingStatus의 공통 Stat에 복사합니다.
        status.MaxHP.SetBaseValue(baseMaxHP);
        status.AttackPower.SetBaseValue(baseAttackPower);
        status.MoveSpeed.SetBaseValue(baseMoveSpeed);
        status.AttackSpeed.SetBaseValue(baseAttackSpeed);
        status.Cooldown.SetBaseValue(baseCooldown);
    }

    public void Init()
    {
        // 현재 체력을 최대 체력으로 채우고 체력 변경 이벤트를 발행합니다.
        status.Init();
        PublishHealthChanged();
    }

    public void TakeDamage(float damage)
    {
        // 디버그나 테스트 코드에서 숫자만 넘겨도 같은 데미지 흐름을 타도록 감쌉니다.
        TakeDamage(
            new DamageInfo(
                gameObject,
                null,
                null,
                transform.position,
                Vector3.zero,
                damage
            )
        );
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        // 다른 대상용 DamageInfo가 잘못 전달된 경우에는 처리하지 않습니다.
        if (!IsTargetSelf(damageInfo.TargetObject)) return;

        // 이미 죽은 몬스터는 추가 피해를 무시합니다.
        if (status.IsDead) return;

        // 음수 데미지나 0 데미지는 적용하지 않습니다.
        float damage = Mathf.Max(0f, damageInfo.Damage);
        if (Mathf.Approximately(damage, 0f)) return;

        status.CurrentHP -= damage;

        if (status.CurrentHP < 0f)
            status.CurrentHP = 0f;

        PublishHealthChanged();
        PublishDamaged(damageInfo, damage);

        // HP가 0이 되면 사망 이벤트를 발행합니다.
        if (status.IsDead)
            EventBus<MonsterDeadEvent>.Publish(new MonsterDeadEvent(gameObject));
    }

    public float GetMaxHP()
    {
        return status.MaxHP.FinalValue;
    }

    public float GetCurrentHP()
    {
        return status.CurrentHP;
    }

    public float GetAttackPower()
    {
        return status.AttackPower.FinalValue;
    }

    public float GetMoveSpeed()
    {
        return status.MoveSpeed.FinalValue;
    }

    private bool IsTargetSelf(GameObject targetObject)
    {
        // 대상 정보가 비어 있으면 직접 호출로 보고 현재 몬스터에게 적용합니다.
        if (targetObject == null) return true;

        // 자식 콜라이더가 맞아도 부모 몬스터가 맞은 것으로 처리합니다.
        return targetObject == gameObject || targetObject.transform.IsChildOf(transform);
    }

    private void PublishHealthChanged()
    {
        // UI나 디버그 표시가 몬스터 체력을 구독할 수 있게 알립니다.
        EventBus<MonsterHealthChangedEvent>.Publish(
            new MonsterHealthChangedEvent(
                gameObject,
                status.CurrentHP,
                status.MaxHP.FinalValue
            )
        );
    }

    private void PublishDamaged(DamageInfo damageInfo, float appliedDamage)
    {
        // 후처리 시스템이 실제 적용된 데미지와 피격 정보를 함께 받을 수 있게 알립니다.
        EventBus<MonsterDamagedEvent>.Publish(
            new MonsterDamagedEvent(
                gameObject,
                new DamageInfo(
                    gameObject,
                    damageInfo.HitCollider,
                    damageInfo.AttackerObject,
                    damageInfo.HitPoint,
                    damageInfo.HitDirection,
                    appliedDamage
                ),
                status.CurrentHP,
                status.MaxHP.FinalValue
            )
        );
    }

    private void EnsureHitFlashFeedback()
    {
        // RequireComponent는 새로 붙일 때만 보장되므로 기존 오브젝트를 위해 한 번 더 확인합니다.
        if (GetComponent<HitFlashFeedback>() != null) return;

        gameObject.AddComponent<HitFlashFeedback>();
    }
}
