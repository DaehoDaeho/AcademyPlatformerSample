using UnityEngine;

    /// <summary>설정된 두 지점 사이에서 적을 왕복 이동시킵니다.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PatrolEnemy : MonoBehaviour
    {
        // 순찰 구간의 왼쪽 경계를 저장하는 변수입니다.
        [SerializeField] private float leftX;
        // 순찰 구간의 오른쪽 경계를 저장하는 변수입니다.
        [SerializeField] private float rightX;
        // 적의 이동 속도를 저장하는 변수입니다.
        [SerializeField, Min(0f)] private float speed = 2f;
        // 적의 물리 이동을 제어할 Rigidbody2D를 저장하는 변수입니다.
        private Rigidbody2D body;
        // 진행 방향의 지면과 벽을 검사하는 센서를 저장하는 변수입니다.
        private EnemyNavigationSensor navigationSensor;
        // 현재 이동 방향을 저장하는 변수입니다.
        private int direction = 1;

        /// <summary>순찰 범위와 이동 속도를 설정합니다.</summary>
        /// <param name="left">순찰 구간의 왼쪽 경계입니다.</param>
        /// <param name="right">순찰 구간의 오른쪽 경계입니다.</param>
        /// <param name="moveSpeed">적의 이동 속도입니다.</param>
        public void Configure(float left, float right, float moveSpeed = 2f)
        {
            leftX = left;
            rightX = right;
            speed = moveSpeed;
        }

        /// <summary>필요한 Rigidbody2D 참조를 가져옵니다.</summary>
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            navigationSensor = GetComponent<EnemyNavigationSensor>();
        }
        /// <summary>고정 물리 프레임마다 순찰 방향과 속도를 갱신합니다.</summary>
        private void FixedUpdate()
        {
            // 오른쪽 순찰 경계에 도착하면 왼쪽으로 방향을 바꿉니다.
            bool reachedRightBoundary = transform.position.x >= rightX; // 오른쪽 순찰 경계에 도착했는지 여부입니다.
            bool reachedLeftBoundary = transform.position.x <= leftX; // 왼쪽 순찰 경계에 도착했는지 여부입니다.
            bool pathBlocked = true; // 진행 방향에 낭떠러지나 벽이 있는지 여부입니다.
            if (navigationSensor != null)
            {
                pathBlocked = navigationSensor.CanMove(direction) == false;
            }
            if (reachedRightBoundary == true)
            {
                direction = -1;
            }
            // 왼쪽 순찰 경계에 도착하면 오른쪽으로 방향을 바꿉니다.
            else if (reachedLeftBoundary == true)
            {
                direction = 1;
            }
            else if (pathBlocked == true)
            {
                direction *= -1;
            }
            body.linearVelocity = new Vector2(direction * speed, body.linearVelocity.y);
            transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }
