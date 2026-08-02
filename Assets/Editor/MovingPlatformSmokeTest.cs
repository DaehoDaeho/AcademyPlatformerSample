using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>3스테이지 이동 플랫폼이 탑승한 플레이어의 상대 위치를 유지하는지 플레이 모드에서 검사합니다.</summary>
[InitializeOnLoad]
public static class MovingPlatformSmokeTest
{
    // 테스트가 실행 중인지 저장하는 세션 키입니다.
    private const string RunningKey =
        "AcademyPlatformer.MovingPlatformSmokeRunning";
    // 플레이어 배치가 끝났는지 저장하는 세션 키입니다.
    private const string SetupKey =
        "AcademyPlatformer.MovingPlatformSmokeSetup";
    // 상대 위치 측정이 시작됐는지 저장하는 세션 키입니다.
    private const string MeasuringKey =
        "AcademyPlatformer.MovingPlatformSmokeMeasuring";
    // 테스트 시작 시간을 저장하는 세션 키입니다.
    private const string StartTimeKey =
        "AcademyPlatformer.MovingPlatformSmokeStart";
    // 상대 위치 측정 시작 시간을 저장하는 세션 키입니다.
    private const string MeasureTimeKey =
        "AcademyPlatformer.MovingPlatformMeasureStart";
    // 측정 시작 시 플레이어와 플랫폼의 상대 X 좌표를 저장하는 세션 키입니다.
    private const string RelativeXKey =
        "AcademyPlatformer.MovingPlatformRelativeX";
    // 측정 시작 시 플랫폼의 X 좌표를 저장하는 세션 키입니다.
    private const string PlatformXKey =
        "AcademyPlatformer.MovingPlatformStartX";
    // 테스트 실패 여부를 저장하는 세션 키입니다.
    private const string FailedKey =
        "AcademyPlatformer.MovingPlatformSmokeFailed";

    /// <summary>스크립트 재로딩 후 실행 중이던 테스트의 업데이트 처리를 다시 연결합니다.</summary>
    static MovingPlatformSmokeTest()
    {
        if (SessionState.GetBool(RunningKey, false) == false)
        {
            return;
        }

        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    /// <summary>3스테이지를 열고 이동 플랫폼 탑승 안정성 테스트를 시작합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Run Moving Platform Smoke Test")]
    public static void Run()
    {
        SessionState.SetBool(RunningKey, true);
        SessionState.SetBool(SetupKey, false);
        SessionState.SetBool(MeasuringKey, false);
        SessionState.SetBool(FailedKey, false);
        SessionState.SetFloat(
            StartTimeKey,
            (float)EditorApplication.timeSinceStartup);
        EditorSceneManager.OpenScene("Assets/Scenes/Stage3.unity");
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>플레이어 배치, 상대 위치 측정 및 테스트 종료를 순서대로 처리합니다.</summary>
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
            elapsedTime >= 0.7d &&
            SessionState.GetBool(SetupKey, false) == false)
        {
            PlacePlayerOnPlatform();
            return;
        }

        if (EditorApplication.isPlaying == true &&
            SessionState.GetBool(SetupKey, false) == true &&
            SessionState.GetBool(MeasuringKey, false) == false &&
            elapsedTime >= 1.2d)
        {
            BeginMeasurement();
            return;
        }

        double measureDuration =
            EditorApplication.timeSinceStartup -
            SessionState.GetFloat(MeasureTimeKey, 0f);
        if (EditorApplication.isPlaying == true &&
            SessionState.GetBool(MeasuringKey, false) == true &&
            measureDuration >= 2d)
        {
            CheckMeasurement();
            EditorApplication.ExitPlaymode();
            return;
        }

        bool finishedPlayMode =
            EditorApplication.isPlayingOrWillChangePlaymode == false;
        if (finishedPlayMode == true &&
            SessionState.GetBool(MeasuringKey, false) == true)
        {
            FinishTest();
        }
    }

    /// <summary>플레이어 조작을 끄고 현재 이동 플랫폼의 윗면 중앙에 배치합니다.</summary>
    private static void PlacePlayerOnPlatform()
    {
        MovingPlatform platform =
            Object.FindFirstObjectByType<MovingPlatform>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (platform == null || player == null)
        {
            RecordFailure("Moving platform smoke test could not find required objects.");
            EditorApplication.ExitPlaymode();
            return;
        }

        PlayerInputReader input = player.GetComponent<PlayerInputReader>();
        PlayerMotor2D motor = player.GetComponent<PlayerMotor2D>();
        PlayerJump jump = player.GetComponent<PlayerJump>();
        input.enabled = false;
        motor.enabled = false;
        jump.enabled = false;

        Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        Collider2D platformCollider = platform.GetComponent<Collider2D>();
        float playerY =
            platformCollider.bounds.max.y +
            playerCollider.bounds.extents.y + 0.03f;
        playerBody.position = new Vector2(
            platform.transform.position.x,
            playerY);
        playerBody.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
        SessionState.SetBool(SetupKey, true);
    }

    /// <summary>플레이어와 이동 플랫폼의 기준 상대 위치를 저장합니다.</summary>
    private static void BeginMeasurement()
    {
        MovingPlatform platform =
            Object.FindFirstObjectByType<MovingPlatform>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        float relativeX =
            player.transform.position.x - platform.transform.position.x;
        SessionState.SetFloat(RelativeXKey, relativeX);
        SessionState.SetFloat(PlatformXKey, platform.transform.position.x);
        SessionState.SetFloat(
            MeasureTimeKey,
            (float)EditorApplication.timeSinceStartup);
        SessionState.SetBool(MeasuringKey, true);
    }

    /// <summary>플랫폼이 이동한 동안 플레이어의 상대 X 위치가 유지됐는지 검사합니다.</summary>
    private static void CheckMeasurement()
    {
        MovingPlatform platform =
            Object.FindFirstObjectByType<MovingPlatform>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        float currentRelativeX =
            player.transform.position.x - platform.transform.position.x;
        float initialRelativeX =
            SessionState.GetFloat(RelativeXKey, 0f);
        float relativeDrift = Mathf.Abs(
            currentRelativeX - initialRelativeX);
        float platformTravel = Mathf.Abs(
            platform.transform.position.x -
            SessionState.GetFloat(PlatformXKey, platform.transform.position.x));
        bool platformMoved = platformTravel >= 0.5f;
        bool playerStayedCentered = relativeDrift <= 0.2f;
        if (platformMoved == false || playerStayedCentered == false)
        {
            RecordFailure(
                "Moving platform smoke test failed. Travel=" +
                platformTravel + ", drift=" + relativeDrift);
        }
    }

    /// <summary>테스트 결과를 콘솔에 출력하고 Unity 프로세스를 종료합니다.</summary>
    private static void FinishTest()
    {
        bool failed = SessionState.GetBool(FailedKey, false);
        SessionState.SetBool(RunningKey, false);
        EditorApplication.update -= Update;
        Debug.Log(
            failed == true
                ? "MOVING_PLATFORM_SMOKE_TEST_FAILED"
                : "MOVING_PLATFORM_SMOKE_TEST_PASSED");
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
