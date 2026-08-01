using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 레벨 클리어 시 플레이어 조작을 멈추고 승리 점프와 금빛 점멸 연출을 재생합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerClearSequence : MonoBehaviour
{
    /// <summary>
    /// 클리어 연출 전체 재생 시간입니다.
    /// </summary>
    [SerializeField, Min(0.2f)] private float sequenceDuration = 1.3f;

    /// <summary>
    /// 승리 점프가 올라가는 최대 높이입니다.
    /// </summary>
    [SerializeField, Min(0f)] private float jumpHeight = 0.8f;

    /// <summary>
    /// 승리 점프와 위치 고정에 사용할 플레이어 물리 본체입니다.
    /// </summary>
    [SerializeField] private Rigidbody2D body;

    /// <summary>
    /// 금빛 점멸을 적용할 캐릭터 렌더러입니다.
    /// </summary>
    [SerializeField] private SpriteRenderer visualRenderer;

    /// <summary>
    /// 클리어 중 비활성화할 플레이어 입력 컴포넌트입니다.
    /// </summary>
    [SerializeField] private PlayerInputReader inputReader;

    /// <summary>
    /// 클리어 중 비활성화할 플레이어 이동 컴포넌트입니다.
    /// </summary>
    [SerializeField] private PlayerMotor2D motor;

    /// <summary>
    /// 클리어 중 비활성화할 플레이어 점프 컴포넌트입니다.
    /// </summary>
    [SerializeField] private PlayerJump jump;

    /// <summary>
    /// 클리어 연출 중 피해를 차단할 플레이어 체력 컴포넌트입니다.
    /// </summary>
    [SerializeField] private Health health;

    /// <summary>
    /// 클리어 연출이 이미 시작됐는지를 나타냅니다.
    /// </summary>
    public bool IsPlaying { get; private set; }

    /// <summary>
    /// 클리어 연출이 끝난 뒤 호출되는 이벤트입니다.
    /// </summary>
    public event Action Completed;

    /// <summary>
    /// 생성 코드에서 클리어 연출에 필요한 플레이어 컴포넌트를 설정합니다.
    /// </summary>
    /// <param name="targetBody">위치 연출에 사용할 Rigidbody2D입니다.</param>
    /// <param name="targetRenderer">색상 연출에 사용할 SpriteRenderer입니다.</param>
    /// <param name="targetInputReader">비활성화할 입력 컴포넌트입니다.</param>
    /// <param name="targetMotor">비활성화할 이동 컴포넌트입니다.</param>
    /// <param name="targetJump">비활성화할 점프 컴포넌트입니다.</param>
    /// <param name="targetHealth">피해를 차단할 체력 컴포넌트입니다.</param>
    public void Configure(
        Rigidbody2D targetBody,
        SpriteRenderer targetRenderer,
        PlayerInputReader targetInputReader,
        PlayerMotor2D targetMotor,
        PlayerJump targetJump,
        Health targetHealth)
    {
        body = targetBody;
        visualRenderer = targetRenderer;
        inputReader = targetInputReader;
        motor = targetMotor;
        jump = targetJump;
        health = targetHealth;
    }

    /// <summary>
    /// 누락된 Rigidbody2D 참조를 자동으로 가져옵니다.
    /// </summary>
    private void Awake()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// 클리어 연출이 중복 실행되지 않도록 확인한 뒤 코루틴을 시작합니다.
    /// </summary>
    public void Play()
    {
        if (IsPlaying == true)
        {
            return;
        }

        IsPlaying = true;
        DisablePlayerControl();
        StartCoroutine(PlaySequence());
    }

    /// <summary>
    /// 플레이어 입력과 물리 이동을 멈추고 연출 중 피해를 차단합니다.
    /// </summary>
    private void DisablePlayerControl()
    {
        if (inputReader != null)
        {
            inputReader.enabled = false;
        }

        if (motor != null)
        {
            motor.enabled = false;
        }

        if (jump != null)
        {
            jump.enabled = false;
        }

        if (health != null)
        {
            health.SetDamageEnabled(false);
        }

        body.linearVelocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic;
    }

    /// <summary>
    /// 포물선 형태의 승리 점프와 금빛 색상 점멸을 재생합니다.
    /// </summary>
    /// <returns>프레임마다 연출을 진행하는 코루틴 열거자를 반환합니다.</returns>
    private IEnumerator PlaySequence()
    {
        // 클리어 연출이 시작된 플레이어 위치입니다.
        Vector3 startPosition = transform.position;

        // 클리어 연출이 시작된 뒤 흐른 시간입니다.
        float elapsedTime = 0f;

        while (elapsedTime < sequenceDuration)
        {
            elapsedTime += Time.deltaTime;

            // 전체 클리어 연출의 진행 비율입니다.
            float sequenceProgress = Mathf.Clamp01(elapsedTime / sequenceDuration);

            // 시작과 끝은 0이고 중간 지점에서 1이 되는 승리 점프 높이 비율입니다.
            float jumpProgress = Mathf.Sin(sequenceProgress * Mathf.PI);

            transform.position = startPosition + Vector3.up * jumpProgress * jumpHeight;
            ApplyCelebrationColor(elapsedTime);
            yield return null;
        }

        transform.position = startPosition;

        if (visualRenderer != null)
        {
            visualRenderer.color = Color.white;
            visualRenderer.enabled = false;
        }

        Completed?.Invoke();
    }

    /// <summary>
    /// 시간에 따라 흰색과 금빛 사이를 반복하는 색상을 캐릭터에 적용합니다.
    /// </summary>
    /// <param name="elapsedTime">클리어 연출이 시작된 뒤 흐른 시간입니다.</param>
    private void ApplyCelebrationColor(float elapsedTime)
    {
        if (visualRenderer == null)
        {
            return;
        }

        // 흰색과 금빛 사이를 반복해서 왕복하는 색상 혼합 비율입니다.
        float colorBlend = Mathf.PingPong(elapsedTime * 5f, 1f);

        visualRenderer.color = Color.Lerp(
            Color.white,
            new Color(1f, 0.82f, 0.2f, 1f),
            colorBlend);
    }
}
