using UnityEngine;

// 테스트용 몬스터 투사체입니다.
// 플레이어 Status에 직접 데미지를 전달하고, PlayerParry가 패리할 수 있도록 IParryableProjectile을 구현합니다.
[RequireComponent(typeof(SphereCollider))]
public class TestMonsterProjectile : MonoBehaviour, IParryableProjectile
{
    // 인스펙터에서 별도 hitMask를 지정하지 않았을 때 기본으로 맞출 레이어입니다.
    private const string DefaultHitLayerName = "Player";

    [Header("Movement")]
    // Init에서 받은 방향으로 초당 이동하는 거리입니다.
    [SerializeField] private float speed = 25f;

    // 충돌하지 않고 남은 테스트 투사체가 씬에 계속 쌓이지 않도록 자동 제거되는 시간입니다.
    [SerializeField] private float lifeTime = 3f;

    // 테스트 몬스터 투사체가 맞출 수 있는 레이어입니다. 기본값은 Player 레이어로 보정됩니다.
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Color")]
    // 플레이어가 패리할 수 있는 범위에 들어왔을 때 표시할 색상입니다.
    [SerializeField] private Color parryReadyColor = Color.yellow;

    private Vector3 moveDirection;
    private float damage;

    // Init 호출 전에는 투사체가 움직이거나 충돌 판정을 하지 않도록 막습니다.
    private bool isInitialized;

    // 충돌, 패리, Trigger가 중복 처리되는 것을 막습니다.
    private bool isHit;

    // 발사자인 몬스터와 그 자식 콜라이더를 맞추지 않기 위해 저장합니다.
    private GameObject owner;
    private Collider projectileCollider;
    private float projectileRadius = 0.1f;
    private Renderer[] targetRenderers;
    private Material[][] targetMaterials;
    private Color[][] originalColors;
    private string[][] colorProperties;
    private bool isParryReadyVisualActive;

    public Vector3 Position => transform.position;

    private void Awake()
    {
        projectileCollider = GetComponent<Collider>();
        projectileRadius = GetProjectileRadius(projectileCollider);

        EnsureDefaultHitMask();
        CacheVisualMaterials();
    }

    private void OnValidate()
    {
        EnsureDefaultHitMask();
    }

    public void Init(Vector3 direction, float attackDamage, GameObject attackOwner = null)
    {
        // 발사 시점에 공격 방향, 데미지, 발사자를 주입받아 투사체를 활성화합니다.
        moveDirection = direction.normalized;
        damage = attackDamage;
        owner = attackOwner;
        isInitialized = true;

        Destroy(gameObject, lifeTime);
    }

    public bool TryParry(GameObject parryOwner)
    {
        // 패리된 투사체는 데미지 이벤트를 발행하지 않고 즉시 제거됩니다.
        if (isHit) return false;

        isHit = true;
        owner = parryOwner;

        Destroy(gameObject);
        return true;
    }

    public bool IsOwnedBy(GameObject candidate)
    {
        if (candidate == null || owner == null) return false;

        return owner == candidate || owner.transform.IsChildOf(candidate.transform);
    }

    public void SetParryReadyVisual(bool isReady)
    {
        // 같은 표시 상태를 매 프레임 반복 적용하지 않도록 변경 시에만 색을 갱신합니다.
        if (isParryReadyVisualActive == isReady) return;

        isParryReadyVisualActive = isReady;

        if (isReady)
        {
            SetColor(parryReadyColor);
            return;
        }

        RestoreOriginalColors();
    }

    private void Update()
    {
        if (!isInitialized || isHit) return;

        Vector3 movement = moveDirection * speed * Time.deltaTime;

        if (TryHitAlongMovement(movement))
            return;

        transform.position += movement;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other, transform.position);
    }

    private bool TryHitAlongMovement(Vector3 movement)
    {
        // 빠른 투사체가 프레임 사이에 플레이어를 뚫고 지나가지 않도록 이동 경로를 검사합니다.
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
        Vector3 hitPoint = hit.collider.ClosestPoint(
            transform.position + moveDirection * hit.distance
        );

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

        // 테스트 몬스터 투사체는 플레이어 Status를 직접 호출해 공용 전투 이벤트 의존을 만들지 않습니다.
        PlayerStatus playerStatus = other.GetComponentInParent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.TakeDamage(
                new DamageInfo(
                    playerStatus.gameObject,
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
            Debug.LogWarning($"플레이어 Status를 찾지 못했습니다: {other.gameObject.name}");
        }

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

        int playerMask = LayerMask.GetMask(DefaultHitLayerName);
        if (playerMask == 0) return;

        hitMask = playerMask;
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

    private void CacheVisualMaterials()
    {
        // 패리 가능 표시를 위해 투사체와 자식 오브젝트의 머티리얼 색상 정보를 캐싱합니다.
        targetRenderers = GetComponentsInChildren<Renderer>();
        targetMaterials = new Material[targetRenderers.Length][];
        originalColors = new Color[targetRenderers.Length][];
        colorProperties = new string[targetRenderers.Length][];

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            targetMaterials[i] = targetRenderers[i].materials;
            originalColors[i] = new Color[targetMaterials[i].Length];
            colorProperties[i] = new string[targetMaterials[i].Length];

            for (int j = 0; j < targetMaterials[i].Length; j++)
            {
                colorProperties[i][j] = GetColorProperty(targetMaterials[i][j]);
                if (string.IsNullOrEmpty(colorProperties[i][j])) continue;

                originalColors[i][j] = targetMaterials[i][j].GetColor(colorProperties[i][j]);
            }
        }
    }

    private void SetColor(Color color)
    {
        if (targetMaterials == null) return;

        for (int i = 0; i < targetMaterials.Length; i++)
        {
            for (int j = 0; j < targetMaterials[i].Length; j++)
            {
                if (string.IsNullOrEmpty(colorProperties[i][j])) continue;

                targetMaterials[i][j].SetColor(colorProperties[i][j], color);
            }
        }
    }

    private void RestoreOriginalColors()
    {
        if (targetMaterials == null || originalColors == null || colorProperties == null)
            return;

        for (int i = 0; i < targetMaterials.Length; i++)
        {
            for (int j = 0; j < targetMaterials[i].Length; j++)
            {
                if (string.IsNullOrEmpty(colorProperties[i][j])) continue;

                targetMaterials[i][j].SetColor(colorProperties[i][j], originalColors[i][j]);
            }
        }
    }

    private string GetColorProperty(Material material)
    {
        if (material == null) return string.Empty;
        if (material.HasProperty("_BaseColor")) return "_BaseColor";
        if (material.HasProperty("_Color")) return "_Color";

        return string.Empty;
    }
}
