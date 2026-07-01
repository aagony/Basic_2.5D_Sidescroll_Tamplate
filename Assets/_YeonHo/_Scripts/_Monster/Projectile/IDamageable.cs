public interface IDamageable
{
    bool CanTakeDamage { get; }

    // DamageInfo를 받아 데미지와 피격 처리를 수행합니다
    void TakeDamage(DamageInfo damageInfo);
}
