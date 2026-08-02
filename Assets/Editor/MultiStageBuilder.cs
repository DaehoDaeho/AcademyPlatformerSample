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

    /// <summary>요청된 두 스타만 기존 씬에서 찾아 새 위치로 이동합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Fix Two Star Positions")]
    public static void FixTwoStarPositions()
    {
        MoveStarInScene(
            2,
            new Vector2(0f, -2f),
            new Vector2(23f, -2f));
        MoveStarInScene(
            3,
            new Vector2(91f, 26f),
            new Vector2(83f, 23f));
        AssetDatabase.SaveAssets();
        Debug.Log("TWO_STAR_POSITION_FIX_COMPLETED");
    }

    /// <summary>지정한 스테이지에서 기존 좌표와 일치하는 스타 하나만 새 좌표로 이동합니다.</summary>
    /// <param name="stageNumber">수정할 스테이지 번호입니다.</param>
    /// <param name="oldPosition">현재 스타의 월드 좌표입니다.</param>
    /// <param name="newPosition">스타를 옮길 새 월드 좌표입니다.</param>
    private static void MoveStarInScene(
        int stageNumber,
        Vector2 oldPosition,
        Vector2 newPosition)
    {
        string scenePath =
            SceneFolderPath + "/Stage" + stageNumber + ".unity";
        Scene scene = EditorSceneManager.OpenScene(
            scenePath,
            OpenSceneMode.Single);
        Collectible[] stars = Object.FindObjectsByType<Collectible>(
            FindObjectsSortMode.None);
        Collectible targetStar = null;
        foreach (Collectible star in stars)
        {
            float distance = Vector2.Distance(
                star.transform.position,
                oldPosition);
            if (distance <= 0.01f)
            {
                targetStar = star;
                break;
            }
        }

        if (targetStar == null)
        {
            throw new System.InvalidOperationException(
                "Stage " + stageNumber +
                " does not contain the requested star at " +
                oldPosition + ".");
        }

        targetStar.transform.position = new Vector3(
            newPosition.x,
            newPosition.y,
            targetStar.transform.position.z);
        EditorUtility.SetDirty(targetStar.transform);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

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
            ReplaceMovingPlatforms(stageNumber);
            ConfigureStageCamera(stageNumber);
            ReplaceStageEnemies(stageNumber);
            ReplaceStageItemsAndGoal(stageNumber);
            ValidateStarEnemySpacing(stageNumber);
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

        HealthPotionBuilder.Build();
        GameEndPresentationBuilder.Build();
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
                new PlatformSection(13, 30, -4, true),
                new PlatformSection(33, 42, -4, true),
                new PlatformSection(53, 66, -4, true),
                new PlatformSection(64, 76, -1, false),
                new PlatformSection(74, 86, 2, false),
                new PlatformSection(84, 96, 5, false),
                new PlatformSection(94, 106, 8, false),
                new PlatformSection(100, 112, 11, false),
                new PlatformSection(90, 102, 14, false),
                new PlatformSection(80, 92, 17, false),
                new PlatformSection(88, 100, 20, false),
                new PlatformSection(98, 112, 23, false)
            };
        }

        if (stageNumber == 2)
        {
            return new PlatformSection[]
            {
                new PlatformSection(-2, 8, -4, true),
                new PlatformSection(11, 25, -4, true),
                new PlatformSection(28, 40, -4, true),
                new PlatformSection(43, 61, -4, true),
                new PlatformSection(49, 62, 2, false),
                new PlatformSection(38, 51, 5, false),
                new PlatformSection(27, 40, 8, false),
                new PlatformSection(16, 29, 11, false),
                new PlatformSection(24, 37, 14, false),
                new PlatformSection(35, 48, 17, false),
                new PlatformSection(46, 59, 20, false),
                new PlatformSection(57, 70, 23, false),
                new PlatformSection(68, 80, 26, false)
            };
        }

        return new PlatformSection[]
        {
            new PlatformSection(-2, 9, -4, true),
            new PlatformSection(12, 23, -4, true),
            new PlatformSection(26, 36, -4, true),
            new PlatformSection(39, 50, -4, true),
            new PlatformSection(58, 64, 1, false),
            new PlatformSection(68, 74, 3, false),
            new PlatformSection(78, 84, 5, false),
            new PlatformSection(68, 74, 7, false),
            new PlatformSection(58, 64, 9, false),
            new PlatformSection(48, 54, 11, false),
            new PlatformSection(38, 44, 13, false),
            new PlatformSection(48, 54, 15, false),
            new PlatformSection(58, 64, 17, false),
            new PlatformSection(68, 74, 19, false),
            new PlatformSection(78, 84, 21, false),
            new PlatformSection(88, 94, 24, false)
        };
    }

    /// <summary>스테이지의 새 수평 길이에 맞춰 카메라 추적 범위를 개별 설정합니다.</summary>
    /// <param name="stageNumber">카메라 범위를 적용할 스테이지 번호입니다.</param>
    /// <summary>함정이 놓인 발판의 양쪽에 안전한 점프 여백이 있는지 확인합니다.</summary>
    /// <param name="stageNumber">검사할 스테이지 번호입니다.</param>
    /// <param name="hazardCell">검사할 함정의 타일 좌표입니다.</param>
    /// <param name="minimumClearTiles">발판 양끝에 확보할 최소 타일 수입니다.</param>
    /// <returns>함정 양쪽에 지정한 여백이 있으면 참을 반환합니다.</returns>
    /// <summary>기존 이동 플랫폼을 제거하고 스테이지별 필수 진행 경로에 새 이동 플랫폼을 배치합니다.</summary>
    /// <param name="stageNumber">이동 플랫폼을 배치할 스테이지 번호입니다.</param>
    private static void ReplaceMovingPlatforms(int stageNumber)
    {
        MovingPlatform[] existingPlatforms =
            Object.FindObjectsByType<MovingPlatform>(
                FindObjectsSortMode.None);
        foreach (MovingPlatform existingPlatform in existingPlatforms)
        {
            Object.DestroyImmediate(existingPlatform.gameObject);
        }

        Vector2[] pathPoints;
        float moveSpeed;
        string platformName;
        if (stageNumber == 1)
        {
            pathPoints = new Vector2[]
            {
                new Vector2(45f, -3.35f),
                new Vector2(50f, -3.35f)
            };
            moveSpeed = 2f;
            platformName = "Forest Horizontal Moving Platform";
        }
        else if (stageNumber == 2)
        {
            pathPoints = new Vector2[]
            {
                new Vector2(64.5f, -3.35f),
                new Vector2(64.5f, 1.7f)
            };
            moveSpeed = 1.6f;
            platformName = "Twilight Vertical Moving Platform";
        }
        else
        {
            pathPoints = new Vector2[]
            {
                new Vector2(53f, -2.4f),
                new Vector2(55f, -0.2f),
                new Vector2(56f, 1.5f)
            };
            moveSpeed = 1.8f;
            platformName = "Crimson Multi Path Moving Platform";
        }

        ValidateMovingPlatformClearance(stageNumber, pathPoints);
        Tilemap tilemap = Object.FindFirstObjectByType<Tilemap>();
        Sprite platformSprite = tilemap.GetSprite(
            new Vector3Int(0, -4, 0));
        TilemapRenderer tilemapRenderer =
            tilemap.GetComponent<TilemapRenderer>();
        GameObject platformObject = new GameObject(platformName);
        platformObject.layer = LayerMask.NameToLayer("Ground");
        platformObject.transform.position = pathPoints[0];

        Rigidbody2D platformBody =
            platformObject.AddComponent<Rigidbody2D>();
        platformBody.bodyType = RigidbodyType2D.Kinematic;
        platformBody.gravityScale = 0f;
        platformBody.useFullKinematicContacts = true;
        platformBody.freezeRotation = true;
        platformBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        platformBody.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        BoxCollider2D platformCollider =
            platformObject.AddComponent<BoxCollider2D>();
            platformCollider.size = new Vector2(3f, 1f);

        int visualIndex = -1;
        while (visualIndex <= 1)
        {
            GameObject visualObject =
                new GameObject("Platform Tile " + (visualIndex + 2));
            visualObject.transform.SetParent(
                platformObject.transform,
                false);
            visualObject.transform.localPosition =
                new Vector3(visualIndex, 0f, 0f);
            SpriteRenderer visualRenderer =
                visualObject.AddComponent<SpriteRenderer>();
            visualRenderer.sprite = platformSprite;
            visualRenderer.sortingLayerID =
                tilemapRenderer.sortingLayerID;
            visualRenderer.sortingOrder =
                tilemapRenderer.sortingOrder + 1;
            visualIndex++;
        }

        MovingPlatform movingPlatform =
            platformObject.AddComponent<MovingPlatform>();
        movingPlatform.Configure(pathPoints, moveSpeed, 0.55f);
    }

    /// <summary>이동 플랫폼의 전체 이동 경로가 고정 발판 영역과 겹치지 않는지 검사합니다.</summary>
    /// <param name="stageNumber">검사할 스테이지 번호입니다.</param>
    /// <param name="pathPoints">플랫폼이 순서대로 이동할 경로 지점 목록입니다.</param>
    private static void ValidateMovingPlatformClearance(
        int stageNumber,
        Vector2[] pathPoints)
    {
        PlatformSection[] sections = GetStageSections(stageNumber);
        int pathIndex = 1;
        while (pathIndex <= pathPoints.Length)
        {
            Vector2 startPoint = pathPoints[pathIndex - 1];
            int endPointIndex = pathIndex % pathPoints.Length;
            Vector2 endPoint = pathPoints[endPointIndex];
            float segmentDistance = Vector2.Distance(
                startPoint,
                endPoint);
            int sampleCount = Mathf.Max(
                1,
                Mathf.CeilToInt(segmentDistance / 0.2f));
            int sampleIndex = 0;
            while (sampleIndex <= sampleCount)
            {
                float sampleRatio =
                    (float)sampleIndex / sampleCount;
                Vector2 samplePosition = Vector2.Lerp(
                    startPoint,
                    endPoint,
                    sampleRatio);
                Rect movingBounds = new Rect(
                    samplePosition.x - 1.5f,
                    samplePosition.y - 0.5f,
                    3f,
                    1f);
                foreach (PlatformSection section in sections)
                {
                    Rect sectionBounds = new Rect(
                        section.StartX,
                        section.HeightY,
                        section.EndX - section.StartX + 1f,
                        1f);
                    bool overlapsSection =
                        movingBounds.Overlaps(sectionBounds);
                    if (overlapsSection == true)
                    {
                        throw new System.InvalidOperationException(
                            "Stage " + stageNumber +
                            " moving platform overlaps a fixed platform at " +
                            samplePosition + ".");
                    }
                }

                sampleIndex++;
            }

            pathIndex++;
        }
    }

    /// <summary>함정이 놓인 발판의 양쪽에 안전한 점프 여백이 있는지 확인합니다.</summary>
    /// <param name="stageNumber">검사할 스테이지 번호입니다.</param>
    /// <param name="hazardCell">검사할 함정의 타일 좌표입니다.</param>
    /// <param name="minimumClearTiles">발판 양끝에 확보할 최소 타일 수입니다.</param>
    /// <returns>함정 양쪽에 지정한 여백이 있으면 참을 반환합니다.</returns>
    public static bool HasSafeHazardRunUp(
        int stageNumber,
        Vector2Int hazardCell,
        int minimumClearTiles)
    {
        PlatformSection[] sections = GetStageSections(stageNumber);
        foreach (PlatformSection section in sections)
        {
            bool matchesHeight = section.HeightY == hazardCell.y;
            bool isInsideSection =
                hazardCell.x >= section.StartX &&
                hazardCell.x <= section.EndX;
            if (matchesHeight == true && isInsideSection == true)
            {
                int leftClearTiles = hazardCell.x - section.StartX;
                int rightClearTiles = section.EndX - hazardCell.x;
                bool hasLeftClearance =
                    leftClearTiles >= minimumClearTiles;
                bool hasRightClearance =
                    rightClearTiles >= minimumClearTiles;
                if (hasLeftClearance == true && hasRightClearance == true)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>함정과 다른 오브젝트가 동일한 정적 발판 구간 위에 있는지 확인합니다.</summary>
    /// <param name="stageNumber">검사할 스테이지 번호입니다.</param>
    /// <param name="hazardCell">기준으로 사용할 함정의 타일 좌표입니다.</param>
    /// <param name="otherX">비교할 다른 오브젝트의 X 좌표입니다.</param>
    /// <returns>두 오브젝트가 같은 정적 발판 구간에 있으면 참을 반환합니다.</returns>
    public static bool SharesPlatformSection(
        int stageNumber,
        Vector2Int hazardCell,
        float otherX)
    {
        PlatformSection[] sections = GetStageSections(stageNumber);
        foreach (PlatformSection section in sections)
        {
            bool matchesHeight = section.HeightY == hazardCell.y;
            bool containsHazard =
                hazardCell.x >= section.StartX &&
                hazardCell.x <= section.EndX;
            bool containsOther =
                otherX >= section.StartX &&
                otherX <= section.EndX;
            if (matchesHeight == true &&
                containsHazard == true &&
                containsOther == true)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>스테이지의 수평 길이에 맞춰 카메라 추적 범위를 설정합니다.</summary>
    /// <param name="stageNumber">카메라 범위를 적용할 스테이지 번호입니다.</param>
    private static void ConfigureStageCamera(int stageNumber)
    {
        CameraFollow cameraFollow =
            Object.FindFirstObjectByType<CameraFollow>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (cameraFollow == null || player == null)
        {
            return;
        }

        float maximumX = stageNumber == 1 ? 108f : 84f;
        if (stageNumber == 3)
        {
            maximumX = 94f;
        }
        cameraFollow.Configure(player.transform, 0f, maximumX);
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
            bool needsMovingPlatform =
                heightDifference > 3 || horizontalGap > 3;
            bool hasMovingPlatformConnection =
                IsMovingPlatformConnection(stageNumber, sectionIndex);
            float verticalClearance = heightDifference - 1f;
            bool hasLowOverhead =
                heightDifference > 0 && verticalClearance < 1.3f;
            bool hasSideClearance = horizontalGap >= 2;
            if (hasLowOverhead == true &&
                hasSideClearance == false)
            {
                throw new System.InvalidOperationException(
                    "Stage " + stageNumber +
                    " platform " + sectionIndex +
                    " does not leave enough side clearance for the player collider.");
            }

            if (needsMovingPlatform == true &&
                hasMovingPlatformConnection == false)
            {
                throw new System.InvalidOperationException(
                    "Stage " + stageNumber +
                    "의 발판 " + sectionIndex +
                    " 구간은 한 번의 점프로 도달할 수 없습니다.");
            }

            sectionIndex++;
        }
    }

    /// <summary>정적 발판 사이의 큰 간격이 의도된 이동 플랫폼 구간인지 확인합니다.</summary>
    /// <param name="stageNumber">검사할 스테이지 번호입니다.</param>
    /// <param name="sectionIndex">현재 도착 발판의 배열 인덱스입니다.</param>
    /// <returns>이동 플랫폼으로 연결되는 구간이면 참을 반환합니다.</returns>
    private static bool IsMovingPlatformConnection(
        int stageNumber,
        int sectionIndex)
    {
        if (stageNumber == 1 && sectionIndex == 3)
        {
            return true;
        }

        if (stageNumber == 2 && sectionIndex == 4)
        {
            return true;
        }

        if (stageNumber == 3 && sectionIndex == 4)
        {
            return true;
        }

        return false;
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
                20f,
                -2.2f,
                18f,
                22f,
                1.45f);
            SpawnEnemy(
                chasingPrefab,
                "Forest Chaser",
                38f,
                -2.2f);
            SpawnRanged(
                rangedPrefab,
                "Forest Lookout",
                63f,
                -2.2f,
                5f,
                51f,
                66f);
            SpawnPatrol(
                patrolPrefab,
                "Forest Upper Patrol",
                90f,
                6.8f,
                86f,
                94f,
                1.2f);
            return;
        }

        if (stageNumber == 2)
        {
            SpawnPatrol(
                patrolPrefab,
                "Twilight Patrol",
                18f,
                -2.2f,
                15f,
                18f,
                1.55f);
            SpawnEnemy(
                chasingPrefab,
                "Twilight Chaser",
                32f,
                -2.2f);
            SpawnRanged(
                rangedPrefab,
                "Twilight Lookout",
                55f,
                -2.2f,
                5f,
                43f,
                61f);
            SpawnPatrol(
                patrolPrefab,
                "Twilight Upper Patrol",
                44f,
                6.8f,
                40f,
                49f,
                1.25f);
            return;
        }

        SpawnPatrol(
            patrolPrefab,
            "Crimson Patrol",
            42f,
            -2.2f,
            40f,
            43f,
            1.4f);
        SpawnEnemy(
            chasingPrefab,
            "Crimson Chaser",
            32f,
            -2.2f);
        SpawnRanged(
            rangedPrefab,
            "Crimson Lookout",
            21f,
            -2.2f,
            4f,
            12f,
            23f);
        SpawnPatrol(
            patrolPrefab,
            "Crimson Upper Patrol",
            73f,
            4.8f,
            72f,
            73f,
            1.15f);
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
        goal.transform.position = GetStageGoalPosition(stageNumber);

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

    /// <summary>서로 다른 최종 발판에 맞는 스테이지별 목표 위치를 반환합니다.</summary>
    /// <param name="stageNumber">목표 위치를 가져올 스테이지 번호입니다.</param>
    /// <returns>해당 스테이지의 목표 월드 위치입니다.</returns>
    /// <summary>스타가 적의 시작 위치나 순찰 범위와 겹치지 않는지 검사합니다.</summary>
    /// <param name="stageNumber">검사할 스테이지 번호입니다.</param>
    private static void ValidateStarEnemySpacing(int stageNumber)
    {
        Collectible[] stars =
            Object.FindObjectsByType<Collectible>(
                FindObjectsSortMode.None);
        StompableEnemy[] enemies =
            Object.FindObjectsByType<StompableEnemy>(
                FindObjectsSortMode.None);
        foreach (Collectible star in stars)
        {
            foreach (StompableEnemy enemy in enemies)
            {
                float verticalDistance = Mathf.Abs(
                    star.transform.position.y -
                    enemy.transform.position.y);
                bool isOnSamePlatform = verticalDistance < 1.5f;
                if (isOnSamePlatform == false)
                {
                    continue;
                }

                float enemyLeftX = enemy.transform.position.x;
                float enemyRightX = enemy.transform.position.x;
                PatrolEnemy patrolEnemy =
                    enemy.GetComponent<PatrolEnemy>();
                if (patrolEnemy != null)
                {
                    SerializedObject patrolData =
                        new SerializedObject(patrolEnemy);
                    enemyLeftX =
                        patrolData.FindProperty("leftX").floatValue;
                    enemyRightX =
                        patrolData.FindProperty("rightX").floatValue;
                }

                float leftClearance = enemyLeftX -
                    star.transform.position.x;
                float rightClearance =
                    star.transform.position.x - enemyRightX;
                bool isSafelyLeft = leftClearance >= 3f;
                bool isSafelyRight = rightClearance >= 3f;
                bool hasSafeSeparation =
                    isSafelyLeft == true || isSafelyRight == true;
                if (hasSafeSeparation == false)
                {
                    throw new System.InvalidOperationException(
                        "Stage " + stageNumber +
                        " star at " + star.transform.position +
                        " overlaps the safe space of enemy " +
                        enemy.name + ".");
                }
            }
        }
    }

    /// <summary>스테이지의 마지막 발판에 맞는 목표 위치를 반환합니다.</summary>
    /// <param name="stageNumber">목표 위치를 가져올 스테이지 번호입니다.</param>
    /// <returns>해당 스테이지의 목표 지점 월드 좌표입니다.</returns>
    private static Vector3 GetStageGoalPosition(int stageNumber)
    {
        if (stageNumber == 1)
        {
            return new Vector3(108f, 25.75f, 0f);
        }

        if (stageNumber == 2)
        {
            return new Vector3(77f, 28.75f, 0f);
        }

        return new Vector3(91f, 26.75f, 0f);
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
                new Vector2(34f, -2f),
                new Vector2(56f, -2f),
                new Vector2(70f, 1f),
                new Vector2(80f, 4f),
                new Vector2(98f, 10f),
                new Vector2(104f, 10f),
                new Vector2(106f, 13f),
                new Vector2(96f, 16f),
                new Vector2(86f, 19f),
                new Vector2(94f, 22f),
                new Vector2(105f, 25f)
            };
        }

        if (stageNumber == 2)
        {
            return new Vector2[]
            {
                new Vector2(23f, -2f),
                new Vector2(12f, -2f),
                new Vector2(28f, -2f),
                new Vector2(50f, -2f),
                new Vector2(60f, -2f),
                new Vector2(60f, 4f),
                new Vector2(55f, 4f),
                new Vector2(39f, 10f),
                new Vector2(33f, 10f),
                new Vector2(22f, 13f),
                new Vector2(30f, 16f),
                new Vector2(42f, 19f),
                new Vector2(53f, 22f),
                new Vector2(64f, 25f),
                new Vector2(75f, 28f)
            };
        }

        return new Vector2[]
        {
            new Vector2(2f, -2f),
            new Vector2(16f, -2f),
            new Vector2(27f, -2f),
            new Vector2(36f, -2f),
            new Vector2(46f, -2f),
            new Vector2(59f, 3f),
            new Vector2(68f, 5f),
            new Vector2(79f, 7f),
            new Vector2(71f, 9f),
            new Vector2(63f, 11f),
            new Vector2(52f, 13f),
            new Vector2(40f, 15f),
            new Vector2(54f, 17f),
            new Vector2(63f, 19f),
            new Vector2(71f, 21f),
            new Vector2(79f, 23f),
            new Vector2(83f, 23f)
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

    /// <summary>투사체를 피할 준비 거리와 적을 밟을 공간을 검증한 뒤 원거리 적을 배치합니다.</summary>
    /// <param name="prefab">배치할 원거리 적 프리팹입니다.</param>
    /// <param name="name">씬에서 사용할 적 이름입니다.</param>
    /// <param name="x">적을 배치할 X 좌표입니다.</param>
    /// <param name="y">적을 배치할 Y 좌표입니다.</param>
    /// <param name="sightDistance">플레이어를 감지할 수평 거리입니다.</param>
    /// <param name="approachStartX">플레이어가 전투 발판에 진입하는 X 좌표입니다.</param>
    /// <param name="platformEndX">전투 발판의 오른쪽 끝 X 좌표입니다.</param>
    /// <returns>검증 후 생성된 원거리 적 오브젝트입니다.</returns>
    private static GameObject SpawnRanged(
        GameObject prefab,
        string name,
        float x,
        float y,
        float sightDistance,
        float approachStartX,
        float platformEndX)
    {
        float preparationDistance =
            x - sightDistance - approachStartX;
        float stompLandingDistance = platformEndX - x;
        bool hasPreparationDistance = preparationDistance >= 5f;
        bool hasStompLandingDistance = stompLandingDistance >= 2f;
        if (hasPreparationDistance == false ||
            hasStompLandingDistance == false)
        {
            throw new System.InvalidOperationException(
                name + " does not have enough projectile avoidance space.");
        }

        GameObject enemy = SpawnEnemy(prefab, name, x, y);
        RangedEnemyLookout lookout =
            enemy.GetComponent<RangedEnemyLookout>();
        lookout.SetSightDistance(sightDistance);
        return enemy;
    }
}
