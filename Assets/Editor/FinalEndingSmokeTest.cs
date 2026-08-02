using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>3스테이지 클리어부터 전용 엔딩 씬 진입까지의 전체 흐름을 플레이 모드에서 검사합니다.</summary>
[InitializeOnLoad]
public static class FinalEndingSmokeTest
{
    // 엔딩 흐름 테스트 실행 상태를 저장하는 세션 키입니다.
    private const string RunningKey =
        "AcademyPlatformer.FinalEndingSmokeRunning";
    // 마지막 스테이지 클리어 요청 여부를 저장하는 세션 키입니다.
    private const string ClearRequestedKey =
        "AcademyPlatformer.FinalEndingClearRequested";
    // 중간 종료 연출 검사 완료 여부를 저장하는 세션 키입니다.
    private const string PresentationCheckedKey =
        "AcademyPlatformer.FinalEndingPresentationChecked";
    // 테스트 실패 여부를 저장하는 세션 키입니다.
    private const string FailedKey =
        "AcademyPlatformer.FinalEndingSmokeFailed";
    // 테스트 시작 시간을 저장하는 세션 키입니다.
    private const string StartTimeKey =
        "AcademyPlatformer.FinalEndingSmokeStart";
    // 마지막 스테이지 클리어를 실제 요청한 시간을 저장하는 세션 키입니다.
    private const string ClearRequestTimeKey =
        "AcademyPlatformer.FinalEndingClearRequestTime";

    /// <summary>스크립트 재로딩 후 실행 중인 테스트의 업데이트 처리를 다시 연결합니다.</summary>
    static FinalEndingSmokeTest()
    {
        if (SessionState.GetBool(RunningKey, false) == false)
        {
            return;
        }

        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    /// <summary>3스테이지를 열고 마지막 클리어 엔딩 흐름 테스트를 시작합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Run Final Ending Smoke Test")]
    public static void Run()
    {
        SessionState.SetBool(RunningKey, true);
        SessionState.SetBool(ClearRequestedKey, false);
        SessionState.SetBool(PresentationCheckedKey, false);
        SessionState.SetBool(FailedKey, false);
        SessionState.SetFloat(
            StartTimeKey,
            (float)EditorApplication.timeSinceStartup);
        EditorSceneManager.OpenScene("Assets/Scenes/Stage3.unity");
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>클리어 요청, 종료 연출 검사, 엔딩 씬 검사를 시간 순서대로 실행합니다.</summary>
    private static void Update()
    {
        if (SessionState.GetBool(RunningKey, false) == false)
        {
            return;
        }

        double elapsedTime =
            EditorApplication.timeSinceStartup -
            SessionState.GetFloat(StartTimeKey, 0f);
        if (EditorApplication.isPlaying == true &&
            elapsedTime >= 1d &&
            SessionState.GetBool(ClearRequestedKey, false) == false)
        {
            RequestFinalClear();
            return;
        }

        double elapsedAfterClearRequest =
            EditorApplication.timeSinceStartup -
            SessionState.GetFloat(
                ClearRequestTimeKey,
                (float)EditorApplication.timeSinceStartup);

        if (EditorApplication.isPlaying == true &&
            elapsedAfterClearRequest >= 1.7d &&
            SessionState.GetBool(PresentationCheckedKey, false) == false)
        {
            CheckFinalPresentation();
            return;
        }

        if (EditorApplication.isPlaying == true &&
            elapsedAfterClearRequest >= 5.8d)
        {
            CheckEndingScene();
            EditorApplication.ExitPlaymode();
            return;
        }

        bool finishedPlayMode =
            EditorApplication.isPlayingOrWillChangePlaymode == false;
        if (finishedPlayMode == true &&
            SessionState.GetBool(ClearRequestedKey, false) == true)
        {
            FinishTest();
        }
    }

    /// <summary>현재 3스테이지의 GameManager에 클리어를 요청합니다.</summary>
    private static void RequestFinalClear()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null || manager.StageNumber != 3)
        {
            RecordFailure("Final ending smoke test could not find Stage3 GameManager.");
            EditorApplication.ExitPlaymode();
            return;
        }

        SessionState.SetBool(ClearRequestedKey, true);
        SessionState.SetFloat(
            ClearRequestTimeKey,
            (float)EditorApplication.timeSinceStartup);
        manager.Win();
    }

    /// <summary>일반 클리어 패널이 숨겨지고 팡파레와 폭죽 연출이 시작됐는지 검사합니다.</summary>
    private static void CheckFinalPresentation()
    {
        GameManager manager = GameManager.Instance;
        GameHUD hud = Object.FindFirstObjectByType<GameHUD>();
        GameEndPresentation presentation =
            Object.FindFirstObjectByType<GameEndPresentation>();
        AudioSource presentationAudio = presentation == null
            ? null
            : presentation.GetComponent<AudioSource>();
        GameObject fireworkLayer = GameObject.Find("Clear Fireworks Layer");
        bool validPresentation =
            manager != null &&
            manager.GameEnded == true &&
            hud != null &&
            hud.EndScreenVisible == false &&
            presentation != null &&
            presentationAudio != null &&
            presentationAudio.isPlaying == true &&
            fireworkLayer != null &&
            fireworkLayer.transform.childCount > 0;
        if (validPresentation == false)
        {
            RecordFailure(
                "Final ending presentation did not play before the Ending scene.");
        }

        SessionState.SetBool(PresentationCheckedKey, true);
    }

    /// <summary>페이드 전환 후 엔딩 씬의 핵심 UI와 버튼이 생성됐는지 검사합니다.</summary>
    private static void CheckEndingScene()
    {
        bool loadedEndingScene =
            SceneManager.GetActiveScene().name == "Ending";
        EndingScreenController controller =
            Object.FindFirstObjectByType<EndingScreenController>();
        Button[] buttons = Object.FindObjectsByType<Button>(
            FindObjectsSortMode.None);
        GameObject endingTitle = GameObject.Find("Ending Title");
        bool validEndingScreen =
            loadedEndingScene == true &&
            controller != null &&
            buttons.Length == 3 &&
            endingTitle != null;
        if (validEndingScreen == false)
        {
            RecordFailure(
                "Ending scene did not contain the expected controller, title, and buttons.");
        }
    }

    /// <summary>테스트 결과를 출력하고 Unity 프로세스를 종료합니다.</summary>
    private static void FinishTest()
    {
        bool failed = SessionState.GetBool(FailedKey, false);
        SessionState.SetBool(RunningKey, false);
        EditorApplication.update -= Update;
        Debug.Log(
            failed == true
                ? "FINAL_ENDING_SMOKE_TEST_FAILED"
                : "FINAL_ENDING_SMOKE_TEST_PASSED");
        EditorApplication.Exit(failed == true ? 1 : 0);
    }

    /// <summary>테스트 실패 상태와 실패 원인을 기록합니다.</summary>
    /// <param name="message">콘솔에 출력할 실패 원인입니다.</param>
    private static void RecordFailure(string message)
    {
        SessionState.SetBool(FailedKey, true);
        Debug.LogError(message);
    }
}
