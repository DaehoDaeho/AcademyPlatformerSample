using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>게임 종료 결과에 따라 폭죽 애니메이션과 승리 또는 실패 효과음을 재생합니다.</summary>
[RequireComponent(typeof(AudioSource))]
public sealed class GameEndPresentation : MonoBehaviour
{
    // 스테이지 클리어 시 재생할 팡파레 효과음입니다.
    [SerializeField] private AudioClip clearFanfareClip;
    // 게임 오버 시 재생할 처량한 실패 효과음입니다.
    [SerializeField] private AudioClip gameOverClip;
    // 폭죽 파티클 UI가 생성될 화면 전체 영역입니다.
    private RectTransform effectLayer;
    // 종료 효과음을 재생할 오디오 소스입니다.
    private AudioSource audioSource;
    // 현재 실행 중인 폭죽 코루틴입니다.
    private Coroutine fireworksRoutine;

    /// <summary>오디오 소스와 폭죽 표시 영역을 준비합니다.</summary>
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        CreateEffectLayer();
    }

    /// <summary>게임 관리자의 종료 이벤트에 결과 연출 함수를 등록합니다.</summary>
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameFinished += PlayResult;
        }
    }

    /// <summary>오브젝트가 제거될 때 게임 종료 이벤트 등록을 해제합니다.</summary>
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameFinished -= PlayResult;
        }
    }

    /// <summary>에디터 생성 코드에서 승리와 실패 효과음을 설정합니다.</summary>
    /// <param name="fanfareClip">클리어 팡파레 효과음입니다.</param>
    /// <param name="sadClip">게임 오버 실패 효과음입니다.</param>
    public void Configure(AudioClip fanfareClip, AudioClip sadClip)
    {
        clearFanfareClip = fanfareClip;
        gameOverClip = sadClip;
    }

    /// <summary>승리하면 팡파레와 폭죽을, 패배하면 실패 효과음을 재생합니다.</summary>
    /// <param name="won">스테이지 클리어 여부입니다.</param>
    private void PlayResult(bool won)
    {
        if (won == true)
        {
            PlayClip(clearFanfareClip);
            if (fireworksRoutine != null)
            {
                StopCoroutine(fireworksRoutine);
            }

            fireworksRoutine = StartCoroutine(PlayFireworks());
            return;
        }

        PlayClip(gameOverClip);
    }

    /// <summary>지정한 종료 효과음을 처음부터 한 번 재생합니다.</summary>
    /// <param name="clip">재생할 오디오 클립입니다.</param>
    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    /// <summary>HUD의 마지막 자식으로 화면 전체 폭죽 표시 영역을 생성합니다.</summary>
    private void CreateEffectLayer()
    {
        GameObject layerObject = new GameObject(
            "Clear Fireworks Layer",
            typeof(RectTransform));
        layerObject.transform.SetParent(transform, false);
        effectLayer = layerObject.GetComponent<RectTransform>();
        effectLayer.anchorMin = Vector2.zero;
        effectLayer.anchorMax = Vector2.one;
        effectLayer.offsetMin = Vector2.zero;
        effectLayer.offsetMax = Vector2.zero;
        effectLayer.SetAsLastSibling();
    }

    /// <summary>화면 전체의 서로 다른 높이에서 다채로운 폭죽 열두 발을 연속으로 생성합니다.</summary>
    /// <returns>폭죽 발사 간격을 처리하는 코루틴 열거자입니다.</returns>
    private IEnumerator PlayFireworks()
    {
        Vector2[] burstPositions =
        {
            new Vector2(-610f, 250f),
            new Vector2(560f, 280f),
            new Vector2(-260f, 170f),
            new Vector2(250f, 210f),
            new Vector2(-500f, 20f),
            new Vector2(520f, 40f),
            new Vector2(-80f, 320f),
            new Vector2(70f, 40f),
            new Vector2(-690f, 80f),
            new Vector2(680f, 130f),
            new Vector2(-350f, 300f),
            new Vector2(380f, 330f)
        };
        Color[] burstColors =
        {
            new Color(1f, 0.78f, 0.18f, 1f),
            new Color(0.25f, 0.85f, 1f, 1f),
            new Color(1f, 0.35f, 0.62f, 1f),
            new Color(0.45f, 1f, 0.48f, 1f),
            new Color(0.72f, 0.42f, 1f, 1f),
            new Color(1f, 0.48f, 0.18f, 1f),
            new Color(1f, 0.92f, 0.35f, 1f),
            new Color(0.25f, 1f, 0.84f, 1f),
            new Color(1f, 0.3f, 0.38f, 1f),
            new Color(0.35f, 0.6f, 1f, 1f),
            new Color(0.95f, 0.48f, 1f, 1f),
            new Color(0.55f, 1f, 0.3f, 1f)
        };

        int burstIndex = 0;
        while (burstIndex < burstPositions.Length)
        {
            StartCoroutine(PlaySingleBurst(
                burstPositions[burstIndex],
                burstColors[burstIndex]));
            burstIndex++;
            yield return new WaitForSecondsRealtime(0.16f);
        }
    }

    /// <summary>한 지점에서 여러 UI 파티클이 원형으로 퍼지는 폭죽 한 발을 재생합니다.</summary>
    /// <param name="center">폭죽이 터질 화면 기준 위치입니다.</param>
    /// <param name="baseColor">폭죽의 기본 색상입니다.</param>
    /// <returns>파티클 이동과 소멸을 처리하는 코루틴 열거자입니다.</returns>
    private IEnumerator PlaySingleBurst(Vector2 center, Color baseColor)
    {
        const int particleCount = 36;
        const float duration = 1.45f;
        List<RectTransform> particleRects =
            new List<RectTransform>();
        List<Image> particleImages =
            new List<Image>();
        List<Vector2> particleDirections =
            new List<Vector2>();
        List<float> particleDistances =
            new List<float>();

        GameObject flashObject = new GameObject(
            "Firework Center Flash",
            typeof(RectTransform),
            typeof(Image));
        flashObject.transform.SetParent(effectLayer, false);
        RectTransform flashRect =
            flashObject.GetComponent<RectTransform>();
        flashRect.anchorMin = new Vector2(0.5f, 0.5f);
        flashRect.anchorMax = new Vector2(0.5f, 0.5f);
        flashRect.anchoredPosition = center;
        flashRect.sizeDelta = new Vector2(82f, 82f);
        flashRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Image flashImage = flashObject.GetComponent<Image>();
        flashImage.color = Color.white;
        flashImage.raycastTarget = false;

        int particleIndex = 0;
        while (particleIndex < particleCount)
        {
            float ringOffset = particleIndex % 2 == 0 ? 0f : 0.08f;
            float angle =
                Mathf.PI * 2f * particleIndex / particleCount +
                ringOffset;
            Vector2 direction =
                new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            GameObject particleObject = new GameObject(
                "Firework Particle",
                typeof(RectTransform),
                typeof(Image));
            particleObject.transform.SetParent(effectLayer, false);
            RectTransform particleRect =
                particleObject.GetComponent<RectTransform>();
            particleRect.anchorMin = new Vector2(0.5f, 0.5f);
            particleRect.anchorMax = new Vector2(0.5f, 0.5f);
            particleRect.anchoredPosition = center;
            float particleLength =
                particleIndex % 3 == 0 ? 34f : 25f;
            particleRect.sizeDelta =
                new Vector2(11f, particleLength);
            particleRect.localRotation = Quaternion.Euler(
                0f,
                0f,
                angle * Mathf.Rad2Deg - 90f);
            Image particleImage = particleObject.GetComponent<Image>();
            float brightness = particleIndex % 2 == 0 ? 1f : 0.72f;
            particleImage.color = Color.Lerp(
                Color.white,
                baseColor,
                brightness);
            particleImage.raycastTarget = false;
            particleRects.Add(particleRect);
            particleImages.Add(particleImage);
            particleDirections.Add(direction);
            float travelDistance =
                particleIndex % 2 == 0 ? 265f : 205f;
            particleDistances.Add(travelDistance);
            particleIndex++;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float flashProgress = Mathf.Clamp01(progress / 0.24f);
            flashRect.localScale = Vector3.one *
                Mathf.Lerp(0.25f, 1.8f, flashProgress);
            Color flashColor = Color.Lerp(
                Color.white,
                baseColor,
                flashProgress);
            flashColor.a = 1f - flashProgress;
            flashImage.color = flashColor;
            int updateIndex = 0;
            while (updateIndex < particleRects.Count)
            {
                float travelDistance = Mathf.Lerp(
                    0f,
                    particleDistances[updateIndex],
                    progress);
                Vector2 gravityOffset =
                    Vector2.down * 95f * progress * progress;
                particleRects[updateIndex].anchoredPosition =
                    center +
                    particleDirections[updateIndex] * travelDistance +
                    gravityOffset;
                float particleScale = Mathf.Lerp(1.25f, 0.12f, progress);
                particleRects[updateIndex].localScale =
                    Vector3.one * particleScale;
                Color particleColor = particleImages[updateIndex].color;
                particleColor.a = 1f - progress;
                particleImages[updateIndex].color = particleColor;
                updateIndex++;
            }

            yield return null;
        }

        foreach (RectTransform particleRect in particleRects)
        {
            if (particleRect != null)
            {
                Destroy(particleRect.gameObject);
            }
        }

        if (flashObject != null)
        {
            Destroy(flashObject);
        }
    }
}
