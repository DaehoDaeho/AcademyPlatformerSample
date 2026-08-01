using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 클리어 연출과 지연된 LEVEL CLEAR UI를 실제 Play Mode에서 자동 검증합니다.
/// </summary>
[InitializeOnLoad]
public static class ClearSequenceSmokeTest
{
    /// <summary>
    /// 테스트 실행 상태를 저장할 세션 키입니다.
    /// </summary>
    private const string RunningKey = "AcademyPlatformer.ClearSmokeRunning";

    /// <summary>
    /// 테스트 실패 상태를 저장할 세션 키입니다.
    /// </summary>
    private const string FailedKey = "AcademyPlatformer.ClearSmokeFailed";

    /// <summary>
    /// 테스트 시작 시간을 저장할 세션 키입니다.
    /// </summary>
    private const string StartKey = "AcademyPlatformer.ClearSmokeStart";

    /// <summary>
    /// 클리어 요청 완료 상태를 저장할 세션 키입니다.
    /// </summary>
    private const string ClearRequestedKey = "AcademyPlatformer.ClearRequested";

    /// <summary>
    /// 클리어 요청이 실제로 전달된 시간을 저장할 세션 키입니다.
    /// </summary>
    private const string ClearRequestTimeKey = "AcademyPlatformer.ClearRequestTime";

    /// <summary>
    /// 스크립트 재로딩 뒤 진행 중인 테스트 업데이트를 다시 연결합니다.
    /// </summary>
    static ClearSequenceSmokeTest()
    {
        if (SessionState.GetBool(RunningKey, false) == false)
        {
            return;
        }

        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    /// <summary>
    /// 메인 씬을 열고 클리어 연출 자동 검사를 시작합니다.
    /// </summary>
    [MenuItem("Tools/Academy Platformer/Run Clear Sequence Smoke Test")]
    public static void Run()
    {
        SessionState.SetBool(RunningKey, true);
        SessionState.SetBool(FailedKey, false);
        SessionState.SetBool(ClearRequestedKey, false);
        SessionState.SetFloat(StartKey, (float)EditorApplication.timeSinceStartup);
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>
    /// 지정된 시간에 클리어를 요청하고 UI 지연과 최종 정지 상태를 검사합니다.
    /// </summary>
    private static void Update()
    {
        if (SessionState.GetBool(RunningKey, false) == false)
        {
            return;
        }

        // 테스트를 시작한 뒤 흐른 에디터 시간입니다.
        double elapsedTime =
            EditorApplication.timeSinceStartup - SessionState.GetFloat(StartKey, 0f);

        if (EditorApplication.isPlaying == true &&
            elapsedTime >= 1d &&
            SessionState.GetBool(ClearRequestedKey, false) == false)
        {
            SessionState.SetBool(ClearRequestedKey, true);
            SessionState.SetFloat(
                ClearRequestTimeKey,
                (float)EditorApplication.timeSinceStartup);
            RequestClearAndCheckDelay();
        }

        // 실제 클리어 요청을 보낸 뒤 흐른 시간입니다.
        double elapsedAfterClearRequest =
            EditorApplication.timeSinceStartup -
            SessionState.GetFloat(ClearRequestTimeKey, (float)EditorApplication.timeSinceStartup);

        if (EditorApplication.isPlaying == true &&
            SessionState.GetBool(ClearRequestedKey, false) == true &&
            elapsedAfterClearRequest >= 1.6d)
        {
            CheckCompletedClear();
            EditorApplication.ExitPlaymode();
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode == true ||
            SessionState.GetBool(ClearRequestedKey, false) == false ||
            elapsedAfterClearRequest < 2d)
        {
            return;
        }

        // 테스트 과정에서 실패가 기록됐는지를 나타냅니다.
        bool failed = SessionState.GetBool(FailedKey, false);

        SessionState.SetBool(RunningKey, false);
        EditorApplication.update -= Update;
        Debug.Log(failed == true
            ? "CLEAR_SEQUENCE_SMOKE_TEST_FAILED"
            : "CLEAR_SEQUENCE_SMOKE_TEST_PASSED");
        EditorApplication.Exit(failed == true ? 1 : 0);
    }

    /// <summary>
    /// 클리어 직후 UI와 게임 정지가 발생하지 않고 연출이 시작됐는지 검사합니다.
    /// </summary>
    private static void RequestClearAndCheckDelay()
    {
        // 씬에 배치된 게임 상태 관리자입니다.
        GameManager manager = GameManager.Instance;

        // 클리어 UI 표시 여부를 확인할 HUD입니다.
        GameHUD hud = Object.FindFirstObjectByType<GameHUD>();

        // 플레이어에 연결된 클리어 연출 컴포넌트입니다.
        PlayerClearSequence clearSequence =
            Object.FindFirstObjectByType<PlayerClearSequence>();

        if (manager == null)
        {
            RecordFailure("Clear sequence test failed: GameManager is missing.");
            return;
        }

        manager.Win();

        if (manager.GameEnded == true ||
            hud == null ||
            hud.EndScreenVisible == true ||
            clearSequence == null ||
            clearSequence.IsPlaying == false ||
            Mathf.Approximately(Time.timeScale, 1f) == false)
        {
            RecordFailure("Clear sequence test failed: LEVEL CLEAR appeared before the presentation finished.");
        }
    }

    /// <summary>
    /// 연출이 끝난 뒤 LEVEL CLEAR UI와 게임 정지가 적용됐는지 검사합니다.
    /// </summary>
    private static void CheckCompletedClear()
    {
        // 최종 승리 상태를 확인할 게임 상태 관리자입니다.
        GameManager manager = GameManager.Instance;

        // 최종 LEVEL CLEAR UI 표시 여부를 확인할 HUD입니다.
        GameHUD hud = Object.FindFirstObjectByType<GameHUD>();
        PlayerClearSequence clearSequence =
            Object.FindFirstObjectByType<PlayerClearSequence>();
        SpriteRenderer playerRenderer =
            clearSequence != null
                ? clearSequence.GetComponentInChildren<SpriteRenderer>()
                : null;

        if (manager == null ||
            manager.GameEnded == false ||
            hud == null ||
            hud.EndScreenVisible == false ||
            playerRenderer == null ||
            playerRenderer.enabled == true ||
            Mathf.Approximately(Time.timeScale, 0f) == false)
        {
            RecordFailure("Clear sequence delay test failed: LEVEL CLEAR did not appear after the presentation.");
        }
    }

    /// <summary>
    /// 테스트 실패 상태와 원인을 기록합니다.
    /// </summary>
    /// <param name="message">Console에 출력할 실패 원인입니다.</param>
    private static void RecordFailure(string message)
    {
        SessionState.SetBool(FailedKey, true);
        Debug.LogError(message);
    }
}
