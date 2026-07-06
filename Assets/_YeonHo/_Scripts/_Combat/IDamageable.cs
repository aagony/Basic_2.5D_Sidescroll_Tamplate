public interface IDamageable
{
    bool CanTakeDamage { get; }

    // 대상의 체력만 감소시킵니다
    void TakeDamage(float amount);
}
