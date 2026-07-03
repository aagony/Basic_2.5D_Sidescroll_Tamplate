using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DefenseStageZone : MonoBehaviour
{
    [SerializeField] private DefenseStageController stageController;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool ensureKinematicRigidbody = true;

    private bool hasTriggered;
    private Collider triggerCollider;

    // 방어 구간 진입 감지에 필요한 Collider와 Rigidbody를 준비합니다
    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        if (stageController == null)
        {
            stageController = GetComponentInParent<DefenseStageController>();
        }

        EnsureTriggerRigidbody();
    }

    // 컴포넌트를 처음 붙였을 때 기본 값을 보정합니다
    private void Reset()
    {
        stageController = GetComponentInParent<DefenseStageController>();

        Collider foundCollider = GetComponent<Collider>();

        if (foundCollider != null)
        {
            foundCollider.isTrigger = true;
        }
    }

    // 플레이어가 방어 구간에 들어오면 스테이지를 시작합니다
    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }

        PlayerStatus playerStatus = other.GetComponentInParent<PlayerStatus>();

        if (playerStatus == null)
        {
            return;
        }

        if (stageController == null)
        {
            Debug.LogWarning("DefenseStageController is missing", this);
            return;
        }

        hasTriggered = true;
        stageController.StartStage();
    }

    // Trigger 이벤트가 안정적으로 발생하도록 Kinematic Rigidbody를 보장합니다
    private void EnsureTriggerRigidbody()
    {
        if (!ensureKinematicRigidbody)
        {
            return;
        }

        Rigidbody rigidbody = GetComponent<Rigidbody>();

        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
    }
}