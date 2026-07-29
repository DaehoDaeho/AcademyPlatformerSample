using UnityEngine;

    /// <summary>플레이어를 부드럽게 추적하며 카메라의 수평 이동 범위를 제한합니다.</summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        // 카메라가 추적할 대상을 저장하는 변수입니다.
        [SerializeField] private Transform target;
        // 추적 대상과 카메라 사이의 위치 차이를 저장하는 변수입니다.
        [SerializeField] private Vector2 offset = new(2f, 1f);
        // 카메라 이동의 부드러움을 저장하는 변수입니다.
        [SerializeField, Min(0.01f)] private float smoothTime = 0.18f;
        // 카메라의 최소 수평 좌표를 저장하는 변수입니다.
        [SerializeField] private float minX = 0f;
        // 카메라의 최대 수평 좌표를 저장하는 변수입니다.
        [SerializeField] private float maxX = 24f;
        // 부드러운 이동 계산에 사용하는 현재 속도를 저장하는 변수입니다.
        private Vector3 velocity;

        /// <summary>카메라 추적 대상과 수평 이동 범위를 설정합니다.</summary>
        /// <param name="followTarget">카메라가 추적할 대상입니다.</param>
        /// <param name="minimumX">카메라의 최소 수평 좌표입니다.</param>
        /// <param name="maximumX">카메라의 최대 수평 좌표입니다.</param>
        public void Configure(Transform followTarget, float minimumX, float maximumX)
        {
            target = followTarget;
            minX = minimumX;
            maxX = maximumX;
        }

        /// <summary>모든 이동 처리가 끝난 뒤 카메라 위치를 부드럽게 갱신합니다.</summary>
        private void LateUpdate()
        {
            // 추적 대상이 없으면 카메라를 이동하지 않습니다.
            if (target == null)
            {
                return;
            }
            float desiredX = Mathf.Clamp(
                target.position.x + offset.x,
                minX,
                maxX); // 수평 제한 범위 안에서 계산한 목표 X 좌표입니다.
            float desiredY =
                target.position.y + offset.y; // 플레이어 높이와 카메라 간격을 반영한 목표 Y 좌표입니다.
            Vector3 desiredPosition = new(
                desiredX,
                desiredY,
                transform.position.z); // 카메라가 부드럽게 이동할 최종 목표 위치입니다.
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                smoothTime);
        }
    }
