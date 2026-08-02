using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>모든 씬의 버튼음과 스테이지 HUD의 이미지 체력 바 및 Star 설명 표시를 구성합니다.</summary>
public static class UiFeedbackUpgradeBuilder
{
    // 생성할 맑은 버튼 클릭 효과음의 프로젝트 경로입니다.
    private const string ClickClipPath =
        "Assets/AcademyPlatformer/Audio/UiClearClick.wav";
    // 완전히 각진 체력 바에 사용할 직사각형 PNG 스프라이트 경로입니다.
    private const string HealthBarSpritePath =
        "Assets/AcademyPlatformer/UI/HealthBarRectangle.png";
    // 버튼 클릭 효과음의 초당 샘플 수입니다.
    private const int SampleRate = 44100;

    /// <summary>버튼 효과음을 생성하고 모든 씬의 UI 피드백을 갱신합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Upgrade HUD And Button Audio")]
    public static void Build()
    {
        CreateRectangularHealthBarSprite();
        CreateClearClickSound();
        AssetDatabase.ImportAsset(
            HealthBarSpritePath,
            ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(
            ClickClipPath,
            ImportAssetOptions.ForceSynchronousImport);
        ConfigureHealthBarSpriteImporter();
        ConfigureClickImporter();
        AudioClip clickClip =
            AssetDatabase.LoadAssetAtPath<AudioClip>(ClickClipPath);
        string previousScenePath = SceneManager.GetActiveScene().path;
        string[] scenePaths =
        {
            "Assets/Scenes/Title.unity",
            "Assets/Scenes/StageSelect.unity",
            "Assets/Scenes/Main.unity",
            "Assets/Scenes/Stage1.unity",
            "Assets/Scenes/Stage2.unity",
            "Assets/Scenes/Stage3.unity"
        };

        foreach (string scenePath in scenePaths)
        {
            if (File.Exists(scenePath) == false)
            {
                continue;
            }

            ConfigureScene(scenePath, clickClip);
        }

        if (string.IsNullOrEmpty(previousScenePath) == false)
        {
            EditorSceneManager.OpenScene(
                previousScenePath,
                OpenSceneMode.Single);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("UI_FEEDBACK_UPGRADE_COMPLETED");
    }

    /// <summary>한 씬에 버튼 효과음 재생기를 추가하고 게임 HUD가 있으면 시각 상태 표시를 교체합니다.</summary>
    /// <param name="scenePath">갱신할 씬의 프로젝트 경로입니다.</param>
    /// <param name="clickClip">모든 버튼에 연결할 클릭 효과음입니다.</param>
    private static void ConfigureScene(
        string scenePath,
        AudioClip clickClip)
    {
        Scene scene = EditorSceneManager.OpenScene(
            scenePath,
            OpenSceneMode.Single);
        SceneButtonAudio buttonAudio =
            Object.FindFirstObjectByType<SceneButtonAudio>();
        if (buttonAudio == null)
        {
            GameObject audioObject = new GameObject("Scene Button Audio");
            audioObject.AddComponent<AudioSource>();
            buttonAudio = audioObject.AddComponent<SceneButtonAudio>();
        }
        buttonAudio.Configure(clickClip);
        EditorUtility.SetDirty(buttonAudio);

        GameHUD hud = Object.FindFirstObjectByType<GameHUD>();
        if (hud != null)
        {
            ConfigureGameHud(hud);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    /// <summary>기존 숫자 체력 표시를 이미지 체력 바로 교체하고 Star 텍스트의 의미를 명확히 표시합니다.</summary>
    /// <param name="hud">상태 표시를 갱신할 게임 HUD입니다.</param>
    private static void ConfigureGameHud(GameHUD hud)
    {
        Transform existingGroup = hud.transform.Find("Health Status Group");
        if (existingGroup != null)
        {
            Object.DestroyImmediate(existingGroup.gameObject);
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Sprite healthBarSprite =
            AssetDatabase.LoadAssetAtPath<Sprite>(HealthBarSpritePath);
        GameObject groupObject = new GameObject(
            "Health Status Group",
            typeof(RectTransform));
        groupObject.transform.SetParent(hud.transform, false);
        groupObject.transform.SetAsFirstSibling();
        RectTransform groupRect = groupObject.GetComponent<RectTransform>();
        SetTopLeftRect(
            groupRect,
            new Vector2(28f, -24f),
            new Vector2(430f, 92f));

        Text healthLabel = CreateText(
            groupObject.transform,
            "Health Label",
            "HEALTH",
            font,
            22,
            new Color(0.92f, 0.97f, 1f, 1f));
        SetTopLeftRect(
            healthLabel.rectTransform,
            Vector2.zero,
            new Vector2(180f, 28f));

        GameObject frameObject = CreateImage(
            groupObject.transform,
            "Health Bar Frame",
            healthBarSprite,
            new Color(0.055f, 0.08f, 0.13f, 0.94f));
        RectTransform frameRect = frameObject.GetComponent<RectTransform>();
        SetTopLeftRect(
            frameRect,
            new Vector2(0f, -34f),
            new Vector2(330f, 30f));

        Image delayedFill = CreateFillImage(
            frameObject.transform,
            "Delayed Health Fill",
            healthBarSprite,
            new Color(1f, 0.34f, 0.22f, 1f));
        Image healthFill = CreateFillImage(
            frameObject.transform,
            "Current Health Fill",
            healthBarSprite,
            new Color(0.92f, 0.08f, 0.12f, 1f));

        Text starText = FindStatusText(hud);
        if (starText == null)
        {
            starText = CreateText(
                hud.transform,
                "Star Collected Text",
                "STAR COLLECTED   0 / 0",
                font,
                25,
                new Color(1f, 0.85f, 0.25f, 1f));
        }
        starText.name = "Star Collected Text";
        starText.fontSize = 25;
        starText.fontStyle = FontStyle.Bold;
        starText.alignment = TextAnchor.UpperLeft;
        starText.color = new Color(1f, 0.85f, 0.25f, 1f);
        starText.text = "STAR COLLECTED   0 / 0";
        SetTopLeftRect(
            starText.rectTransform,
            new Vector2(28f, -112f),
            new Vector2(520f, 42f));

        hud.ConfigureVisualStatus(
            healthFill,
            delayedFill,
            starText);
        EditorUtility.SetDirty(hud);
    }

    /// <summary>현재 HUD가 참조하는 기존 상태 텍스트를 직렬화 오브젝트에서 찾습니다.</summary>
    /// <param name="hud">상태 텍스트를 찾을 게임 HUD입니다.</param>
    /// <returns>기존 상태 텍스트이며 찾지 못하면 null입니다.</returns>
    private static Text FindStatusText(GameHUD hud)
    {
        SerializedObject serializedHud = new SerializedObject(hud);
        SerializedProperty statusProperty =
            serializedHud.FindProperty("statusText");
        return statusProperty.objectReferenceValue as Text;
    }

    /// <summary>부모 영역을 가득 채우는 가로 채우기 방식의 체력 이미지를 생성합니다.</summary>
    /// <param name="parent">체력 이미지를 배치할 프레임 Transform입니다.</param>
    /// <param name="name">생성할 이미지 오브젝트 이름입니다.</param>
    /// <param name="sprite">채우기 이미지에 사용할 UI Sprite입니다.</param>
    /// <param name="color">체력 이미지에 적용할 색상입니다.</param>
    /// <returns>생성하고 설정한 체력 Image입니다.</returns>
    private static Image CreateFillImage(
        Transform parent,
        string name,
        Sprite sprite,
        Color color)
    {
        GameObject imageObject = CreateImage(parent, name, sprite, color);
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = new Vector2(5f, 5f);
        imageRect.offsetMax = new Vector2(-5f, -5f);
        Image image = imageObject.GetComponent<Image>();
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = 0;
        image.fillAmount = 1f;
        image.raycastTarget = false;
        return image;
    }

    /// <summary>지정한 부모 아래에 색상과 Sprite가 적용된 UI 이미지를 생성합니다.</summary>
    /// <param name="parent">이미지를 배치할 부모 Transform입니다.</param>
    /// <param name="name">생성할 이미지 이름입니다.</param>
    /// <param name="sprite">이미지에 표시할 UI Sprite입니다.</param>
    /// <param name="color">이미지에 적용할 색상입니다.</param>
    /// <returns>생성한 이미지 GameObject입니다.</returns>
    private static GameObject CreateImage(
        Transform parent,
        string name,
        Sprite sprite,
        Color color)
    {
        GameObject imageObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return imageObject;
    }

    /// <summary>지정한 부모 아래에 HUD용 텍스트를 생성합니다.</summary>
    /// <param name="parent">텍스트를 배치할 부모 Transform입니다.</param>
    /// <param name="name">생성할 텍스트 이름입니다.</param>
    /// <param name="content">처음 표시할 문자열입니다.</param>
    /// <param name="font">텍스트에 사용할 Font입니다.</param>
    /// <param name="fontSize">텍스트 크기입니다.</param>
    /// <param name="color">텍스트에 적용할 색상입니다.</param>
    /// <returns>생성하고 설정한 Text입니다.</returns>
    private static Text CreateText(
        Transform parent,
        string name,
        string content,
        Font font,
        int fontSize,
        Color color)
    {
        GameObject textObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.UpperLeft;
        text.color = color;
        text.text = content;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>UI RectTransform을 화면 왼쪽 위 기준 위치와 크기로 설정합니다.</summary>
    /// <param name="rectTransform">위치와 크기를 변경할 RectTransform입니다.</param>
    /// <param name="position">왼쪽 위 모서리 기준 위치입니다.</param>
    /// <param name="size">UI 요소의 가로와 세로 크기입니다.</param>
    private static void SetTopLeftRect(
        RectTransform rectTransform,
        Vector2 position,
        Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
    }

    /// <summary>짧은 고음과 배음이 부드럽게 감쇠하는 맑은 버튼 클릭 WAV를 생성합니다.</summary>
    private static void CreateClearClickSound()
    {
        float duration = 0.16f;
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        short[] samples = new short[sampleCount];
        int sampleIndex = 0;
        while (sampleIndex < sampleCount)
        {
            float time = (float)sampleIndex / SampleRate;
            float attack = Mathf.Clamp01(time / 0.006f);
            float firstEnvelope = Mathf.Exp(-20f * time);
            float firstTone = Mathf.Sin(Mathf.PI * 2f * 1174.66f * time);
            float firstHarmonic =
                Mathf.Sin(Mathf.PI * 2f * 2349.32f * time) * 0.28f;
            float secondTime = Mathf.Max(0f, time - 0.045f);
            float secondEnvelope =
                time >= 0.045f ? Mathf.Exp(-24f * secondTime) : 0f;
            float secondTone =
                Mathf.Sin(Mathf.PI * 2f * 1567.98f * secondTime);
            float sampleValue = attack * 0.38f *
                ((firstTone + firstHarmonic) * firstEnvelope +
                secondTone * secondEnvelope * 0.5f);
            samples[sampleIndex] = (short)Mathf.Clamp(
                sampleValue * short.MaxValue,
                short.MinValue,
                short.MaxValue);
            sampleIndex++;
        }

        WriteWaveFile(ClickClipPath, samples);
    }

    /// <summary>둥근 모서리나 여백이 전혀 없는 흰색 직사각형 PNG 이미지를 생성합니다.</summary>
    private static void CreateRectangularHealthBarSprite()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string absolutePath = Path.Combine(projectRoot, HealthBarSpritePath);
        string absoluteFolder = Path.GetDirectoryName(absolutePath);
        Directory.CreateDirectory(absoluteFolder);
        Texture2D texture = new Texture2D(
            64,
            16,
            TextureFormat.RGBA32,
            false);
        Color[] pixels = new Color[64 * 16];
        int pixelIndex = 0;
        while (pixelIndex < pixels.Length)
        {
            pixels[pixelIndex] = Color.white;
            pixelIndex++;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(absolutePath, pngData);
        Object.DestroyImmediate(texture);
    }

    /// <summary>직사각형 PNG를 여백과 테두리 분할이 없는 단일 UI Sprite로 임포트합니다.</summary>
    private static void ConfigureHealthBarSpriteImporter()
    {
        TextureImporter importer =
            AssetImporter.GetAtPath(HealthBarSpritePath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = false;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteBorder = Vector4.zero;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    /// <summary>버튼 클릭 효과음을 씬 시작 전에 미리 읽도록 임포트 설정을 적용합니다.</summary>
    private static void ConfigureClickImporter()
    {
        AudioImporter importer =
            AssetImporter.GetAtPath(ClickClipPath) as AudioImporter;
        if (importer == null)
        {
            return;
        }

        AudioImporterSampleSettings sampleSettings =
            importer.defaultSampleSettings;
        sampleSettings.preloadAudioData = true;
        importer.defaultSampleSettings = sampleSettings;
        importer.loadInBackground = false;
        importer.SaveAndReimport();
    }

    /// <summary>생성한 16비트 모노 샘플을 Unity가 읽을 수 있는 WAV 파일로 저장합니다.</summary>
    /// <param name="assetPath">프로젝트 기준 WAV 파일 경로입니다.</param>
    /// <param name="samples">파일에 기록할 오디오 샘플 배열입니다.</param>
    private static void WriteWaveFile(string assetPath, short[] samples)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string absolutePath = Path.Combine(projectRoot, assetPath);
        int dataSize = samples.Length * sizeof(short);
        using (FileStream stream = File.Create(absolutePath))
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(SampleRate);
            writer.Write(SampleRate * sizeof(short));
            writer.Write((short)sizeof(short));
            writer.Write((short)16);
            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);
            foreach (short sample in samples)
            {
                writer.Write(sample);
            }
        }
    }
}
