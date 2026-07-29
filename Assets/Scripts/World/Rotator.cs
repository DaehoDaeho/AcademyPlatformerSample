using UnityEngine;

    /// <summary>수집 아이템을 일정한 속도로 회전시킵니다.</summary>
    public sealed class Rotator : MonoBehaviour
    {
        // 초당 회전 각도를 저장하는 변수입니다.
        [SerializeField] private float degreesPerSecond = 90f;

        /// <summary>매 프레임 오브젝트를 설정된 속도로 회전시킵니다.</summary>
        private void Update()
        {
            float rotationAmount =
                degreesPerSecond * Time.deltaTime; // 현재 프레임에 회전할 각도입니다.
            transform.Rotate(0f, 0f, rotationAmount);
        }
    }
