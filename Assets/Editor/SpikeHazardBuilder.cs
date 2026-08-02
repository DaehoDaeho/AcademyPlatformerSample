using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>기존 픽셀 스프라이트를 조합한 가시 프리팹을 만들고 모든 스테이지에 안전하게 배치합니다.</summary>
public static class SpikeHazardBuilder
{
    // 가시 함정 프리팹을 저장할 프로젝트 경로입니다.
    private const string SpikePrefabPath =
        "Assets/AcademyPlatformer/Prefabs/SpikeHazard.prefab";

    /// <summary>가시 프리팹을 생성하고 세 스테이지의 지정된 발판에 배치합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Rebuild Spike Hazards")]
    public static void BuildAllStages()
    {
        GameObject spikePrefab = CreateSpikePrefab();
        int stageNumber = 1;
        while (stageNumber <= StageProgressData.TotalStageCount)
        {
            Scene scene = EditorSceneManager.OpenScene(
                "Assets/Scenes/Stage" + stageNumber + ".unity",
                OpenSceneMode.Single);
            ReplaceHazardsForOpenStage(stageNumber, spikePrefab);
            EditorSceneManager.SaveScene(scene);
            stageNumber++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("SPIKE_HAZARD_BUILD_COMPLETED");
    }

    /// <summary>현재 열려 있는 스테이지에서 기존 가시를 제거하고 새 위치에 다시 배치합니다.</summary>
    /// <param name="stageNumber">가시 배치를 적용할 스테이지 번호입니다.</param>
    /// <param name="spikePrefab">배치할 가시 함정 프리팹입니다.</param>
    public static void ReplaceHazardsForOpenStage(
        int stageNumber,
        GameObject spikePrefab)
    {
        SpikeHazard[] existingHazards =
            Object.FindObjectsByType<SpikeHazard>(
                FindObjectsSortMode.None);
        foreach (SpikeHazard existingHazard in existingHazards)
        {
            Object.DestroyImmediate(existingHazard.gameObject);
        }

        Vector2Int[] cells = GetHazardCells(stageNumber);
        int hazardIndex = 0;
        foreach (Vector2Int cell in cells)
        {
            bool hasSafeRunUp =
                MultiStageBuilder.HasSafeHazardRunUp(
                    stageNumber,
                    cell,
                    3);
            if (hasSafeRunUp == false)
            {
                throw new System.InvalidOperationException(
                    "Stage " + stageNumber +
                    " spike at " + cell +
                    " does not leave three clear tiles at both platform edges.");
            }

            ValidateEnemyClearance(stageNumber, cell);
            ValidateStarClearance(stageNumber, cell);

            GameObject hazard =
                (GameObject)PrefabUtility.InstantiatePrefab(spikePrefab);
            hazard.name =
                "Stage " + stageNumber + " Spike " + (hazardIndex + 1);
            hazard.transform.position = new Vector3(
                cell.x,
                cell.y + 1.08f,
                0f);
            hazardIndex++;
        }
    }

    /// <summary>함정이 같은 발판의 적 또는 순찰 경로와 겹치지 않는지 검사합니다.</summary>
    /// <param name="stageNumber">검사할 스테이지 번호입니다.</param>
    /// <param name="hazardCell">검사할 함정의 타일 좌표입니다.</param>
    private static void ValidateEnemyClearance(
        int stageNumber,
        Vector2Int hazardCell)
    {
        StompableEnemy[] enemies =
            Object.FindObjectsByType<StompableEnemy>(
                FindObjectsSortMode.None);
        float hazardY = hazardCell.y + 1.08f;
        foreach (StompableEnemy enemy in enemies)
        {
            float verticalDistance =
                Mathf.Abs(enemy.transform.position.y - hazardY);
            bool isOnSamePlatform = verticalDistance < 1.5f;
            if (isOnSamePlatform == false)
            {
                continue;
            }

            bool sharesPlatform =
                MultiStageBuilder.SharesPlatformSection(
                    stageNumber,
                    hazardCell,
                    enemy.transform.position.x);
            if (sharesPlatform == true)
            {
                throw new System.InvalidOperationException(
                    "Stage " + stageNumber +
                    " spike at " + hazardCell +
                    " overlaps the safe space of enemy " + enemy.name + ".");
            }
        }
    }

    /// <summary>함정과 같은 발판에 있는 스타가 안전한 점프 간격을 확보했는지 검사합니다.</summary>
    /// <param name="stageNumber">검사할 스테이지 번호입니다.</param>
    /// <param name="hazardCell">검사할 함정의 타일 좌표입니다.</param>
    private static void ValidateStarClearance(
        int stageNumber,
        Vector2Int hazardCell)
    {
        Collectible[] stars =
            Object.FindObjectsByType<Collectible>(
                FindObjectsSortMode.None);
        float hazardY = hazardCell.y + 1.08f;
        foreach (Collectible star in stars)
        {
            float verticalDistance = Mathf.Abs(
                star.transform.position.y - hazardY);
            bool isOnSamePlatform = verticalDistance < 1.5f;
            if (isOnSamePlatform == false)
            {
                continue;
            }

            bool sharesPlatform =
                MultiStageBuilder.SharesPlatformSection(
                    stageNumber,
                    hazardCell,
                    star.transform.position.x);
            if (sharesPlatform == false)
            {
                continue;
            }

            float horizontalDistance = Mathf.Abs(
                star.transform.position.x - hazardCell.x);
            bool hasSafeDistance = horizontalDistance >= 3f;
            if (hasSafeDistance == false)
            {
                throw new System.InvalidOperationException(
                    "Stage " + stageNumber +
                    " spike at " + hazardCell +
                    " is too close to star " + star.name + ".");
            }
        }
    }

    /// <summary>가시 비주얼, 삼각형 트리거와 피해 기능을 포함한 프리팹을 생성합니다.</summary>
    /// <returns>생성된 가시 함정 프리팹을 반환합니다.</returns>
    private static GameObject CreateSpikePrefab()
    {
        Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/AcademyPlatformer/Art/Square.png");
        GameObject root = new GameObject("Spike Hazard");
        SpikeHazard hazard = root.AddComponent<SpikeHazard>();
        hazard.Configure(1);
        PolygonCollider2D trigger =
            root.AddComponent<PolygonCollider2D>();
        trigger.isTrigger = true;
        trigger.points = new Vector2[]
        {
            new Vector2(-0.78f, 0.02f),
            new Vector2(-0.48f, 0.72f),
            new Vector2(-0.23f, 0.02f),
            new Vector2(0f, 0.82f),
            new Vector2(0.23f, 0.02f),
            new Vector2(0.48f, 0.72f),
            new Vector2(0.78f, 0.02f)
        };

        CreateBaseVisual(
            root.transform,
            squareSprite,
            "Dark Base",
            new Vector3(0f, 0.08f, 0f),
            new Vector3(1.72f, 0.22f, 1f),
            new Color(0.16f, 0.08f, 0.1f, 1f),
            4);
        CreateBaseVisual(
            root.transform,
            squareSprite,
            "Warning Stripe",
            new Vector3(0f, 0.16f, 0f),
            new Vector3(1.56f, 0.1f, 1f),
            new Color(1f, 0.68f, 0.08f, 1f),
            5);

        float[] spikePositions = { -0.46f, 0f, 0.46f };
        int spikeIndex = 0;
        while (spikeIndex < spikePositions.Length)
        {
            GameObject visualObject = new GameObject(
                "Spike Visual " + (spikeIndex + 1));
            visualObject.transform.SetParent(root.transform, false);
            visualObject.transform.localPosition = new Vector3(
                spikePositions[spikeIndex],
                spikeIndex == 1 ? 0.48f : 0.42f,
                0f);
            visualObject.transform.localRotation =
                Quaternion.Euler(0f, 0f, 45f);
            visualObject.transform.localScale =
                spikeIndex == 1
                ? new Vector3(0.52f, 0.52f, 1f)
                : new Vector3(0.46f, 0.46f, 1f);
            SpriteRenderer renderer =
                visualObject.AddComponent<SpriteRenderer>();
            renderer.sprite = squareSprite;
            renderer.color = spikeIndex == 1
                ? new Color(0.92f, 0.98f, 1f, 1f)
                : new Color(0.62f, 0.76f, 0.86f, 1f);
            renderer.sortingOrder = 6;
            spikeIndex++;
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
            root,
            SpikePrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    /// <summary>가시 아래에 함정임을 강조하는 받침대 또는 경고 띠 비주얼을 생성합니다.</summary>
    /// <param name="parent">비주얼이 소속될 가시 루트입니다.</param>
    /// <param name="sprite">사각형 비주얼에 사용할 스프라이트입니다.</param>
    /// <param name="name">생성할 비주얼 오브젝트 이름입니다.</param>
    /// <param name="position">가시 루트 기준 위치입니다.</param>
    /// <param name="scale">가시 루트 기준 크기입니다.</param>
    /// <param name="color">비주얼에 적용할 색상입니다.</param>
    /// <param name="sortingOrder">다른 비주얼과 구분할 렌더링 순서입니다.</param>
    private static void CreateBaseVisual(
        Transform parent,
        Sprite sprite,
        string name,
        Vector3 position,
        Vector3 scale,
        Color color,
        int sortingOrder)
    {
        GameObject visualObject = new GameObject(name);
        visualObject.transform.SetParent(parent, false);
        visualObject.transform.localPosition = position;
        visualObject.transform.localScale = scale;
        SpriteRenderer renderer =
            visualObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    /// <summary>각 스테이지에서 적, 별과 주요 착지 지점을 피한 가시 배치 셀을 반환합니다.</summary>
    /// <param name="stageNumber">가시 위치를 가져올 스테이지 번호입니다.</param>
    /// <returns>가시가 놓일 타일 셀 좌표 배열입니다.</returns>
    private static Vector2Int[] GetHazardCells(int stageNumber)
    {
        if (stageNumber == 1)
        {
            return new Vector2Int[]
            {
                new Vector2Int(7, -4),
                new Vector2Int(83, 2),
                new Vector2Int(93, 14)
            };
        }

        if (stageNumber == 2)
        {
            return new Vector2Int[]
            {
                new Vector2Int(5, -4),
                new Vector2Int(36, 8),
                new Vector2Int(34, 14),
                new Vector2Int(56, 20)
            };
        }

        return new Vector2Int[]
        {
                new Vector2Int(5, -4)
        };
    }
}
