using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 사망 직후 입력 차단, 반동, 붉은 깜박임과 투명도 감소 연출을 재생합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerDeathSequence : MonoBehaviour
{
    /// <summary>
    /// 사망 연출 전체 재생 시간입니다.
    /// </summary>
    [SerializeField, Min(0.2f)] private float sequenceDuration = 2.2f;

    /// <summary>
    /// 사망할 때 플레이어를 위로 튕겨 올리는 속도입니다.
    /// </summary>
    [SerializeField, Min(0f)] private float upwardBounceSpeed = 6.5f;

    /// <summary>
    /// 붉은색과 흰색을 전환하는 시간 간격입니다.
    /// </summary>
    [SerializeField, Min(0.05f)] private float flashInterval = 0.12f;

    /// <summary>
    /// 연출 중 투명도 감소가 시작되는 시점입니다.
    /// </summary>
    [SerializeField, Min(0f)] private float fadeStartTime = 1.15f;

    /// <summary>
    /// 사망 반동을 적용할 플레이어 물리 본체입니다.
    /// </summary>
    [SerializeField] private Rigidbody2D body;

    /// <summary>
    /// 붉은 깜박임과 투명도 변화를 적용할 캐릭터 렌더러입니다.
    /// </summary>
    [SerializeField] private SpriteRenderer visualRenderer;

    /// <summary>
    /// 사망 시 비활성화할 플레이어 입력 컴포넌트입니다.
    /// </summary>
    [SerializeField] private PlayerInputReader inputReader;

    /// <summary>
    /// 사망 시 비활성화할 플레이어 이동 컴포넌트입니다.
    /// </summary>
    [SerializeField] private PlayerMotor2D motor;

    /// <summary>
    /// 사망 시 비활성화할 플레이어 점프 컴포넌트입니다.
    /// </summary>
    [SerializeField] private PlayerJump jump;

    /// <summary>
    /// 사망 연출과 겹치지 않도록 중단할 피격 시각 효과 컴포넌트입니다.
    /// </summary>
    [SerializeField] private PlayerDamageFeedback damageFeedback;

    /// <summary>
    /// 사망 연출이 이미 시작됐는지를 나타냅니다.
    /// </summary>
    public bool IsPlaying { get; private set; }

    /// <summary>
    /// 사망 연출이 끝난 뒤 호출되는 이벤트입니다.
    /// </summary>
    public event Action Completed;

    /// <summary>
    /// 생성 코드에서 사망 연출에 필요한 플레이어 컴포넌트를 설정합니다.
    /// </summary>
    /// <param name="targetBody">사망 반동을 적용할 Rigidbody2D입니다.</param>
    /// <param name="targetRenderer">시각 효과를 적용할 SpriteRenderer입니다.</param>
    /// <param name="targetInputReader">비활성화할 입력 컴포넌트입니다.</param>
    /// <param name="targetMotor">비활성화할 이동 컴포넌트입니다.</param>
    /// <param name="targetJump">비활성화할 점프 컴포넌트입니다.</param>
    /// <param name="targetDamageFeedback">중단할 피격 효과 컴포넌트입니다.</param>
    public void Configure(
        Rigidbody2D targetBody,
        SpriteRenderer targetRenderer,
        PlayerInputReader targetInputReader,
        PlayerMotor2D targetMotor,
        PlayerJump targetJump,
        PlayerDamageFeedback targetDamageFeedback)
    {
        body = targetBody;
        visualRenderer = targetRenderer;
        inputReader = targetInputReader;
        motor = targetMotor;
        jump = targetJump;
        damageFeedback = targetDamageFeedback;
    }

    /// <summary>사망 연출의 전체 시간과 캐릭터가 사라지기 시작하는 시점을 설정합니다.</summary>
    /// <param name="duration">게임오버 화면 전까지 재생할 전체 시간입니다.</param>
    /// <param name="fadeStart">캐릭터의 투명도 감소가 시작되는 시간입니다.</param>
    public void ConfigureTiming(float duration, float fadeStart)
    {
        sequenceDuration = Mathf.Max(0.2f, duration);
        fadeStartTime = Mathf.Clamp(fadeStart, 0f, sequenceDuration);
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
    /// 사망 연출이 중복 실행되지 않도록 확인하고 연출 코루틴을 시작합니다.
    /// </summary>
    public void Play()
    {
        if (IsPlaying == true)
        {
            return;
        }

        IsPlaying = true;
        DisablePlayerControl();
        PrepareVisual();
        KeepFallenPlayerVisible();
        body.linearVelocity = new Vector2(0f, upwardBounceSpeed);
        StartCoroutine(PlaySequence());
    }

    /// <summary>
    /// 플레이어의 입력, 이동과 점프 기능을 비활성화합니다.
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
    }

    /// <summary>
    /// 기존 피격 깜박임을 중단하고 사망 연출을 위한 렌더러 상태를 준비합니다.
    /// </summary>
    private void PrepareVisual()
    {
        if (damageFeedback != null)
        {
            damageFeedback.StopAllCoroutines();
            damageFeedback.enabled = false;
        }

        if (visualRenderer != null)
        {
            visualRenderer.enabled = true;
            visualRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// 추락 사망 시에도 연출이 보이도록 플레이어를 카메라 화면 아래쪽으로 옮깁니다.
    /// </summary>
    private void KeepFallenPlayerVisible()
    {
        // 현재 플레이어를 촬영하는 메인 카메라입니다.
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        // 카메라 화면 안에서 사망 연출이 보일 수 있는 최소 높이입니다.
        float minimumVisibleHeight =
            mainCamera.transform.position.y - mainCamera.orthographicSize + 1.2f;

        if (transform.position.y < minimumVisibleHeight)
        {
            transform.position = new Vector3(
                transform.position.x,
                minimumVisibleHeight,
                transform.position.z);
        }
    }

    /// <summary>
    /// 일정 시간 동안 붉은 깜박임과 투명도 감소를 적용한 뒤 완료 이벤트를 보냅니다.
    /// </summary>
    /// <returns>프레임마다 연출을 진행하는 코루틴 열거자를 반환합니다.</returns>
    private IEnumerator PlaySequence()
    {
        // 사망 연출이 시작된 뒤 흐른 시간입니다.
        float elapsedTime = 0f;

        // 다음 색상 전환이 일어날 시간입니다.
        float nextFlashTime = 0f;

        // 현재 붉은색을 표시하고 있는지를 나타냅니다.
        bool showingRed = false;

        while (elapsedTime < sequenceDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            if (elapsedTime >= nextFlashTime)
            {
                showingRed = showingRed == false;
                nextFlashTime += flashInterval;
            }

            ApplyVisualColor(elapsedTime, showingRed);
            yield return null;
        }

        if (visualRenderer != null)
        {
            visualRenderer.color = new Color(1f, 1f, 1f, 0f);
        }

        Completed?.Invoke();
    }

    /// <summary>
    /// 현재 연출 시간에 맞는 깜박임 색상과 투명도를 캐릭터에 적용합니다.
    /// </summary>
    /// <param name="elapsedTime">사망 연출이 시작된 뒤 흐른 시간입니다.</param>
    /// <param name="showingRed">현재 붉은색을 표시할지 여부입니다.</param>
    private void ApplyVisualColor(float elapsedTime, bool showingRed)
    {
        if (visualRenderer == null)
        {
            return;
        }

        // 깜박임 상태에 따라 선택한 기본 색상입니다.
        Color displayColor = Color.white;

        if (showingRed == true)
        {
            displayColor = new Color(1f, 0.25f, 0.2f, 1f);
        }

        // 투명도 감소 구간에서 사용할 진행 비율입니다.
        float fadeProgress = Mathf.InverseLerp(
            fadeStartTime,
            sequenceDuration,
            elapsedTime);

        displayColor.a = 1f - fadeProgress;
        visualRenderer.color = displayColor;
    }
}
