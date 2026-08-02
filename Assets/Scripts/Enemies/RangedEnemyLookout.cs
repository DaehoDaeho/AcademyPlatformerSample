using UnityEngine;

/// <summary>
/// 원거리 적이 평상시에는 좌우를 살피고 플레이어가 보이면 플레이어를 바라보게 합니다.
/// </summary>
[DefaultExecutionOrder(-100)]
public sealed class RangedEnemyLookout : MonoBehaviour
{
    /// <summary>
    /// 플레이어를 발견할 수 있는 최대 수평 거리입니다.
    /// </summary>
    [SerializeField, Min(1f)] private float sightDistance = 7f;

    /// <summary>
    /// 플레이어를 발견할 수 있는 최대 높이 차이입니다.
    /// </summary>
    [SerializeField, Min(0.5f)] private float sightHeight = 2.5f;

    /// <summary>
    /// 평상시에 반대 방향으로 고개를 돌릴 때까지 기다리는 시간입니다.
    /// </summary>
    [SerializeField, Min(0.2f)] private float lookInterval = 1.4f;

    /// <summary>
    /// 벽과 지형을 검사할 때 사용하는 레이어입니다.
    /// </summary>
    [SerializeField] private LayerMask obstacleLayers;

    /// <summary>
    /// 좌우 반전을 적용할 캐릭터의 SpriteRenderer입니다.
    /// </summary>
    [SerializeField] private SpriteRenderer visualRenderer;

    /// <summary>
    /// 씬에서 찾아낸 플레이어의 Transform입니다.
    /// </summary>
    private Transform playerTransform;

    /// <summary>
    /// 다음 방향 전환까지 남은 시간입니다.
    /// </summary>
    private float lookTimer;

    /// <summary>
    /// 현재 적이 바라보는 수평 방향입니다.
    /// </summary>
    public float FacingDirection { get; private set; } = 1f;

    /// <summary>
    /// 설정된 플레이어 감지 거리를 외부 검증 코드에 제공합니다.
    /// </summary>
    public float SightDistance => sightDistance;

    /// <summary>
    /// 현재 플레이어가 사거리와 시야 조건을 모두 만족하는지를 나타냅니다.
    /// </summary>
    public bool PlayerVisible { get; private set; }

    /// <summary>
    /// 생성 코드에서 시야 거리와 캐릭터 렌더러를 설정합니다.
    /// </summary>
    /// <param name="distance">플레이어를 발견할 최대 수평 거리입니다.</param>
    /// <param name="height">플레이어를 발견할 최대 높이 차이입니다.</param>
    /// <param name="interval">평상시 방향 전환 간격입니다.</param>
    /// <param name="renderer">좌우 반전을 적용할 SpriteRenderer입니다.</param>
    public void Configure(float distance, float height, float interval, SpriteRenderer renderer)
    {
        sightDistance = Mathf.Max(1f, distance);
        sightHeight = Mathf.Max(0.5f, height);
        lookInterval = Mathf.Max(0.2f, interval);
        visualRenderer = renderer;
        obstacleLayers = LayerMask.GetMask("Ground");
    }

    /// <summary>현재 스테이지의 회피 공간에 맞춰 수평 시야 거리를 변경합니다.</summary>
    /// <param name="distance">새로 적용할 최대 수평 감지 거리입니다.</param>
    public void SetSightDistance(float distance)
    {
        sightDistance = Mathf.Max(1f, distance);
    }

    /// <summary>
    /// 플레이어를 찾고 최초 방향을 화면에 반영합니다.
    /// </summary>
    private void Awake()
    {
        // Player 태그를 가진 플레이어 오브젝트입니다.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        ApplyFacingDirection();
    }

    /// <summary>
    /// 매 프레임 플레이어 감지 또는 평상시 두리번거리기 행동을 선택합니다.
    /// </summary>
    private void Update()
    {
        PlayerVisible = IsPlayerVisibleNow();

        if (PlayerVisible == true)
        {
            FacePlayer();
        }
        else
        {
            LookAround();
        }
    }

    /// <summary>현재 프레임의 위치와 바라보는 방향을 사용해 플레이어가 실제로 보이는지 즉시 검사합니다.</summary>
    /// <returns>플레이어가 정면 시야 안에 있고 지형에 가려지지 않았으면 true를 반환합니다.</returns>
    public bool IsPlayerVisibleNow()
    {
        return CheckPlayerVisible();
    }

    /// <summary>
    /// 플레이어가 거리, 높이, 진행 방향, 장애물 조건을 만족하는지 확인합니다.
    /// </summary>
    /// <returns>플레이어를 실제로 볼 수 있으면 true를 반환합니다.</returns>
    private bool CheckPlayerVisible()
    {
        if (playerTransform == null)
        {
            return false;
        }

        // 적에서 플레이어까지의 위치 차이입니다.
        Vector2 offsetToPlayer = playerTransform.position - transform.position;

        // 플레이어와의 수평 거리입니다.
        float horizontalDistance = Mathf.Abs(offsetToPlayer.x);

        // 플레이어와의 높이 차이입니다.
        float verticalDistance = Mathf.Abs(offsetToPlayer.y);

        if (horizontalDistance > sightDistance || verticalDistance > sightHeight)
        {
            return false;
        }

        // 플레이어가 현재 바라보는 방향의 앞쪽에 있는지 나타냅니다.
        bool playerInFront =
            offsetToPlayer.x * FacingDirection > 0f;

        if (playerInFront == false)
        {
            return false;
        }

        // 적과 플레이어 사이의 지형을 검사하는 광선 결과입니다.
        RaycastHit2D obstacleHit = Physics2D.Raycast(
            transform.position,
            offsetToPlayer.normalized,
            offsetToPlayer.magnitude,
            obstacleLayers);

        if (obstacleHit.collider != null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 플레이어가 있는 수평 방향을 바라봅니다.
    /// </summary>
    private void FacePlayer()
    {
        // 플레이어가 적의 오른쪽에 있는지를 나타냅니다.
        bool playerOnRight = playerTransform.position.x >= transform.position.x;

        if (playerOnRight == true)
        {
            FacingDirection = 1f;
        }
        else
        {
            FacingDirection = -1f;
        }

        ApplyFacingDirection();
    }

    /// <summary>
    /// 플레이어가 보이지 않을 때 일정 간격으로 좌우 방향을 전환합니다.
    /// </summary>
    private void LookAround()
    {
        lookTimer -= Time.deltaTime;

        if (lookTimer > 0f)
        {
            return;
        }

        FacingDirection *= -1f;
        lookTimer = lookInterval;
        ApplyFacingDirection();
    }

    /// <summary>
    /// 현재 바라보는 방향에 맞춰 캐릭터 스프라이트를 좌우 반전합니다.
    /// </summary>
    private void ApplyFacingDirection()
    {
        if (visualRenderer == null)
        {
            return;
        }

        visualRenderer.flipX = FacingDirection < 0f;
    }
}
