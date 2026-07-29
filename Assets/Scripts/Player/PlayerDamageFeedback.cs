using System.Collections;
using UnityEngine;

    /// <summary>
    /// 확정된 피해를 넉백과 무적 시간 깜박임으로 표현합니다.
    /// 피해 규칙은 체력 컴포넌트가 관리하고 이 컴포넌트는 시각적 반응만 담당합니다.
    /// </summary>
    [RequireComponent(typeof(Health), typeof(Rigidbody2D))]
    public sealed class PlayerDamageFeedback : MonoBehaviour
    {
        // 넉백 속도를 적용할 Rigidbody2D를 저장하는 변수입니다.
        [SerializeField] private Rigidbody2D body;
        // 깜박임에 사용할 스프라이트 렌더러 배열을 저장하는 변수입니다.
        [SerializeField] private SpriteRenderer[] renderers;
        // 수평 및 수직 넉백 속도를 저장하는 변수입니다.
        [SerializeField] private Vector2 knockbackVelocity = new(8f, 7f);
        // 깜박임 상태가 전환되는 시간 간격을 저장하는 변수입니다.
        [SerializeField, Min(0.02f)] private float blinkInterval = 0.08f;

        // 피해 이벤트를 제공할 체력 컴포넌트를 저장하는 변수입니다.
        private Health health;
        // 실행 중인 깜박임 코루틴을 저장하는 변수입니다.
        private Coroutine blinkRoutine;

        /// <summary>넉백 대상과 깜박임 대상 렌더러를 설정합니다.</summary>
        /// <param name="targetBody">넉백을 적용할 Rigidbody2D입니다.</param>
        /// <param name="targetRenderers">깜박임에 사용할 렌더러 배열입니다.</param>
        public void Configure(Rigidbody2D targetBody, params SpriteRenderer[] targetRenderers)
        {
            body = targetBody;
            renderers = targetRenderers;
        }

        /// <summary>피격 반응에 필요한 컴포넌트 참조를 가져옵니다.</summary>
        private void Awake()
        {
            health = GetComponent<Health>();
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<SpriteRenderer>();
            }
        }

        /// <summary>체력의 피해 이벤트에 피격 반응 함수를 등록합니다.</summary>
        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }
            health.Damaged += OnDamaged;
        }

        /// <summary>체력의 피해 이벤트에서 피격 반응 함수를 해제합니다.</summary>
        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
            }
            SetVisible(true);
        }

        /// <summary>피해 방향에 따른 넉백과 무적 시간 깜박임을 시작합니다.</summary>
        /// <param name="sourcePosition">피해가 발생한 월드 위치입니다.</param>
        /// <param name="invulnerabilityDuration">피해 후 무적 시간입니다.</param>
        private void OnDamaged(Vector2 sourcePosition, float invulnerabilityDuration)
        {
            float horizontalDirection = -1f; // 피해 원점에서 멀어지는 수평 넉백 방향입니다.
            if (transform.position.x >= sourcePosition.x)
            {
                horizontalDirection = 1f;
            }
            body.linearVelocity = new Vector2(
                horizontalDirection * knockbackVelocity.x,
                knockbackVelocity.y);

            if (blinkRoutine != null)
            {
                StopCoroutine(blinkRoutine);
            }
            blinkRoutine = StartCoroutine(Blink(invulnerabilityDuration));
        }

        /// <summary>지정된 시간 동안 스프라이트 표시 상태를 반복 전환합니다.</summary>
        /// <param name="duration">깜박임을 유지할 시간입니다.</param>
        /// <returns>코루틴 실행을 위한 열거자입니다.</returns>
        private IEnumerator Blink(float duration)
        {
            float elapsed = 0f; // 깜박임을 시작한 뒤 현재까지 흐른 시간입니다.
            while (elapsed < duration)
            {
                SetVisible(false);
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
                SetVisible(true);
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }
            SetVisible(true);
            blinkRoutine = null;
        }

        /// <summary>모든 플레이어 스프라이트의 표시 상태를 설정합니다.</summary>
        /// <param name="visible">스프라이트 표시 여부입니다.</param>
        private void SetVisible(bool visible)
        {
            if (renderers == null)
            {
                return;
            }
            foreach (SpriteRenderer spriteRenderer in renderers)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = visible;
                }
            }
        }
    }
