using UnityEngine;

// 점프 입력으로 실제 점프 힘을 적용하는 짧은 상태입니다.
// 점프를 시작한 직후 공중 이동 처리는 FallState로 넘깁니다.
public class JumpState : PlayerBaseState
{
    public JumpState(PlayerController controller) : base(controller) { }

    public override void EnterState()
    {
        Debug.Log("Jump Enter");

        controller.Movement.Jump();
        controller.TransitionTo(controller.FallState);
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        Debug.Log("Jump Exit");
    }

    public override void FixedUpdateState()
    {
    }
}
