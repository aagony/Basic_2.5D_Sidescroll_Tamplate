using UnityEngine;

[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerCheckpointTracker))]
public class CheckpointHealSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private PlayerCheckpointTracker checkpointTracker;

    [Header("Recovery")]
    [SerializeField] private bool healToFull = true;
    [SerializeField] private float fixedHealAmount = 50f;

    // 체크포인트 회복 시스템에 필요한 참조를 준비합니다
    private void Awake()
    {
        if (playerStatus == null)
        {
            playerStatus = GetComponent<PlayerStatus>();
        }

        if (checkpointTracker == null)
        {
            checkpointTracker = GetComponent<PlayerCheckpointTracker>();
        }
    }

    // 플레이어를 회복한 뒤 회복된 상태로 체크포인트 스냅샷을 갱신합니다
    public void RecoverAndRefreshSnapshot()
    {
        RecoverPlayer();

        if (checkpointTracker != null)
        {
            checkpointTracker.RefreshCurrentSnapshot();
        }
    }

    // 설정에 따라 플레이어 체력을 완전 또는 고정량 회복합니다
    private void RecoverPlayer()
    {
        if (playerStatus == null || playerStatus.Status == null)
        {
            return;
        }

        float healAmount = fixedHealAmount;

        if (healToFull)
        {
            float maxHp = playerStatus.Status.MaxHP.FinalValue;
            float currentHp = playerStatus.Status.CurrentHP;
            healAmount = maxHp - currentHp;
        }

        if (healAmount <= 0f)
        {
            return;
        }

        playerStatus.Heal(healAmount);
    }
}
