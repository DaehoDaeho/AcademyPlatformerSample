using UnityEngine;

/// <summary>플레이어가 추락 구역에 들어오면 무적 상태와 관계없이 즉시 사망 처리합니다.</summary>
public sealed class FallDeathZone : MonoBehaviour
{
    /// <summary>추락 구역에 들어온 충돌체의 체력을 찾아 즉시 사망 처리를 시도합니다.</summary>
    /// <param name="other">추락 구역에 들어온 충돌체입니다.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other);
    }

    /// <summary>전달받은 컴포넌트의 상위 오브젝트에서 체력을 찾아 추락 사망을 적용합니다.</summary>
    /// <param name="other">사망 처리를 적용할 대상 컴포넌트입니다.</param>
    /// <returns>대상이 새롭게 사망 처리되었는지 여부입니다.</returns>
    public bool TryKill(Component other)
    {
        if (other == null)
        {
            return false;
        }
        Health health = other.GetComponentInParent<Health>(); // 추락한 대상의 체력 컴포넌트입니다.
        if (health == null)
        {
            return false;
        }
        return health.ForceDeath(transform.position);
    }
}
