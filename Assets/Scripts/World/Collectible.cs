using UnityEngine;

    /// <summary>플레이어가 닿은 수집 아이템의 점수와 효과음을 처리합니다.</summary>
    public sealed class Collectible : MonoBehaviour
    {
        // 아이템 하나가 제공하는 점수를 저장하는 변수입니다.
        [SerializeField, Min(1)] private int value = 1;
        // Star가 원래 위치를 기준으로 위아래로 움직일 거리입니다.
        [SerializeField, Min(0f)] private float floatingHeight = 0.12f;
        // Star가 위아래로 움직이는 속도입니다.
        [SerializeField, Min(0f)] private float floatingSpeed = 2.2f;
        // 위아래 움직임을 계산할 Star의 기준 월드 위치입니다.
        private Vector3 startingPosition;

        /// <summary>게임 시작 시 Star가 배치된 기준 위치를 저장합니다.</summary>
        private void Start()
        {
            startingPosition = transform.position;
        }

        /// <summary>Star가 수집 아이템임을 쉽게 알아볼 수 있도록 위아래로 부드럽게 움직입니다.</summary>
        private void Update()
        {
            float verticalOffset =
                Mathf.Sin(Time.time * floatingSpeed) * floatingHeight;
            transform.position = startingPosition + Vector3.up * verticalOffset;
        }

        /// <summary>플레이어가 닿았을 때 효과음과 점수를 처리한 뒤 아이템을 제거합니다.</summary>
        /// <param name="other">아이템 트리거에 들어온 충돌체입니다.</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 플레이어가 아닌 충돌체는 수집 처리에서 제외합니다.
            if (other.CompareTag("Player") == false)
            {
                return;
            }
            PlayerAudioFeedback audioFeedback =
                other.GetComponent<PlayerAudioFeedback>(); // 수집 효과음을 담당하는 플레이어 컴포넌트입니다.
            if (audioFeedback != null)
            {
                audioFeedback.PlayCollectible();
            }
            PlayerVfxFeedback vfxFeedback =
                other.GetComponent<PlayerVfxFeedback>(); // Star 획득 파티클을 생성할 플레이어 시각 효과 컴포넌트입니다.
            if (vfxFeedback != null)
            {
                vfxFeedback.PlayCollectibleEffect(transform.position);
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(value);
            }
            Destroy(gameObject);
        }
    }
