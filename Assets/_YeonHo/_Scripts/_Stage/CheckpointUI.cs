using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CheckpointUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text checkpointNumberText;

    [Header("Text")]
    [SerializeField] private string titleMessage = "CHECKPOINT";
    [SerializeField] private string checkpointFormat = "Checkpoint {0}";

    [Header("Timing")]
    [SerializeField] private float visibleDuration = 2f;
    [SerializeField] private float fadeDuration = 0.4f;

    private Coroutine displayRoutine;

    // 체크포인트 UI에 필요한 참조와 초기 상태를 준비합니다
    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        SetVisibleImmediately(false);
    }

    // 컴포넌트가 비활성화될 때 실행 중인 표시 루틴을 정리합니다
    private void OnDisable()
    {
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            displayRoutine = null;
        }

        SetVisibleImmediately(false);
    }

    // 전달받은 체크포인트 번호를 화면에 표시합니다
    public void ShowCheckpoint(int checkpointNumber)
    {
        UpdateText(checkpointNumber);

        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
        }

        displayRoutine = StartCoroutine(DisplayRoutine());
    }

    // 전달받은 체크포인트 번호를 UI 문구에 반영합니다
    private void UpdateText(int checkpointNumber)
    {
        if (titleText != null)
        {
            titleText.text = titleMessage;
        }

        if (checkpointNumberText != null)
        {
            checkpointNumberText.text = string.Format(
                checkpointFormat,
                checkpointNumber
            );
        }
    }

    // UI를 일정 시간 표시한 뒤 서서히 숨깁니다
    private IEnumerator DisplayRoutine()
    {
        SetVisibleImmediately(true);

        yield return new WaitForSecondsRealtime(visibleDuration);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float ratio = fadeDuration > 0f
                ? Mathf.Clamp01(elapsed / fadeDuration)
                : 1f;

            canvasGroup.alpha = 1f - ratio;
            yield return null;
        }

        SetVisibleImmediately(false);
        displayRoutine = null;
    }

    // 체크포인트 UI 표시 여부를 즉시 설정합니다
    private void SetVisibleImmediately(bool visible)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
