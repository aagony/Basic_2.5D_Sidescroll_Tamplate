using UnityEngine;

public class DefenseEnemySpawnPoint : MonoBehaviour
{
    [SerializeField] private bool drawGizmo = true;
    [SerializeField] private float gizmoRadius = 0.35f;

    // 지정된 몬스터 프리팹을 이 위치에 생성합니다
    public GameObject Spawn(GameObject enemyPrefab, Transform parent)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab is missing", this);
            return null;
        }

        GameObject enemy = Instantiate(enemyPrefab, transform.position, transform.rotation);

        if (parent != null)
        {
            enemy.transform.SetParent(parent);
        }

        return enemy;
    }

    // Scene 뷰에서 스폰 위치를 표시합니다
    private void OnDrawGizmos()
    {
        if (!drawGizmo)
        {
            return;
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
    }
}