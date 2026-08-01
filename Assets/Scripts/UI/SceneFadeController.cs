using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>씬이 바뀌는 동안 화면 전체의 페이드 아웃과 페이드 인을 담당합니다.</summary>
public sealed class SceneFadeController : MonoBehaviour
{
    // 모든 씬에서 공유하는 페이드 컨트롤러 인스턴스입니다.
    private static SceneFadeController instance;
    // 화면을 검게 덮는 이미지입니다.
    [SerializeField] private Image fadeImage;
    // 페이드 한 단계에 사용하는 시간입니다.
    [SerializeField, Min(0.1f)] private float fadeDuration = 0.65f;
    // 씬 전환이 진행 중인지 나타냅니다.
    private bool isTransitioning;

    /// <summary>도메인을 다시 로드하지 않는 Play Mode에서도 이전 실행의 정적 참조를 제거합니다.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        instance = null;
    }

    /// <summary>어떤 씬에서도 페이드 전환을 사용할 수 있도록 전역 컨트롤러를 준비합니다.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeFirstScene()
    {
        Shader.SetGlobalFloat("BinaryEffectEnabled", 0f);
        if (instance == null)
        {
            CreateRuntimeController();
        }
    }

    /// <summary>현재 씬에 컨트롤러가 없을 때 런타임 페이드 캔버스를 생성합니다.</summary>
    private static void CreateRuntimeController()
    {
        GameObject root = new GameObject(
            "Global Scene Fade",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        GameObject imageObject = new GameObject(
            "Fade Image",
            typeof(RectTransform),
            typeof(Image));
        imageObject.transform.SetParent(root.transform, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = imageObject.GetComponent<Image>();
        image.color = Color.black;

        SceneFadeController controller =
            root.AddComponent<SceneFadeController>();
        controller.Configure(image, 0.65f);
    }

    /// <summary>현재 위치와 관계없이 지정한 씬으로 페이드 전환합니다.</summary>
    /// <param name="sceneName">불러올 씬의 이름입니다.</param>
    public static void LoadSceneWithFade(string sceneName)
    {
        if (instance == null)
        {
            CreateRuntimeController();
        }

        instance.LoadScene(sceneName);
    }

    /// <summary>컨트롤러를 씬 전환 중에도 유지하고 첫 화면의 페이드 인을 시작합니다.</summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        if (fadeImage == null)
        {
            fadeImage = GetComponentInChildren<Image>();
        }
        if (fadeImage == null)
        {
            return;
        }
        fadeImage.color = Color.black;
        StartCoroutine(FadeTo(0f));
    }

    /// <summary>현재 전환 관리자가 파괴될 때 남아 있는 정적 참조를 안전하게 해제합니다.</summary>
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>검은 화면으로 전환한 다음 지정한 씬을 불러옵니다.</summary>
    /// <param name="sceneName">불러올 씬의 이름입니다.</param>
    public void LoadScene(string sceneName)
    {
        if (isTransitioning == true)
        {
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    /// <summary>페이드 아웃, 씬 로드, 페이드 인을 순서대로 실행합니다.</summary>
    /// <param name="sceneName">불러올 씬의 이름입니다.</param>
    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isTransitioning = true;
        yield return FadeTo(1f);

        Time.timeScale = 0f;
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        while (loadOperation.isDone == false)
        {
            yield return null;
        }

        if (sceneName == "Title" ||
            sceneName == "StageSelect")
        {
            Shader.SetGlobalFloat("BinaryEffectEnabled", 0f);
        }
        yield return FadeTo(0f);
        Time.timeScale = 1f;
        isTransitioning = false;
    }

    /// <summary>검은 오버레이의 투명도를 목표 값까지 부드럽게 변경합니다.</summary>
    /// <param name="targetAlpha">도달할 알파 값입니다.</param>
    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float elapsedTime = 0f;

        fadeImage.raycastTarget = true;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fadeDuration);
            Color currentColor = fadeImage.color;
            currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, progress);
            fadeImage.color = currentColor;
            yield return null;
        }

        Color completedColor = fadeImage.color;
        completedColor.a = targetAlpha;
        fadeImage.color = completedColor;
        if (targetAlpha == 0f)
        {
            fadeImage.raycastTarget = false;
        }
    }

    /// <summary>에디터 생성 코드에서 페이드 이미지와 시간을 설정합니다.</summary>
    /// <param name="image">화면을 덮을 이미지입니다.</param>
    /// <param name="duration">페이드 한 단계의 시간입니다.</param>
    public void Configure(Image image, float duration)
    {
        fadeImage = image;
        fadeDuration = duration;
    }
}
