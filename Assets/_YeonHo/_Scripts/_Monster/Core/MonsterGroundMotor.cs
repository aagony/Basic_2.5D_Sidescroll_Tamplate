using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MonsterGroundMotor : MonoBehaviour
{
    [Header("Ground Motor")]
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedStickForce = -2f;

    private CharacterController characterController;
    private float fixedZ;
    private float verticalVelocity;
    private float horizontalVelocity;
    private bool initialized;

    public CharacterController Controller
    {
        get { return characterController; }
    }

    // CharacterController 참조를 준비합니다
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    // 이동 기준 Z값을 초기화합니다
    public void Initialize(float newFixedZ)
    {
        fixedZ = newFixedZ;
        initialized = true;
        FixDepthPosition();
    }

    // 이번 프레임의 수평 이동 속도를 초기화합니다
    public void ResetHorizontalVelocity()
    {
        horizontalVelocity = 0f;
    }

    // 수평 이동 속도를 설정합니다
    public void SetHorizontalVelocity(float velocity)
    {
        horizontalVelocity = velocity;
    }

    // 수평 이동을 멈춥니다
    public void StopHorizontalMovement()
    {
        horizontalVelocity = 0f;
    }

    // CharacterController 사용 여부를 변경합니다
    public void SetCharacterControllerEnabled(bool enabled)
    {
        if (characterController == null)
        {
            return;
        }

        characterController.enabled = enabled;
    }

    // 중력과 수평 이동을 계산해 CharacterController에 적용합니다
    public void TickMovement()
    {
        EnsureInitialized();

        if (characterController == null || !characterController.enabled)
        {
            FixDepthPosition();
            return;
        }

        ApplyGravity();
        ApplyMovement();
        FixDepthPosition();
    }

    // 현재 위치의 Z값을 고정합니다
    public void FixDepthPosition()
    {
        transform.position = FixDepthVector(transform.position);
    }

    // Vector3의 Z값을 고정합니다
    public Vector3 FixDepthVector(Vector3 position)
    {
        EnsureInitialized();
        position.z = fixedZ;
        return position;
    }

    // 초기화가 누락된 경우 현재 Z값을 기준으로 초기화합니다
    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        Initialize(transform.position.z);
    }

    // 중력 값을 계산합니다
    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickForce;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    // 계산된 이동 값을 CharacterController에 적용합니다
    private void ApplyMovement()
    {
        Vector3 movement = new Vector3(horizontalVelocity, verticalVelocity, 0f);
        characterController.Move(movement * Time.deltaTime);
    }
}
