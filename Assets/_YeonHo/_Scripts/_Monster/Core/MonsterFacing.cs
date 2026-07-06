using UnityEngine;

public class MonsterFacing : MonoBehaviour
{
    [SerializeField] private bool invertVisualFacing;

    private bool facingRight = true;

    public bool FacingRight
    {
        get { return facingRight; }
    }

    // 대상 위치를 기준으로 바라보는 방향을 갱신합니다
    public void FaceTarget(Transform target)
    {
        if (target == null)
        {
            return;
        }

        if (target.position.x > transform.position.x)
        {
            SetFacingDirection(true);
        }
        else if (target.position.x < transform.position.x)
        {
            SetFacingDirection(false);
        }
    }

    // 몬스터의 바라보는 방향을 설정합니다
    public void SetFacingDirection(bool lookRight)
    {
        facingRight = lookRight;

        float visualSign = facingRight ? 1f : -1f;

        if (invertVisualFacing)
        {
            visualSign *= -1f;
        }

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * visualSign;
        transform.localScale = scale;
    }

    // 현재 바라보는 방향을 월드 방향으로 반환합니다
    public Vector3 GetFacingDirectionVector()
    {
        return facingRight ? Vector3.right : Vector3.left;
    }

    // 현재 바라보는 방향에 맞춰 오프셋을 계산합니다
    public Vector3 GetFacingOffset(Vector3 offset)
    {
        float direction = facingRight ? 1f : -1f;
        return new Vector3(offset.x * direction, offset.y, offset.z);
    }
}
