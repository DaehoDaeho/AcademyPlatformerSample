using UnityEngine;

    /// <summary>적의 물리 이동 속도를 애니메이터 상태 값으로 변환합니다.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyAnimationController : MonoBehaviour
    {
        // 애니메이터의 이동 속도 매개변수 식별자를 저장하는 변수입니다.
        private static readonly int SpeedId = Animator.StringToHash("Speed");
        // 제어할 적 애니메이터를 저장하는 변수입니다.
        [SerializeField] private Animator animator;
        // 적의 물리 속도를 읽을 Rigidbody2D를 저장하는 변수입니다.
        private Rigidbody2D body;

        /// <summary>제어할 애니메이터를 설정합니다.</summary>
        /// <param name="targetAnimator">제어 대상 애니메이터입니다.</param>
        public void Configure(Animator targetAnimator)
        {
            animator = targetAnimator;
        }
        /// <summary>필요한 Rigidbody2D 참조를 가져옵니다.</summary>
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        /// <summary>매 프레임 적의 수평 속도를 애니메이터에 전달합니다.</summary>
        private void Update()
        {
            // 애니메이터가 준비된 경우 현재 이동 속도를 전달합니다.
            if (animator != null)
            {
                animator.SetFloat(SpeedId, Mathf.Abs(body.linearVelocity.x));
            }
        }
    }
