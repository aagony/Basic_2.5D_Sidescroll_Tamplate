using UnityEngine;

// 플레이어가 발사하는 전용 탄환입니다.
// 충돌한 몬스터의 Status를 직접 찾아 데미지 처리를 요청합니다.
[RequireComponent(typeof(SphereCollider))]
public class BulletProjectile : MonoBehaviour
{
    // 인스펙터에서 별도 hitMask를 지정하지 않았을 때 플레이어 탄환이 기본으로 맞출 레이어입니다.
    private const string DefaultHitLayerName = "Monster";

    [Header("Movement")]
    // Init에서 받은 방향으로 초당 이동하는 거리입니다.
    [SerializeField] private float speed = 25f;

    // 충돌하지 않고 남은 탄환이 씬에 계속 쌓이지 않도록 자동 제거되는 시간입니다.
    [SerializeField] private float lifeTime = 3f;

    // 플레이어 탄환이 맞출 수 있는 레이어입니다. 기본값은 Monster 레이어로 보정됩니다.
    [SerializeField] private LayerMask hitMask = ~0;

    private Vector3 moveDirection;
    private float damage;

    // Init 호출 전에는 탄환이 움직이거나 충돌 판정을 하지 않도록 막습니다.
    private bool isInitialized;

    // SphereCast와 Trigger가 같은 프레임에 중복 처리되는 것을 막습니다.
    private bool isHit;

    // 발사자인 플레이어와 그 자식 콜라이더를 맞추지 않기 위해 저장합니다.
    private GameObject owner;
    private Collider projectileCollider;
    private float projectileRadius = 0.1f;

    private void Awake()
    {
        projectileCollider = GetComponent<Collider>();
        projectileRadius = GetProjectileRadius(projectileCollider);
        EnsureDefaultHitMask();
    }

    private void OnValidate()
    {
        EnsureDefaultHitMask();
    }

    public void Init(Vector3 direction, float attackDamage, GameObject attackOwner = null)
    {
        // 발사 시점에 공격 방향, 데미지, 발사자를 주입받아 탄환을 활성화합니다.
        moveDirection = direction.normalized;
        damage = attackDamage;
        owner = attackOwner;
        isInitialized = true;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!isInitialized || isHit) return;

        Vector3 movement = moveDirection * speed * Time.deltaTime;

        if (TryHitAlongMovement(movement)) return;

        transform.position += movement;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other, transform.position);
    }

    private bool TryHitAlongMovement(Vector3 movement)
    {
        // 빠른 탄환이 프레임 사이에 대상을 뚫고 지나가지 않도록 이동 경로를 구체로 검사합니다.
        float distance = movement.magnitude;
        if (distance <= Mathf.Epsilon) return false;

        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,
            projectileRadius,
            moveDirection,
            distance,
            hitMask,
            QueryTriggerInteraction.Collide
        );

        int closestHitIndex = -1;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;

            if (ShouldIgnoreCollider(hitCollider)) continue;
            if (!IsInHitMask(hitCollider.gameObject)) continue;
            if (hits[i].distance >= closestDistance) continue;

            closestHitIndex = i;
            closestDistance = hits[i].distance;
        }

        if (closestHitIndex < 0) return false;

        RaycastHit hit = hits[closestHitIndex];
        Vector3 hitPoint = hit.collider.ClosestPoint(transform.position + moveDirection * hit.distance);
        transform.position = hitPoint;
        HandleHit(hit.collider, hitPoint);
        return true;
    }

    private void HandleHit(Collider other, Vector3 hitPoint)
    {
        if (isHit) return;
        if (ShouldIgnoreCollider(other)) return;
        if (!IsInHitMask(other.gameObject)) return;

        isHit = true;

        Debug.Log($"플레이어 탄환 충돌: {other.gameObject.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");

        // 플레이어 탄환은 몬스터 Status를 직접 호출해 공용 전투 이벤트 의존을 만들지 않습니다.
        MonsterStatus monsterStatus = other.GetComponentInParent<MonsterStatus>();
        if (monsterStatus != null)
        {
            monsterStatus.TakeDamage(
                new DamageInfo(
                    monsterStatus.gameObject,
                    other,
                    owner,
                    hitPoint,
                    moveDirection,
                    damage
                )
            );
        }
        else
        {
            Debug.LogWarning($"몬스터 Status를 찾지 못했습니다: {other.gameObject.name}");
        }

        // 사운드, 이펙트처럼 플레이어 탄환 충돌에 반응하는 연출용 이벤트입니다.
        EventBus<PlayerBulletHitEvent>.Publish(
            new PlayerBulletHitEvent(
                other.gameObject,
                other,
                hitPoint,
                moveDirection,
                damage
            )
        );

        Destroy(gameObject);
    }

    private bool ShouldIgnoreCollider(Collider other)
    {
        if (other == null) return true;
        if (other == projectileCollider) return true;
        if (other.transform == transform || other.transform.IsChildOf(transform)) return true;

        return IsOwnerCollider(other);
    }

    private bool IsOwnerCollider(Collider other)
    {
        if (owner == null) return false;

        return other.gameObject == owner || other.transform.IsChildOf(owner.transform);
    }

    private bool IsInHitMask(GameObject target)
    {
        return (hitMask.value & (1 << target.layer)) != 0;
    }

    private void EnsureDefaultHitMask()
    {
        // 사용자가 직접 레이어를 지정한 경우에는 그 값을 유지합니다.
        if (hitMask.value != 0 && hitMask.value != ~0) return;

        int monsterMask = LayerMask.GetMask(DefaultHitLayerName);
        if (monsterMask == 0) return;

        hitMask = monsterMask;
    }

    private float GetProjectileRadius(Collider collider)
    {
        // SphereCast에 사용할 반지름을 현재 콜라이더와 스케일 기준으로 계산합니다.
        if (collider == null) return projectileRadius;

        float maxScale = GetMaxAbsScale(transform.lossyScale);

        if (collider is SphereCollider sphereCollider)
            return Mathf.Max(0.01f, sphereCollider.radius * maxScale);

        if (collider is CapsuleCollider capsuleCollider)
            return Mathf.Max(0.01f, capsuleCollider.radius * maxScale);

        return Mathf.Max(0.01f, collider.bounds.extents.magnitude);
    }

    private float GetMaxAbsScale(Vector3 scale)
    {
        return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
    }
}
