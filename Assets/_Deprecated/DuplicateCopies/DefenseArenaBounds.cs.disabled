using UnityEngine;

public class DefenseArenaBounds : MonoBehaviour
{
    [Header("Bounds")]
    [SerializeField] private Vector2 size = new Vector2(18f, 8f);
    [SerializeField] private float depth = 4f;

    [Header("Gizmo")]
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.35f);

    public Bounds WorldBounds
    {
        get
        {
            Vector3 boundsSize = new Vector3(size.x, size.y, depth);
            return new Bounds(transform.position, boundsSize);
        }
    }

    public float LeftX
    {
        get { return WorldBounds.min.x; }
    }

    public float RightX
    {
        get { return WorldBounds.max.x; }
    }

    public float BottomY
    {
        get { return WorldBounds.min.y; }
    }

    public float TopY
    {
        get { return WorldBounds.max.y; }
    }

    // 인스펙터 값이 잘못 들어갔을 때 최소 크기를 보장합니다
    private void OnValidate()
    {
        size.x = Mathf.Max(0.1f, size.x);
        size.y = Mathf.Max(0.1f, size.y);
        depth = Mathf.Max(0.1f, depth);
    }

    // 전달된 위치가 아레나 안에 있는지 확인합니다
    public bool ContainsPosition(Vector3 position)
    {
        return WorldBounds.Contains(position);
    }

    // 전달된 위치를 아레나 범위 안으로 제한합니다
    public Vector3 ClampPosition(Vector3 position)
    {
        Bounds bounds = WorldBounds;

        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);

        return position;
    }

    // Scene 뷰에서 아레나 범위를 표시합니다
    private void OnDrawGizmosSelected()
    {
        Bounds bounds = WorldBounds;

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(bounds.center, bounds.size);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}