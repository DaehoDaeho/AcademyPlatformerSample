using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>스테이지 선택 화면의 실제 레이아웃을 이미지로 캡처합니다.</summary>
public static class StageSelectVisualCaptureTest
{
    // 테스트를 시작한 에디터 시간입니다.
    private static double startTime;
    // 화면 캡처를 요청했는지 나타냅니다.
    private static bool hasCapturedScreen;

    /// <summary>스테이지 선택 씬을 실행하고 화면 캡처를 시작합니다.</summary>
    public static void Run()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene(
            "Assets/Scenes/StageSelect.unity");
        startTime = EditorApplication.timeSinceStartup;
        hasCapturedScreen = false;
        EditorApplication.update += UpdateCapture;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>UI 초기화를 기다린 뒤 선택 화면을 캡처하고 에디터를 종료합니다.</summary>
    private static void UpdateCapture()
    {
        double elapsedTime =
            EditorApplication.timeSinceStartup - startTime;
        if (EditorApplication.isPlaying == false)
        {
            return;
        }

        if (elapsedTime >= 2d && hasCapturedScreen == false)
        {
            ScreenCapture.CaptureScreenshot(
                "C:/SampleProject/AcademyPlatformerSample/Logs/StageSelectCapture.png");
            hasCapturedScreen = true;
        }

        if (elapsedTime >= 4d)
        {
            EditorApplication.update -= UpdateCapture;
            EditorApplication.ExitPlaymode();
            EditorApplication.delayCall += ExitEditor;
        }
    }

    /// <summary>캡처 파일 저장을 마친 Unity 에디터를 종료합니다.</summary>
    private static void ExitEditor()
    {
        EditorSettings.enterPlayModeOptionsEnabled = false;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
        EditorApplication.Exit(0);
    }
}
