using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>타이틀 화면에서 게임 씬까지 이어지는 페이드 전환을 자동으로 검증합니다.</summary>
public static class TitleScreenSmokeTest
{
    // 테스트가 시작된 에디터 시간입니다.
    private static double testStartTime;
    // 타이틀에서 시작 요청을 보냈는지 나타냅니다.
    private static bool hasRequestedStart;

    /// <summary>타이틀 씬을 열고 Play Mode 전환 검증을 시작합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Run Title Screen Smoke Test")]
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Title.unity");
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        testStartTime = EditorApplication.timeSinceStartup;
        hasRequestedStart = false;
        EditorApplication.update += CheckTransition;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>타이틀 컨트롤러를 실행하고 게임 씬의 페이드 완료 상태를 확인합니다.</summary>
    private static void CheckTransition()
    {
        if (EditorApplication.isPlaying == false)
        {
            if (EditorApplication.timeSinceStartup - testStartTime > 20f)
            {
                Fail("Play Mode가 제한 시간 안에 시작되지 않았습니다.");
            }
            return;
        }

        if (hasRequestedStart == false)
        {
            TitleScreenController titleController = Object.FindFirstObjectByType<TitleScreenController>();
            if (titleController == null)
            {
                Fail("타이틀 화면 컨트롤러를 찾을 수 없습니다.");
                return;
            }

            hasRequestedStart = true;
            titleController.StartGame();
            return;
        }

        if (SceneManager.GetActiveScene().name == "StageSelect" &&
            Time.timeScale == 1f &&
            EditorApplication.timeSinceStartup - testStartTime > 2f)
        {
            Debug.Log("TITLE_SCREEN_SMOKE_TEST_PASSED");
            EditorApplication.update -= CheckTransition;
            EditorApplication.ExitPlaymode();
            EditorApplication.delayCall += ExitEditor;
            return;
        }

        if (EditorApplication.timeSinceStartup - testStartTime > 20f)
        {
            Fail("타이틀에서 게임 씬으로 전환되지 않았습니다.");
        }
    }

    /// <summary>테스트 실패 원인을 기록한 뒤 에디터를 종료합니다.</summary>
    /// <param name="message">실패 원인을 설명하는 문장입니다.</param>
    private static void Fail(string message)
    {
        Debug.LogError("TITLE_SCREEN_SMOKE_TEST_FAILED: " + message);
        EditorApplication.update -= CheckTransition;
        EditorApplication.ExitPlaymode();
        EditorApplication.delayCall += ExitEditorWithError;
    }

    /// <summary>성공한 배치 테스트의 Unity 에디터를 종료합니다.</summary>
    private static void ExitEditor()
    {
        RestorePlayModeSettings();
        EditorApplication.Exit(0);
    }

    /// <summary>실패한 배치 테스트의 Unity 에디터를 오류 코드와 함께 종료합니다.</summary>
    private static void ExitEditorWithError()
    {
        RestorePlayModeSettings();
        EditorApplication.Exit(1);
    }

    /// <summary>테스트가 변경한 Play Mode 옵션을 안전한 기본값으로 되돌립니다.</summary>
    private static void RestorePlayModeSettings()
    {
        EditorSettings.enterPlayModeOptionsEnabled = false;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
    }
}
