using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private CommonProjectile projectilePrefab;
    [SerializeField] private Transform container;

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 32;
    [SerializeField] private int maxSize = 256;
    [SerializeField] private int prewarmCount = 16;
    [SerializeField] private bool collectionCheck = true;

    private IObjectPool<CommonProjectile> projectilePool;

    // 풀을 생성하고 필요하면 미리 투사체를 만들어둡니다
    private void Awake()
    {
        CreatePool();
        PrewarmPool();
    }

    // 투사체 풀을 생성합니다
    private void CreatePool()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is missing", this);
            return;
        }

        projectilePool = new ObjectPool<CommonProjectile>(
            CreateProjectile,
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile,
            collectionCheck,
            Mathf.Max(1, defaultCapacity),
            Mathf.Max(1, maxSize)
        );
    }

    // 지정된 수량만큼 투사체를 미리 생성합니다
    private void PrewarmPool()
    {
        if (projectilePool == null || prewarmCount <= 0)
        {
            return;
        }

        int count = Mathf.Min(prewarmCount, maxSize);
        List<CommonProjectile> cachedProjectiles = new List<CommonProjectile>(count);

        for (int i = 0; i < count; i++)
        {
            cachedProjectiles.Add(projectilePool.Get());
        }

        for (int i = 0; i < cachedProjectiles.Count; i++)
        {
            projectilePool.Release(cachedProjectiles[i]);
        }
    }

    // 풀에서 투사체를 꺼내 발사합니다
    public CommonProjectile Spawn(ProjectileLaunchData launchData)
    {
        if (projectilePool == null)
        {
            Debug.LogError("Projectile pool is not initialized", this);
            return null;
        }

        CommonProjectile projectile = projectilePool.Get();
        projectile.Launch(launchData);
        return projectile;
    }

    // 풀에 넣을 새 투사체를 생성합니다
    private CommonProjectile CreateProjectile()
    {
        CommonProjectile projectile = Instantiate(projectilePrefab, container);
        projectile.SetPool(projectilePool);
        projectile.gameObject.SetActive(false);
        return projectile;
    }

    // 풀에서 투사체를 꺼낼 때 호출됩니다
    private void OnGetProjectile(CommonProjectile projectile)
    {
        projectile.MarkTakenFromPool();
        projectile.gameObject.SetActive(true);
    }

    // 투사체가 풀로 돌아올 때 호출됩니다
    private void OnReleaseProjectile(CommonProjectile projectile)
    {
        projectile.ResetForPool();
        projectile.gameObject.SetActive(false);
    }

    // 풀이 초과된 투사체를 제거할 때 호출됩니다
    private void OnDestroyProjectile(CommonProjectile projectile)
    {
        if (projectile == null)
        {
            return;
        }

        Destroy(projectile.gameObject);
    }
}
