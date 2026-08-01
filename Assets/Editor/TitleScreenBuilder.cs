using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>수업용 타이틀 화면과 씬 전환 오브젝트를 자동으로 생성합니다.</summary>
public static class TitleScreenBuilder
{
    // 타이틀 씬을 저장할 경로입니다.
    private const string TitleScenePath = "Assets/Scenes/Title.unity";
    // 게임 씬이 저장된 경로입니다.
    private const string MainScenePath = "Assets/Scenes/Main.unity";

    /// <summary>작업 완료 후 Unity 에디터가 타이틀 씬을 표시하도록 엽니다.</summary>
    public static void OpenTitleScene()
    {
        EditorSceneManager.OpenScene(TitleScenePath);
    }

    /// <summary>타이틀 씬을 만들고 타이틀이 먼저 실행되도록 빌드 순서를 설정합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Rebuild Title Screen")]
    public static void BuildTitleScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Title";

        Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.025f, 0.055f, 0.11f);
        camera.gameObject.AddComponent<AudioListener>();

        Canvas canvas = CreateCanvas("Title Canvas", 0);
        Image clickArea = CreateImage(canvas.transform, "Clickable Background",
            new Color(0.035f, 0.09f, 0.17f, 1f), Vector2.zero, Vector2.one);
        TitleScreenController titleController = clickArea.gameObject.AddComponent<TitleScreenController>();

        CreateImage(clickArea.transform, "Top Glow",
            new Color(0.1f, 0.48f, 0.72f, 0.22f), new Vector2(0f, 0.58f), Vector2.one);
        CreateText(clickArea.transform, "Game Title", "ACADEMY\nPLATFORMER",
            new Vector2(0.5f, 0.64f), new Vector2(900f, 250f), 72, Color.white);
        CreateText(clickArea.transform, "Subtitle", "2D PLATFORMER SAMPLE PROJECT",
            new Vector2(0.5f, 0.48f), new Vector2(700f, 60f), 25,
            new Color(0.35f, 0.86f, 1f));

        Button startButton = CreateButton(clickArea.transform);
        CreateText(startButton.transform, "Button Text", "GAME START",
            new Vector2(0.5f, 0.5f), new Vector2(430f, 85f), 34, Color.white);
        CreateText(clickArea.transform, "Guide", "화면을 클릭하거나  ENTER / SPACE 키를 누르세요",
            new Vector2(0.5f, 0.22f), new Vector2(750f, 55f), 22,
            new Color(0.72f, 0.82f, 0.9f));

        titleController.Configure("StageSelect");
        UnityEventTools.AddPersistentListener(startButton.onClick, titleController.StartGame);

        GameObject eventSystem = new GameObject("Event System");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        EditorSceneManager.SaveScene(scene, TitleScenePath);
        BuildStageSelectScene();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(TitleScenePath, true),
            new EditorBuildSettingsScene("Assets/Scenes/StageSelect.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Stage1.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Stage2.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Stage3.unity", true)
        };
        PlayerSettings.productName = "Academy Platformer Sample";
        PlayerSettings.companyName = "Game Academy";
        AssetDatabase.SaveAssets();
        Debug.Log("TITLE_SCREEN_BUILD_COMPLETED");
    }

    /// <summary>해금 상태가 표시되는 3개 스테이지 선택 화면을 생성합니다.</summary>
    private static void BuildStageSelectScene()
    {
        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);
        scene.name = "StageSelect";

        Camera camera =
            new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.025f, 0.035f, 0.075f);
        camera.gameObject.AddComponent<AudioListener>();

        Canvas canvas = CreateCanvas("Stage Select Canvas", 0);
        Image background = CreateImage(
            canvas.transform,
            "Background",
            new Color(0.025f, 0.055f, 0.12f, 1f),
            Vector2.zero,
            Vector2.one);
        StageSelectScreenController screenController =
            background.gameObject.AddComponent<StageSelectScreenController>();

        CreateText(
            background.transform,
            "Title",
            "SELECT STAGE",
            new Vector2(0.5f, 0.84f),
            new Vector2(900f, 120f),
            58,
            Color.white);
        CreateText(
            background.transform,
            "Progress Guide",
            "이전 스테이지를 클리어하면 다음 스테이지가 해금됩니다",
            new Vector2(0.5f, 0.74f),
            new Vector2(900f, 50f),
            23,
            new Color(0.62f, 0.8f, 0.92f));

        CreateStageCard(
            background.transform,
            1,
            "SNOW FOREST",
            new Vector2(0.22f, 0.48f),
            new Color(0.1f, 0.46f, 0.62f));
        CreateStageCard(
            background.transform,
            2,
            "VIOLET TWILIGHT",
            new Vector2(0.5f, 0.48f),
            new Color(0.42f, 0.2f, 0.62f));
        CreateStageCard(
            background.transform,
            3,
            "CRIMSON SUMMIT",
            new Vector2(0.78f, 0.48f),
            new Color(0.68f, 0.2f, 0.16f));

        Button backButton = CreateMenuButton(
            background.transform,
            "Back Button",
            "BACK TO TITLE",
            new Vector2(0.5f, 0.15f),
            new Vector2(360f, 70f),
            new Color(0.12f, 0.3f, 0.42f));
        UnityEventTools.AddPersistentListener(
            backButton.onClick,
            screenController.ReturnToTitle);

        GameObject eventSystem = new GameObject("Event System");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
        EditorSceneManager.SaveScene(
            scene,
            "Assets/Scenes/StageSelect.unity");
    }

    /// <summary>스테이지의 콘셉트와 잠금 상태를 표시하는 카드 버튼을 생성합니다.</summary>
    /// <param name="parent">카드가 소속될 부모입니다.</param>
    /// <param name="stageNumber">카드가 나타낼 스테이지 번호입니다.</param>
    /// <param name="conceptName">스테이지 콘셉트 이름입니다.</param>
    /// <param name="anchor">화면에서 카드가 위치할 앵커입니다.</param>
    /// <param name="color">해금된 카드의 기본 색상입니다.</param>
    private static void CreateStageCard(
        Transform parent,
        int stageNumber,
        string conceptName,
        Vector2 anchor,
        Color color)
    {
        Button button = CreateMenuButton(
            parent,
            "Stage " + stageNumber + " Button",
            string.Empty,
            anchor,
            new Vector2(430f, 330f),
            color);
        Text label = button.GetComponentInChildren<Text>();
        label.fontSize = 32;
        StageSelectButton stageButton =
            button.gameObject.AddComponent<StageSelectButton>();
        stageButton.Configure(
            stageNumber,
            button,
            label,
            conceptName);
        UnityEventTools.AddPersistentListener(
            button.onClick,
            stageButton.EnterStage);
    }

    /// <summary>스테이지 선택 화면에서 공통으로 사용할 메뉴 버튼을 생성합니다.</summary>
    /// <param name="parent">버튼이 소속될 부모입니다.</param>
    /// <param name="name">버튼 오브젝트 이름입니다.</param>
    /// <param name="label">버튼에 표시할 문구입니다.</param>
    /// <param name="anchor">화면에서 버튼이 위치할 앵커입니다.</param>
    /// <param name="size">버튼의 크기입니다.</param>
    /// <param name="color">버튼의 기본 색상입니다.</param>
    private static Button CreateMenuButton(
        Transform parent,
        string name,
        string label,
        Vector2 anchor,
        Vector2 size,
        Color color)
    {
        GameObject buttonObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = size;
        buttonObject.GetComponent<Image>().color = color;
        Button button = buttonObject.GetComponent<Button>();
        Text text = CreateText(
            buttonObject.transform,
            "Label",
            label,
            new Vector2(0.5f, 0.5f),
            size,
            25,
            Color.white);
        text.rectTransform.anchoredPosition = Vector2.zero;
        return button;
    }

    /// <summary>화면 크기에 맞춰 확장되는 UI 캔버스를 생성합니다.</summary>
    /// <param name="name">캔버스 오브젝트 이름입니다.</param>
    /// <param name="sortingOrder">다른 캔버스 위에 그릴 순서입니다.</param>
    private static Canvas CreateCanvas(string name, int sortingOrder)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    /// <summary>지정한 앵커 영역을 채우는 UI 이미지를 생성합니다.</summary>
    /// <param name="parent">이미지가 소속될 부모입니다.</param>
    /// <param name="name">이미지 오브젝트 이름입니다.</param>
    /// <param name="color">이미지 색상입니다.</param>
    /// <param name="anchorMin">최소 앵커 좌표입니다.</param>
    /// <param name="anchorMax">최대 앵커 좌표입니다.</param>
    private static Image CreateImage(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(Image));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = child.GetComponent<Image>();
        image.color = color;
        return image;
    }

    /// <summary>중앙 정렬된 안내 문구를 생성합니다.</summary>
    /// <param name="parent">문구가 소속될 부모입니다.</param>
    /// <param name="name">문구 오브젝트 이름입니다.</param>
    /// <param name="content">화면에 표시할 내용입니다.</param>
    /// <param name="anchor">문구의 화면 기준 위치입니다.</param>
    /// <param name="size">문구 영역의 크기입니다.</param>
    /// <param name="fontSize">글자 크기입니다.</param>
    /// <param name="color">글자 색상입니다.</param>
    private static Text CreateText(Transform parent, string name, string content,
        Vector2 anchor, Vector2 size, int fontSize, Color color)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(Text));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = size;
        Text text = child.GetComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>타이틀 중앙에 게임 시작 버튼을 생성합니다.</summary>
    /// <param name="parent">버튼이 소속될 부모입니다.</param>
    private static Button CreateButton(Transform parent)
    {
        GameObject child = new GameObject("Start Button", typeof(RectTransform), typeof(Image), typeof(Button));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.34f);
        rect.anchorMax = new Vector2(0.5f, 0.34f);
        rect.sizeDelta = new Vector2(430f, 85f);
        Image image = child.GetComponent<Image>();
        image.color = new Color(0.08f, 0.54f, 0.74f, 1f);
        Button button = child.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.15f, 0.75f, 0.95f, 1f);
        colors.pressedColor = new Color(0.04f, 0.35f, 0.55f, 1f);
        button.colors = colors;
        return button;
    }
}
