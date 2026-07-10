using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class BossPreparationUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Pause")]
    [SerializeField] private bool manageTimeScaleLocally = false;

    [Header("Input")]
    [SerializeField] private bool closeWithEscape = true;

    private float previousTimeScale = 1f;
    private bool ownsTimeScalePause;

    public bool IsOpen { get; private set; }

    // 보스 준비 UI의 참조와 초기 상태를 준비합니다
    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        SetPanelState(false);
    }

    // 보스 준비 화면이 열린 동안 닫기 입력을 처리합니다
    private void Update()
    {
        if (!IsOpen || !closeWithEscape)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    // 컴포넌트가 비활성화될 때 자신이 소유한 일시정지만 복구합니다
    private void OnDisable()
    {
        IsOpen = false;
        SetPanelState(false);
        ReleaseOwnedPause();
    }

    // 보스 준비 화면을 열고 설정된 경우에만 로컬 일시정지를 소유합니다
    public void Open()
    {
        if (IsOpen)
        {
            return;
        }

        IsOpen = true;
        SetPanelState(true);
        AcquirePauseIfNeeded();
    }

    // 보스 준비 화면을 닫고 자신이 소유한 일시정지만 해제합니다
    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        SetPanelState(false);
        ReleaseOwnedPause();
    }

    // 로컬 시간 제어가 활성화된 경우 현재 배속을 보존하고 정지합니다
    private void AcquirePauseIfNeeded()
    {
        if (!manageTimeScaleLocally || ownsTimeScalePause)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        ownsTimeScalePause = true;
    }

    // 자신이 설정한 일시정지 상태만 이전 배속으로 복구합니다
    private void ReleaseOwnedPause()
    {
        if (!ownsTimeScalePause)
        {
            return;
        }

        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            Time.timeScale = previousTimeScale;
        }

        ownsTimeScalePause = false;
    }

    // 보스 준비 패널의 표시와 입력 차단 상태를 설정합니다
    private void SetPanelState(bool visible)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
}
