using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>마지막 스테이지 이후 표시할 전용 엔딩 씬과 UI를 생성합니다.</summary>
public static class EndingSceneBuilder
{
    // 생성할 엔딩 씬의 프로젝트 경로입니다.
    private const string EndingScenePath = "Assets/Scenes/Ending.unity";
    // 모든 엔딩 텍스트에 사용할 기본 글꼴입니다.
    private static Font endingFont;

    /// <summary>엔딩 화면, 버튼, 배경음악을 생성하고 빌드 설정에 씬을 등록합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Build Ending Scene")]
    public static void Build()
    {
        endingFont = Resources.GetBuiltinResource<Font>(
            "LegacyRuntime.ttf");
        Scene endingScene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);
        CreateCamera();
        CreateEventSystem();
        Canvas canvas = CreateCanvas();
        CreateBackground(canvas.transform);
        CreateEndingContent(canvas.transform);
        CreateEndingMusic();
        EditorSceneManager.SaveScene(endingScene, EndingScenePath);
        AddEndingSceneToBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("ENDING_SCENE_BUILD_COMPLETED");
    }

    /// <summary>엔딩 배경색을 출력할 카메라를 생성합니다.</summary>
    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera endingCamera = cameraObject.AddComponent<Camera>();
        endingCamera.clearFlags = CameraClearFlags.SolidColor;
        endingCamera.backgroundColor = new Color(0.025f, 0.035f, 0.1f);
        endingCamera.orthographic = true;
        cameraObject.AddComponent<AudioListener>();
    }

    /// <summary>엔딩 버튼 입력을 처리할 EventSystem을 생성합니다.</summary>
    private static void CreateEventSystem()
    {
        GameObject eventSystemObject = new GameObject("Event System");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    /// <summary>1920×1080 기준의 엔딩 UI 캔버스를 생성합니다.</summary>
    /// <returns>생성된 엔딩 UI 캔버스입니다.</returns>
    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            "Ending Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    /// <summary>밤하늘 색상과 장식 띠를 사용해 엔딩 배경을 구성합니다.</summary>
    /// <param name="parent">배경 오브젝트를 배치할 부모 Transform입니다.</param>
    private static void CreateBackground(Transform parent)
    {
        Image background = CreatePanel(
            parent,
            "Deep Night Background",
            Vector2.zero,
            new Vector2(1920f, 1080f),
            new Color(0.025f, 0.035f, 0.1f, 1f));
        StretchToParent(background.rectTransform);

        Image upperGlow = CreatePanel(
            parent,
            "Upper Glow",
            new Vector2(0f, 380f),
            new Vector2(1920f, 320f),
            new Color(0.18f, 0.22f, 0.52f, 0.42f));
        upperGlow.raycastTarget = false;
        Image lowerGlow = CreatePanel(
            parent,
            "Lower Glow",
            new Vector2(0f, -440f),
            new Vector2(1920f, 220f),
            new Color(0.55f, 0.18f, 0.28f, 0.28f));
        lowerGlow.raycastTarget = false;
    }

    /// <summary>엔딩 제목, 결과, 별 장식과 화면 이동 버튼을 생성합니다.</summary>
    /// <param name="parent">엔딩 내용을 배치할 부모 Transform입니다.</param>
    private static void CreateEndingContent(Transform parent)
    {
        GameObject contentObject = new GameObject(
            "Ending Content",
            typeof(RectTransform),
            typeof(CanvasGroup));
        contentObject.transform.SetParent(parent, false);
        RectTransform contentRect =
            contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(1380f, 850f);

        Image frame = CreatePanel(
            contentRect,
            "Ending Frame",
            Vector2.zero,
            new Vector2(1320f, 790f),
            new Color(0.055f, 0.075f, 0.18f, 0.96f));
        frame.raycastTarget = false;
        Image innerFrame = CreatePanel(
            contentRect,
            "Inner Highlight",
            Vector2.zero,
            new Vector2(1275f, 745f),
            new Color(0.12f, 0.15f, 0.3f, 0.72f));
        innerFrame.raycastTarget = false;

        CreateText(
            contentRect,
            "Ending Title",
            "THE END",
            new Vector2(0f, 270f),
            new Vector2(1000f, 130f),
            96,
            new Color(1f, 0.82f, 0.28f));
        CreateText(
            contentRect,
            "Clear Message",
            "ALL STAGES CLEARED!",
            new Vector2(0f, 155f),
            new Vector2(900f, 80f),
            46,
            new Color(0.52f, 1f, 0.82f));
        CreateText(
            contentRect,
            "Thank You Message",
            "THANK YOU FOR PLAYING",
            new Vector2(0f, 75f),
            new Vector2(900f, 60f),
            30,
            new Color(0.8f, 0.86f, 1f));
        Text resultText = CreateText(
            contentRect,
            "Star Result",
            "TOTAL STARS   0 / 0",
            new Vector2(0f, -35f),
            new Vector2(900f, 80f),
            38,
            Color.white);

        List<RectTransform> decorations =
            CreateStarDecorations(contentRect);
        EndingScreenController controller =
            contentObject.AddComponent<EndingScreenController>();
        controller.Configure(
            contentObject.GetComponent<CanvasGroup>(),
            resultText,
            decorations.ToArray());

        CreateEndingButton(
            contentRect,
            "Replay Button",
            "PLAY FROM STAGE 1",
            new Vector2(-420f, -250f),
            controller.ReplayFromBeginning);
        CreateEndingButton(
            contentRect,
            "Stage Select Button",
            "STAGE SELECT",
            new Vector2(0f, -250f),
            controller.GoToStageSelect);
        CreateEndingButton(
            contentRect,
            "Title Button",
            "RETURN TO TITLE",
            new Vector2(420f, -250f),
            controller.GoToTitle);
    }

    /// <summary>엔딩 프레임 주변에 회전하는 별 문자 장식을 생성합니다.</summary>
    /// <param name="parent">별 장식을 배치할 부모 RectTransform입니다.</param>
    /// <returns>생성된 별 장식 RectTransform 목록입니다.</returns>
    private static List<RectTransform> CreateStarDecorations(
        RectTransform parent)
    {
        Vector2[] positions = new Vector2[]
        {
            new Vector2(-540f, 270f),
            new Vector2(540f, 270f),
            new Vector2(-600f, 40f),
            new Vector2(600f, 40f),
            new Vector2(-510f, -145f),
            new Vector2(510f, -145f)
        };
        List<RectTransform> decorations =
            new List<RectTransform>();
        int starIndex = 0;
        while (starIndex < positions.Length)
        {
            Color starColor = starIndex % 2 == 0
                ? new Color(1f, 0.78f, 0.2f)
                : new Color(0.35f, 0.85f, 1f);
            Text starText = CreateText(
                parent,
                "Ending Star " + (starIndex + 1),
                "★",
                positions[starIndex],
                new Vector2(100f, 100f),
                66,
                starColor);
            decorations.Add(starText.rectTransform);
            starIndex++;
        }

        return decorations;
    }

    /// <summary>엔딩 화면에서 사용할 맑은 클릭음이 연결된 버튼을 생성합니다.</summary>
    /// <param name="parent">버튼을 배치할 부모 RectTransform입니다.</param>
    /// <param name="name">생성할 버튼 오브젝트 이름입니다.</param>
    /// <param name="label">버튼에 표시할 문구입니다.</param>
    /// <param name="position">부모 기준 버튼 위치입니다.</param>
    /// <param name="action">클릭할 때 호출할 엔딩 화면 함수입니다.</param>
    private static void CreateEndingButton(
        RectTransform parent,
        string name,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        Image buttonImage = CreatePanel(
            parent,
            name,
            position,
            new Vector2(350f, 78f),
            new Color(0.16f, 0.42f, 0.68f, 1f));
        Button button = buttonImage.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.68f, 1f, 1f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.55f, 1f);
        button.colors = colors;
        CreateText(
            buttonImage.transform,
            "Label",
            label,
            Vector2.zero,
            buttonImage.rectTransform.sizeDelta,
            23,
            Color.white);
        UnityEventTools.AddPersistentListener(button.onClick, action);

        AudioClip clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/AcademyPlatformer/Audio/UiClearClick.wav");
        SceneButtonAudio buttonAudio =
            buttonImage.gameObject.AddComponent<SceneButtonAudio>();
        buttonAudio.Configure(clickClip);
    }

    /// <summary>지정한 위치와 크기로 단색 UI 이미지를 생성합니다.</summary>
    /// <param name="parent">이미지를 배치할 부모 Transform입니다.</param>
    /// <param name="name">생성할 이미지 오브젝트 이름입니다.</param>
    /// <param name="position">부모 기준 이미지 위치입니다.</param>
    /// <param name="size">이미지의 가로와 세로 크기입니다.</param>
    /// <param name="color">이미지에 적용할 색상입니다.</param>
    /// <returns>생성된 UI Image입니다.</returns>
    private static Image CreatePanel(
        Transform parent,
        string name,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject panelObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image));
        panelObject.transform.SetParent(parent, false);
        RectTransform panelRect =
            panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = position;
        panelRect.sizeDelta = size;
        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = color;
        return panelImage;
    }

    /// <summary>지정한 위치와 스타일로 가운데 정렬된 UI 텍스트를 생성합니다.</summary>
    /// <param name="parent">텍스트를 배치할 부모 Transform입니다.</param>
    /// <param name="name">생성할 텍스트 오브젝트 이름입니다.</param>
    /// <param name="content">화면에 표시할 문자열입니다.</param>
    /// <param name="position">부모 기준 텍스트 위치입니다.</param>
    /// <param name="size">텍스트 영역의 가로와 세로 크기입니다.</param>
    /// <param name="fontSize">텍스트 글자 크기입니다.</param>
    /// <param name="color">텍스트 색상입니다.</param>
    /// <returns>생성된 UI Text입니다.</returns>
    private static Text CreateText(
        Transform parent,
        string name,
        string content,
        Vector2 position,
        Vector2 size,
        int fontSize,
        Color color)
    {
        GameObject textObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform textRect =
            textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = size;
        Text text = textObject.GetComponent<Text>();
        text.font = endingFont;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>RectTransform을 부모 영역 전체에 맞게 확장합니다.</summary>
    /// <param name="rectTransform">부모 영역에 맞출 RectTransform입니다.</param>
    private static void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    /// <summary>엔딩 화면에 은은한 메뉴 배경음악을 직접 연결합니다.</summary>
    private static void CreateEndingMusic()
    {
        AudioClip musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/AcademyPlatformer/Audio/Music/StageSelectAmbient.wav");
        GameObject musicObject = new GameObject("Ending Background Music");
        MenuBackgroundMusic music =
            musicObject.AddComponent<MenuBackgroundMusic>();
        music.Configure(musicClip, 0.085f);
    }

    /// <summary>엔딩 씬이 빌드에 포함되도록 EditorBuildSettings 목록을 갱신합니다.</summary>
    private static void AddEndingSceneToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes =
            new List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);
        bool alreadyAdded = false;
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (scene.path == EndingScenePath)
            {
                alreadyAdded = true;
            }
        }

        if (alreadyAdded == false)
        {
            scenes.Add(new EditorBuildSettingsScene(
                EndingScenePath,
                true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
