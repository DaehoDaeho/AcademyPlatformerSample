using UnityEngine;

/// <summary>
/// 원거리 적의 감지 결과에 따라 일정한 간격으로 투사체를 생성합니다.
/// </summary>
public sealed class EnemyProjectileLauncher : MonoBehaviour
{
    /// <summary>
    /// 발사할 투사체 프리팹입니다.
    /// </summary>
    [SerializeField] private GameObject projectilePrefab;

    /// <summary>
    /// 투사체가 생성되는 위치입니다.
    /// </summary>
    [SerializeField] private Transform firePoint;

    /// <summary>
    /// 플레이어 감지와 바라보는 방향을 제공하는 컴포넌트입니다.
    /// </summary>
    [SerializeField] private RangedEnemyLookout lookout;

    /// <summary>
    /// 연속 발사 사이에 기다리는 시간입니다.
    /// </summary>
    [SerializeField, Min(0.5f)] private float fireInterval = 2.4f;

    /// <summary>
    /// 플레이어를 발견한 뒤 첫 발을 쏘기 전에 기다리는 시간입니다.
    /// </summary>
    [SerializeField, Min(0f)] private float firstShotDelay = 0.6f;

    /// <summary>
    /// 다음 발사까지 남은 시간입니다.
    /// </summary>
    private float fireTimer;

    /// <summary>
    /// 설정된 발사 간격을 외부 검증 코드에 제공합니다.
    /// </summary>
    public float FireInterval => fireInterval;

    /// <summary>
    /// 생성 코드에서 발사에 필요한 참조와 시간 값을 설정합니다.
    /// </summary>
    /// <param name="prefab">발사할 투사체 프리팹입니다.</param>
    /// <param name="point">투사체 생성 위치입니다.</param>
    /// <param name="targetLookout">플레이어 감지 컴포넌트입니다.</param>
    /// <param name="interval">연속 발사 간격입니다.</param>
    /// <param name="delay">첫 발사 전 준비 시간입니다.</param>
    public void Configure(
        GameObject prefab,
        Transform point,
        RangedEnemyLookout targetLookout,
        float interval,
        float delay)
    {
        projectilePrefab = prefab;
        firePoint = point;
        lookout = targetLookout;
        fireInterval = Mathf.Max(0.5f, interval);
        firstShotDelay = Mathf.Max(0f, delay);
    }

    /// <summary>
    /// 첫 감지 직후 즉시 발사되지 않도록 준비 시간을 설정합니다.
    /// </summary>
    private void Awake()
    {
        fireTimer = firstShotDelay;
    }

    /// <summary>
    /// 플레이어가 보이는 동안 발사 대기 시간을 계산하고 투사체를 발사합니다.
    /// </summary>
    private void Update()
    {
        if (lookout == null ||
            lookout.IsPlayerVisibleNow() == false)
        {
            fireTimer = firstShotDelay;
            return;
        }

        fireTimer -= Time.deltaTime;

        if (fireTimer > 0f)
        {
            return;
        }

        FireProjectile();
        fireTimer = fireInterval;
    }

    /// <summary>
    /// 현재 바라보는 방향으로 투사체 인스턴스를 생성합니다.
    /// </summary>
    private void FireProjectile()
    {
        if (projectilePrefab == null ||
            firePoint == null ||
            lookout == null ||
            lookout.IsPlayerVisibleNow() == false)
        {
            return;
        }

        // 현재 적이 바라보는 수평 방향입니다.
        float fireDirection = lookout.FacingDirection;

        // 발사 방향에 맞춰 계산한 투사체 생성 위치입니다.
        Vector3 spawnPosition =
            firePoint.position + Vector3.right * fireDirection * 0.35f;

        // 새로 생성한 적 투사체 오브젝트입니다.
        GameObject projectileObject = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity);

        // 투사체의 이동 방향을 설정할 이동 컴포넌트입니다.
        EnemyProjectileMovement projectileMovement =
            projectileObject.GetComponent<EnemyProjectileMovement>();

        if (projectileMovement != null)
        {
            projectileMovement.Launch(fireDirection);
        }
    }
}
