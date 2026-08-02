using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>회복 포션 스프라이트와 프리팹을 구성하고 세 스테이지의 안전한 진행 지점에 배치합니다.</summary>
public static class HealthPotionBuilder
{
    // 투명 배경 회복 포션 스프라이트의 프로젝트 경로입니다.
    private const string PotionSpritePath =
        "Assets/AcademyPlatformer/Sprites/Items/HealthPotion.png";
    // 완성된 회복 포션 프리팹을 저장할 프로젝트 경로입니다.
    private const string PotionPrefabPath =
        "Assets/AcademyPlatformer/Prefabs/HealthPotion.prefab";

    /// <summary>포션 Sprite 임포트, 프리팹 생성과 세 스테이지 배치를 차례대로 처리합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Build Health Potions")]
    public static void Build()
    {
        AssetDatabase.ImportAsset(
            PotionSpritePath,
            ImportAssetOptions.ForceSynchronousImport);
        ConfigurePotionSprite();
        GameObject potionPrefab = CreatePotionPrefab();
        ValidatePotionFeedbackSetup();
        string previousScenePath = SceneManager.GetActiveScene().path;
        int stageNumber = 1;
        while (stageNumber <= StageProgressData.TotalStageCount)
        {
            PlacePotions(stageNumber, potionPrefab);
            stageNumber++;
        }

        if (string.IsNullOrEmpty(previousScenePath) == false)
        {
            EditorSceneManager.OpenScene(
                previousScenePath,
                OpenSceneMode.Single);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("HEALTH_POTION_BUILD_COMPLETED");
    }

    /// <summary>생성된 포션 PNG를 작은 게임 오브젝트용 픽셀 아트 Sprite로 임포트합니다.</summary>
    private static void ConfigurePotionSprite()
    {
        TextureImporter importer =
            AssetImporter.GetAtPath(PotionSpritePath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 1000f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 512;
        importer.SaveAndReimport();
    }

    /// <summary>SpriteRenderer, 트리거와 회복 행동이 포함된 포션 프리팹을 생성합니다.</summary>
    /// <returns>저장된 회복 포션 프리팹입니다.</returns>
    private static GameObject CreatePotionPrefab()
    {
        Sprite potionSprite =
            AssetDatabase.LoadAssetAtPath<Sprite>(PotionSpritePath);
        GameObject potionObject = new GameObject("Health Potion");
        SpriteRenderer renderer =
            potionObject.AddComponent<SpriteRenderer>();
        renderer.sprite = potionSprite;
        renderer.sortingOrder = 6;
        Rigidbody2D body = potionObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CapsuleCollider2D trigger =
            potionObject.AddComponent<CapsuleCollider2D>();
        trigger.isTrigger = true;
        trigger.direction = CapsuleDirection2D.Vertical;
        trigger.size = new Vector2(0.82f, 1.02f);
        trigger.offset = new Vector2(0f, -0.02f);
        HealthPotion potion = potionObject.AddComponent<HealthPotion>();
        potion.Configure(1);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
            potionObject,
            PotionPrefabPath);
        Object.DestroyImmediate(potionObject);
        return prefab;
    }

    /// <summary>기존 포션을 정리하고 진행 난이도와 적 위치를 고려한 두 지점에 새 포션을 배치합니다.</summary>
    /// <param name="stageNumber">포션을 배치할 스테이지 번호입니다.</param>
    /// <param name="potionPrefab">씬에 인스턴스화할 회복 포션 프리팹입니다.</param>
    private static void PlacePotions(
        int stageNumber,
        GameObject potionPrefab)
    {
        string scenePath =
            "Assets/Scenes/Stage" + stageNumber + ".unity";
        Scene scene = EditorSceneManager.OpenScene(
            scenePath,
            OpenSceneMode.Single);
        HealthPotion[] existingPotions =
            Object.FindObjectsByType<HealthPotion>(
                FindObjectsSortMode.None);
        foreach (HealthPotion existingPotion in existingPotions)
        {
            Object.DestroyImmediate(existingPotion.gameObject);
        }

        Vector2[] potionPositions = GetPotionPositions(stageNumber);
        int potionIndex = 0;
        while (potionIndex < potionPositions.Length)
        {
            ValidatePotionSafety(potionPositions[potionIndex]);
            GameObject potionObject =
                (GameObject)PrefabUtility.InstantiatePrefab(potionPrefab);
            potionObject.name =
                "Stage " + stageNumber +
                " Health Potion " + (potionIndex + 1);
            potionObject.transform.position = potionPositions[potionIndex];
            potionIndex++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    /// <summary>포션이 가시나 적과 너무 가까워 안전하게 획득할 수 없는 위치인지 검사합니다.</summary>
    /// <param name="potionPosition">검사할 포션의 월드 위치입니다.</param>
    private static void ValidatePotionSafety(Vector2 potionPosition)
    {
        SpikeHazard[] hazards = Object.FindObjectsByType<SpikeHazard>(
            FindObjectsSortMode.None); // 현재 스테이지에 배치된 모든 가시 함정입니다.
        foreach (SpikeHazard hazard in hazards)
        {
            float distanceFromHazard = Vector2.Distance(
                potionPosition,
                hazard.transform.position); // 포션과 가시 중심 사이의 거리입니다.
            if (distanceFromHazard < 4f)
            {
                throw new System.InvalidOperationException(
                    "회복 포션이 가시 함정과 너무 가깝습니다: " +
                    potionPosition);
            }
        }

        StompableEnemy[] enemies =
            Object.FindObjectsByType<StompableEnemy>(
                FindObjectsSortMode.None); // 현재 스테이지에 배치된 모든 적입니다.
        foreach (StompableEnemy enemy in enemies)
        {
            float distanceFromEnemy = Vector2.Distance(
                potionPosition,
                enemy.transform.position); // 포션과 적 중심 사이의 거리입니다.
            if (distanceFromEnemy < 4f)
            {
                throw new System.InvalidOperationException(
                    "회복 포션이 적과 너무 가깝습니다: " +
                    potionPosition);
            }
        }

        Collectible[] stars = Object.FindObjectsByType<Collectible>(
            FindObjectsSortMode.None); // 포션과 겹치지 않아야 하는 모든 Star 아이템입니다.
        foreach (Collectible star in stars)
        {
            float distanceFromStar = Vector2.Distance(
                potionPosition,
                star.transform.position); // 포션과 Star 중심 사이의 거리입니다.
            if (distanceFromStar < 2f)
            {
                throw new System.InvalidOperationException(
                    "회복 포션이 Star 아이템과 너무 가깝습니다: " +
                    potionPosition);
            }
        }
    }

    /// <summary>플레이어 프리팹에 회복음과 회복 파티클로 재사용할 애셋이 실제로 연결되어 있는지 검사합니다.</summary>
    private static void ValidatePotionFeedbackSetup()
    {
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/AcademyPlatformer/Prefabs/Player.prefab");
        if (playerPrefab == null)
        {
            throw new System.InvalidOperationException(
                "회복 피드백을 검사할 Player 프리팹이 없습니다.");
        }

        PlayerAudioFeedback audioFeedback =
            playerPrefab.GetComponent<PlayerAudioFeedback>(); // 회복음을 재생할 플레이어 오디오 컴포넌트입니다.
        PlayerVfxFeedback vfxFeedback =
            playerPrefab.GetComponent<PlayerVfxFeedback>(); // 회복 이펙트를 생성할 플레이어 시각 효과 컴포넌트입니다.
        if (audioFeedback == null || vfxFeedback == null)
        {
            throw new System.InvalidOperationException(
                "Player 프리팹에 회복 피드백 컴포넌트가 없습니다.");
        }

        SerializedObject serializedAudio =
            new SerializedObject(audioFeedback); // 오디오 클립 직렬화 참조를 확인할 객체입니다.
        SerializedObject serializedVfx =
            new SerializedObject(vfxFeedback); // 이펙트 프리팹 직렬화 참조를 확인할 객체입니다.
        AudioClip healingClip = serializedAudio.FindProperty(
            "collectibleClip").objectReferenceValue as AudioClip; // 회복음으로 재사용할 밝은 획득 사운드입니다.
        GameObject healingEffect = serializedVfx.FindProperty(
            "collectibleEffectPrefab").objectReferenceValue as GameObject; // 회복 파티클로 재사용할 이펙트 프리팹입니다.
        if (healingClip == null || healingEffect == null)
        {
            throw new System.InvalidOperationException(
                "Player 프리팹에 회복 사운드 또는 이펙트가 연결되지 않았습니다.");
        }

        ParticleSystem healingParticles =
            healingEffect.GetComponent<ParticleSystem>(); // 획득 즉시 재생될 회복 파티클 시스템입니다.
        if (healingParticles == null ||
            healingParticles.main.playOnAwake == false)
        {
            throw new System.InvalidOperationException(
                "회복 이펙트 프리팹이 자동 재생되도록 설정되지 않았습니다.");
        }
    }

    /// <summary>적 전투 직후이면서 안전한 발판 위에 놓인 스테이지별 포션 위치를 반환합니다.</summary>
    /// <param name="stageNumber">위치를 선택할 스테이지 번호입니다.</param>
    /// <returns>해당 스테이지의 포션 월드 위치 두 개입니다.</returns>
    private static Vector2[] GetPotionPositions(int stageNumber)
    {
        if (stageNumber == 1)
        {
            return new Vector2[]
            {
                new Vector2(68f, 1f),
                new Vector2(90f, 19f)
            };
        }

        if (stageNumber == 2)
        {
            return new Vector2[]
            {
            new Vector2(45f, -2f),
                new Vector2(39f, 19f)
            };
        }

        return new Vector2[]
        {
            new Vector2(62f, 3f),
            new Vector2(44f, 15f)
        };
    }
}
