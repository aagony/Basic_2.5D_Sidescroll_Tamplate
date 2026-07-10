using UnityEngine;

public class CheckpointRuntimeCoordinator : MonoBehaviour
{
    [Header("Checkpoint Services")]
    [SerializeField] private CheckpointSaveSystem saveSystem;
    [SerializeField] private CheckpointUI checkpointUI;
    [SerializeField] private BossPreparationUI bossPreparationUI;

    [Header("Boss Checkpoints")]
    [SerializeField] private int[] bossCheckpointNumbers;

    [Header("Debug")]
    [SerializeField] private bool logProcessing;

    // 체크포인트 이벤트를 구독합니다
    private void OnEnable()
    {
        EventBus<CheckpointActivatedEvent>.action += HandleCheckpointActivated;
    }

    // 체크포인트 이벤트 구독을 해제합니다
    private void OnDisable()
    {
        EventBus<CheckpointActivatedEvent>.action -= HandleCheckpointActivated;
    }

    // 체크포인트 후처리를 정해진 순서대로 실행합니다
    private void HandleCheckpointActivated(CheckpointActivatedEvent checkpointEvent)
    {
        if (!TryResolvePlayerServices(
                checkpointEvent.PlayerObject,
                out PlayerCheckpointTracker checkpointTracker,
                out CheckpointHealSystem healSystem))
        {
            return;
        }

        if (logProcessing)
        {
            Debug.Log(
                $"Checkpoint processing started: {checkpointEvent.CheckpointNumber}",
                checkpointEvent.CheckpointObject
            );
        }

        healSystem.RecoverAndRefreshSnapshot();
        SaveCheckpoint(checkpointTracker);
        ShowCheckpointUI(checkpointEvent.CheckpointNumber);
        PlayCheckpointEffect(checkpointEvent.CheckpointObject);
        OpenBossPreparationIfNeeded(checkpointEvent.CheckpointNumber);
    }

    // 이벤트의 플레이어 오브젝트에서 체크포인트 관련 컴포넌트를 찾습니다
    private bool TryResolvePlayerServices(
        GameObject playerObject,
        out PlayerCheckpointTracker checkpointTracker,
        out CheckpointHealSystem healSystem)
    {
        checkpointTracker = null;
        healSystem = null;

        if (playerObject == null)
        {
            Debug.LogWarning("Checkpoint event has no player object", this);
            return false;
        }

        checkpointTracker = playerObject.GetComponentInParent<PlayerCheckpointTracker>();
        healSystem = playerObject.GetComponentInParent<CheckpointHealSystem>();

        if (checkpointTracker == null)
        {
            Debug.LogWarning("PlayerCheckpointTracker is missing on the checkpoint player", playerObject);
            return false;
        }

        if (healSystem == null)
        {
            Debug.LogWarning("CheckpointHealSystem is missing on the checkpoint player", playerObject);
            return false;
        }

        return true;
    }

    // 회복된 체크포인트 스냅샷을 영구 저장합니다
    private void SaveCheckpoint(PlayerCheckpointTracker checkpointTracker)
    {
        if (saveSystem == null)
        {
            return;
        }

        saveSystem.SaveCheckpoint(checkpointTracker);
    }

    // 체크포인트 번호를 알림 UI에 표시합니다
    private void ShowCheckpointUI(int checkpointNumber)
    {
        if (checkpointUI == null)
        {
            return;
        }

        checkpointUI.ShowCheckpoint(checkpointNumber);
    }

    // 실제로 활성화된 체크포인트 오브젝트의 연출만 재생합니다
    private void PlayCheckpointEffect(GameObject checkpointObject)
    {
        if (checkpointObject == null)
        {
            return;
        }

        CheckpointEffect checkpointEffect = checkpointObject.GetComponent<CheckpointEffect>();

        if (checkpointEffect == null)
        {
            checkpointEffect = checkpointObject.GetComponentInChildren<CheckpointEffect>(true);
        }

        if (checkpointEffect != null)
        {
            checkpointEffect.PlayOnce();
        }
    }

    // 보스 직전 체크포인트에서만 준비 화면을 엽니다
    private void OpenBossPreparationIfNeeded(int checkpointNumber)
    {
        if (bossPreparationUI == null)
        {
            return;
        }

        if (!IsBossCheckpoint(checkpointNumber))
        {
            return;
        }

        bossPreparationUI.Open();
    }

    // 전달된 번호가 보스 준비 체크포인트인지 확인합니다
    private bool IsBossCheckpoint(int checkpointNumber)
    {
        if (bossCheckpointNumbers == null)
        {
            return false;
        }

        for (int i = 0; i < bossCheckpointNumbers.Length; i++)
        {
            if (bossCheckpointNumbers[i] == checkpointNumber)
            {
                return true;
            }
        }

        return false;
    }
}
