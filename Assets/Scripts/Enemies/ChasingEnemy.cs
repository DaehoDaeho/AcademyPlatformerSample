using UnityEngine;

/// <summary>평상시에는 짧게 좌우 순찰하고 플레이어가 보이면 조금 더 빠르게 추적합니다.</summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyNavigationSensor))]
public sealed class ChasingEnemy : MonoBehaviour
{
    // 추적할 플레이어 Transform을 저장하는 변수입니다.
    [SerializeField] private Transform target;
    // 플레이어를 감지할 수평 거리를 저장하는 변수입니다.
    [SerializeField, Min(0f)] private float detectionRange = 7f;
    // 플레이어를 감지할 수직 허용 범위를 저장하는 변수입니다.
    [SerializeField, Min(0f)] private float verticalTolerance = 2f;
    // 평상시 순찰 이동 속도를 저장하는 변수입니다.
    [SerializeField, Min(0f)] private float patrolSpeed = 1.5f;
    // 플레이어 추적 중 적용할 이동 속도를 저장하는 변수입니다.
    [SerializeField, Min(0f)] private float chaseSpeed = 3f;
    // 한 방향으로 순찰하는 시간을 저장하는 변수입니다.
    [SerializeField, Min(0.1f)] private float patrolDuration = 1.6f;
    // 순찰 방향을 바꾸기 전에 대기하는 시간을 저장하는 변수입니다.
    [SerializeField, Min(0.1f)] private float pauseDuration = 1f;
    // 적의 물리 이동을 제어할 Rigidbody2D를 저장하는 변수입니다.
    private Rigidbody2D body;
    // 지면, 벽과 직선 시야를 검사할 센서를 저장하는 변수입니다.
    private EnemyNavigationSensor navigationSensor;
    // 현재 순찰 이동 단계인지 저장하는 변수입니다.
    private bool patrolling = true;
    // 현재 순찰 또는 대기 단계에 남은 시간을 저장하는 변수입니다.
    private float remainingPatrolTime;
    // 평상시 순찰할 수평 방향을 저장하는 변수입니다.
    private float patrolDirection = 1f;
    // 추적형 적이 현재 바라보는 수평 방향을 저장하는 변수입니다.
    private float facingDirection = 1f;
    // 이전 물리 프레임에 플레이어를 추적하고 있었는지 저장하는 변수입니다.
    private bool wasChasing;

    /// <summary>평상시 순찰 이동 속도를 제공합니다.</summary>
    public float PatrolSpeed => patrolSpeed;
    /// <summary>플레이어 추적 이동 속도를 제공합니다.</summary>
    public float ChaseSpeed => chaseSpeed;
    /// <summary>추적형 적이 현재 바라보는 수평 방향을 제공합니다.</summary>
    public float FacingDirection => facingDirection;

    /// <summary>감지 거리와 평상시 및 추적 이동 속도를 설정합니다.</summary>
    /// <param name="range">플레이어를 감지할 수평 거리입니다.</param>
    /// <param name="normalSpeed">평상시 순찰 이동 속도입니다.</param>
    /// <param name="followSpeed">플레이어를 추적할 때의 이동 속도입니다.</param>
    public void Configure(float range, float normalSpeed, float followSpeed)
    {
        detectionRange = Mathf.Max(0f, range);
        patrolSpeed = Mathf.Max(0f, normalSpeed);
        chaseSpeed = Mathf.Max(patrolSpeed, followSpeed);
    }

    /// <summary>물리 본체와 이동 판단 센서를 가져오고 첫 순찰 시간을 설정합니다.</summary>
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        navigationSensor = GetComponent<EnemyNavigationSensor>();
        remainingPatrolTime = patrolDuration;
    }

    /// <summary>씬에서 플레이어 Transform을 찾아 추적 대상으로 설정합니다.</summary>
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // 추적 대상으로 사용할 플레이어 오브젝트입니다.
        if (player != null)
        {
            target = player.transform;
        }
    }

    /// <summary>고정 물리 프레임마다 시야 상태에 따라 추적 또는 평상시 순찰을 처리합니다.</summary>
    private void FixedUpdate()
    {
        bool targetVisible = CanSeeTarget(); // 플레이어가 현재 거리와 직선 시야 조건을 만족하는지 여부입니다.
        if (targetVisible == true)
        {
            wasChasing = true;
            FollowTarget();
            return;
        }

        if (wasChasing == true)
        {
            wasChasing = false;
            BeginPatrolPause();
        }
        UpdatePatrol();
    }

    /// <summary>플레이어가 감지 범위 안에 있고 지형에 가려지지 않았는지 확인합니다.</summary>
    /// <returns>플레이어를 추적할 수 있는지 여부입니다.</returns>
    private bool CanSeeTarget()
    {
        if (target == null || navigationSensor == null)
        {
            return false;
        }
        Vector2 difference = target.position - transform.position; // 적에서 플레이어까지의 위치 차이입니다.
        bool withinHorizontalRange =
            Mathf.Abs(difference.x) <= detectionRange; // 플레이어가 수평 감지 거리 안에 있는지 여부입니다.
        bool withinVerticalRange =
            Mathf.Abs(difference.y) <= verticalTolerance; // 플레이어가 허용된 높이 차이 안에 있는지 여부입니다.
        bool targetInFront =
            difference.x * facingDirection > 0f; // 플레이어가 현재 바라보는 방향 앞쪽에 있는지 여부입니다.
        bool withinRange =
            withinHorizontalRange == true &&
            withinVerticalRange == true &&
            targetInFront == true; // 거리, 높이와 정면 방향 조건을 모두 만족하는지 여부입니다.
        if (withinRange == false)
        {
            return false;
        }
        return navigationSensor.HasClearSight(target.position);
    }

    /// <summary>플레이어가 있는 방향으로 안전한 범위 안에서 추적 이동합니다.</summary>
    private void FollowTarget()
    {
        float differenceX = target.position.x - transform.position.x; // 플레이어까지 남은 수평 거리입니다.
        if (Mathf.Abs(differenceX) < 0.2f)
        {
            StopHorizontalMovement();
            return;
        }
        float chaseDirection = -1f; // 플레이어가 있는 수평 방향입니다.
        if (differenceX >= 0f)
        {
            chaseDirection = 1f;
        }
        if (navigationSensor.CanMove(chaseDirection) == false)
        {
            StopHorizontalMovement();
            return;
        }
        body.linearVelocity = new Vector2(chaseDirection * chaseSpeed, body.linearVelocity.y);
        ApplyFacingDirection(chaseDirection);
    }

    /// <summary>설정된 이동 및 대기 시간을 번갈아 적용하며 평상시 순찰을 처리합니다.</summary>
    private void UpdatePatrol()
    {
        remainingPatrolTime -= Time.fixedDeltaTime;
        if (patrolling == false)
        {
            StopHorizontalMovement();
            if (remainingPatrolTime <= 0f)
            {
                patrolling = true;
                remainingPatrolTime = patrolDuration;
            }
            return;
        }

        bool pathAvailable = false; // 현재 순찰 방향으로 안전하게 이동할 수 있는지 여부입니다.
        if (navigationSensor != null)
        {
            pathAvailable = navigationSensor.CanMove(patrolDirection);
        }
        if (pathAvailable == false || remainingPatrolTime <= 0f)
        {
            patrolDirection *= -1f;
            BeginPatrolPause();
            return;
        }
        body.linearVelocity = new Vector2(patrolDirection * patrolSpeed, body.linearVelocity.y);
        ApplyFacingDirection(patrolDirection);
    }

    /// <summary>논리적인 시야 방향과 캐릭터의 좌우 방향을 함께 변경합니다.</summary>
    /// <param name="direction">새로 바라볼 수평 방향입니다.</param>
    private void ApplyFacingDirection(float direction)
    {
        if (direction >= 0f)
        {
            facingDirection = 1f;
        }
        else
        {
            facingDirection = -1f;
        }

        transform.localScale =
            new Vector3(facingDirection, 1f, 1f);
    }

    /// <summary>수평 이동을 멈추고 평상시 순찰의 대기 단계로 전환합니다.</summary>
    private void BeginPatrolPause()
    {
        patrolling = false;
        remainingPatrolTime = pauseDuration;
        StopHorizontalMovement();
    }

    /// <summary>현재 수직 속도를 유지하면서 수평 이동 속도만 제거합니다.</summary>
    private void StopHorizontalMovement()
    {
        body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
    }
}
