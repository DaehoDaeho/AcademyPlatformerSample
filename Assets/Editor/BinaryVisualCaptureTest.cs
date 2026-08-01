using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>사망 흑백 효과가 적용된 실제 게임 화면을 이미지로 캡처합니다.</summary>
public static class BinaryVisualCaptureTest
{
    // 테스트를 시작한 에디터 시간입니다.
    private static double startTime;
    // 흑백 효과를 활성화했는지 나타냅니다.
    private static bool hasActivatedEffect;
    // 화면 캡처를 요청했는지 나타냅니다.
    private static bool hasCapturedScreen;

    /// <summary>Main 씬을 실행하고 흑백 효과의 실제 출력 캡처를 시작합니다.</summary>
    public static void Run()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        startTime = EditorApplication.timeSinceStartup;
        hasActivatedEffect = false;
        hasCapturedScreen = false;
        EditorApplication.update += UpdateCapture;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>효과 활성화, 캡처, 에디터 종료를 시간 순서대로 실행합니다.</summary>
    private static void UpdateCapture()
    {
        double elapsedTime = EditorApplication.timeSinceStartup - startTime;
        if (EditorApplication.isPlaying == false)
        {
            return;
        }

        if (elapsedTime >= 2d && hasActivatedEffect == false)
        {
            DeathScreenGrayscale effect =
                Object.FindFirstObjectByType<DeathScreenGrayscale>();
            if (effect != null)
            {
                effect.Play();
                hasActivatedEffect = true;
            }
        }

        if (elapsedTime >= 3d && hasCapturedScreen == false)
        {
            ScreenCapture.CaptureScreenshot(
                "C:/SampleProject/AcademyPlatformerSample/Logs/BinaryVisualCapture.png");
            hasCapturedScreen = true;
        }

        if (elapsedTime >= 5d)
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
