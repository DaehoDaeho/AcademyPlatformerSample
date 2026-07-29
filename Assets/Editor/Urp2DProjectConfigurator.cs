using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 프로젝트가 사용할 URP 2D 렌더링 에셋을 생성하고 프로젝트 설정에 연결합니다.
/// </summary>
public static class Urp2DProjectConfigurator
{
    /// <summary>
    /// URP 설정 에셋을 저장할 폴더 경로입니다.
    /// </summary>
    private const string SettingsFolderPath = "Assets/Settings";

    /// <summary>
    /// 2D Renderer Data 에셋을 저장할 경로입니다.
    /// </summary>
    private const string RendererDataPath = SettingsFolderPath + "/Academy2DRenderer.asset";

    /// <summary>
    /// URP Pipeline 에셋을 저장할 경로입니다.
    /// </summary>
    private const string PipelineAssetPath = SettingsFolderPath + "/AcademyURP2D.asset";

    /// <summary>
    /// 에디터가 스크립트를 다시 불러온 뒤 URP 2D 설정을 자동으로 적용합니다.
    /// </summary>
    [InitializeOnLoadMethod]
    private static void ApplySettingsAfterScriptReload()
    {
        ApplyUrp2DSettings();
    }

    /// <summary>
    /// 메뉴에서 URP 2D 설정을 다시 생성하거나 재연결할 수 있게 합니다.
    /// </summary>
    [MenuItem("Tools/Academy Platformer/Apply URP 2D Settings")]
    public static void ApplyUrp2DSettings()
    {
        EnsureSettingsFolderExists();

        // 프로젝트에 저장되어 있는 2D Renderer Data 에셋입니다.
        Renderer2DData rendererData =
            AssetDatabase.LoadAssetAtPath<Renderer2DData>(RendererDataPath);

        if (rendererData == null)
        {
            rendererData = CreateRendererData();
        }

        // 프로젝트에 저장되어 있는 URP Pipeline 에셋입니다.
        UniversalRenderPipelineAsset pipelineAsset =
            AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);

        if (pipelineAsset == null)
        {
            pipelineAsset = CreatePipelineAsset(rendererData);
        }

        AssignPipelineToProject(pipelineAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("URP 2D 렌더링 파이프라인 설정을 적용했습니다.");
    }

    /// <summary>
    /// URP 설정 에셋을 보관할 폴더가 없으면 새로 만듭니다.
    /// </summary>
    private static void EnsureSettingsFolderExists()
    {
        // URP 설정 폴더가 이미 존재하는지를 나타냅니다.
        bool settingsFolderExists = AssetDatabase.IsValidFolder(SettingsFolderPath);

        if (settingsFolderExists == false)
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }
    }

    /// <summary>
    /// 2D 조명과 Sprite 렌더링을 담당할 Renderer Data 에셋을 생성합니다.
    /// </summary>
    /// <returns>새로 생성한 2D Renderer Data 에셋을 반환합니다.</returns>
    private static Renderer2DData CreateRendererData()
    {
        // 새로 생성할 2D Renderer Data 인스턴스입니다.
        Renderer2DData rendererData = ScriptableObject.CreateInstance<Renderer2DData>();

        AssetDatabase.CreateAsset(rendererData, RendererDataPath);
        ResourceReloader.ReloadAllNullIn(rendererData, UniversalRenderPipelineAsset.packagePath);
        EditorUtility.SetDirty(rendererData);

        return rendererData;
    }

    /// <summary>
    /// 2D Renderer Data를 기본 렌더러로 사용하는 URP Pipeline 에셋을 생성합니다.
    /// </summary>
    /// <param name="rendererData">Pipeline 에셋에 연결할 2D Renderer Data입니다.</param>
    /// <returns>새로 생성한 URP Pipeline 에셋을 반환합니다.</returns>
    private static UniversalRenderPipelineAsset CreatePipelineAsset(Renderer2DData rendererData)
    {
        // 2D Renderer Data를 사용하는 새 URP Pipeline 에셋입니다.
        UniversalRenderPipelineAsset pipelineAsset =
            UniversalRenderPipelineAsset.Create(rendererData);

        AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
        EditorUtility.SetDirty(pipelineAsset);

        return pipelineAsset;
    }

    /// <summary>
    /// Graphics 설정과 모든 Quality 단계에 동일한 URP Pipeline 에셋을 연결합니다.
    /// </summary>
    /// <param name="pipelineAsset">프로젝트 전체에서 사용할 URP Pipeline 에셋입니다.</param>
    private static void AssignPipelineToProject(UniversalRenderPipelineAsset pipelineAsset)
    {
        GraphicsSettings.defaultRenderPipeline = pipelineAsset;

        // 프로젝트에 정의되어 있는 전체 Quality 단계의 개수입니다.
        int qualityLevelCount = QualitySettings.names.Length;

        // 설정 변경이 끝난 뒤 복원할 현재 Quality 단계의 번호입니다.
        int originalQualityLevelIndex = QualitySettings.GetQualityLevel();

        // 현재 URP Pipeline 에셋을 연결하고 있는 Quality 단계의 번호입니다.
        int qualityLevelIndex = 0;

        while (qualityLevelIndex < qualityLevelCount)
        {
            QualitySettings.SetQualityLevel(qualityLevelIndex, false);
            QualitySettings.renderPipeline = pipelineAsset;
            qualityLevelIndex++;
        }

        QualitySettings.SetQualityLevel(originalQualityLevelIndex, false);
    }
}
