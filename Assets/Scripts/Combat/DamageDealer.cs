using UnityEngine;
    /// <summary>충돌 대상의 체력에 피해를 적용하고 밟기 공격 예외를 판정합니다.</summary>
    public sealed class DamageDealer : MonoBehaviour
    {
        // 충돌 한 번에 적용할 피해량을 저장하는 변수입니다.
        [SerializeField, Min(1)] private int damage = 1;
        /// <summary>이 오브젝트가 적용할 피해량을 설정합니다.</summary>
        /// <param name="amount">설정할 피해량입니다.</param>
        public void Configure(int amount)
        {
            damage = Mathf.Max(1, amount);
        }
        /// <summary>트리거에 들어온 대상에게 피해 적용을 시도합니다.</summary>
        /// <param name="other">트리거에 들어온 충돌체입니다.</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            TryApplyDamage(other);
        }
        /// <summary>일반 충돌의 접촉 방향을 확인한 뒤 피해 적용을 시도합니다.</summary>
        /// <param name="collision">발생한 2D 충돌 정보입니다.</param>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            PlayerStompAttack stompAttack = // 충돌 대상이 가진 플레이어 밟기 공격 컴포넌트입니다.
                collision.collider.GetComponentInParent<PlayerStompAttack>();
            bool playerCanStomp = stompAttack != null; // 충돌 대상이 밟기 공격 기능을 가진 플레이어인지 여부입니다.
            bool playerCenterAboveEnemy =
                collision.collider.transform.position.y > transform.position.y; // 플레이어 중심이 적 중심보다 위에 있는지 여부입니다.
            bool playerTouchedEnemyTop =
                HasPlayerAboveContact(collision); // 충돌 접촉면이 적의 윗면인지 여부입니다.
            if (playerCanStomp == true &&
                playerCenterAboveEnemy == true &&
                playerTouchedEnemyTop == true)
            {
                return;
            }
            TryApplyDamage(collision.collider);
        }

        /// <summary>플레이어가 적 윗면과 접촉했는지 확인합니다.</summary>
        /// <param name="collision">검사할 2D 충돌 정보입니다.</param>
        /// <returns>윗면 접촉 여부입니다.</returns>
        private static bool HasPlayerAboveContact(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y < -0.5f)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>밟기 공격 예외와 체력 컴포넌트를 확인해 피해를 적용합니다.</summary>
        /// <param name="other">피해 적용을 시도할 컴포넌트입니다.</param>
        /// <returns>피해 적용 성공 여부입니다.</returns>
        public bool TryApplyDamage(Component other)
        {
            StompableEnemy stompable = GetComponent<StompableEnemy>(); // 이 오브젝트가 가진 밟기 가능 적 컴포넌트입니다.
            bool enemyAlreadyDefeated = stompable != null &&
                stompable.IsStomped == true; // 이 피해 오브젝트가 이미 밟혀 비활성화된 적인지 여부입니다.
            if (enemyAlreadyDefeated == true)
            {
                return false;
            }
            PlayerStompAttack stompAttack = other.GetComponentInParent<PlayerStompAttack>(); // 피해 대상이 가진 밟기 공격 컴포넌트입니다.
            bool playerIsStomping = stompAttack != null &&
                stompAttack.CanStomp(transform) == true; // 대상 플레이어가 현재 밟기 조건을 만족하는지 여부입니다.
            if (playerIsStomping == true)
            {
                return false;
            }
            Health health = other.GetComponentInParent<Health>(); // 피해를 적용할 대상의 체력 컴포넌트입니다.
            if (health == null)
            {
                return false;
            }
            return health.TakeDamage(damage, transform.position);
        }
    }
