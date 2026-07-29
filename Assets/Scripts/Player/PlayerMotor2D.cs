using UnityEngine;

    /// <summary>플레이어의 수평 가속, 감속과 방향 전환을 처리합니다.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputReader))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        // 플레이어의 최대 수평 속도를 저장하는 변수입니다.
        [SerializeField, Min(0f)] private float maxSpeed = 7f;
        // 이동 입력 중 적용할 가속도를 저장하는 변수입니다.
        [SerializeField, Min(0f)] private float acceleration = 55f;
        // 이동 입력이 없을 때 적용할 감속도를 저장하는 변수입니다.
        [SerializeField, Min(0f)] private float deceleration = 70f;

        // 플레이어의 물리 이동을 제어할 Rigidbody2D를 저장하는 변수입니다.
        private Rigidbody2D body;
        // 플레이어 입력을 읽을 컴포넌트를 저장하는 변수입니다.
        private PlayerInputReader input;

        /// <summary>이동 처리에 필요한 컴포넌트 참조를 가져옵니다.</summary>
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInputReader>();
        }

        /// <summary>고정 물리 프레임마다 수평 속도와 바라보는 방향을 갱신합니다.</summary>
        private void FixedUpdate()
        {
            float targetSpeed = input.Move * maxSpeed; // 이동 입력으로 계산한 목표 수평 속도입니다.
            bool hasMoveInput = Mathf.Abs(input.Move) > 0.01f; // 의미 있는 수평 이동 입력이 있는지 여부입니다.
            float speedChangeRate = deceleration; // 현재 프레임에 사용할 속도 변화량입니다.
            if (hasMoveInput == true)
            {
                speedChangeRate = acceleration;
            }
            float nextHorizontalSpeed = Mathf.MoveTowards(
                body.linearVelocity.x,
                targetSpeed,
                speedChangeRate * Time.fixedDeltaTime); // 목표 속도를 향해 가속 또는 감속한 다음 수평 속도입니다.
            body.linearVelocity = new Vector2(
                nextHorizontalSpeed,
                body.linearVelocity.y);

            if (hasMoveInput == true)
            {
                transform.localScale = new Vector3(Mathf.Sign(input.Move), 1f, 1f);
            }
        }
    }
