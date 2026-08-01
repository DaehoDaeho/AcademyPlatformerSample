using UnityEngine;

/// <summary>
/// 적 투사체가 플레이어나 지형에 닿았을 때 피해와 제거를 처리합니다.
/// </summary>
public sealed class EnemyProjectileDamage : MonoBehaviour
{
    /// <summary>
    /// 플레이어에게 적용할 피해량입니다.
    /// </summary>
    [SerializeField, Min(1)] private int damage = 1;

    /// <summary>
    /// 투사체가 트리거 충돌한 대상에 따라 피해를 적용하거나 투사체를 제거합니다.
    /// </summary>
    /// <param name="other">투사체와 접촉한 Collider2D입니다.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 접촉한 오브젝트 또는 부모가 가진 플레이어 체력 컴포넌트입니다.
        Health targetHealth = other.GetComponentInParent<Health>();

        if (targetHealth != null)
        {
            // 무적 상태가 아닐 때 실제로 피해가 적용되었는지를 나타냅니다.
            bool damageApplied =
                targetHealth.TakeDamage(damage, transform.position);

            if (damageApplied == true)
            {
                // 충돌한 플레이어가 가진 시각 효과 처리 컴포넌트입니다.
                PlayerVfxFeedback vfxFeedback =
                    other.GetComponentInParent<PlayerVfxFeedback>();

                if (vfxFeedback != null)
                {
                    // 플레이어 콜라이더 표면에서 투사체와 가장 가까운 충돌 위치입니다.
                    Vector2 impactPosition =
                        other.ClosestPoint(transform.position);
                    vfxFeedback.PlayProjectileImpactEffect(impactPosition);
                }
            }

            Destroy(gameObject);
            return;
        }

        // 접촉한 오브젝트가 지형 레이어에 속하는지를 나타냅니다.
        bool touchedGround = other.gameObject.layer == LayerMask.NameToLayer("Ground");

        if (touchedGround == true)
        {
            Destroy(gameObject);
        }
    }
}
