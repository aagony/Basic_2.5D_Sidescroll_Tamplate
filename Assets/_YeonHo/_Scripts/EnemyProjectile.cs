using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Hit")]
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask playerHitMask;
    [SerializeField] private LayerMask destroyHitMask;

    [Header("Move")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float fixedZ = 0f;

    private Rigidbody projectileRigidbody;
    private bool hasInitialized;
    private bool hasHit;

    // 탄환에 필요한 물리 설정을 준비합니다
    private void Awake()
    {
        projectileRigidbody = GetComponent<Rigidbody>();

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;

        projectileRigidbody.useGravity = false;
        projectileRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        projectileRigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        FixDepthPosition();
    }

    // 일정 시간이 지나면 탄환을 제거합니다
    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // 탄환의 발사 방향과 속도를 설정합니다
    public void Initialize(Vector3 direction, float speed, int projectileDamage, GameObject owner)
    {
        damage = projectileDamage;

        Vector3 normalizedDirection = direction.normalized;

        if (normalizedDirection == Vector3.zero)
        {
            normalizedDirection = Vector3.right;
        }

        projectileRigidbody.linearVelocity = normalizedDirection * speed;
        hasInitialized = true;
    }

    // 탄환이 Trigger Collider에 닿았을 때 피격 또는 파괴 처리를 합니다
    private void OnTriggerEnter(Collider other)
    {
        if (!hasInitialized)
        {
            return;
        }

        if (hasHit)
        {
            return;
        }

        int otherLayer = other.gameObject.layer;

        if (IsInLayerMask(otherLayer, playerHitMask))
        {
            HitPlayer(other);
            return;
        }

        if (IsInLayerMask(otherLayer, destroyHitMask))
        {
            DestroyProjectile();
            return;
        }
    }

    // 플레이어에게 데미지를 주고 탄환을 제거합니다
    private void HitPlayer(Collider other)
    {
        Damageable damageable = other.GetComponentInParent<Damageable>();

        if (damageable == null)
        {
            DestroyProjectile();
            return;
        }

        if (!damageable.IsAlive)
        {
            DestroyProjectile();
            return;
        }

        hasHit = true;
        damageable.TakeDamage(damage, gameObject);
        DestroyProjectile();
    }

    // 탄환 오브젝트를 제거합니다
    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    // 대상 레이어가 LayerMask에 포함되어 있는지 확인합니다
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    // 2.5D 횡스크롤 이동을 위해 Z 위치를 고정합니다
    private void FixDepthPosition()
    {
        Vector3 position = transform.position;
        position.z = fixedZ;
        transform.position = position;
    }
}