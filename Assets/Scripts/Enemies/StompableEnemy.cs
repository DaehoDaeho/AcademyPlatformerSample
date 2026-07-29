using UnityEngine;

    /// <summary>플레이어의 밟기 공격을 받은 적의 상태와 제거 처리를 담당합니다.</summary>
    public sealed class StompableEnemy : MonoBehaviour
    {
        /// <summary>이 적이 이미 밟혔는지 여부를 제공합니다.</summary>
        public bool IsStomped { get; private set; }

        /// <summary>적을 밟힌 상태로 전환하고 게임 오브젝트를 비활성화합니다.</summary>
        /// <returns>처음 밟혀 처리되었는지 여부입니다.</returns>
        public bool Stomp()
        {
            // 이미 밟힌 적은 중복 처리하지 않습니다.
            if (IsStomped == true)
            {
                return false;
            }
            IsStomped = true;
            gameObject.SetActive(false);
            return true;
        }
    }
