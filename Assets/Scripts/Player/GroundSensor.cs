using UnityEngine;

    /// <summary>플레이어 발밑의 지면을 검사해 착지 상태를 제공합니다.</summary>
    public sealed class GroundSensor : MonoBehaviour
    {
        // 지면으로 판정할 물리 레이어를 저장하는 변수입니다.
        [SerializeField] private LayerMask groundMask;
        // 지면 검사 상자의 크기를 저장하는 변수입니다.
        [SerializeField] private Vector2 boxSize = new(0.7f, 0.12f);
        // 지면 검사 거리를 저장하는 변수입니다.
        [SerializeField] private float distance = 0.08f;

        /// <summary>현재 플레이어의 지면 접촉 여부를 제공합니다.</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>고정 물리 프레임마다 발밑의 지면을 검사합니다.</summary>
        private void FixedUpdate()
        {
            RaycastHit2D groundHit = Physics2D.BoxCast(
                transform.position,
                boxSize,
                0f,
                Vector2.down,
                distance,
                groundMask); // 플레이어 발밑의 상자 검사에서 감지한 지면 정보입니다.
            IsGrounded = groundHit.collider != null;
        }

        /// <summary>지면 검사에 사용할 레이어를 설정합니다.</summary>
        /// <param name="mask">지면으로 판정할 레이어 마스크입니다.</param>
        public void Configure(LayerMask mask)
        {
            groundMask = mask;
        }

        /// <summary>선택한 오브젝트의 지면 검사 범위를 기즈모로 표시합니다.</summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            if (IsGrounded == true)
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireCube((Vector2)transform.position + Vector2.down * distance, boxSize);
        }
    }
