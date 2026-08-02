using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>모든 스테이지 클리어 결과를 표시하고 엔딩 화면의 버튼과 등장 애니메이션을 관리합니다.</summary>
public sealed class EndingScreenController : MonoBehaviour
{
    // 엔딩 내용 전체의 투명도와 크기 연출에 사용할 CanvasGroup입니다.
    [SerializeField] private CanvasGroup contentGroup;
    // 전체 스타 획득 결과를 표시할 텍스트입니다.
    [SerializeField] private Text starResultText;
    // 엔딩 화면에서 천천히 회전할 별 장식 목록입니다.
    [SerializeField] private RectTransform[] starDecorations;

    /// <summary>엔딩 통계를 갱신하고 화면 등장 연출을 시작합니다.</summary>
    private void Start()
    {
        Time.timeScale = 1f;
        int collectedStars =
            StageProgressData.GetTotalCollectedStars();
        int availableStars =
            StageProgressData.GetTotalAvailableStars();
        starResultText.text =
            "TOTAL STARS   " + collectedStars + " / " + availableStars;
        StartCoroutine(PlayEntrance());
    }

    /// <summary>매 프레임 별 장식을 서로 다른 속도로 회전시킵니다.</summary>
    private void Update()
    {
        int starIndex = 0;
        while (starIndex < starDecorations.Length)
        {
            RectTransform starDecoration =
                starDecorations[starIndex];
            float direction = starIndex % 2 == 0 ? 1f : -1f;
            starDecoration.Rotate(
                0f,
                0f,
                direction * (12f + starIndex * 2f) *
                Time.unscaledDeltaTime);
            starIndex++;
        }
    }

    /// <summary>엔딩 내용이 서서히 나타나면서 원래 크기로 커지는 연출을 재생합니다.</summary>
    /// <returns>등장 연출을 프레임별로 처리하는 코루틴 열거자입니다.</returns>
    private IEnumerator PlayEntrance()
    {
        float elapsedTime = 0f;
        const float duration = 1.2f;
        contentGroup.alpha = 0f;
        contentGroup.transform.localScale = Vector3.one * 0.9f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float easedProgress = 1f -
                Mathf.Pow(1f - progress, 3f);
            contentGroup.alpha = easedProgress;
            contentGroup.transform.localScale = Vector3.one *
                Mathf.Lerp(0.9f, 1f, easedProgress);
            yield return null;
        }

        contentGroup.alpha = 1f;
        contentGroup.transform.localScale = Vector3.one;
    }

    /// <summary>첫 번째 스테이지부터 다시 플레이합니다.</summary>
    public void ReplayFromBeginning()
    {
        SceneFadeController.LoadSceneWithFade("Stage1");
    }

    /// <summary>스테이지 선택 화면으로 이동합니다.</summary>
    public void GoToStageSelect()
    {
        SceneFadeController.LoadSceneWithFade("StageSelect");
    }

    /// <summary>타이틀 화면으로 이동합니다.</summary>
    public void GoToTitle()
    {
        SceneFadeController.LoadSceneWithFade("Title");
    }

    /// <summary>에디터 빌더가 엔딩 UI 참조를 연결합니다.</summary>
    /// <param name="group">등장 연출을 적용할 CanvasGroup입니다.</param>
    /// <param name="resultText">스타 결과를 표시할 텍스트입니다.</param>
    /// <param name="decorations">회전시킬 별 장식 배열입니다.</param>
    public void Configure(
        CanvasGroup group,
        Text resultText,
        RectTransform[] decorations)
    {
        contentGroup = group;
        starResultText = resultText;
        starDecorations = decorations;
    }
}
