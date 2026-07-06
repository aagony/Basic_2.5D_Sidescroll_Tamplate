using UnityEngine;

public static class DamageableResolver
{
    // Collider의 부모 계층에서 IDamageable 대상을 찾습니다
    public static IDamageable FindDamageable(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return null;
        }

        return hitCollider.GetComponentInParent<IDamageable>();
    }

    // 중복 데미지 방지에 사용할 기준 오브젝트를 반환합니다
    public static Object GetTargetKey(Collider hitCollider, IDamageable damageable)
    {
        Object damageableObject = damageable as Object;

        if (damageableObject != null)
        {
            return damageableObject;
        }

        if (hitCollider != null && hitCollider.attachedRigidbody != null)
        {
            return hitCollider.attachedRigidbody;
        }

        if (hitCollider != null)
        {
            return hitCollider.transform.root;
        }

        return null;
    }

    // 피격 이벤트에 사용할 GameObject를 반환합니다
    public static GameObject GetTargetObject(Collider hitCollider, IDamageable damageable)
    {
        Component damageableComponent = damageable as Component;

        if (damageableComponent != null)
        {
            return damageableComponent.gameObject;
        }

        if (hitCollider != null)
        {
            return hitCollider.gameObject;
        }

        return null;
    }
}
