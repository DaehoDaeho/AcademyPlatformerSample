using System;
using UnityEngine;

    /// <summary>최대 체력, 현재 체력, 무적 시간과 사망 이벤트를 관리합니다.</summary>
    public sealed class Health : MonoBehaviour
    {
        // 최대 체력을 저장하는 변수입니다.
        [SerializeField, Min(1)] private int maxHealth = 3;
        // 피해 후 무적 시간을 저장하는 변수입니다.
        [SerializeField, Min(0f)] private float invulnerabilitySeconds = 0.8f;
        /// <summary>현재 체력을 제공합니다.</summary>
        public int Current { get; private set; }
        /// <summary>최대 체력을 제공합니다.</summary>
        public int Max => maxHealth;
        /// <summary>피해 후 무적 시간을 제공합니다.</summary>
        public float InvulnerabilitySeconds => invulnerabilitySeconds;
        /// <summary>사망 여부를 제공합니다.</summary>
        public bool IsDead => Current <= 0;
        /// <summary>체력이 변경될 때 호출되는 이벤트입니다.</summary>
        public event Action<int, int> Changed;
        /// <summary>피해가 확정될 때 호출되는 이벤트입니다.</summary>
        public event Action<Vector2, float> Damaged;
        /// <summary>체력이 0이 될 때 호출되는 이벤트입니다.</summary>
        public event Action Died;
        // 무적 상태가 끝나는 시각을 저장하는 변수입니다.
        private float invulnerableUntil;
        // 피해와 강제 사망 요청을 받아들일 수 있는지를 나타냅니다.
        private bool damageEnabled = true;

        /// <summary>외부 연출에서 플레이어의 피해 허용 상태를 변경합니다.</summary>
        /// <param name="enabled">피해를 허용하려면 true, 차단하려면 false입니다.</param>
        public void SetDamageEnabled(bool enabled)
        {
            damageEnabled = enabled;
        }

        /// <summary>게임 시작 시 현재 체력을 최대 체력으로 초기화합니다.</summary>
        private void Awake()
        {
            Current = maxHealth;
        }

        /// <summary>현재 위치를 피해 원점으로 사용해 피해를 적용합니다.</summary>
        /// <param name="amount">적용할 피해량입니다.</param>
        /// <returns>피해 적용 성공 여부입니다.</returns>
        public bool TakeDamage(int amount)
        {
            return TakeDamage(amount, transform.position);
        }

        /// <summary>피해량과 피해 원점을 받아 체력 감소를 처리합니다.</summary>
        /// <param name="amount">적용할 피해량입니다.</param>
        /// <param name="sourcePosition">피해가 발생한 월드 위치입니다.</param>
        /// <returns>피해 적용 성공 여부입니다.</returns>
        public bool TakeDamage(int amount, Vector2 sourcePosition)
        {
            bool invalidDamageAmount = amount <= 0; // 피해량이 유효하지 않은지 여부입니다.
            bool alreadyDead = IsDead == true; // 대상이 이미 사망했는지 여부입니다.
            bool currentlyInvulnerable =
                Time.time < invulnerableUntil; // 피해 후 무적 시간이 아직 남아 있는지 여부입니다.
            if (damageEnabled == false ||
                invalidDamageAmount == true ||
                alreadyDead == true ||
                currentlyInvulnerable == true)
            {
                return false;
            }
            Current = Mathf.Max(0, Current - amount);
            invulnerableUntil = Time.time + invulnerabilitySeconds;
            Changed?.Invoke(Current, maxHealth);
            Damaged?.Invoke(sourcePosition, invulnerabilitySeconds);
            if (IsDead == true)
            {
                Died?.Invoke();
            }
            return true;
        }

        /// <summary>무적 상태를 무시하고 현재 체력을 0으로 만들어 즉시 사망 처리합니다.</summary>
        /// <param name="sourcePosition">사망 원인이 발생한 월드 위치입니다.</param>
        /// <returns>새롭게 사망 처리되었는지 여부입니다.</returns>
        public bool ForceDeath(Vector2 sourcePosition)
        {
            if (damageEnabled == false || IsDead == true)
            {
                return false;
            }
            Current = 0;
            Changed?.Invoke(Current, maxHealth);
            Damaged?.Invoke(sourcePosition, 0f);
            Died?.Invoke();
            return true;
        }
    }
