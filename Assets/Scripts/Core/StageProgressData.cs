using UnityEngine;

/// <summary>클리어한 스테이지와 현재 해금된 최고 스테이지를 저장하고 제공합니다.</summary>
public static class StageProgressData
{
    // 해금된 최고 스테이지를 PlayerPrefs에 저장할 때 사용하는 키입니다.
    private const string HighestUnlockedStageKey = "HighestUnlockedStage";
    // 각 스테이지의 실제 클리어 여부를 저장할 키의 앞부분입니다.
    private const string ClearedStageKeyPrefix = "ClearedStage";
    // 각 스테이지에서 가장 많이 획득한 스타 수를 저장할 키의 앞부분입니다.
    private const string CollectedStarsKeyPrefix = "CollectedStars";
    // 각 스테이지에 배치된 전체 스타 수를 저장할 키의 앞부분입니다.
    private const string AvailableStarsKeyPrefix = "AvailableStars";
    // 현재 프로젝트에 포함된 전체 스테이지 수입니다.
    public const int TotalStageCount = 3;

    /// <summary>지정한 스테이지에 입장할 수 있는지 확인합니다.</summary>
    /// <param name="stageNumber">확인할 스테이지 번호입니다.</param>
    public static bool IsStageUnlocked(int stageNumber)
    {
        int highestUnlockedStage =
            PlayerPrefs.GetInt(HighestUnlockedStageKey, 1);
        return stageNumber >= 1 &&
            stageNumber <= highestUnlockedStage;
    }

    /// <summary>클리어한 스테이지를 기록하고 다음 스테이지를 해금합니다.</summary>
    /// <param name="clearedStageNumber">클리어한 스테이지 번호입니다.</param>
    public static void RecordStageClear(int clearedStageNumber)
    {
        PlayerPrefs.SetInt(
            ClearedStageKeyPrefix + clearedStageNumber,
            1);
        int nextStageNumber = Mathf.Min(
            clearedStageNumber + 1,
            TotalStageCount);
        int highestUnlockedStage =
            PlayerPrefs.GetInt(HighestUnlockedStageKey, 1);
        if (nextStageNumber > highestUnlockedStage)
        {
            PlayerPrefs.SetInt(
                HighestUnlockedStageKey,
                nextStageNumber);
        }
        PlayerPrefs.Save();
    }

    /// <summary>스테이지에서 획득한 스타 기록을 기존 최고 기록과 비교하여 저장합니다.</summary>
    /// <param name="stageNumber">스타 기록을 저장할 스테이지 번호입니다.</param>
    /// <param name="collectedStars">이번 플레이에서 획득한 스타 수입니다.</param>
    /// <param name="availableStars">해당 스테이지에 배치된 전체 스타 수입니다.</param>
    public static void RecordStageStars(
        int stageNumber,
        int collectedStars,
        int availableStars)
    {
        int safeStageNumber = Mathf.Clamp(
            stageNumber,
            1,
            TotalStageCount);
        int safeAvailableStars = Mathf.Max(0, availableStars);
        int safeCollectedStars = Mathf.Clamp(
            collectedStars,
            0,
            safeAvailableStars);
        string collectedKey =
            CollectedStarsKeyPrefix + safeStageNumber;
        int previousBest = PlayerPrefs.GetInt(collectedKey, 0);
        if (safeCollectedStars > previousBest)
        {
            PlayerPrefs.SetInt(collectedKey, safeCollectedStars);
        }

        PlayerPrefs.SetInt(
            AvailableStarsKeyPrefix + safeStageNumber,
            safeAvailableStars);
        PlayerPrefs.Save();
    }

    /// <summary>모든 스테이지에서 기록된 최고 스타 획득 수의 합계를 반환합니다.</summary>
    /// <returns>전체 스테이지의 획득 스타 합계입니다.</returns>
    public static int GetTotalCollectedStars()
    {
        int totalCollectedStars = 0;
        int stageNumber = 1;
        while (stageNumber <= TotalStageCount)
        {
            totalCollectedStars += PlayerPrefs.GetInt(
                CollectedStarsKeyPrefix + stageNumber,
                0);
            stageNumber++;
        }

        return totalCollectedStars;
    }

    /// <summary>모든 스테이지에 배치된 스타 수 기록의 합계를 반환합니다.</summary>
    /// <returns>전체 스테이지의 배치 스타 합계입니다.</returns>
    public static int GetTotalAvailableStars()
    {
        int totalAvailableStars = 0;
        int stageNumber = 1;
        while (stageNumber <= TotalStageCount)
        {
            totalAvailableStars += PlayerPrefs.GetInt(
                AvailableStarsKeyPrefix + stageNumber,
                GetDefaultAvailableStarCount(stageNumber));
            stageNumber++;
        }

        return totalAvailableStars;
    }

    /// <summary>아직 저장 기록이 없는 스테이지의 기본 배치 스타 수를 반환합니다.</summary>
    /// <param name="stageNumber">기본 스타 수를 확인할 스테이지 번호입니다.</param>
    /// <returns>현재 레벨 설계에 포함된 기본 스타 수입니다.</returns>
    private static int GetDefaultAvailableStarCount(int stageNumber)
    {
        if (stageNumber == 1)
        {
            return 13;
        }

        if (stageNumber == 2)
        {
            return 15;
        }

        return 17;
    }

    /// <summary>지정한 스테이지를 이전에 클리어했는지 확인합니다.</summary>
    /// <param name="stageNumber">클리어 여부를 확인할 스테이지 번호입니다.</param>
    /// <returns>명시적인 클리어 기록이 있거나 다음 스테이지가 해금되어 있으면 true를 반환합니다.</returns>
    public static bool IsStageCleared(int stageNumber)
    {
        bool hasClearRecord =
            PlayerPrefs.GetInt(
                ClearedStageKeyPrefix + stageNumber,
                0) == 1;
        int highestUnlockedStage =
            PlayerPrefs.GetInt(HighestUnlockedStageKey, 1);
        bool inferredFromProgress =
            stageNumber < highestUnlockedStage;
        return hasClearRecord == true ||
            inferredFromProgress == true;
    }

    /// <summary>스테이지 번호에 해당하는 씬 이름을 반환합니다.</summary>
    /// <param name="stageNumber">씬 이름을 찾을 스테이지 번호입니다.</param>
    public static string GetStageSceneName(int stageNumber)
    {
        return "Stage" + Mathf.Clamp(
            stageNumber,
            1,
            TotalStageCount);
    }
}
