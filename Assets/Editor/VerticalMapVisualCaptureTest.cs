using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>세로 상승 구간 정상 렌더링을 확인하기 위해 정상 부근 화면을 캡처합니다.</summary>
public static class VerticalMapVisualCaptureTest
{
    // 테스트를 시작한 에디터 시간입니다.
    private static double startTime;
    // 플레이어를 정상 구간으로 이동했는지 나타냅니다.
    private static bool hasMovedPlayer;
    // 정상 구간 화면을 캡처했는지 나타냅니다.
    private static bool hasCapturedScreen;

    /// <summary>Main 씬을 실행하고 정상 구간 캡처 테스트를 시작합니다.</summary>
    public static void Run()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        startTime = EditorApplication.timeSinceStartup;
        hasMovedPlayer = false;
        hasCapturedScreen = false;
        EditorApplication.update += UpdateCapture;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>플레이어 이동, 카메라 대기, 화면 캡처를 순서대로 진행합니다.</summary>
    private static void UpdateCapture()
    {
        double elapsedTime = EditorApplication.timeSinceStartup - startTime;
        if (EditorApplication.isPlaying == false)
        {
            return;
        }

        if (elapsedTime >= 2d && hasMovedPlayer == false)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(68f, 30.2f, 0f);
                Rigidbody2D body = player.GetComponent<Rigidbody2D>();
                if (body != null)
                {
                    body.linearVelocity = Vector2.zero;
                }
                hasMovedPlayer = true;
            }
        }

        if (elapsedTime >= 4d && hasCapturedScreen == false)
        {
            ScreenCapture.CaptureScreenshot(
                "C:/SampleProject/AcademyPlatformerSample/Logs/VerticalSummitCapture.png");
            hasCapturedScreen = true;
        }

        if (elapsedTime >= 6d)
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
