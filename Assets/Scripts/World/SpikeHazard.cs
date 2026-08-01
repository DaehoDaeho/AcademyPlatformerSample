using UnityEngine;

/// <summary>플레이어가 가시에 닿으면 피해를 주는 고정형 진행 방해 요소입니다.</summary>
public sealed class SpikeHazard : MonoBehaviour
{
    // 플레이어가 가시에 닿았을 때 적용할 피해량입니다.
    [SerializeField, Min(1)] private int damage = 1;

    /// <summary>트리거 영역에 들어온 플레이어에게 가시 위치를 원점으로 피해를 적용합니다.</summary>
    /// <param name="other">가시 트리거에 들어온 충돌체입니다.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") == false)
        {
            return;
        }

        Health playerHealth = other.GetComponent<Health>();
        if (playerHealth == null)
        {
            return;
        }

        bool damageApplied =
            playerHealth.TakeDamage(damage, transform.position);
        if (damageApplied == true)
        {
            PlaySpikeCollisionFeedback(other);
        }
    }

    /// <summary>에디터 생성 코드에서 가시가 줄 피해량을 설정합니다.</summary>
    /// <param name="damageAmount">플레이어에게 적용할 피해량입니다.</param>
    public void Configure(int damageAmount)
    {
        damage = Mathf.Max(1, damageAmount);
    }

    /// <summary>플레이어에게 가시 전용 피격 이펙트와 날카로운 효과음을 요청합니다.</summary>
    /// <param name="other">가시와 충돌한 플레이어 콜라이더입니다.</param>
    private void PlaySpikeCollisionFeedback(Collider2D other)
    {
        PlayerVfxFeedback vfxFeedback =
            other.GetComponentInParent<PlayerVfxFeedback>(); // 가시 피격 이펙트를 출력할 플레이어 컴포넌트입니다.
        PlayerAudioFeedback audioFeedback =
            other.GetComponentInParent<PlayerAudioFeedback>(); // 가시 피격음을 출력할 플레이어 컴포넌트입니다.
        Vector2 impactPosition =
            other.ClosestPoint(transform.position); // 플레이어 표면에서 가시와 가장 가까운 충돌 위치입니다.
        if (vfxFeedback != null)
        {
            vfxFeedback.PlaySpikeCollisionEffect(impactPosition);
        }
        if (audioFeedback != null)
        {
            audioFeedback.PlaySpikeCollision();
        }
    }
}
