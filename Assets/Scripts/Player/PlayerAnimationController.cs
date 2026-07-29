using UnityEngine;

    /// <summary>
    /// 플레이어의 물리 상태를 애니메이터 매개변수로 변환합니다.
    /// 이동과 점프 규칙은 수정하지 않고 현재 상태를 애니메이션 시스템에 전달하는 역할만 담당합니다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerAnimationController : MonoBehaviour
    {
        // 애니메이터의 이동 속도 매개변수 식별자를 저장하는 변수입니다.
        private static readonly int SpeedId = Animator.StringToHash("Speed");
        // 애니메이터의 지면 상태 매개변수 식별자를 저장하는 변수입니다.
        private static readonly int GroundedId = Animator.StringToHash("Grounded");

        // 제어할 플레이어 애니메이터를 저장하는 변수입니다.
        [SerializeField] private Animator animator;
        // 지면 상태를 제공할 센서를 저장하는 변수입니다.
        [SerializeField] private GroundSensor groundSensor;
        // 플레이어의 이동 속도를 읽을 Rigidbody2D를 저장하는 변수입니다.
        private Rigidbody2D body;

        /// <summary>애니메이터와 지면 센서를 설정합니다.</summary>
        /// <param name="targetAnimator">제어 대상 애니메이터입니다.</param>
        /// <param name="sensor">지면 상태를 제공할 센서입니다.</param>
        public void Configure(Animator targetAnimator, GroundSensor sensor)
        {
            animator = targetAnimator;
            groundSensor = sensor;
        }

        /// <summary>필요한 Rigidbody2D 참조를 가져옵니다.</summary>
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        /// <summary>매 프레임 이동 속도와 지면 상태를 애니메이터에 전달합니다.</summary>
        private void Update()
        {
            // 애니메이션에 필요한 참조가 없으면 갱신을 중단합니다.
            if (animator == null || groundSensor == null)
            {
                return;
            }
            animator.SetFloat(SpeedId, Mathf.Abs(body.linearVelocity.x));
            animator.SetBool(GroundedId, groundSensor.IsGrounded == true);
        }
    }
