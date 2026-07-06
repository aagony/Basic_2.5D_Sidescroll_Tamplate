using UnityEngine;

public class MonsterStats : MonoBehaviour, ICombatStatsProvider
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float attackPower = 10f;

    public float MoveSpeed
    {
        get { return moveSpeed; }
    }

    public float AttackPower
    {
        get { return attackPower; }
    }

    // 인스펙터 값이 잘못 들어갔을 때 최소 값을 보장합니다
    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        attackPower = Mathf.Max(0f, attackPower);
    }

    // 이동 속도가 유효하지 않을 때 fallback 값을 반환합니다
    public float GetMoveSpeedOrFallback(float fallbackMoveSpeed)
    {
        if (moveSpeed <= 0f)
        {
            return Mathf.Max(0f, fallbackMoveSpeed);
        }

        return moveSpeed;
    }

    // 공격력이 유효하지 않을 때 fallback 값을 반환합니다
    public float GetAttackPowerOrFallback(float fallbackAttackPower)
    {
        if (attackPower <= 0f)
        {
            return Mathf.Max(0f, fallbackAttackPower);
        }

        return attackPower;
    }
}
