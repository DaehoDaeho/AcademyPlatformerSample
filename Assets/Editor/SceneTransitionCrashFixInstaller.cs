using UnityEditor;

/// <summary>씬 전환 크래시 수정에 필요한 씬 재생성과 에디터 설정 복구를 한 번에 수행합니다.</summary>
public static class SceneTransitionCrashFixInstaller
{
    /// <summary>타이틀 씬을 다시 만들고 도메인 다시 로드가 활성화되는 안전한 Play Mode 설정을 저장합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Apply Scene Transition Crash Fix")]
    public static void ApplyFix()
    {
        EditorSettings.enterPlayModeOptionsEnabled = false;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
        TitleScreenBuilder.BuildTitleScene();
        AssetDatabase.SaveAssets();
        UnityEngine.Debug.Log("SCENE_TRANSITION_CRASH_FIX_APPLIED");
    }
}
