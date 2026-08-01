using UnityEngine;

    /// <summary>
    /// 배경 레이어 하나의 화면상 스크롤 속도를 제어합니다.
    /// 먼 레이어는 작은 이동 계수를 사용하고 가까운 레이어는 큰 이동 계수를 사용합니다.
    /// </summary>
    public sealed class ParallaxLayer : MonoBehaviour
    {
        // 패럴랙스 이동의 기준이 되는 카메라를 저장하는 변수입니다.
        [SerializeField] private Transform cameraTransform;
        // 카메라 이동량에 적용할 패럴랙스 계수를 저장하는 변수입니다.
        [SerializeField, Range(0f, 1f)] private float movementFactor = 0.2f;
        // 세로 방향에서 카메라와 같은 거리만큼 이동할지 저장하는 변수입니다.
        [SerializeField, Range(0f, 1f)] private float verticalFollowFactor = 1f;
        // 이전 씬 데이터와의 호환을 위해 남겨 둔 세로 고정 추적 값입니다.
        private bool followVertical;

        /// <summary>현재 패럴랙스 이동 계수를 제공합니다.</summary>
        public float MovementFactor => movementFactor;
        // 시작 시점의 카메라 위치를 저장하는 변수입니다.
        private Vector3 cameraStart;
        // 시작 시점의 레이어 위치를 저장하는 변수입니다.
        private Vector3 layerStart;

        /// <summary>기준 카메라와 수평 이동 계수 및 세로 고정 추적 여부를 설정합니다.</summary>
        /// <param name="targetCamera">기준으로 사용할 카메라 Transform입니다.</param>
        /// <param name="factor">카메라의 수평 이동량에 적용할 패럴랙스 계수입니다.</param>
        /// <param name="vertical">배경을 카메라와 같은 세로 이동량으로 추적할지 여부입니다.</param>
        public void Configure(
            Transform targetCamera,
            float factor,
            float verticalFactor = 1f)
        {
            cameraTransform = targetCamera;
            movementFactor = Mathf.Clamp01(factor);
            verticalFollowFactor = Mathf.Clamp01(verticalFactor);
        }

        /// <summary>카메라와 레이어의 시작 위치를 기록합니다.</summary>
        private void Start()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            if (cameraTransform == null)
            {
                return;
            }
            cameraStart = cameraTransform.position;
            layerStart = transform.position;
        }

        /// <summary>카메라 이동량에 따라 레이어 위치를 갱신합니다.</summary>
        private void LateUpdate()
        {
            if (cameraTransform == null)
            {
                return;
            }
            Vector3 cameraDelta = cameraTransform.position - cameraStart; // 게임 시작 후 카메라가 이동한 전체 거리입니다.
            // A layer that follows the camera completely appears stationary on screen.
            // Therefore the transform follows (1 - factor), leaving "factor" as the
            // visible scroll amount: far 0.05, middle 0.20, near 0.40.
            float followFactor = 1f - movementFactor; // 화면에서 원하는 패럴랙스 속도를 만들기 위한 실제 Transform 추적 비율입니다.
            float y = layerStart.y; // 배경 레이어에 적용할 세로 위치입니다.
            if (followVertical == true)
            {
                // Y축에는 패럴랙스 비율을 적용하지 않고 카메라와 같은 거리만큼 이동합니다.
                y = layerStart.y + cameraDelta.y;
            }
            y = layerStart.y + cameraDelta.y * verticalFollowFactor;
            transform.position = new Vector3(
                layerStart.x + cameraDelta.x * followFactor,
                y,
                layerStart.z);
        }
    }
