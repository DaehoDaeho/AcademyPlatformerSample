using UnityEngine;

/// <summary>
/// 플레이어와 관련된 게임 이벤트를 받아 상황에 맞는 일회성 시각 효과를 생성합니다.
/// </summary>
[RequireComponent(typeof(Health), typeof(PlayerStompAttack))]
public sealed class PlayerVfxFeedback : MonoBehaviour
{
    /// <summary>
    /// Star 아이템을 획득할 때 생성할 파티클 프리팹입니다.
    /// </summary>
    [SerializeField] private GameObject collectibleEffectPrefab;

    /// <summary>
    /// 적을 밟았을 때 생성할 파티클 프리팹입니다.
    /// </summary>
    [SerializeField] private GameObject stompEffectPrefab;

    /// <summary>
    /// 플레이어가 피해를 받을 때 생성할 파티클 프리팹입니다.
    /// </summary>
    [SerializeField] private GameObject damagedEffectPrefab;

    /// <summary>
    /// 피해 이벤트를 제공하는 플레이어 체력 컴포넌트입니다.
    /// </summary>
    private Health health;

    /// <summary>
    /// 밟기 성공 이벤트를 제공하는 플레이어 공격 컴포넌트입니다.
    /// </summary>
    private PlayerStompAttack stompAttack;

    /// <summary>
    /// Star 획득 이펙트를 생성한 횟수입니다.
    /// </summary>
    public int CollectibleEffectPlayCount { get; private set; }

    /// <summary>
    /// 적 밟기 이펙트를 생성한 횟수입니다.
    /// </summary>
    public int StompEffectPlayCount { get; private set; }

    /// <summary>
    /// 피격 이펙트를 생성한 횟수입니다.
    /// </summary>
    public int DamagedEffectPlayCount { get; private set; }

    /// <summary>
    /// 투사체 충돌 이펙트를 생성한 횟수입니다.
    /// </summary>
    public int ProjectileImpactEffectPlayCount { get; private set; }

    /// <summary>
    /// 생성 코드에서 세 종류의 이펙트 프리팹을 설정합니다.
    /// </summary>
    /// <param name="collectibleEffect">Star 획득 이펙트 프리팹입니다.</param>
    /// <param name="stompEffect">적 밟기 이펙트 프리팹입니다.</param>
    /// <param name="damagedEffect">플레이어 피격 이펙트 프리팹입니다.</param>
    public void Configure(
        GameObject collectibleEffect,
        GameObject stompEffect,
        GameObject damagedEffect)
    {
        collectibleEffectPrefab = collectibleEffect;
        stompEffectPrefab = stompEffect;
        damagedEffectPrefab = damagedEffect;
    }

    /// <summary>
    /// 필요한 플레이어 이벤트 제공 컴포넌트를 가져옵니다.
    /// </summary>
    private void Awake()
    {
        health = GetComponent<Health>();
        stompAttack = GetComponent<PlayerStompAttack>();
    }

    /// <summary>
    /// 플레이어의 피격과 밟기 성공 이벤트에 시각 효과 함수를 등록합니다.
    /// </summary>
    private void OnEnable()
    {
        if (health != null)
        {
            health.Damaged += PlayDamagedEffect;
        }

        if (stompAttack != null)
        {
            stompAttack.Stomped += PlayStompEffect;
        }
    }

    /// <summary>
    /// 오브젝트 비활성화 시 등록했던 시각 효과 이벤트를 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        if (health != null)
        {
            health.Damaged -= PlayDamagedEffect;
        }

        if (stompAttack != null)
        {
            stompAttack.Stomped -= PlayStompEffect;
        }
    }

    /// <summary>
    /// Star 아이템이 있던 월드 위치에 획득 이펙트를 생성합니다.
    /// </summary>
    /// <param name="worldPosition">Star 아이템을 획득한 월드 위치입니다.</param>
    public void PlayCollectibleEffect(Vector3 worldPosition)
    {
        if (collectibleEffectPrefab == null)
        {
            return;
        }

        Instantiate(collectibleEffectPrefab, worldPosition, Quaternion.identity);
        CollectibleEffectPlayCount++;
    }

    /// <summary>
    /// 투사체가 닿은 위치에 피격 스파크 이펙트를 생성합니다.
    /// </summary>
    /// <param name="worldPosition">투사체가 플레이어와 충돌한 월드 위치입니다.</param>
    public void PlayProjectileImpactEffect(Vector3 worldPosition)
    {
        if (damagedEffectPrefab == null)
        {
            return;
        }

        Instantiate(damagedEffectPrefab, worldPosition, Quaternion.identity);
        ProjectileImpactEffectPlayCount++;
    }

    /// <summary>
    /// 적과 충돌한 위치에 일반 또는 강한 접촉 피격 이펙트를 생성합니다.
    /// </summary>
    /// <param name="worldPosition">적과 플레이어가 충돌한 월드 위치입니다.</param>
    /// <param name="powerfulImpact">대시형 적의 강한 충돌이면 true입니다.</param>
    public void PlayEnemyCollisionEffect(
        Vector3 worldPosition,
        bool powerfulImpact)
    {
        if (damagedEffectPrefab == null)
        {
            return;
        }

        if (powerfulImpact == false)
        {
            CreateScaledDamagedEffect(worldPosition, 0.85f);
            return;
        }

        CreateScaledDamagedEffect(worldPosition, 1.65f);
        CreateScaledDamagedEffect(
            worldPosition + new Vector3(-0.28f, 0.18f, 0f),
            1.05f);
        CreateScaledDamagedEffect(
            worldPosition + new Vector3(0.28f, -0.12f, 0f),
            1.05f);
    }

    /// <summary>
    /// 가시에 닿은 위치에 날카롭게 퍼지는 피격 이펙트를 생성합니다.
    /// </summary>
    /// <param name="worldPosition">플레이어가 가시에 닿은 월드 위치입니다.</param>
    public void PlaySpikeCollisionEffect(Vector3 worldPosition)
    {
        if (damagedEffectPrefab == null)
        {
            return;
        }

        CreateScaledDamagedEffect(worldPosition, 1.15f);
        CreateScaledDamagedEffect(
            worldPosition + Vector3.up * 0.22f,
            0.7f);
    }

    /// <summary>
    /// 지정한 위치와 크기로 공용 피격 이펙트 프리팹을 생성합니다.
    /// </summary>
    /// <param name="worldPosition">이펙트를 생성할 월드 위치입니다.</param>
    /// <param name="effectScale">이펙트에 적용할 크기 배율입니다.</param>
    private void CreateScaledDamagedEffect(
        Vector3 worldPosition,
        float effectScale)
    {
        GameObject effectObject = Instantiate(
            damagedEffectPrefab,
            worldPosition,
            Quaternion.identity);
        effectObject.transform.localScale = Vector3.one * effectScale;
    }

    /// <summary>
    /// 플레이어 발 아래에 적 밟기 충격 이펙트를 생성합니다.
    /// </summary>
    private void PlayStompEffect()
    {
        if (stompEffectPrefab == null)
        {
            return;
        }

        // 플레이어 발 아래로 조정한 밟기 이펙트 생성 위치입니다.
        Vector3 effectPosition = transform.position + Vector3.down * 0.65f;

        Instantiate(stompEffectPrefab, effectPosition, Quaternion.identity);
        StompEffectPlayCount++;
    }

    /// <summary>
    /// 피해를 받은 플레이어 중심에 붉은 피격 이펙트를 생성합니다.
    /// </summary>
    /// <param name="sourcePosition">피해를 발생시킨 오브젝트의 월드 위치입니다.</param>
    /// <param name="invulnerabilityDuration">피해 후 적용되는 무적 시간입니다.</param>
    private void PlayDamagedEffect(Vector2 sourcePosition, float invulnerabilityDuration)
    {
        if (damagedEffectPrefab == null)
        {
            return;
        }

        Instantiate(damagedEffectPrefab, transform.position, Quaternion.identity);
        DamagedEffectPlayCount++;
    }
}
