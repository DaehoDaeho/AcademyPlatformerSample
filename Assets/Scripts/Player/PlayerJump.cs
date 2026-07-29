using System;
using UnityEngine;

    /// <summary>코요테 타임, 입력 버퍼와 가변 높이를 포함한 점프를 처리합니다.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputReader))]
    public sealed class PlayerJump : MonoBehaviour
    {
        /// <summary>점프가 실제로 시작될 때 호출되는 이벤트입니다.</summary>
        public event Action Jumped;

        // 플레이어의 지면 상태를 확인할 센서를 저장하는 변수입니다.
        [SerializeField] private GroundSensor groundSensor;
        // 점프 시작 시 적용할 수직 속도를 저장하는 변수입니다.
        [SerializeField, Min(0f)] private float jumpSpeed = 15f;
        // 지면을 떠난 뒤 점프를 허용하는 시간을 저장하는 변수입니다.
        [SerializeField, Min(0f)] private float coyoteTime = 0.12f;
        // 점프 입력을 미리 기억하는 시간을 저장하는 변수입니다.
        [SerializeField, Min(0f)] private float inputBuffer = 0.12f;
        // 점프 키를 놓았을 때 적용할 상승 속도 비율을 저장하는 변수입니다.
        [SerializeField, Range(0.1f, 1f)] private float jumpCut = 0.45f;

        // 플레이어의 물리 속도를 제어할 Rigidbody2D를 저장하는 변수입니다.
        private Rigidbody2D body;
        // 점프 입력을 읽을 컴포넌트를 저장하는 변수입니다.
        private PlayerInputReader input;
        // 마지막으로 지면에 닿아 있던 시각을 저장하는 변수입니다.
        private float lastGrounded = float.NegativeInfinity;
        // 마지막으로 점프 키를 누른 시각을 저장하는 변수입니다.
        private float lastJumpPressed = float.NegativeInfinity;

        /// <summary>현재 설정된 점프 속도를 제공합니다.</summary>
        public float JumpSpeed => jumpSpeed;

        /// <summary>점프 처리에 필요한 컴포넌트 참조를 가져옵니다.</summary>
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInputReader>();
        }

        /// <summary>점프가 사용할 지면 센서를 설정합니다.</summary>
        /// <param name="sensor">지면 상태를 제공할 센서입니다.</param>
        public void Configure(GroundSensor sensor)
        {
            groundSensor = sensor;
        }

        /// <summary>매 프레임 점프 입력과 점프 상태를 갱신합니다.</summary>
        private void Update()
        {
            // 지면에 닿아 있으면 코요테 타임 기준 시각을 갱신합니다.
            if (groundSensor != null && groundSensor.IsGrounded == true)
            {
                lastGrounded = Time.time;
            }
            // 점프 입력이 들어오면 입력 버퍼 기준 시각을 갱신합니다.
            if (input.JumpPressed == true)
            {
                lastJumpPressed = Time.time;
            }

            if (CanStartJump() == true)
            {
                StartJump();
            }

            // 점프 키를 놓으면 상승 속도를 줄여 점프 높이를 조절합니다.
            if (input.JumpHeld == false && body.linearVelocity.y > 0f)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y * jumpCut);
            }
        }

        /// <summary>코요테 타임과 입력 버퍼가 모두 유효해 지금 점프할 수 있는지 확인합니다.</summary>
        /// <returns>현재 프레임에 점프를 시작할 수 있는지 여부입니다.</returns>
        private bool CanStartJump()
        {
            float timeSinceGrounded = Time.time - lastGrounded; // 마지막 지면 접촉 이후 흐른 시간입니다.
            float timeSinceJumpPressed = Time.time - lastJumpPressed; // 마지막 점프 입력 이후 흐른 시간입니다.
            bool coyoteTimeValid = timeSinceGrounded <= coyoteTime; // 코요테 타임이 아직 남아 있는지 여부입니다.
            bool inputBufferValid = timeSinceJumpPressed <= inputBuffer; // 점프 입력 버퍼가 아직 남아 있는지 여부입니다.
            return coyoteTimeValid == true && inputBufferValid == true;
        }

        /// <summary>수직 속도를 적용하고 사용한 타이밍 기록을 초기화한 뒤 점프 이벤트를 알립니다.</summary>
        private void StartJump()
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpSpeed);
            lastGrounded = float.NegativeInfinity;
            lastJumpPressed = float.NegativeInfinity;
            Jumped?.Invoke();
        }
    }
