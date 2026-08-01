using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>기본 맵을 바탕으로 서로 다른 콘셉트와 적 구성을 가진 3개 스테이지를 생성합니다.</summary>
public static class MultiStageBuilder
{
    // 기본 스테이지의 원본으로 사용할 씬 경로입니다.
    private const string MainScenePath = "Assets/Scenes/Main.unity";
    // 생성된 스테이지 씬을 저장할 폴더입니다.
    private const string SceneFolderPath = "Assets/Scenes";

    /// <summary>스테이지 1부터 3까지 씬을 복제하고 각각 다른 구성을 적용합니다.</summary>
    public static void BuildStageScenes()
    {
        int stageNumber = 1;
        while (stageNumber <= StageProgressData.TotalStageCount)
        {
            Scene scene = EditorSceneManager.OpenScene(
                MainScenePath,
                OpenSceneMode.Single);
            ApplyStageConcept(stageNumber);
            RebuildStageLayout(stageNumber);
            ReplaceStageEnemies(stageNumber);
            ReplaceStageItemsAndGoal(stageNumber);
            GameObject spikePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/AcademyPlatformer/Prefabs/SpikeHazard.prefab");
            if (spikePrefab != null)
            {
                SpikeHazardBuilder.ReplaceHazardsForOpenStage(
                    stageNumber,
                    spikePrefab);
            }
            RebuildTilemapCollider();
            EditorSceneManager.SaveScene(
                scene,
                SceneFolderPath + "/Stage" + stageNumber + ".unity");
            stageNumber++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("MULTI_STAGE_BUILD_COMPLETED");
    }

    /// <summary>스테이지별 타일 색상, 배경 색감과 일부 발판 구조를 변경합니다.</summary>
    /// <param name="stageNumber">구성을 적용할 스테이지 번호입니다.</param>
    private static void ApplyStageConcept(int stageNumber)
    {
        Camera camera = Camera.main;
        Tilemap tilemap = Object.FindFirstObjectByType<Tilemap>();
        GameObject backgroundRoot =
            GameObject.Find("Parallax Background");

        if (stageNumber == 1)
        {
            camera.backgroundColor = new Color(0.08f, 0.12f, 0.22f);
            tilemap.color = Color.white;
            TintBackground(
                backgroundRoot,
                Color.white,
                Color.white,
                Color.white);
            return;
        }

        if (stageNumber == 2)
        {
            camera.backgroundColor = new Color(0.09f, 0.035f, 0.16f);
            tilemap.color = new Color(0.68f, 0.82f, 1f);
            TintBackground(
                backgroundRoot,
                new Color(0.72f, 0.58f, 1f),
                new Color(0.58f, 0.38f, 0.8f),
                new Color(0.38f, 0.3f, 0.62f));
            return;
        }

        camera.backgroundColor = new Color(0.16f, 0.025f, 0.025f);
        tilemap.color = new Color(1f, 0.62f, 0.38f);
        TintBackground(
            backgroundRoot,
            new Color(1f, 0.58f, 0.42f),
            new Color(0.72f, 0.24f, 0.26f),
            new Color(0.42f, 0.12f, 0.15f));
    }

    /// <summary>발판 한 구간의 시작과 끝 셀, 높이를 이해하기 쉽게 묶어 저장합니다.</summary>
    private struct PlatformSection
    {
        // 발판이 시작되는 X 셀입니다.
        public int StartX;
        // 발판이 끝나는 X 셀입니다.
        public int EndX;
        // 발판 윗면이 놓이는 Y 셀입니다.
        public int HeightY;
        // 두 줄로 채워진 지면인지 한 줄 공중 발판인지 나타냅니다.
        public bool IsGround;

        /// <summary>새 발판 구간의 셀 범위와 종류를 설정합니다.</summary>
        /// <param name="startX">발판 시작 X 셀입니다.</param>
        /// <param name="endX">발판 끝 X 셀입니다.</param>
        /// <param name="heightY">발판 윗면 Y 셀입니다.</param>
        /// <param name="isGround">두 줄 지면으로 만들지 여부입니다.</param>
        public PlatformSection(
            int startX,
            int endX,
            int heightY,
            bool isGround)
        {
            StartX = startX;
            EndX = endX;
            HeightY = heightY;
            IsGround = isGround;
        }
    }

    /// <summary>현재 타일 모양을 유지하면서 스테이지별로 안전한 진행 경로를 새로 배치합니다.</summary>
    /// <param name="stageNumber">새 경로를 적용할 스테이지 번호입니다.</param>
    private static void RebuildStageLayout(int stageNumber)
    {
        Tilemap tilemap = Object.FindFirstObjectByType<Tilemap>();
        TileBase groundLeft = tilemap.GetTile(new Vector3Int(-2, -4, 0));
        TileBase groundMiddle = tilemap.GetTile(new Vector3Int(0, -4, 0));
        TileBase groundRight = tilemap.GetTile(new Vector3Int(7, -4, 0));
        TileBase fillLeft = tilemap.GetTile(new Vector3Int(-2, -5, 0));
        TileBase fillMiddle = tilemap.GetTile(new Vector3Int(0, -5, 0));
        TileBase fillRight = tilemap.GetTile(new Vector3Int(7, -5, 0));
        TileBase floatingLeft = tilemap.GetTile(new Vector3Int(6, -1, 0));
        TileBase floatingMiddle = tilemap.GetTile(new Vector3Int(7, -1, 0));
        TileBase floatingRight = tilemap.GetTile(new Vector3Int(9, -1, 0));

        PlatformSection[] sections = GetStageSections(stageNumber);
        ValidateStageRoute(stageNumber, sections);
        tilemap.ClearAllTiles();
        foreach (PlatformSection section in sections)
        {
            if (section.IsGround == true)
            {
                PaintStrip(
                    tilemap,
                    section.StartX,
                    section.EndX,
                    section.HeightY,
                    groundLeft,
                    groundMiddle,
                    groundRight);
                PaintStrip(
                    tilemap,
                    section.StartX,
                    section.EndX,
                    section.HeightY - 1,
                    fillLeft,
                    fillMiddle,
                    fillRight);
            }
            else
            {
                PaintStrip(
                    tilemap,
                    section.StartX,
                    section.EndX,
                    section.HeightY,
                    floatingLeft,
                    floatingMiddle,
                    floatingRight);
            }
        }

        tilemap.CompressBounds();
        tilemap.RefreshAllTiles();
    }

    /// <summary>스테이지마다 서로 다른 형태이면서 연속 점프로 통과 가능한 발판 목록을 반환합니다.</summary>
    /// <param name="stageNumber">발판 목록을 가져올 스테이지 번호입니다.</param>
    /// <returns>플레이 순서대로 정렬된 발판 구간 배열입니다.</returns>
    private static PlatformSection[] GetStageSections(int stageNumber)
    {
        if (stageNumber == 1)
        {
            return new PlatformSection[]
            {
                new PlatformSection(-2, 10, -4, true),
                new PlatformSection(13, 25, -4, true),
                new PlatformSection(28, 40, -4, true),
                new PlatformSection(43, 55, -4, true),
                new PlatformSection(52, 60, -1, false),
                new PlatformSection(59, 67, 2, false),
                new PlatformSection(66, 74, 5, false),
                new PlatformSection(59, 67, 8, false),
                new PlatformSection(66, 74, 11, false),
                new PlatformSection(59, 67, 14, false),
                new PlatformSection(66, 74, 17, false),
                new PlatformSection(59, 67, 20, false),
                new PlatformSection(66, 74, 23, false),
                new PlatformSection(59, 67, 26, false),
                new PlatformSection(66, 74, 29, false),
                new PlatformSection(70, 79, 32, false)
            };
        }

        if (stageNumber == 2)
        {
            return new PlatformSection[]
            {
                new PlatformSection(-2, 8, -4, true),
                new PlatformSection(11, 21, -4, true),
                new PlatformSection(24, 34, -4, true),
                new PlatformSection(37, 48, -4, true),
                new PlatformSection(51, 63, -4, true),
                new PlatformSection(55, 64, -1, false),
                new PlatformSection(48, 57, 2, false),
                new PlatformSection(41, 50, 5, false),
                new PlatformSection(48, 57, 8, false),
                new PlatformSection(55, 64, 11, false),
                new PlatformSection(62, 71, 14, false),
                new PlatformSection(55, 64, 17, false),
                new PlatformSection(48, 57, 20, false),
                new PlatformSection(55, 64, 23, false),
                new PlatformSection(62, 71, 26, false),
                new PlatformSection(68, 77, 29, false),
                new PlatformSection(70, 79, 32, false)
            };
        }

        return new PlatformSection[]
        {
            new PlatformSection(-2, 9, -4, true),
            new PlatformSection(12, 23, -4, true),
            new PlatformSection(26, 37, -4, true),
            new PlatformSection(40, 52, -4, true),
            new PlatformSection(49, 58, -1, false),
            new PlatformSection(55, 64, 2, false),
            new PlatformSection(61, 70, 5, false),
            new PlatformSection(54, 63, 8, false),
            new PlatformSection(47, 56, 11, false),
            new PlatformSection(54, 63, 14, false),
            new PlatformSection(61, 70, 17, false),
            new PlatformSection(68, 77, 20, false),
            new PlatformSection(61, 70, 23, false),
            new PlatformSection(54, 63, 26, false),
            new PlatformSection(61, 70, 29, false),
            new PlatformSection(69, 79, 32, false)
        };
    }

    /// <summary>발판 사이 높이와 수평 간격이 플레이어의 점프 한계를 넘지 않는지 검사합니다.</summary>
    /// <param name="stageNumber">검사 중인 스테이지 번호입니다.</param>
    /// <param name="sections">진행 순서대로 배치된 발판 목록입니다.</param>
    private static void ValidateStageRoute(
        int stageNumber,
        PlatformSection[] sections)
    {
        int sectionIndex = 1;
        while (sectionIndex < sections.Length)
        {
            PlatformSection previous = sections[sectionIndex - 1];
            PlatformSection current = sections[sectionIndex];
            int heightDifference =
                current.HeightY - previous.HeightY;
            int horizontalGap = Mathf.Max(
                current.StartX - previous.EndX - 1,
                previous.StartX - current.EndX - 1,
                0);
            if (heightDifference > 3 || horizontalGap > 3)
            {
                throw new System.InvalidOperationException(
                    "Stage " + stageNumber +
                    "의 발판 " + sectionIndex +
                    " 구간은 한 번의 점프로 도달할 수 없습니다.");
            }

            sectionIndex++;
        }
    }

    /// <summary>시작, 가운데, 끝 타일을 구분해 가로 발판 한 줄을 그립니다.</summary>
    /// <param name="tilemap">타일을 배치할 타일맵입니다.</param>
    /// <param name="startX">시작 X 셀입니다.</param>
    /// <param name="endX">끝 X 셀입니다.</param>
    /// <param name="heightY">배치할 Y 셀입니다.</param>
    /// <param name="leftTile">왼쪽 끝 타일입니다.</param>
    /// <param name="middleTile">가운데 반복 타일입니다.</param>
    /// <param name="rightTile">오른쪽 끝 타일입니다.</param>
    private static void PaintStrip(
        Tilemap tilemap,
        int startX,
        int endX,
        int heightY,
        TileBase leftTile,
        TileBase middleTile,
        TileBase rightTile)
    {
        int currentX = startX;
        while (currentX <= endX)
        {
            TileBase selectedTile = middleTile;
            if (currentX == startX)
            {
                selectedTile = leftTile;
            }
            else if (currentX == endX)
            {
                selectedTile = rightTile;
            }

            tilemap.SetTile(
                new Vector3Int(currentX, heightY, 0),
                selectedTile);
            currentX++;
        }
    }

    /// <summary>배경의 세 레이어에 스테이지 콘셉트 색상을 적용합니다.</summary>
    /// <param name="root">패럴랙스 배경의 루트 오브젝트입니다.</param>
    /// <param name="farColor">먼 하늘 레이어의 색상입니다.</param>
    /// <param name="middleColor">중간 숲 레이어의 색상입니다.</param>
    /// <param name="nearColor">가까운 나무 레이어의 색상입니다.</param>
    private static void TintBackground(
        GameObject root,
        Color farColor,
        Color middleColor,
        Color nearColor)
    {
        if (root == null)
        {
            return;
        }

        SpriteRenderer[] renderers =
            root.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer.gameObject.name == "Far Sky")
            {
                renderer.color = farColor;
            }
            if (renderer.gameObject.name == "Distant Forest")
            {
                renderer.color = middleColor;
            }
            if (renderer.gameObject.name == "Near Trees")
            {
                renderer.color = nearColor;
            }
        }
    }

    /// <summary>타일 종류를 유지하면서 지정한 발판을 다른 높이로 이동합니다.</summary>
    /// <param name="tilemap">발판이 배치된 타일맵입니다.</param>
    /// <param name="startX">발판의 시작 X 셀입니다.</param>
    /// <param name="endX">발판의 끝 X 셀입니다.</param>
    /// <param name="sourceY">기존 발판의 Y 셀입니다.</param>
    /// <param name="targetY">이동할 Y 셀입니다.</param>
    private static void MovePlatformStrip(
        Tilemap tilemap,
        int startX,
        int endX,
        int sourceY,
        int targetY)
    {
        int currentX = startX;
        while (currentX <= endX)
        {
            Vector3Int sourcePosition =
                new Vector3Int(currentX, sourceY, 0);
            Vector3Int targetPosition =
                new Vector3Int(currentX, targetY, 0);
            TileBase tile = tilemap.GetTile(sourcePosition);
            tilemap.SetTile(sourcePosition, null);
            tilemap.SetTile(targetPosition, tile);
            currentX++;
        }

        tilemap.RefreshAllTiles();
    }

    /// <summary>타일 이동 결과가 합성 콜라이더에 즉시 반영되도록 충돌 형상을 다시 생성합니다.</summary>
    private static void RebuildTilemapCollider()
    {
        TilemapCollider2D tilemapCollider =
            Object.FindFirstObjectByType<TilemapCollider2D>();
        if (tilemapCollider == null)
        {
            return;
        }

        tilemapCollider.ProcessTilemapChanges();
        CompositeCollider2D compositeCollider =
            tilemapCollider.GetComponent<CompositeCollider2D>();
        if (compositeCollider != null)
        {
            compositeCollider.GenerateGeometry();
        }

        Physics2D.SyncTransforms();
    }

    /// <summary>기존 적을 제거하고 진행을 막지 않는 위치에 세 종류의 적을 함께 배치합니다.</summary>
    /// <param name="stageNumber">적 구성을 적용할 스테이지 번호입니다.</param>
    private static void ReplaceStageEnemies(int stageNumber)
    {
        StompableEnemy[] existingEnemies =
            Object.FindObjectsByType<StompableEnemy>(
                FindObjectsSortMode.None);
        foreach (StompableEnemy enemy in existingEnemies)
        {
            Object.DestroyImmediate(enemy.gameObject);
        }

        GameObject patrolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/AcademyPlatformer/Prefabs/PatrolEnemy.prefab");
        GameObject chasingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/AcademyPlatformer/Prefabs/ChasingEnemy.prefab");
        GameObject rangedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/AcademyPlatformer/Prefabs/RangedEnemy.prefab");

        if (stageNumber == 1)
        {
            SpawnPatrol(
                patrolPrefab,
                "Forest Patrol",
                18f,
                -2.2f,
                16f,
                21f,
                1.45f);
            SpawnEnemy(
                chasingPrefab,
                "Forest Chaser",
                34f,
                -2.2f);
            SpawnEnemy(
                rangedPrefab,
                "Forest Lookout",
                49f,
                -2.2f);
            SpawnPatrol(
                patrolPrefab,
                "Forest Upper Patrol",
                62f,
                9.8f,
                60f,
                64f,
                1.2f);
            return;
        }

        if (stageNumber == 2)
        {
            SpawnPatrol(
                patrolPrefab,
                "Twilight Patrol",
                16f,
                -2.2f,
                14f,
                19f,
                1.55f);
            SpawnEnemy(
                chasingPrefab,
                "Twilight Chaser",
                29f,
                -2.2f);
            SpawnEnemy(
                rangedPrefab,
                "Twilight Lookout",
                43f,
                -2.2f);
            SpawnPatrol(
                patrolPrefab,
                "Twilight Upper Patrol",
                52f,
                3.8f,
                49f,
                54f,
                1.25f);
            SpawnEnemy(
                rangedPrefab,
                "Twilight Upper Lookout",
                69f,
                15.8f);
            return;
        }

        SpawnPatrol(
            patrolPrefab,
            "Crimson Patrol",
            17f,
            -2.2f,
            15f,
            20f,
            1.4f);
        SpawnEnemy(
            chasingPrefab,
            "Crimson Chaser",
            31f,
            -2.2f);
        SpawnEnemy(
            rangedPrefab,
            "Crimson Lookout",
            47f,
            -2.2f);
        SpawnPatrol(
            patrolPrefab,
            "Crimson Upper Patrol",
            50f,
            12.8f,
            48f,
            52f,
            1.15f);
        SpawnPatrol(
            patrolPrefab,
            "Crimson High Patrol",
            65f,
            18.8f,
            64f,
            67f,
            1.1f);
    }

    /// <summary>기존 별과 목표를 제거하고 새 진행 경로의 안전한 위치에 다시 배치합니다.</summary>
    /// <param name="stageNumber">아이템을 다시 배치할 스테이지 번호입니다.</param>
    private static void ReplaceStageItemsAndGoal(int stageNumber)
    {
        Collectible[] existingCollectibles =
            Object.FindObjectsByType<Collectible>(
                FindObjectsSortMode.None);
        foreach (Collectible collectible in existingCollectibles)
        {
            Object.DestroyImmediate(collectible.gameObject);
        }

        Goal[] existingGoals =
            Object.FindObjectsByType<Goal>(
                FindObjectsSortMode.None);
        foreach (Goal existingGoal in existingGoals)
        {
            Object.DestroyImmediate(existingGoal.gameObject);
        }

        GameObject starPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/AcademyPlatformer/Prefabs/Star.prefab");
        GameObject goalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/AcademyPlatformer/Prefabs/Goal.prefab");
        Vector2[] starPositions = GetStageStarPositions(stageNumber);
        foreach (Vector2 starPosition in starPositions)
        {
            GameObject star =
                (GameObject)PrefabUtility.InstantiatePrefab(starPrefab);
            star.name = "Stage " + stageNumber + " Star";
            star.transform.position = starPosition;
        }

        GameObject goal =
            (GameObject)PrefabUtility.InstantiatePrefab(goalPrefab);
        goal.name = "Stage " + stageNumber + " Goal";
        goal.transform.position = new Vector3(77f, 34.75f, 0f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameManager manager =
            Object.FindFirstObjectByType<GameManager>();
        if (player != null && manager != null)
        {
            manager.Configure(
                player.GetComponent<Health>(),
                starPositions.Length,
                player.GetComponent<PlayerDeathSequence>(),
                player.GetComponent<PlayerClearSequence>(),
                stageNumber);
        }
    }

    /// <summary>각 스테이지의 새 발판 경로를 안내하도록 배치한 별 위치를 반환합니다.</summary>
    /// <param name="stageNumber">별 위치를 가져올 스테이지 번호입니다.</param>
    /// <returns>해당 스테이지의 모든 별 월드 좌표입니다.</returns>
    private static Vector2[] GetStageStarPositions(int stageNumber)
    {
        if (stageNumber == 1)
        {
            return new Vector2[]
            {
                new Vector2(4f, -2f),
                new Vector2(14f, -2f),
                new Vector2(30f, -2f),
                new Vector2(52f, -2f),
                new Vector2(56f, 1f),
                new Vector2(64f, 4f),
                new Vector2(70f, 7f),
                new Vector2(62f, 10f),
                new Vector2(70f, 13f),
                new Vector2(62f, 16f),
                new Vector2(70f, 19f),
                new Vector2(62f, 22f),
                new Vector2(70f, 25f),
                new Vector2(62f, 28f),
                new Vector2(70f, 31f),
                new Vector2(76f, 34f)
            };
        }

        if (stageNumber == 2)
        {
            return new Vector2[]
            {
                new Vector2(4f, -2f),
                new Vector2(12f, -2f),
                new Vector2(27f, -2f),
                new Vector2(39f, -2f),
                new Vector2(58f, -2f),
                new Vector2(61f, 1f),
                new Vector2(51f, 4f),
                new Vector2(45f, 7f),
                new Vector2(53f, 10f),
                new Vector2(60f, 13f),
                new Vector2(67f, 16f),
                new Vector2(58f, 19f),
                new Vector2(52f, 22f),
                new Vector2(60f, 25f),
                new Vector2(67f, 28f),
                new Vector2(75f, 34f)
            };
        }

        return new Vector2[]
        {
            new Vector2(4f, -2f),
            new Vector2(13f, -2f),
            new Vector2(29f, -2f),
            new Vector2(42f, -2f),
            new Vector2(54f, 1f),
            new Vector2(59f, 4f),
            new Vector2(66f, 7f),
            new Vector2(58f, 10f),
            new Vector2(51f, 13f),
            new Vector2(58f, 16f),
            new Vector2(66f, 19f),
            new Vector2(73f, 22f),
            new Vector2(65f, 25f),
            new Vector2(58f, 28f),
            new Vector2(66f, 31f),
            new Vector2(76f, 34f)
        };
    }

    /// <summary>순찰 적을 지정한 이동 범위와 함께 배치합니다.</summary>
    /// <param name="prefab">배치할 순찰 적 프리팹입니다.</param>
    /// <param name="name">씬에서 사용할 적 이름입니다.</param>
    /// <param name="x">시작 X 좌표입니다.</param>
    /// <param name="y">시작 Y 좌표입니다.</param>
    /// <param name="leftX">순찰 범위의 왼쪽 좌표입니다.</param>
    /// <param name="rightX">순찰 범위의 오른쪽 좌표입니다.</param>
    /// <param name="speed">순찰 이동 속도입니다.</param>
    private static void SpawnPatrol(
        GameObject prefab,
        string name,
        float x,
        float y,
        float leftX,
        float rightX,
        float speed)
    {
        GameObject enemy = SpawnEnemy(prefab, name, x, y);
        enemy.GetComponent<PatrolEnemy>().Configure(
            leftX,
            rightX,
            speed);
    }

    /// <summary>적 프리팹을 지정한 위치에 배치하고 생성된 오브젝트를 반환합니다.</summary>
    /// <param name="prefab">배치할 적 프리팹입니다.</param>
    /// <param name="name">씬에서 사용할 적 이름입니다.</param>
    /// <param name="x">배치할 X 좌표입니다.</param>
    /// <param name="y">배치할 Y 좌표입니다.</param>
    private static GameObject SpawnEnemy(
        GameObject prefab,
        string name,
        float x,
        float y)
    {
        GameObject enemy =
            (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        enemy.name = name;
        enemy.transform.position = new Vector3(x, y, 0f);
        return enemy;
    }
}
