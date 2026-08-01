using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>타이틀과 스테이지 선택 화면을 반복 이동한 뒤 스테이지 2 진입을 검증합니다.</summary>
public static class SceneTransitionStressTest
{
    // 현재까지 완료한 씬 전환 횟수입니다.
    private static int transitionCount;
    // 마지막 씬 전환을 요청한 에디터 시간입니다.
    private static double lastRequestTime;
    // 전체 테스트가 시작된 에디터 시간입니다.
    private static double testStartTime;
    // 현재 씬에 대한 전환 요청을 이미 보냈는지 나타냅니다.
    private static bool hasRequestedTransition;

    /// <summary>크래시가 발생했던 순서를 자동으로 반복하는 테스트를 시작합니다.</summary>
    public static void Run()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene("Assets/Scenes/Title.unity");
        transitionCount = 0;
        lastRequestTime = 0d;
        testStartTime = EditorApplication.timeSinceStartup;
        hasRequestedTransition = false;
        EditorApplication.update += UpdateTest;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>현재 씬을 확인하고 다음 화면으로 순서대로 이동합니다.</summary>
    private static void UpdateTest()
    {
        if (EditorApplication.isPlaying == false)
        {
            return;
        }

        if (EditorApplication.timeSinceStartup - testStartTime > 45d)
        {
            FinishTest(false, "제한 시간 안에 반복 씬 전환을 마치지 못했습니다.");
            return;
        }

        if (hasRequestedTransition == true)
        {
            if (EditorApplication.timeSinceStartup - lastRequestTime < 1.8d)
            {
                return;
            }

            hasRequestedTransition = false;
            transitionCount += 1;
        }

        if (transitionCount < 6)
        {
            string nextSceneName =
                SceneManager.GetActiveScene().name == "Title"
                ? "StageSelect"
                : "Title";
            RequestTransition(nextSceneName);
            return;
        }

        if (transitionCount == 6)
        {
            RequestTransition("StageSelect");
            return;
        }

        if (transitionCount == 7)
        {
            RequestTransition("Stage2");
            return;
        }

        if (SceneManager.GetActiveScene().name == "Stage2")
        {
            FinishTest(true, "반복 전환 후 스테이지 2 진입에 성공했습니다.");
        }
    }

    /// <summary>지정한 씬으로 페이드 전환을 요청하고 완료 대기를 시작합니다.</summary>
    /// <param name="sceneName">불러올 씬의 이름입니다.</param>
    private static void RequestTransition(string sceneName)
    {
        hasRequestedTransition = true;
        lastRequestTime = EditorApplication.timeSinceStartup;
        SceneFadeController.LoadSceneWithFade(sceneName);
    }

    /// <summary>테스트 결과를 기록하고 Play Mode와 배치 에디터를 종료합니다.</summary>
    /// <param name="hasPassed">검증 성공 여부입니다.</param>
    /// <param name="message">결과를 설명하는 문장입니다.</param>
    private static void FinishTest(bool hasPassed, string message)
    {
        EditorApplication.update -= UpdateTest;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
        if (hasPassed == true)
        {
            Debug.Log("SCENE_TRANSITION_STRESS_TEST_PASSED: " + message);
        }
        else
        {
            Debug.LogError("SCENE_TRANSITION_STRESS_TEST_FAILED: " + message);
        }

        EditorApplication.ExitPlaymode();
        EditorApplication.delayCall += () => EditorApplication.Exit(hasPassed == true ? 0 : 1);
    }
}
