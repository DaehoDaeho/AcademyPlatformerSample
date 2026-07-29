using UnityEngine;

    /// <summary>플레이어가 목표 지점에 도착했을 때 스테이지 승리를 요청합니다.</summary>
    public sealed class Goal : MonoBehaviour
    {
        /// <summary>트리거에 들어온 충돌체가 플레이어인지 확인해 승리를 처리합니다.</summary>
        /// <param name="other">목표 지점에 들어온 충돌체입니다.</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 플레이어가 목표 지점에 도착했을 때만 승리 처리합니다.
            if (other.CompareTag("Player") == false)
            {
                return;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Win();
            }
        }
    }
