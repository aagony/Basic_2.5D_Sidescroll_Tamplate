using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private bool destroyOnDeath = false;

    private int currentHealth;
    private bool isAlive = true;

    public bool IsAlive
    {
        get { return isAlive; }
    }

    public int CurrentHealth
    {
        get { return currentHealth; }
    }

    // 체력 상태를 초기화합니다
    private void Awake()
    {
        currentHealth = maxHealth;
        isAlive = true;
    }

    // 외부 공격으로부터 데미지를 받습니다
    public void TakeDamage(int damage, GameObject attacker)
    {
        if (!isAlive)
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        Debug.Log(gameObject.name + " took " + damage + " damage from " + attacker.name + ". HP " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 체력이 0 이하가 되었을 때 사망 처리를 합니다
    private void Die()
    {
        isAlive = false;

        Debug.Log(gameObject.name + " died");

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}