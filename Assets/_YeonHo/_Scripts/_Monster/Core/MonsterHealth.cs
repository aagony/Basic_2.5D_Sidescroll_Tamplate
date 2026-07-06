using UnityEngine;

public class MonsterHealth : MonoBehaviour, IDamageable, IDeadState
{
    [Header("Health")]
    [SerializeField] private float maxHp = 30f;
    [SerializeField] private bool resetHpOnEnable = true;
    [SerializeField] private bool invulnerable;

    private float currentHp;
    private bool isDead;

    public float MaxHp
    {
        get { return maxHp; }
    }

    public float CurrentHp
    {
        get { return currentHp; }
    }

    public bool IsDead
    {
        get { return isDead; }
    }

    public bool CanTakeDamage
    {
        get { return !isDead && !invulnerable; }
    }

    // 처음 생성될 때 체력을 초기화합니다
    private void Awake()
    {
        ResetHealth();
    }

    // 오브젝트가 다시 활성화될 때 필요하면 체력을 초기화합니다
    private void OnEnable()
    {
        if (resetHpOnEnable)
        {
            ResetHealth();
        }
    }

    // 인스펙터 값이 잘못 들어갔을 때 최소 체력을 보장합니다
    private void OnValidate()
    {
        maxHp = Mathf.Max(1f, maxHp);
    }

    // 대상의 체력만 감소시킵니다
    public void TakeDamage(float amount)
    {
        if (!CanTakeDamage)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        currentHp = Mathf.Max(0f, currentHp - amount);
        PublishHealthChangedEvent();

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    // 몬스터 체력을 최대 체력으로 되돌립니다
    public void ResetHealth()
    {
        maxHp = Mathf.Max(1f, maxHp);
        currentHp = maxHp;
        isDead = false;
        PublishHealthChangedEvent();
    }

    // 몬스터를 즉시 사망 처리합니다
    public void Kill()
    {
        if (isDead)
        {
            return;
        }

        currentHp = 0f;
        PublishHealthChangedEvent();
        Die();
    }

    // 무적 상태를 설정합니다
    public void SetInvulnerable(bool value)
    {
        invulnerable = value;
    }

    // 체력이 0이 되었을 때 사망 이벤트를 발행합니다
    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        EventBus<MonsterDeadEvent>.Publish(new MonsterDeadEvent(gameObject));
    }

    // 체력 변경 사실을 이벤트 버스로 알립니다
    private void PublishHealthChangedEvent()
    {
        EventBus<HealthChangedEvent>.Publish(new HealthChangedEvent(gameObject, currentHp, maxHp));
    }
}
