using UnityEngine;

/// <summary>체력이 부족한 플레이어에게 회복을 제공하고 천천히 떠다니는 포션 아이템입니다.</summary>
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public sealed class HealthPotion : MonoBehaviour
{
    // 포션 하나를 마셨을 때 회복할 체력입니다.
    [SerializeField, Min(1)] private int healAmount = 1;
    // 포션이 원래 위치를 기준으로 위아래로 움직일 거리입니다.
    [SerializeField, Min(0f)] private float floatingHeight = 0.12f;
    // 포션이 위아래로 움직이는 속도입니다.
    [SerializeField, Min(0f)] private float floatingSpeed = 2.4f;
    // 위아래 움직임을 계산할 기준 월드 위치입니다.
    private Vector3 startingPosition;
    // 한 프레임에 여러 플레이어 충돌 이벤트가 들어와도 중복 획득되지 않도록 저장하는 변수입니다.
    private bool collected;

    /// <summary>에디터 생성 코드에서 포션의 회복량을 설정합니다.</summary>
    /// <param name="amount">포션 하나가 회복할 체력입니다.</param>
    public void Configure(int amount)
    {
        healAmount = Mathf.Max(1, amount);
    }

    /// <summary>게임 시작 시 포션이 배치된 기준 위치를 저장합니다.</summary>
    private void Start()
    {
        startingPosition = transform.position;
    }

    /// <summary>아이템임을 알아보기 쉽도록 포션을 기준 위치 위아래로 부드럽게 움직입니다.</summary>
    private void Update()
    {
        float verticalOffset =
            Mathf.Sin(Time.time * floatingSpeed) * floatingHeight;
        transform.position = startingPosition + Vector3.up * verticalOffset;
    }

    /// <summary>체력이 부족한 플레이어와 접촉하면 체력을 회복하고 효과를 재생한 뒤 포션을 제거합니다.</summary>
    /// <param name="other">포션의 트리거에 들어온 플레이어 측 Collider2D입니다.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    /// <summary>플레이어가 포션 트리거 안에 머물 때도 회복 가능 여부를 다시 확인합니다.</summary>
    /// <param name="other">포션 트리거 안에 머물고 있는 플레이어 측 Collider2D입니다.</param>
    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other);
    }

    /// <summary>충돌 대상의 플레이어 체력을 찾아 실제 회복과 획득 피드백을 한 번만 처리합니다.</summary>
    /// <param name="other">포션과 접촉한 Collider2D입니다.</param>
    private void TryCollect(Collider2D other)
    {
        if (collected == true)
        {
            return;
        }

        Health playerHealth =
            other.GetComponentInParent<Health>(); // 포션으로 회복할 플레이어 체력 컴포넌트입니다.
        if (playerHealth == null)
        {
            return;
        }

        bool healed = playerHealth.Heal(healAmount); // 플레이어 체력이 실제로 회복되었는지 여부입니다.
        if (healed == false)
        {
            return;
        }

        collected = true;

        PlayerAudioFeedback audioFeedback =
            other.GetComponentInParent<PlayerAudioFeedback>(); // 회복음을 재생할 플레이어 오디오 컴포넌트입니다.
        if (audioFeedback != null)
        {
            audioFeedback.PlayHealing();
        }

        PlayerVfxFeedback vfxFeedback =
            other.GetComponentInParent<PlayerVfxFeedback>(); // 회복 파티클을 생성할 플레이어 시각 효과 컴포넌트입니다.
        if (vfxFeedback != null)
        {
            vfxFeedback.PlayHealingEffect(transform.position);
        }

        Destroy(gameObject);
    }
}
