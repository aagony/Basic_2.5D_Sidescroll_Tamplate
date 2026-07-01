using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerPrototypeController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float fixedZ = 0f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.8f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedStickForce = -2f;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;

    private CharacterController characterController;
    private Vector2 moveInput;
    private float verticalVelocity;
    private bool jumpQueued;
    private bool facingRight = true;

    public bool IsFacingRight
    {
        get { return facingRight; }
    }

    // 컴포넌트 참조를 준비합니다
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        FixDepthPosition();
    }

    // 매 프레임 플레이어 이동과 방향 전환을 처리합니다
    private void Update()
    {
        ApplyGravityAndJump();
        ApplyMovement();
        UpdateFacingDirection();
        FixDepthPosition();
    }

    // Input System의 Move 입력을 저장합니다
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Input System의 Jump 입력을 저장합니다
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpQueued = true;
        }
    }

    // Input System의 Attack 입력으로 플레이어 근접 공격을 실행합니다
    public void OnAttack(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        PlayerMeleeAttack meleeAttack = GetComponent<PlayerMeleeAttack>();

        if (meleeAttack == null)
        {
            Debug.LogWarning("PlayerMeleeAttack component is missing");
            return;
        }

        meleeAttack.TryAttack();
    }

    // 중력과 점프 속도를 계산합니다
    private void ApplyGravityAndJump()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickForce;
        }

        if (jumpQueued && characterController.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        jumpQueued = false;
        verticalVelocity += gravity * Time.deltaTime;
    }

    // 좌우 이동과 수직 이동을 CharacterController에 적용합니다
    private void ApplyMovement()
    {
        Vector3 movement = new Vector3(moveInput.x * moveSpeed, verticalVelocity, 0f);
        characterController.Move(movement * Time.deltaTime);
    }

    // 입력 방향에 따라 플레이어의 좌우 방향을 바꿉니다
    private void UpdateFacingDirection()
    {
        if (moveInput.x > 0.01f && !facingRight)
        {
            SetFacingDirection(true);
        }
        else if (moveInput.x < -0.01f && facingRight)
        {
            SetFacingDirection(false);
        }
    }

    // 플레이어의 시각 방향을 설정합니다
    private void SetFacingDirection(bool lookRight)
    {
        facingRight = lookRight;

        Vector3 scale = visualRoot.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        visualRoot.localScale = scale;
    }

    // 2.5D 횡스크롤 이동을 위해 Z 위치를 고정합니다
    private void FixDepthPosition()
    {
        Vector3 position = transform.position;
        position.z = fixedZ;
        transform.position = position;
    }
}