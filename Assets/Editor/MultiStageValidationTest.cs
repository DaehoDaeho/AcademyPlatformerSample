using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>3개 스테이지의 씬, 적 구성, 색상과 선택 화면 구성을 자동으로 검증합니다.</summary>
public static class MultiStageValidationTest
{
    /// <summary>생성된 모든 스테이지와 빌드 목록을 검사하고 결과를 기록합니다.</summary>
    public static void Run()
    {
        bool passed = true;
        passed = ValidateUnlockProgression() && passed;
        passed = ValidateBuildSceneList() && passed;
        passed = ValidateStageSelectScene() && passed;
        passed = ValidateStage(
            1,
            new Color(1f, 1f, 1f)) && passed;
        passed = ValidateStage(
            2,
            new Color(0.68f, 0.82f, 1f)) && passed;
        passed = ValidateStage(
            3,
            new Color(1f, 0.62f, 0.38f)) && passed;

        Debug.Log(
            passed == true
                ? "MULTI_STAGE_VALIDATION_PASSED"
                : "MULTI_STAGE_VALIDATION_FAILED");
        EditorApplication.Exit(passed == true ? 0 : 1);
    }

    /// <summary>이전 스테이지 클리어에 따라 다음 스테이지만 순서대로 해금되는지 확인합니다.</summary>
    private static bool ValidateUnlockProgression()
    {
        const string progressKey = "HighestUnlockedStage";
        int originalProgress = PlayerPrefs.GetInt(progressKey, 1);
        PlayerPrefs.SetInt(progressKey, 1);

        bool initialState =
            StageProgressData.IsStageUnlocked(1) == true &&
            StageProgressData.IsStageUnlocked(2) == false &&
            StageProgressData.IsStageUnlocked(3) == false;
        StageProgressData.RecordStageClear(1);
        bool secondStageState =
            StageProgressData.IsStageUnlocked(2) == true &&
            StageProgressData.IsStageUnlocked(3) == false;
        StageProgressData.RecordStageClear(2);
        bool thirdStageState =
            StageProgressData.IsStageUnlocked(3) == true;

        PlayerPrefs.SetInt(progressKey, originalProgress);
        PlayerPrefs.Save();
        return initialState == true &&
            secondStageState == true &&
            thirdStageState == true;
    }

    /// <summary>타이틀, 선택 화면과 3개 스테이지가 올바른 순서로 등록됐는지 확인합니다.</summary>
    private static bool ValidateBuildSceneList()
    {
        string[] expectedPaths =
        {
            "Assets/Scenes/Title.unity",
            "Assets/Scenes/StageSelect.unity",
            "Assets/Scenes/Stage1.unity",
            "Assets/Scenes/Stage2.unity",
            "Assets/Scenes/Stage3.unity"
        };
        string[] actualPaths = EditorBuildSettings.scenes
            .Where(scene => scene.enabled == true)
            .Select(scene => scene.path)
            .ToArray();
        return expectedPaths.SequenceEqual(actualPaths);
    }

    /// <summary>선택 화면에 3개의 스테이지 버튼이 존재하는지 확인합니다.</summary>
    private static bool ValidateStageSelectScene()
    {
        EditorSceneManager.OpenScene(
            "Assets/Scenes/StageSelect.unity");
        StageSelectButton[] buttons =
            Object.FindObjectsByType<StageSelectButton>(
                FindObjectsSortMode.None);
        return buttons.Length == StageProgressData.TotalStageCount;
    }

    /// <summary>스테이지 하나에 세 종류의 적이 공존하고 타일 색상이 올바른지 확인합니다.</summary>
    /// <param name="stageNumber">검사할 스테이지 번호입니다.</param>
    /// <param name="tileColor">해당 스테이지의 타일 색상입니다.</param>
    private static bool ValidateStage(
        int stageNumber,
        Color tileColor)
    {
        EditorSceneManager.OpenScene(
            "Assets/Scenes/Stage" + stageNumber + ".unity");
        Tilemap tilemap = Object.FindFirstObjectByType<Tilemap>();
        StompableEnemy[] enemies =
            Object.FindObjectsByType<StompableEnemy>(
                FindObjectsSortMode.None);
        bool hasPatrolEnemy = enemies.Any(
            enemy => enemy.GetComponent<PatrolEnemy>() != null);
        bool hasChasingEnemy = enemies.Any(
            enemy => enemy.GetComponent<ChasingEnemy>() != null);
        bool hasRangedEnemy = enemies.Any(
            enemy => enemy.GetComponent<RangedEnemyLookout>() != null);
        bool colorMatches =
            tilemap != null &&
            Approximately(tilemap.color, tileColor);
        return enemies.Length > 0 &&
            hasPatrolEnemy == true &&
            hasChasingEnemy == true &&
            hasRangedEnemy == true &&
            colorMatches == true;
    }

    /// <summary>두 색상의 각 채널 값이 거의 같은지 확인합니다.</summary>
    /// <param name="left">첫 번째 색상입니다.</param>
    /// <param name="right">두 번째 색상입니다.</param>
    private static bool Approximately(Color left, Color right)
    {
        return Mathf.Approximately(left.r, right.r) == true &&
            Mathf.Approximately(left.g, right.g) == true &&
            Mathf.Approximately(left.b, right.b) == true;
    }
}
