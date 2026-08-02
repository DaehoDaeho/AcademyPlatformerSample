using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>플레이 중에는 커서가 숨고 버튼 UI가 나타날 때만 표시되는지 플레이 모드에서 검사합니다.</summary>
[InitializeOnLoad]
public static class CursorVisibilitySmokeTest
{
    // 커서 표시 테스트 실행 상태를 저장하는 세션 키입니다.
    private const string RunningKey =
        "AcademyPlatformer.CursorSmokeRunning";
    // 현재 테스트 진행 단계를 저장하는 세션 키입니다.
    private const string PhaseKey =
        "AcademyPlatformer.CursorSmokePhase";
    // 현재 테스트 단계가 시작된 시간을 저장하는 세션 키입니다.
    private const string PhaseTimeKey =
        "AcademyPlatformer.CursorSmokePhaseTime";
    // 테스트 실패 여부를 저장하는 세션 키입니다.
    private const string FailedKey =
        "AcademyPlatformer.CursorSmokeFailed";

    /// <summary>스크립트 재로딩 후 진행 중인 테스트의 업데이트 처리를 다시 연결합니다.</summary>
    static CursorVisibilitySmokeTest()
    {
        if (SessionState.GetBool(RunningKey, false) == false)
        {
            return;
        }

        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    /// <summary>1스테이지를 열고 커서 표시 상태 테스트를 시작합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Run Cursor Visibility Smoke Test")]
    public static void Run()
    {
        SessionState.SetBool(RunningKey, true);
        SessionState.SetBool(FailedKey, false);
        SessionState.SetInt(PhaseKey, 0);
        SessionState.SetFloat(
            PhaseTimeKey,
            (float)EditorApplication.timeSinceStartup);
        EditorSceneManager.OpenScene("Assets/Scenes/Stage1.unity");
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>일반 플레이, 클리어 UI, 씬 재시작 단계의 커서 상태를 순서대로 검사합니다.</summary>
    private static void Update()
    {
        if (SessionState.GetBool(RunningKey, false) == false)
        {
            return;
        }

        if (EditorApplication.isPlaying == false)
        {
            bool playModeFinished =
                EditorApplication.isPlayingOrWillChangePlaymode == false;
            if (playModeFinished == true &&
                SessionState.GetInt(PhaseKey, 0) >= 3)
            {
                FinishTest();
            }
            return;
        }

        double phaseElapsedTime =
            EditorApplication.timeSinceStartup -
            SessionState.GetFloat(PhaseTimeKey, 0f);
        int currentPhase = SessionState.GetInt(PhaseKey, 0);
        if (currentPhase == 0 && phaseElapsedTime >= 0.7d)
        {
            CheckGameplayAndRequestClear();
            return;
        }

        if (currentPhase == 1 && phaseElapsedTime >= 1.7d)
        {
            CheckClearUiAndRestart();
            return;
        }

        if (currentPhase == 2 && phaseElapsedTime >= 2d)
        {
            CheckRestartedGameplay();
            SessionState.SetInt(PhaseKey, 3);
            EditorApplication.ExitPlaymode();
        }
    }

    /// <summary>버튼이 없는 일반 플레이에서 커서가 숨었는지 확인하고 클리어를 요청합니다.</summary>
    private static void CheckGameplayAndRequestClear()
    {
        CursorVisibilityController controller =
            Object.FindFirstObjectByType<CursorVisibilityController>();
        bool cursorIsHidden =
            controller != null &&
            controller.CursorShouldBeVisible == false;
        if (cursorIsHidden == false)
        {
            RecordFailure("Cursor was visible during normal gameplay.");
        }

        GameManager.Instance.Win();
        BeginNextPhase(1);
    }

    /// <summary>일반 스테이지 클리어 버튼 UI에서 커서가 나타났는지 확인하고 씬을 재시작합니다.</summary>
    private static void CheckClearUiAndRestart()
    {
        CursorVisibilityController controller =
            Object.FindFirstObjectByType<CursorVisibilityController>();
        GameHUD hud = Object.FindFirstObjectByType<GameHUD>();
        bool cursorIsVisibleWithUi =
            controller != null &&
            controller.CursorShouldBeVisible == true &&
            hud != null &&
            hud.EndScreenVisible == true;
        if (cursorIsVisibleWithUi == false)
        {
            RecordFailure("Cursor did not appear with the clear button UI.");
        }

        GameManager.Instance.Restart();
        BeginNextPhase(2);
    }

    /// <summary>씬 재시작 후 버튼 UI와 커서가 다시 숨었는지 확인합니다.</summary>
    private static void CheckRestartedGameplay()
    {
        CursorVisibilityController controller =
            Object.FindFirstObjectByType<CursorVisibilityController>();
        GameHUD hud = Object.FindFirstObjectByType<GameHUD>();
        bool returnedToHiddenState =
            SceneManager.GetActiveScene().name == "Stage1" &&
            controller != null &&
            controller.CursorShouldBeVisible == false &&
            hud != null &&
            hud.EndScreenVisible == false;
        if (returnedToHiddenState == false)
        {
            RecordFailure("Cursor did not hide after the button UI disappeared.");
        }
    }

    /// <summary>테스트 진행 단계를 변경하고 새 단계의 시작 시간을 기록합니다.</summary>
    /// <param name="nextPhase">다음에 실행할 테스트 단계 번호입니다.</param>
    private static void BeginNextPhase(int nextPhase)
    {
        SessionState.SetInt(PhaseKey, nextPhase);
        SessionState.SetFloat(
            PhaseTimeKey,
            (float)EditorApplication.timeSinceStartup);
    }

    /// <summary>테스트 결과를 출력하고 Unity 프로세스를 종료합니다.</summary>
    private static void FinishTest()
    {
        bool failed = SessionState.GetBool(FailedKey, false);
        SessionState.SetBool(RunningKey, false);
        EditorApplication.update -= Update;
        Debug.Log(
            failed == true
                ? "CURSOR_VISIBILITY_SMOKE_TEST_FAILED"
                : "CURSOR_VISIBILITY_SMOKE_TEST_PASSED");
        EditorApplication.Exit(failed == true ? 1 : 0);
    }

    /// <summary>테스트 실패 상태와 원인을 기록합니다.</summary>
    /// <param name="message">콘솔에 출력할 실패 원인입니다.</param>
    private static void RecordFailure(string message)
    {
        SessionState.SetBool(FailedKey, true);
        Debug.LogError(message);
    }
}
