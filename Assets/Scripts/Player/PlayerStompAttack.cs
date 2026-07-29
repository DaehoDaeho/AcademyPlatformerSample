using System;
using UnityEngine;

    /// <summary>적 윗면 밟기 판정과 성공 시 플레이어 반동을 처리합니다.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerStompAttack : MonoBehaviour
    {
        // 밟기 성공 후 적용할 반동 속도를 저장하는 변수입니다.
        [SerializeField, Min(0f)] private float bounceSpeed = 9f;
        // 밟기 판정에 필요한 최소 높이 차이를 저장하는 변수입니다.
        [SerializeField, Min(0f)] private float minimumHeightAdvantage = 0.35f;
        // 밟기 판정에 필요한 최소 낙하 속도를 저장하는 변수입니다.
        [SerializeField, Min(0f)] private float minimumFallSpeed = 0.1f;

        // 플레이어의 반동 속도를 제어할 Rigidbody2D를 저장하는 변수입니다.
        private Rigidbody2D body;
        /// <summary>성공한 밟기 공격 횟수를 제공합니다.</summary>
        public int SuccessfulStomps { get; private set; }
        /// <summary>밟기 공격이 성공할 때 호출되는 이벤트입니다.</summary>
        public event Action Stomped;

        /// <summary>필요한 Rigidbody2D 참조를 가져옵니다.</summary>
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        /// <summary>현재 상태에서 지정한 대상을 밟을 수 있는지 확인합니다.</summary>
        /// <param name="target">밟기 가능 여부를 확인할 대상입니다.</param>
        /// <returns>밟기 가능 여부입니다.</returns>
        public bool CanStomp(Transform target)
        {
            if (target == null)
            {
                return false;
            }
            bool fallingFastEnough =
                body.linearVelocity.y <= -minimumFallSpeed; // 플레이어가 밟기 판정에 필요한 속도로 낙하 중인지 여부입니다.
            float requiredPlayerHeight =
                target.position.y + minimumHeightAdvantage; // 밟기 판정을 위해 필요한 플레이어의 최소 높이입니다.
            bool playerAboveTarget =
                transform.position.y >= requiredPlayerHeight; // 플레이어가 적보다 충분히 위에 있는지 여부입니다.
            return fallingFastEnough == true && playerAboveTarget == true;
        }

        /// <summary>지정한 적에게 밟기 공격을 시도합니다.</summary>
        /// <param name="target">밟기 공격을 적용할 적입니다.</param>
        /// <returns>밟기 공격 성공 여부입니다.</returns>
        public bool TryStomp(StompableEnemy target)
        {
            if (target == null)
            {
                return false;
            }
            if (CanStomp(target.transform) == false)
            {
                return false;
            }
            return ExecuteStomp(target);
        }

        /// <summary>충돌 접촉면과 상대 높이를 확인해 밟기 공격을 처리합니다.</summary>
        /// <param name="collision">발생한 2D 충돌 정보입니다.</param>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            StompableEnemy target = collision.collider.GetComponentInParent<StompableEnemy>(); // 충돌한 오브젝트에서 찾은 밟기 가능한 적입니다.
            if (target == null)
            {
                return;
            }
            bool touchedEnemyTop = HasTopContact(collision); // 충돌 접촉면에 적의 윗면이 포함되었는지 여부입니다.
            bool playerAboveEnemy =
                transform.position.y > target.transform.position.y; // 플레이어 중심이 적 중심보다 위에 있는지 여부입니다.
            if (touchedEnemyTop == false || playerAboveEnemy == false)
            {
                return;
            }
            ExecuteStomp(target);
        }

        /// <summary>적을 밟힌 상태로 만들고 플레이어에게 반동을 적용합니다.</summary>
        /// <param name="target">밟기 공격을 적용할 적입니다.</param>
        /// <returns>밟기 처리 성공 여부입니다.</returns>
        private bool ExecuteStomp(StompableEnemy target)
        {
            if (target == null || target.Stomp() == false)
            {
                return false;
            }
            SuccessfulStomps++;
            body.linearVelocity = new Vector2(body.linearVelocity.x, bounceSpeed);
            Stomped?.Invoke();
            return true;
        }

        /// <summary>충돌 정보에 적 윗면 접촉이 포함되어 있는지 확인합니다.</summary>
        /// <param name="collision">검사할 2D 충돌 정보입니다.</param>
        /// <returns>적 윗면 접촉 여부입니다.</returns>
        public static bool HasTopContact(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y > 0.5f)
                {
                    return true;
                }
            }
            return false;
        }
    }
