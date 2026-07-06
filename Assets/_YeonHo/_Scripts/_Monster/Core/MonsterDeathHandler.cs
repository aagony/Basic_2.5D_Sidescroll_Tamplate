using UnityEngine;

public class MonsterDeathHandler : MonoBehaviour
{
    [Header("Death")]
    [SerializeField] private Animator animator;
    [SerializeField] private string deadTriggerName = "Dead";
    [SerializeField] private bool destroyAfterDeath = true;
    [SerializeField] private float destroyDelay = 1.5f;
    [SerializeField] private bool disableCharacterControllerOnDeath = true;

    private bool handled;

    // 몬스터 사망 이벤트를 구독합니다
    private void OnEnable()
    {
        EventBus<MonsterDeadEvent>.action += HandleMonsterDead;
    }

    // 몬스터 사망 이벤트 구독을 해제합니다
    private void OnDisable()
    {
        EventBus<MonsterDeadEvent>.action -= HandleMonsterDead;
    }

    // 시작 시 Animator 참조를 보정합니다
    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    // 자신에게 해당하는 사망 이벤트만 처리합니다
    private void HandleMonsterDead(MonsterDeadEvent deadEvent)
    {
        if (handled)
        {
            return;
        }

        if (deadEvent.MonsterObject == null)
        {
            return;
        }

        if (!IsSameObjectOrChild(deadEvent.MonsterObject))
        {
            return;
        }

        handled = true;
        DisableControllerIfNeeded();
        PlayDeathAnimation();

        if (destroyAfterDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    // 전달된 오브젝트가 자신 또는 자식인지 확인합니다
    private bool IsSameObjectOrChild(GameObject monsterObject)
    {
        if (monsterObject == gameObject)
        {
            return true;
        }

        return monsterObject.transform.IsChildOf(transform) || transform.IsChildOf(monsterObject.transform);
    }

    // 필요하면 CharacterController를 비활성화합니다
    private void DisableControllerIfNeeded()
    {
        if (!disableCharacterControllerOnDeath)
        {
            return;
        }

        CharacterController controller = GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }
    }

    // 사망 애니메이션 트리거를 실행합니다
    private void PlayDeathAnimation()
    {
        if (animator == null || string.IsNullOrEmpty(deadTriggerName))
        {
            return;
        }

        animator.SetTrigger(deadTriggerName);
    }
}
