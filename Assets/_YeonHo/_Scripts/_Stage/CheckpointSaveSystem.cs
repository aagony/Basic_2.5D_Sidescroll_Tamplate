using UnityEngine;

public class CheckpointSaveSystem : MonoBehaviour
{
    private const string HasSaveKey = "Checkpoint.HasSave";
    private const string NumberKey = "Checkpoint.Number";
    private const string PositionXKey = "Checkpoint.PositionX";
    private const string PositionYKey = "Checkpoint.PositionY";
    private const string PositionZKey = "Checkpoint.PositionZ";
    private const string HpKey = "Checkpoint.HP";
    private const string HealItemCountKey = "Checkpoint.HealItemCount";

    [Header("Optional Default Reference")]
    [SerializeField] private PlayerCheckpointTracker checkpointTracker;

    public bool HasCheckpointSave
    {
        get { return PlayerPrefs.GetInt(HasSaveKey, 0) == 1; }
    }

    // 전달받은 플레이어 추적기의 현재 스냅샷을 저장합니다
    public void SaveCheckpoint(PlayerCheckpointTracker targetTracker)
    {
        if (targetTracker == null || !targetTracker.HasActiveCheckpoint)
        {
            return;
        }

        checkpointTracker = targetTracker;
        SaveTrackerSnapshot(targetTracker);
    }

    // 인스펙터에 연결된 기본 추적기의 현재 스냅샷을 저장합니다
    public void SaveCheckpoint()
    {
        if (checkpointTracker == null || !checkpointTracker.HasActiveCheckpoint)
        {
            return;
        }

        SaveTrackerSnapshot(checkpointTracker);
    }

    // 체크포인트 추적기 데이터를 PlayerPrefs에 기록합니다
    private void SaveTrackerSnapshot(PlayerCheckpointTracker targetTracker)
    {
        Vector3 respawnPosition = targetTracker.RespawnPosition;

        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetInt(NumberKey, targetTracker.ActiveCheckpointNumber);
        PlayerPrefs.SetFloat(PositionXKey, respawnPosition.x);
        PlayerPrefs.SetFloat(PositionYKey, respawnPosition.y);
        PlayerPrefs.SetFloat(PositionZKey, respawnPosition.z);
        PlayerPrefs.SetFloat(HpKey, targetTracker.SavedHP);
        PlayerPrefs.SetInt(HealItemCountKey, targetTracker.SavedHealItemCount);
        PlayerPrefs.Save();
    }

    // 저장된 체크포인트 번호를 반환합니다
    public int LoadCheckpointNumber()
    {
        return PlayerPrefs.GetInt(NumberKey, -1);
    }

    // 저장된 체크포인트 리스폰 위치를 반환합니다
    public Vector3 LoadRespawnPosition()
    {
        return new Vector3(
            PlayerPrefs.GetFloat(PositionXKey, 0f),
            PlayerPrefs.GetFloat(PositionYKey, 0f),
            PlayerPrefs.GetFloat(PositionZKey, 0f)
        );
    }

    // 저장된 플레이어 체력을 반환합니다
    public float LoadSavedHP()
    {
        return PlayerPrefs.GetFloat(HpKey, 0f);
    }

    // 저장된 회복 아이템 개수를 반환합니다
    public int LoadSavedHealItemCount()
    {
        return PlayerPrefs.GetInt(HealItemCountKey, 0);
    }

    // 저장된 체크포인트 데이터를 삭제합니다
    public void DeleteCheckpointSave()
    {
        PlayerPrefs.DeleteKey(HasSaveKey);
        PlayerPrefs.DeleteKey(NumberKey);
        PlayerPrefs.DeleteKey(PositionXKey);
        PlayerPrefs.DeleteKey(PositionYKey);
        PlayerPrefs.DeleteKey(PositionZKey);
        PlayerPrefs.DeleteKey(HpKey);
        PlayerPrefs.DeleteKey(HealItemCountKey);
        PlayerPrefs.Save();
    }
}
