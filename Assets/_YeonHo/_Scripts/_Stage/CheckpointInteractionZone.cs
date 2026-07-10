using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckpointInteractionZone :
    MonoBehaviour,
    IPlayerInteractable
{
    [Header("Checkpoint")]
    [SerializeField] private Checkpoint checkpoint;

    [Header("Prompt")]
    [SerializeField] private CanvasGroup promptCanvasGroup;
    [SerializeField] private TMP_Text promptText;
    [SerializeField]
    private string availableMessage =
        "E키로 세이브";
    [SerializeField]
    private string activatedMessage =
        "저장 완료";

    [Header("Behavior")]
    [SerializeField] private bool hideAfterActivation = true;

    private PlayerInteractionController currentPlayerInteraction;
    private PlayerCheckpointTracker currentCheckpointTracker;
    private GameObject currentPlayerObject;
    private bool isActivated;

    public string PromptText
    {
        get
        {
            return isActivated
                ? activatedMessage
                : availableMessage;
        }
    }

    // 체크포인트 상호작용 영역과 안내 UI를 초기화합니다
    private void Awake()
    {
        Collider interactionCollider =
            GetComponent<Collider>();

        interactionCollider.isTrigger = true;

        if (checkpoint == null)
        {
            checkpoint =
                GetComponentInParent<Checkpoint>();
        }

        SetPromptVisible(false);
        UpdatePromptText();
    }

    // 플레이어가 접근하면 상호작용 대상에 등록하고 안내문을 표시합니다
    private void OnTriggerEnter(Collider other)
    {
        PlayerInteractionController interactionController =
            other.GetComponentInParent<PlayerInteractionController>();

        if (interactionController == null)
        {
            return;
        }

        PlayerCheckpointTracker checkpointTracker =
            other.GetComponentInParent<PlayerCheckpointTracker>();

        if (checkpointTracker == null)
        {
            return;
        }

        currentPlayerInteraction = interactionController;
        currentCheckpointTracker = checkpointTracker;
        currentPlayerObject =
            interactionController.gameObject;

        currentPlayerInteraction.RegisterInteractable(this);

        UpdatePromptText();
        SetPromptVisible(CanShowPrompt());
    }

    // 플레이어가 영역을 벗어나면 상호작용 등록과 안내문을 해제합니다
    private void OnTriggerExit(Collider other)
    {
        PlayerInteractionController interactionController =
            other.GetComponentInParent<PlayerInteractionController>();

        if (interactionController == null)
        {
            return;
        }

        if (interactionController != currentPlayerInteraction)
        {
            return;
        }

        currentPlayerInteraction.UnregisterInteractable(this);

        currentPlayerInteraction = null;
        currentCheckpointTracker = null;
        currentPlayerObject = null;

        SetPromptVisible(false);
    }

    // 컴포넌트 비활성화 시 등록된 상호작용을 안전하게 해제합니다
    private void OnDisable()
    {
        if (currentPlayerInteraction != null)
        {
            currentPlayerInteraction.UnregisterInteractable(this);
        }

        currentPlayerInteraction = null;
        currentCheckpointTracker = null;
        currentPlayerObject = null;

        SetPromptVisible(false);
    }

    // 현재 플레이어가 이 체크포인트를 사용할 수 있는지 확인합니다
    public bool CanInteract(GameObject playerObject)
    {
        if (checkpoint == null)
        {
            return false;
        }

        if (playerObject == null)
        {
            return false;
        }

        if (playerObject != currentPlayerObject)
        {
            return false;
        }

        if (currentCheckpointTracker == null)
        {
            return false;
        }

        if (isActivated)
        {
            return false;
        }

        return true;
    }

    // 기존 PlayerCheckpointTracker를 통해 체크포인트를 활성화합니다
    public void Interact(GameObject playerObject)
    {
        if (!CanInteract(playerObject))
        {
            return;
        }

        bool activated =
            currentCheckpointTracker.TryActivateCheckpoint(
                checkpoint
            );

        if (!activated)
        {
            return;
        }

        isActivated = true;
        UpdatePromptText();

        if (hideAfterActivation)
        {
            SetPromptVisible(false);

            if (currentPlayerInteraction != null)
            {
                currentPlayerInteraction.UnregisterInteractable(
                    this
                );
            }

            return;
        }

        SetPromptVisible(true);
    }

    // 현재 상태에 맞는 안내 문구를 갱신합니다
    private void UpdatePromptText()
    {
        if (promptText == null)
        {
            return;
        }

        promptText.text = PromptText;
    }

    // 현재 체크포인트 안내문을 표시할 수 있는지 확인합니다
    private bool CanShowPrompt()
    {
        if (isActivated && hideAfterActivation)
        {
            return false;
        }

        return currentPlayerObject != null;
    }

    // World Space 안내 Canvas의 표시 상태를 설정합니다
    private void SetPromptVisible(bool visible)
    {
        if (promptCanvasGroup == null)
        {
            return;
        }

        promptCanvasGroup.alpha = visible ? 1f : 0f;
        promptCanvasGroup.interactable = false;
        promptCanvasGroup.blocksRaycasts = false;
    }
}